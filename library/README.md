# Drone Drop Prediction Library

An embedded C library that predicts the crash-point probability distribution of a
falling drone.  Given altitude, horizontal speed, and vertical speed from GPS,
it instantly returns the expected landing offset and safety exclusion radius.

---

## Scope — What This Library Does and Does NOT Do

| Responsibility | Owner |
|---|---|
| Detect that a crash / failure is occurring | **You** (flight controller, ESC feedback, watchdog, etc.) |
| Read GPS data at the moment of failure | **You** |
| Predict *where* the drone will land | **This library** |
| Broadcast / act on the prediction | **You** |

**This library has one job:** once you decide a failure has happened and you hand it
the GPS state at that moment, it returns a statistical prediction of the landing zone.
It does not monitor motors, parse GPS streams, or trigger any alerts on its own.

---

## 1. How It Works

```
Simulator (PC)                      Drone Board
┌──────────────────┐                ┌──────────────────────────────────┐
│ DroneCrashSim    │  sweep.csv     │  SD Card                         │
│  Grid Sweep:     │ ─────────────► │  └── sweep.csv                   │
│  alt × h_spd     │                │                                  │
│  × v_spd         │                │  MCU (STM32, etc.)               │
│  400 trials each │                │  ├── drone_drop_init(csv_path)   │
└──────────────────┘                │  │     (once, at boot)           │
                                    │  │                               │
                                    │  ├── [YOUR CODE detects failure] │
                                    │  │                               │
                                    │  └── drone_drop_predict(         │
                                    │         alt, h_spd, v_spd, &out) │
                                    └──────────────────────────────────┘
```

---

## 2. Required Hardware

| Component | Recommended | Purpose |
|---|---|---|
| MCU | STM32F405 / STM32H743 | Main processor |
| GPS module | u-blox NEO-M9N / SAM-M10Q | Altitude + 3-D velocity |
| Micro SD socket | Standard SPI type | Store the CSV file |
| 3.3 V LDO | AMS1117-3.3 | Power for GPS and SD |
| Capacitors | 100 nF × 4, 10 µF × 2 | Bypass / power stabilisation |

> The library only needs GPS data.  An IMU is **not required** by this library.  
> u-blox NEO series provides 3-D velocity directly via the `UBX-NAV-VELNED` message
> (vN, vE, vD), which is all you need.

---

## 3. Circuit

### Block Diagram

```
                    3.3 V
                      │
            ┌─────────┼──────────┐
            │         │          │
          [MCU]   [GPS Module]  [SD Socket]
            │         │          │
            │  UART   │   SPI    │
            │ ───────►│ ─────────►│
            │  TX→RX  │  MOSI    │
            │  RX←TX  │  MISO    │
            │         │  SCK     │
            │         │  CS      │
            │                    │
           GND ────────────────GND
```

### Pin Connections (STM32F405)

#### GPS — UART

| GPS pin | MCU pin | Notes |
|---|---|---|
| TX | PA3 (USART2_RX) | GPS → MCU |
| RX | PA2 (USART2_TX) | MCU → GPS (config only) |
| VCC | 3.3 V | |
| GND | GND | |

#### Micro SD — SPI

| SD pin | MCU pin | Notes |
|---|---|---|
| MOSI | PA7 (SPI1_MOSI) | |
| MISO | PA6 (SPI1_MISO) | |
| SCK | PA5 (SPI1_SCK) | |
| CS | PA4 (GPIO_OUT) | Chip select |
| VCC | 3.3 V | |
| GND | GND | |

> Add 10 kΩ pull-up resistors on the SD data lines for reliable card detection.

### Power Supply

```
Drone 5 V rail
      │
  [AMS1117-3.3]──┬── 10 µF (electrolytic) ── GND
                 ├── 100 nF (ceramic)      ── GND
                 │
               3.3 V ──► MCU VDD
                      ──► GPS VCC
                      ──► SD VCC
```

---

## 4. Firmware Integration

### 4-1. Add Files

Drop two files into your project — no other dependencies required.

```
your_project/
├── Core/
│   ├── drone_drop.h   ← add this
│   └── drone_drop.c   ← add this
└── ...
```

Link with the math library (`-lm`).  The library is C99 compatible and uses
**no dynamic memory allocation**.

### 4-2. FatFS (SD Card)

Enable the FatFS middleware in STM32CubeMX and select the SPI-linked driver.

```c
#include "fatfs.h"
#include "drone_drop.h"
```

### 4-3. Initialisation (call once at boot)

```c
FATFS fs;

/* Mount SD card */
if (f_mount(&fs, "", 1) != FR_OK) {
    /* handle error — SD not present or unformatted */
}

/* Load LUT from CSV — blocks until the file is fully parsed */
DroneDrop_Status st = drone_drop_init("sweep.csv");
if (st != DRONE_DROP_OK) {
    /* handle error — see error code table */
}
/* From this point, drone_drop_predict() is available */
```

### 4-4. Predicting the Landing Zone

**You are responsible for detecting the failure.**  
Call `drone_drop_predict` at the moment you determine a failure has occurred,
using the GPS values read at that instant.

```c
/* ------------------------------------------------------------------ *
 *  Step 1 — YOUR CODE detects a failure                               *
 *  (e.g. ESC reports motor loss, watchdog fires, flight-controller    *
 *   sets a failsafe flag ...)                                         *
 * ------------------------------------------------------------------ */
if (failure_detected()) {

    /* Step 2 — read GPS state at this exact moment */
    float altitude_m  = gps_get_altitude_m();    /* AGL altitude      */
    float vel_n       = gps_get_velocity_n();    /* northward   m/s   */
    float vel_e       = gps_get_velocity_e();    /* eastward    m/s   */
    float vel_d       = gps_get_velocity_d();    /* downward    m/s   */

    float h_speed_mps = sqrtf(vel_n * vel_n + vel_e * vel_e);
    float v_speed_mps = -vel_d;     /* library convention: positive = climbing */
    float heading_rad = atan2f(vel_e, vel_n);

    /* Step 3 — predict landing zone */
    DroneDrop_Prediction pred;
    DroneDrop_Status st = drone_drop_predict(altitude_m,
                                              h_speed_mps,
                                              v_speed_mps,
                                              &pred);
    if (st == DRONE_DROP_OK) {

        /* pred offsets are in the drone's body frame (+X = forward) */
        /* Rotate to geographic (North/East) frame using heading      */
        float crash_north_m = pred.mean_x_m * cosf(heading_rad)
                            - pred.mean_y_m * sinf(heading_rad);
        float crash_east_m  = pred.mean_x_m * sinf(heading_rad)
                            + pred.mean_y_m * cosf(heading_rad);

        float safety_radius_m = pred.p95_distance_m;

        /* Step 4 — YOUR CODE acts on the prediction                  *
         * e.g. broadcast via UART/CAN, trigger alarm, geofence alert */
    }
}
```

---

## 5. Output Fields

```
DroneDrop_Prediction
├── mean_x_m          Mean landing offset — drone forward direction (m)
├── mean_y_m          Mean landing offset — drone right direction   (m)
├── std_x_m           Standard deviation — forward                  (m)
├── std_y_m           Standard deviation — lateral                  (m)
├── mean_distance_m   Mean distance from failure point              (m)
└── p95_distance_m    95th-percentile safety exclusion radius       (m)
```

All offsets are in the **drone body frame** at the moment of failure:

```
        [Failure point]
               │
   ← lat ──── ┼ ──── lat →
               │
         mean_x_m (forward)
               ▼
        [Predicted centroid]
      ┌────────────────────┐
      │  95 % of landings  │  ← p95_distance_m radius
      │  fall inside here  │
      └────────────────────┘
```

Rotate the `(mean_x_m, mean_y_m)` vector by the drone's heading to get the offset
in North / East coordinates, then convert to GPS lat/lon if needed.

---

## 6. Memory Footprint

| Region | Size | Notes |
|---|---|---|
| `s_lut[3000]` | ~105 KB | LUT entries (9 floats each) |
| `s_idx[20×15×10]` | ~12 KB | 3-D index map (int) |
| **Total (BSS)** | **~117 KB** | Static, no heap |

If your MCU has less RAM, reduce the limits before including the header:

```c
/* Add these defines BEFORE #include "drone_drop.h" */
#define DRONE_DROP_MAX_ALT_LEVELS    10
#define DRONE_DROP_MAX_HSPEED_LEVELS  8
#define DRONE_DROP_MAX_VSPEED_LEVELS  5
```

---

## 7. Generating the CSV

1. Run `DroneCrashSimulator` on a PC.
2. Enter the drone specs (mass, drag coefficient, reference area).
3. Set altitude / horizontal-speed / vertical-speed sweep ranges.
4. Click **Run Sweep** → a `sweep_YYYYMMDD_HHMMSS.csv` file is created.
5. Rename it to `sweep.csv` and copy it to the **root directory of the SD card**.

The simulator performs a full grid sweep of all
`(altitude × h_speed × v_speed)` combinations, running 400 Monte Carlo trials
per combination with randomised wind speed, wind direction, and turbulence.

---

## 8. Error Codes

| Code | Value | Meaning |
|---|---|---|
| `DRONE_DROP_OK` | 0 | Success |
| `DRONE_DROP_ERR_FILE` | -1 | Cannot open CSV (check SD mount) |
| `DRONE_DROP_ERR_FORMAT` | -2 | CSV header or parse error |
| `DRONE_DROP_ERR_OVERFLOW` | -3 | Grid exceeds compile-time limits — reduce sweep ranges or increase macros |
| `DRONE_DROP_ERR_NO_DATA` | -4 | CSV contains no valid data rows |
| `DRONE_DROP_ERR_NOT_INIT` | -5 | `drone_drop_predict` called before `drone_drop_init` |
