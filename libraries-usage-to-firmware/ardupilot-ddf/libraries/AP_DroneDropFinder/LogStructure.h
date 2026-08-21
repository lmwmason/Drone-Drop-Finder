#pragma once

#include <AP_Logger/LogStructure.h>
#include "AP_DroneDropFinder_config.h"

#define LOG_IDS_FROM_DRONEDROPFINDER \
    LOG_DDF_MSG

// @LoggerMessage: DDF
// @Description: Drone Drop Finder crash-location prediction
// @Field: TimeUS: Time since system startup
// @Field: Trig: Bitmask of trigger source(s) that caused this prediction (1:RadioFS,2:GCSFS,4:BattFS,8:Crash)
// @Field: Alt: AGL altitude used for the LUT lookup
// @Field: HSpd: Horizontal ground speed used for the LUT lookup
// @Field: VSpd: Vertical speed used for the LUT lookup, positive is climbing
// @Field: MeanX: Predicted mean crash offset, body-frame forward
// @Field: MeanY: Predicted mean crash offset, body-frame right
// @Field: StdX: Predicted crash offset standard deviation, body-frame forward
// @Field: StdY: Predicted crash offset standard deviation, body-frame right
// @Field: MeanD: Predicted mean crash distance from the failure point
// @Field: P95D: Predicted 95th-percentile crash distance, safety radius
// @Field: Lat: Predicted crash point latitude
// @Field: Lng: Predicted crash point longitude

struct PACKED log_DDF {
    LOG_PACKET_HEADER;
    uint64_t time_us;
    uint8_t  trigger;
    float    alt;
    float    hspd;
    float    vspd;
    float    mean_x;
    float    mean_y;
    float    std_x;
    float    std_y;
    float    mean_dist;
    float    p95_dist;
    int32_t  lat;
    int32_t  lng;
};

#if AP_DRONEDROPFINDER_ENABLED
// streaming=false: this is a rare, safety-critical event (a predicted
// crash location), not a periodic sensor stream, so it must never be
// dropped by AP_Logger's rate limiter.
#define LOG_STRUCTURE_FROM_DRONEDROPFINDER \
    { LOG_DDF_MSG, sizeof(log_DDF), \
        "DDF", "QBfffffffffLL", "TimeUS,Trig,Alt,HSpd,VSpd,MeanX,MeanY,StdX,StdY,MeanD,P95D,Lat,Lng", "s-nnnmmmmmmDU", "F-000000000GG", false },
#else
#define LOG_STRUCTURE_FROM_DRONEDROPFINDER
#endif
