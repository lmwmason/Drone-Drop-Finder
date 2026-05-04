namespace DroneCrashSimulator.Domain.Trials;

public sealed class TrialResult
{
    public required Trajectory Trajectory { get; init; }
    public required CrashPoint CrashPoint { get; init; }
    public required int TrialIndex { get; init; }
    public required int Seed { get; init; }
}
