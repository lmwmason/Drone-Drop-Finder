/*
 * This file is part of Cleanflight and Betaflight.
 *
 * Cleanflight and Betaflight are free software. You can redistribute
 * this software and/or modify this software under the terms of the
 * GNU General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option)
 * any later version.
 *
 * Cleanflight and Betaflight are distributed in the hope that they
 * will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this software.
 *
 * If not, see <http://www.gnu.org/licenses/>.
 */

#include "platform.h"

#ifdef USE_DRONE_DROP_FINDER

#include <math.h>

#include "common/maths.h"
#include "common/utils.h"
#include "drivers/time.h"
#include "io/asyncfatfs/asyncfatfs.h"
#include "io/drone_drop.h"
#include "io/gps.h"

#include "fc/runtime_config.h"
#include "flight/failsafe.h"
#include "flight/position.h"

#ifdef USE_BLACKBOX
#include "blackbox/blackbox.h"
#include "blackbox/blackbox_fielddefs.h"
#endif

#include "pg/pg_ids.h"

#include "flight/drone_drop_finder.h"

#define DRONE_DROP_FINDER_FS_TIMEOUT_MS 3000
#define DRONE_DROP_FINDER_CSV_PATH      "sweep.csv"

PG_REGISTER_WITH_RESET_TEMPLATE(droneDropFinderConfig_t, droneDropFinderConfig, PG_DRONE_DROP_FINDER_CONFIG, 0);

PG_RESET_TEMPLATE(droneDropFinderConfig_t, droneDropFinderConfig,
    .enabled = 1,
    .triggerOnRxLoss = 0,
    .updateIntervalMs = 1000,
);

static bool s_lutReady;
static bool s_hasPrediction;
static DroneDrop_Prediction s_lastPrediction;
static failsafePhase_e s_prevPhase = FAILSAFE_IDLE;
static timeMs_t s_lastUpdateMs;

static int32_t  s_lastLatE7;
static int32_t  s_lastLonE7;
static uint16_t s_lastHeadingDeciDeg; // degrees * 10, matches gpsSol.groundCourse convention
static float    s_lastAltitudeM;
static float    s_lastHSpeedMps;
static float    s_lastVSpeedMps;

// FAILSAFE_RX_LOSS_DETECTED is included only when triggerOnRxLoss is set, so
// users who find an early (pre-rescue) prediction more useful than noise can
// opt in without changing behaviour for everyone else.
static bool isActiveFailurePhase(failsafePhase_e phase)
{
    if (phase == FAILSAFE_LANDING || phase == FAILSAFE_GPS_RESCUE) {
        return true;
    }
    return droneDropFinderConfig()->triggerOnRxLoss && phase == FAILSAFE_RX_LOSS_DETECTED;
}

bool droneDropFinderInit(void)
{
    s_lutReady = false;
    s_hasPrediction = false;

    if (!droneDropFinderConfig()->enabled) {
        // Skip the SD-card wait entirely when disabled, so a user who doesn't
        // want this feature doesn't pay its boot-time latency either.
        return false;
    }

    const timeMs_t deadline = millis() + DRONE_DROP_FINDER_FS_TIMEOUT_MS;
    while (afatfs_getFilesystemState() == AFATFS_FILESYSTEM_STATE_INITIALIZATION) {
        afatfs_poll();
        if (cmp32(millis(), deadline) >= 0) {
            break;
        }
    }

    if (afatfs_getFilesystemState() != AFATFS_FILESYSTEM_STATE_READY) {
        return false;
    }

    s_lutReady = (drone_drop_init(DRONE_DROP_FINDER_CSV_PATH) == DRONE_DROP_OK);
    return s_lutReady;
}

void droneDropFinderUpdate(failsafePhase_e phase)
{
    const bool wasActive = isActiveFailurePhase(s_prevPhase);
    const bool isActive = isActiveFailurePhase(phase);
    s_prevPhase = phase;

    if (!isActive || !s_lutReady || !droneDropFinderConfig()->enabled || !STATE(GPS_FIX)) {
        return;
    }

    const timeMs_t now = millis();
    const bool justEntered = !wasActive;
    if (!justEntered && cmp32(now, s_lastUpdateMs) < (int32_t)droneDropFinderConfig()->updateIntervalMs) {
        // Still in the same failure episode as last time we predicted, and
        // not enough time has passed — re-predicting every scheduler tick
        // would burn CPU for no benefit since GPS itself only updates a few
        // times a second.
        return;
    }

    const float altitudeM = getAltitudeCm() / 100.0f;
    const float velN = gpsSol.velned.velN / 100.0f;
    const float velE = gpsSol.velned.velE / 100.0f;
    const float velD = gpsSol.velned.velD / 100.0f;
    const float hSpeedMps = sqrtf(velN * velN + velE * velE);
    const float vSpeedMps = -velD;

    if (drone_drop_predict(altitudeM, hSpeedMps, vSpeedMps, &s_lastPrediction) != DRONE_DROP_OK) {
        return;
    }
    s_lastUpdateMs = now;

    // Captured from the same GPS sample used for the prediction inputs above,
    // and using the same atan2f(velE, velN) heading convention the library's
    // own README documents for rotating a body-frame prediction back to
    // North/East — so a companion app doing that rotation from the OSD-read
    // heading reproduces exactly what drone_drop_predict() assumed.
    s_lastLatE7 = gpsSol.llh.lat;
    s_lastLonE7 = gpsSol.llh.lon;
    s_lastHeadingDeciDeg = (uint16_t)(fmodf(atan2f(velE, velN) * (1800.0f / M_PIf) + 3600.0f, 3600.0f));
    s_lastAltitudeM = altitudeM;
    s_lastHSpeedMps = hSpeedMps;
    s_lastVSpeedMps = vSpeedMps;

    s_hasPrediction = true;

#ifdef USE_BLACKBOX
    flightLogEventData_t eventData;
    eventData.droneDropPrediction.meanXMeters = s_lastPrediction.mean_x_m;
    eventData.droneDropPrediction.meanYMeters = s_lastPrediction.mean_y_m;
    eventData.droneDropPrediction.meanDistanceMeters = s_lastPrediction.mean_distance_m;
    eventData.droneDropPrediction.p95DistanceMeters = s_lastPrediction.p95_distance_m;
    blackboxLogEvent(FLIGHT_LOG_EVENT_DRONE_DROP_PREDICTION, &eventData);
#endif
}

bool droneDropFinderGetPrediction(DroneDrop_Prediction *out)
{
    if (!s_hasPrediction) {
        return false;
    }
    *out = s_lastPrediction;
    return true;
}

bool droneDropFinderGetGpsSnapshot(int32_t *latE7, int32_t *lonE7, uint16_t *headingDeciDeg)
{
    if (!s_hasPrediction) {
        return false;
    }
    *latE7 = s_lastLatE7;
    *lonE7 = s_lastLonE7;
    *headingDeciDeg = s_lastHeadingDeciDeg;
    return true;
}

bool droneDropFinderGetComboSnapshot(float *altitudeM, float *hSpeedMps, float *vSpeedMps)
{
    if (!s_hasPrediction) {
        return false;
    }
    *altitudeM = s_lastAltitudeM;
    *hSpeedMps = s_lastHSpeedMps;
    *vSpeedMps = s_lastVSpeedMps;
    return true;
}

#endif // USE_DRONE_DROP_FINDER
