#include "Copter.h"

#if AP_DRONEDROPFINDER_ENABLED

// ddf_update - drives AP_DroneDropFinder; called at 10Hz from the scheduler
void Copter::ddf_update()
{
    uint8_t trigger_source = 0;
    if (failsafe.radio) {
        trigger_source |= uint8_t(AP_DroneDropFinder::TriggerSource::RADIO_FAILSAFE);
    }
    if (failsafe.gcs) {
        trigger_source |= uint8_t(AP_DroneDropFinder::TriggerSource::GCS_FAILSAFE);
    }
    if (battery.has_failsafed()) {
        trigger_source |= uint8_t(AP_DroneDropFinder::TriggerSource::BATTERY_FAILSAFE);
    }
    const bool trigger_active = (trigger_source != 0) && motors->armed() && !ap.land_complete;

    g2.dronedropfinder.set_altitude_m(flightmode->get_alt_above_ground_m());
    g2.dronedropfinder.update(trigger_active, trigger_source);
}

#endif // AP_DRONEDROPFINDER_ENABLED
