#include "AP_DroneDropFinder.h"

#if AP_DRONEDROPFINDER_ENABLED

#include <math.h>

#include <AP_HAL/AP_HAL.h>
#include <AP_AHRS/AP_AHRS.h>
#include <GCS_MAVLink/GCS.h>
#include <AP_Logger/AP_Logger.h>

#include "LogStructure.h"

// give up retrying drone_drop_init() this many ms after the first attempt,
// so a permanently-missing sweep.csv doesn't retry forever
#define AP_DRONEDROPFINDER_INIT_RETRY_BUDGET_MS 10000

const AP_Param::GroupInfo AP_DroneDropFinder::var_info[] = {

    // @Param: ENABLE
    // @DisplayName: Drone Drop Finder enable
    // @Description: Enables post-failure crash-location prediction using a pre-loaded Monte-Carlo sweep.csv lookup table.
    // @Values: 0:Disabled,1:Enabled
    // @User: Standard
    AP_GROUPINFO_FLAGS("ENABLE", 0, AP_DroneDropFinder, _enabled, 0, AP_PARAM_FLAG_ENABLE),

    // @Param: TRIGGER
    // @DisplayName: Drone Drop Finder trigger sources
    // @Description: Bitmask of failsafe/crash conditions that trigger a crash-location prediction.
    // @Bitmask: 0:Radio failsafe,1:GCS failsafe,2:Battery failsafe,3:Crash detected
    // @User: Standard
    AP_GROUPINFO("TRIGGER", 1, AP_DroneDropFinder, _trigger_mask, 1),

    // @Param: INTERVAL_MS
    // @DisplayName: Drone Drop Finder minimum re-predict interval
    // @Description: Minimum time between successive predictions once triggered, to bound CPU cost. The first prediction of a new trigger episode always fires immediately regardless of this value.
    // @Units: ms
    // @Range: 200 5000
    // @User: Advanced
    AP_GROUPINFO("INTERVAL_MS", 2, AP_DroneDropFinder, _interval_ms, 1000),

    AP_GROUPEND
};

AP_DroneDropFinder::AP_DroneDropFinder()
{
#if CONFIG_HAL_BOARD == HAL_BOARD_SITL
    if (_singleton != nullptr) {
        AP_HAL::panic("AP_DroneDropFinder must be singleton");
    }
#endif
    _singleton = this;
    AP_Param::setup_object_defaults(this, var_info);
}

void AP_DroneDropFinder::init()
{
    if (_enabled <= 0) {
        return;
    }
    _first_init_attempt_ms = AP_HAL::millis();
    try_init(); // best-effort now; update() retries if this fails
}

bool AP_DroneDropFinder::try_init()
{
    if (_lut_ready || _init_failed_permanently) {
        return _lut_ready;
    }
    if (drone_drop_init(AP_DRONEDROPFINDER_CSV_PATH) == DRONE_DROP_OK) {
        _lut_ready = true;
        GCS_SEND_TEXT(MAV_SEVERITY_INFO, "DDF: loaded %d LUT entries", drone_drop_lut_size());
    } else if (AP_HAL::millis() - _first_init_attempt_ms > AP_DRONEDROPFINDER_INIT_RETRY_BUDGET_MS) {
        _init_failed_permanently = true;
        GCS_SEND_TEXT(MAV_SEVERITY_WARNING, "DDF: sweep.csv load failed");
    }
    return _lut_ready;
}

void AP_DroneDropFinder::update(bool trigger_active, uint8_t trigger_source)
{
    if (_enabled <= 0) {
        return;
    }
    if (!_lut_ready && !try_init()) {
        return; // still waiting on the filesystem, or gave up permanently
    }

    trigger_source &= (uint8_t)_trigger_mask.get();
    if (trigger_source == 0) {
        trigger_active = false;
    }
    if (!trigger_active) {
        _prev_trigger_active = false;
        return;
    }

    const uint32_t now_ms = AP_HAL::millis();
    const bool just_entered = !_prev_trigger_active;
    _prev_trigger_active = true;
    if (!just_entered && (now_ms - _last_predict_ms) < (uint32_t)_interval_ms.get()) {
        return;
    }
    _last_predict_ms = now_ms;

    predict_and_report(trigger_source);
}

void AP_DroneDropFinder::predict_and_report(uint8_t trigger_source)
{
    Location loc;
    if (!AP::ahrs().get_location(loc)) {
        return;
    }
    Vector3f vel_ned;
    if (!AP::ahrs().get_velocity_NED(vel_ned)) {
        return;
    }

    const float h_speed = vel_ned.xy().length();
    const float v_speed = -vel_ned.z; // NED down -> climb positive, matches drone_drop.h convention

    DroneDrop_Prediction pred;
    if (drone_drop_predict(_altitude_m, h_speed, v_speed, &pred) != DRONE_DROP_OK) {
        return;
    }

    // rotate the body-frame (fwd=+X, right=+Y) prediction into a NED offset
    // using the current heading, then apply it to get a world-frame
    // predicted-crash location
    const float yaw_rad = AP::ahrs().get_yaw_rad();
    const float north = pred.mean_x_m * cosf(yaw_rad) - pred.mean_y_m * sinf(yaw_rad);
    const float east  = pred.mean_x_m * sinf(yaw_rad) + pred.mean_y_m * cosf(yaw_rad);
    Location crash_loc = loc;
    crash_loc.offset(north, east);

    GCS_SEND_TEXT(MAV_SEVERITY_WARNING, "DDF: predicted crash %.0fm away, p95 %.0fm",
                  (double)pred.mean_distance_m, (double)pred.p95_distance_m);

    Write_DDF(pred, _altitude_m, h_speed, v_speed, crash_loc, trigger_source);
}

void AP_DroneDropFinder::Write_DDF(const DroneDrop_Prediction &pred, float alt_m, float h_spd, float v_spd,
                                    const Location &crash_loc, uint8_t trigger_source) const
{
#if HAL_LOGGING_ENABLED
    const struct log_DDF pkt{
        LOG_PACKET_HEADER_INIT(LOG_DDF_MSG),
        time_us   : AP_HAL::micros64(),
        trigger   : trigger_source,
        alt       : alt_m,
        hspd      : h_spd,
        vspd      : v_spd,
        mean_x    : pred.mean_x_m,
        mean_y    : pred.mean_y_m,
        std_x     : pred.std_x_m,
        std_y     : pred.std_y_m,
        mean_dist : pred.mean_distance_m,
        p95_dist  : pred.p95_distance_m,
        lat       : crash_loc.lat,
        lng       : crash_loc.lng,
    };
    AP::logger().WriteBlock(&pkt, sizeof(pkt));
#endif
}

AP_DroneDropFinder *AP_DroneDropFinder::_singleton;

namespace AP {
AP_DroneDropFinder *dronedropfinder()
{
    return AP_DroneDropFinder::get_singleton();
}
}

#endif // AP_DRONEDROPFINDER_ENABLED
