namespace DroneCrashSimulator.Domain.Statistics;

public sealed record ConfidenceRegion(
    double CenterXMeters,
    double CenterYMeters,
    double RadiusMeters,
    double ConfidenceLevel);
