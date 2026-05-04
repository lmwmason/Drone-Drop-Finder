namespace DroneCrashSimulator.Physics.Simulation;

public sealed record TimestepConfiguration(
    double TimestepSeconds,
    double MaxSimulationDurationSeconds)
{
    public static TimestepConfiguration Default => new(
        TimestepSeconds: 0.01,
        MaxSimulationDurationSeconds: 120.0);

    public static TimestepConfiguration FastBatch => new(
        TimestepSeconds: 0.04,
        MaxSimulationDurationSeconds: 120.0);
}
