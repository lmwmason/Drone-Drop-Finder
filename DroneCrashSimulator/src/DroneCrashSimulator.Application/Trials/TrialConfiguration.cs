using DroneCrashSimulator.Domain.Flight;

namespace DroneCrashSimulator.Application.Trials;

public sealed record TrialConfiguration(
    Position FailurePosition,
    Velocity FailureVelocity,
    double TrialCount,
    double CruiseSpeedMetersPerSecond = 8.0);
