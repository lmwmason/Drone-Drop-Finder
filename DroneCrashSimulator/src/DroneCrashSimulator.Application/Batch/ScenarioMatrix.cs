namespace DroneCrashSimulator.Application.Batch;

public sealed class ScenarioMatrix
{
    private readonly List<Scenario> _scenarios;

    public ScenarioMatrix(IEnumerable<Scenario> scenarios)
    {
        _scenarios = [.. scenarios];
    }

    public IReadOnlyList<Scenario> Scenarios => _scenarios;
}
