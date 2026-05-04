namespace DroneCrashSimulator.Domain.Trials;

public sealed record CrashPoint(
    double XMeters,
    double YMeters,
    double DistanceFromOriginMeters,
    double BearingDegrees);
