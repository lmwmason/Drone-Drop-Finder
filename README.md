# Drone Drop Finder

> **Rigid-Body Simulation-Based Probabilistic Analysis of Drone Crash Impact Points and Its Embedded System Application**

A physics-based simulator that generates crash impact point probability distributions for drones, paired with a lightweight C library that runs the predictions on embedded flight controllers.

[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![C](https://img.shields.io/badge/C-C99-A8B9CC)](https://en.wikipedia.org/wiki/C99)

---

## Overview

When a drone crashes, most flight controllers report only the last GPS position before signal loss — assuming the drone falls straight down. In reality, wind, residual velocity, and attitude dynamics cause the drone to land far from the last known position.

**Drone Drop Finder** addresses this by:

1. Running Monte Carlo rigid-body simulations across a grid of flight states
2. Building a 3D lookup table (LUT) from the resulting crash point distributions
3. Packaging the LUT into a C library that runs on STM32 with no heap allocation

---

## Reports

The full technical reports are available in both languages:

- [**English Report**](Rigid-Body%20Simulation-Based_Probabilistic_Analysis_of_Drone_Crash_Impact_Points_and_Its_Embedded_System_Application/Report_EN.pdf)
- [**한국어 보고서**](Rigid-Body%20Simulation-Based_Probabilistic_Analysis_of_Drone_Crash_Impact_Points_and_Its_Embedded_System_Application/Report_KR.pdf)

---

## Repository Structure

```
Drone-Drop-Finder/
├── DroneCrashSimulator/        # PC simulator (C# / .NET 8)
│   ├── src/                    # 6 projects (Domain, Physics, Application, Io, Visualization, App)
│   ├── tests/                  # 3 test projects, 22 tests
│   ├── results/                # Example sweep CSV outputs
│   ├── build-all.ps1           # Windows build script
│   └── build-all.sh            # macOS / Linux build script
└── library/                    # Embedded C library
    ├── drone_drop.h
    ├── drone_drop.c
    ├── example.c
    ├── CMakeLists.txt
    └── README.md
```

> Pre-built simulator binaries are distributed via [GitHub Releases](../../releases), not committed to the repository.

---

## How It Works

```
① Set drone specs (mass, Cd, reference area)
② Set sweep ranges (altitude, horizontal speed, vertical speed)
③ Run Grid Sweep → generates sweep.csv
④ Copy sweep.csv to SD card
⑤ Add drone_drop.h / drone_drop.c to STM32 firmware
⑥ Call drone_drop_init("sweep.csv") once at boot
⑦ On crash detection, read GPS velocity → call drone_drop_predict()
⑧ Use p95_distance_m as the safe search radius
```

---

## PC Simulator

### Download

Pre-built binaries are available on the [Releases](../../releases) page. No .NET installation required — the runtime is bundled.

| Platform | Binary |
|----------|--------|
| Windows x64 | `DroneCrashSimulator-win-x64.zip` |
| Windows ARM64 | `DroneCrashSimulator-win-arm64.zip` |
| macOS Intel | `DroneCrashSimulator-osx-x64.zip` |
| macOS Apple Silicon | `DroneCrashSimulator-osx-arm64.zip` |
| Linux x64 | `DroneCrashSimulator-linux-x64.zip` |
| Linux ARM64 | `DroneCrashSimulator-linux-arm64.zip` |

> **macOS**: Before first run, remove the quarantine flag:
> ```bash
> xattr -d com.apple.quarantine ./DroneCrashSimulator.App
> ```

### Build from Source

Requirements: [.NET 8 SDK](https://dotnet.microsoft.com/download)

**Windows**
```powershell
cd DroneCrashSimulator
.\build-all.ps1
```

**macOS / Linux**
```bash
cd DroneCrashSimulator
chmod +x build-all.sh
./build-all.sh
```

### Example Results

Example sweep CSV outputs are available in [`DroneCrashSimulator/results/`](DroneCrashSimulator/results/) for reference. These can be loaded directly by the C library to test integration without running a fresh sweep.

### Simulation Parameters

**Fixed variables** (grid axes)

| Parameter | Description |
|-----------|-------------|
| `altitude_m` | Altitude at crash moment (m) |
| `cruise_speed_mps` | Cruise speed (m/s) |
| `init_speed_mps` | Initial drop speed (m/s) |

**Random variables** (Monte Carlo per grid cell)

| Parameter | Distribution |
|-----------|-------------|
| `wind_speed` | Uniform [0, max] |
| `wind_bearing` | Uniform [0°, 360°] |
| `turbulence` | Dryden model |

### Output CSV Format

```
altitude_m, cruise_speed_mps, trial_index,
init_speed_mps, init_bearing_deg,
wind_speed_mps, wind_bearing_deg,
crash_x_m, crash_y_m, crash_distance_m, crash_bearing_deg
```

---

## Embedded C Library

### API

```c
// Load LUT from CSV at boot (call once)
DroneDrop_Status drone_drop_init(const char *csv_path);

// Predict crash distribution from current GPS state
DroneDrop_Status drone_drop_predict(float altitude_m,
                                    float h_speed_mps,
                                    float v_speed_mps,
                                    DroneDrop_Prediction *out);

// Free resources
void drone_drop_free(void);

// Get number of LUT entries
int drone_drop_lut_size(void);
```

### Prediction Output

```c
typedef struct {
    float mean_x_m;         // Mean offset — forward direction (m)
    float mean_y_m;         // Mean offset — lateral direction (m)
    float std_x_m;          // Standard deviation — forward (m)
    float std_y_m;          // Standard deviation — lateral (m)
    float mean_distance_m;  // Mean crash distance (m)
    float p95_distance_m;   // 95% confidence radius (m)
} DroneDrop_Prediction;
```

`p95_distance_m` is computed as `mean_distance_m + 1.6449 × std`, giving the radius within which the drone lands with 95% probability.

### Integration

```c
#include "drone_drop.h"

// At boot
drone_drop_init("sweep.csv");

// On crash detection
DroneDrop_Prediction pred;
drone_drop_predict(altitude_m, h_speed_mps, v_speed_mps, &pred);

// pred.p95_distance_m → safe search radius
// pred.mean_x_m, pred.mean_y_m → most likely landing offset
```

### Specifications

| Property | Value |
|----------|-------|
| Standard | C99 |
| Memory | Static allocation only (~117 KB BSS) |
| Heap | None |
| Dependencies | `-lm` only |
| Interpolation | Trilinear |
| Target MCU | STM32F405 (works on any C99-compliant MCU) |
| GPS | u-blox NEO-M9N (UBX-NAV-VELNED) |
| Storage | SPI Micro SD + FatFS |

---

## Physics Model

| Component | Detail |
|-----------|--------|
| Engine | BepuPhysics2 v2.4.0 |
| Drag | $F_d = -\frac{1}{2} \rho C_d A \|\mathbf{v}_{rel}\| \mathbf{v}_{rel}$ (isotropic) |
| Turbulence | Dryden model (MIL-HDBK-1797) |
| Coordinate | Domain: (X=East, Y=North, Z=Up) |
| Failure mode | Total thrust loss |

---

## License

Licensed under the [Apache License 2.0](LICENSE).
