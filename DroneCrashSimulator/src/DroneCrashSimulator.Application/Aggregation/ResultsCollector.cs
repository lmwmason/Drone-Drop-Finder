using DroneCrashSimulator.Domain.Trials;

namespace DroneCrashSimulator.Application.Aggregation;

public sealed class ResultsCollector
{
    private readonly List<TrialResult> _results = new();

    public void Collect(TrialResult result)
    {
        _results.Add(result);
    }

    public void Reset()
    {
        _results.Clear();
    }

    public IReadOnlyList<TrialResult> CollectedResults => _results;
}
