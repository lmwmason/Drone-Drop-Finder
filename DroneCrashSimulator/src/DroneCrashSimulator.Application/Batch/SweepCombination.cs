namespace DroneCrashSimulator.Application.Batch;

public sealed record SweepCombination(
    int AltitudeIndex,
    int HorizontalSpeedIndex,
    int VerticalSpeedIndex,
    double AltitudeMeters,
    double HorizontalSpeedMetersPerSecond,
    double VerticalSpeedMetersPerSecond);
