#pragma once

#include <AP_HAL/AP_HAL_Boards.h>
#include <AP_Filesystem/AP_Filesystem_config.h>

// The LUT is ~3000 combos * 36 bytes =~ 110KB of static BSS, plus the CSV
// parser, so this needs both flash/RAM budget and posix-style file reading
// (the CSV is loaded once at boot via AP_Filesystem's posix_compat shim).
#ifndef AP_DRONEDROPFINDER_ENABLED
#define AP_DRONEDROPFINDER_ENABLED (HAL_PROGRAM_SIZE_LIMIT_KB > 2048 && AP_FILESYSTEM_FILE_READING_ENABLED)
#endif

// Path to the sweep.csv produced offline by the Monte-Carlo crash simulator
// (see Drone-Drop-Finder/DroneCrashSimulator). Fixed at compile time since
// AP_Param has no string parameter type.
#ifndef AP_DRONEDROPFINDER_CSV_PATH
#if CONFIG_HAL_BOARD == HAL_BOARD_SITL || CONFIG_HAL_BOARD == HAL_BOARD_LINUX
#define AP_DRONEDROPFINDER_CSV_PATH "sweep.csv"
#else
#define AP_DRONEDROPFINDER_CSV_PATH "/APM/sweep.csv"
#endif
#endif

// LUT grid size limits, must be defined before drone_drop.h's own #ifndef
// guards take effect on first inclusion.
#ifndef DRONE_DROP_MAX_ALT_LEVELS
#define DRONE_DROP_MAX_ALT_LEVELS    20
#endif
#ifndef DRONE_DROP_MAX_HSPEED_LEVELS
#define DRONE_DROP_MAX_HSPEED_LEVELS 15
#endif
#ifndef DRONE_DROP_MAX_VSPEED_LEVELS
#define DRONE_DROP_MAX_VSPEED_LEVELS 10
#endif
