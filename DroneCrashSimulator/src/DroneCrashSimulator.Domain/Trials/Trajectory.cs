namespace DroneCrashSimulator.Domain.Trials;

public sealed class Trajectory
{
    private readonly List<TrajectorySample> _samples;

    public Trajectory(IEnumerable<TrajectorySample> samples)
    {
        _samples = [.. samples];
    }

    public IReadOnlyList<TrajectorySample> Samples => _samples;

    public TrajectorySample First => _samples[0];
    public TrajectorySample Last => _samples[^1];
}
