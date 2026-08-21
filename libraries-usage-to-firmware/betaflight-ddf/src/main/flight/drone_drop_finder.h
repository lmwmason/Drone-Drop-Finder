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

#pragma once

#include <stdbool.h>
#include <stdint.h>

#include "flight/failsafe.h"
#include "io/drone_drop.h"
#include "pg/pg.h"

typedef struct droneDropFinderConfig_s {
    uint8_t enabled;           // master on/off switch; disabling skips the SD-card wait at boot entirely
    uint8_t triggerOnRxLoss;   // also fire a prediction as soon as RX is lost (FAILSAFE_RX_LOSS_DETECTED), not just once landing/rescue begins
    uint16_t updateIntervalMs; // minimum spacing between re-predictions while a failure episode is ongoing
} droneDropFinderConfig_t;

PG_DECLARE(droneDropFinderConfig_t, droneDropFinderConfig);

/*
 * Loads sweep.csv from the SD card into the drone_drop LUT. Call once at
 * boot, after the SD card / afatfs subsystem has been initialised and
 * before the scheduler starts. Blocks until the card responds or a
 * timeout elapses. Returns false if no prediction will be available this
 * flight (missing card, missing/bad file, or feature disabled via
 * droneDropFinderConfig()->enabled) — this is never fatal to boot.
 */
bool droneDropFinderInit(void);

/*
 * Call once per failsafe update cycle with the current failsafe phase.
 * Fires an initial drone_drop_predict() the moment the phase transitions
 * into an active failure phase (FAILSAFE_LANDING or FAILSAFE_GPS_RESCUE,
 * plus FAILSAFE_RX_LOSS_DETECTED when triggerOnRxLoss is enabled), then
 * keeps re-predicting every droneDropFinderConfig()->updateIntervalMs
 * while that episode continues, so the estimate tracks the aircraft's
 * actual GPS state instead of freezing at the first sample.
 */
void droneDropFinderUpdate(failsafePhase_e phase);

/*
 * Fetch the most recent prediction, if any exists yet this flight.
 * Returns false and leaves *out unchanged if no prediction has been made.
 */
bool droneDropFinderGetPrediction(DroneDrop_Prediction *out);

/*
 * Fetch the GPS position and heading captured at the same instant as the
 * most recent prediction. Gated by the same "has a prediction yet" flag as
 * droneDropFinderGetPrediction(), so this is never partially populated.
 */
bool droneDropFinderGetGpsSnapshot(int32_t *latE7, int32_t *lonE7, uint16_t *headingDeciDeg);

/*
 * Fetch the altitude/h-speed/v-speed inputs used for the most recent
 * prediction — the "combo" a search-assistant web app matches against its
 * precomputed grids. Same gating as droneDropFinderGetGpsSnapshot().
 */
bool droneDropFinderGetComboSnapshot(float *altitudeM, float *hSpeedMps, float *vSpeedMps);
