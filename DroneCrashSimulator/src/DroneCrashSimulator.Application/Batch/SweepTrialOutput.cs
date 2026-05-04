using DroneCrashSimulator.Domain.Trials;

namespace DroneCrashSimulator.Application.Batch;

public sealed record SweepTrialOutput(
    SweepCombination Combination,
    TrialResult Result,
    double WindSpeedMetersPerSecond,
    double WindBearingDegrees);
