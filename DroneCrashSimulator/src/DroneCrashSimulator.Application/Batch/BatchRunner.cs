using DroneCrashSimulator.Application.Aggregation;
using DroneCrashSimulator.Application.Trials;
using DroneCrashSimulator.Domain.Trials;
using DroneCrashSimulator.Physics.Simulation;

namespace DroneCrashSimulator.Application.Batch;

public sealed class BatchRunner
{
    private readonly TrialRunner _trialRunner;
    private readonly ResultsCollector _resultsCollector;

    public BatchRunner(TrialRunner trialRunner, ResultsCollector resultsCollector)
    {
        _trialRunner = trialRunner;
        _resultsCollector = resultsCollector;
    }

    public IReadOnlyList<TrialResult> RunScenario(Scenario scenario)
    {
        var seedGenerator = TrialSeedGenerator.FromSystemTime();
        var results = new List<TrialResult>(scenario.TrialCount);

        for (var trialIndex = 0; trialIndex < scenario.TrialCount; trialIndex++)
        {
            var seed = seedGenerator.NextSeed();
            var result = _trialRunner.RunTrial(
                trialIndex,
                seed,
                scenario.Drone,
                scenario.Environment,
                scenario.FailureMode,
                scenario.TrialConfiguration);
            results.Add(result);
            _resultsCollector.Collect(result);
        }

        return results;
    }

    public IReadOnlyDictionary<string, IReadOnlyList<TrialResult>> RunMatrix(
        ScenarioMatrix matrix)
    {
        var allResults = new Dictionary<string, IReadOnlyList<TrialResult>>();
        foreach (var scenario in matrix.Scenarios)
            allResults[scenario.Name] = RunScenario(scenario);
        return allResults;
    }
}
