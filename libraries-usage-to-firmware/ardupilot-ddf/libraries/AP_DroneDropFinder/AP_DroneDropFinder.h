/// @file   AP_DroneDropFinder.h
/// @brief  Post-failure crash-location prediction using a pre-loaded
///         Monte-Carlo sweep lookup table (see drone_drop.h).
#pragma once

#include "AP_DroneDropFinder_config.h"

#if AP_DRONEDROPFINDER_ENABLED

#include <AP_Param/AP_Param.h>
#include <AP_Common/AP_Common.h>
#include <AP_Common/Location.h>

#include "drone_drop.h"

/// @class  AP_DroneDropFinder
/// @brief  Predicts and reports where the vehicle is likely to land after a
///         failure (radio/GCS/battery failsafe), using a lookup table built
///         offline from Monte-Carlo rigid-body crash simulations.
class AP_DroneDropFinder {
public:
    AP_DroneDropFinder();

    CLASS_NO_COPY(AP_DroneDropFinder);

    static const struct AP_Param::GroupInfo var_info[];

    // get singleton instance
    static AP_DroneDropFinder *get_singleton() { return _singleton; }

    // bits for the TRIGGER parameter and the trigger_source argument to update()
    enum class TriggerSource : uint8_t {
        RADIO_FAILSAFE   = 1U << 0,
        GCS_FAILSAFE     = 1U << 1,
        BATTERY_FAILSAFE = 1U << 2,
        CRASH_DETECTED   = 1U << 3,
    };

    /// init - load the sweep CSV. Call once from vehicle init. Safe to call
    /// even if the filesystem isn't mounted yet: update() will retry.
    void init();

    /// set_altitude_m - vehicle pushes in AGL altitude ahead of update(),
    /// since the right altitude source (rangefinder/terrain/home-relative)
    /// is vehicle-specific and not reachable from this library.
    void set_altitude_m(float alt_m) { _altitude_m = alt_m; }

    /// update - drive the trigger/throttle/predict state machine. Call at a
    /// steady low rate (e.g. 10Hz) from the vehicle's scheduler.
    ///   trigger_active - true if any of the vehicle's failure conditions are
    ///                     currently active (and the vehicle is armed/flying)
    ///   trigger_source  - bitmask of TriggerSource describing why
    void update(bool trigger_active, uint8_t trigger_source);

    /// healthy - true once the LUT has been loaded successfully
    bool healthy() const { return _lut_ready; }

private:
    static AP_DroneDropFinder *_singleton;

    // Parameters
    AP_Int8  _enabled;      // DDF_ENABLE
    AP_Int8  _trigger_mask; // DDF_TRIGGER
    AP_Int16 _interval_ms;  // DDF_INTERVAL_MS

    // State
    bool     _lut_ready;
    bool     _init_failed_permanently;
    uint32_t _first_init_attempt_ms;
    uint32_t _last_predict_ms;
    bool     _prev_trigger_active;
    float    _altitude_m;

    bool try_init();
    void predict_and_report(uint8_t trigger_source);
    void Write_DDF(const DroneDrop_Prediction &pred, float alt_m, float h_spd, float v_spd,
                    const Location &crash_loc, uint8_t trigger_source) const;
};

namespace AP {
    AP_DroneDropFinder *dronedropfinder();
};

#endif // AP_DRONEDROPFINDER_ENABLED
