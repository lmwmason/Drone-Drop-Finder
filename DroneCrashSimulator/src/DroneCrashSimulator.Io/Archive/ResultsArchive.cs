using DroneCrashSimulator.Application.Aggregation;
using DroneCrashSimulator.Application.Batch;
using DroneCrashSimulator.Domain.Trials;
using DroneCrashSimulator.Io.Csv;
using DroneCrashSimulator.Io.Json;

namespace DroneCrashSimulator.Io.Archive;

public sealed class ResultsArchive
{
    private readonly SummaryJsonWriter _jsonWriter;
    private readonly StatisticsAggregator _aggregator;

    public ResultsArchive(SummaryJsonWriter jsonWriter, StatisticsAggregator aggregator)
    {
        _jsonWriter = jsonWriter;
        _aggregator = aggregator;
    }

    public void SaveScenarioResults(
        string archiveDirectory,
        Scenario scenario,
        IReadOnlyList<TrialResult> results,
        bool saveTrajectories)
    {
        var scenarioSlug = SanitizeForPath(scenario.Name);
        var scenarioDirectory = Path.Combine(archiveDirectory, scenarioSlug);
        Directory.CreateDirectory(scenarioDirectory);

        SaveCrashPoints(scenarioDirectory, results);
        SaveConditions(scenarioDirectory, scenario);
        SaveSummary(scenarioDirectory, results);

        if (saveTrajectories)
            SaveTrajectories(scenarioDirectory, results);
    }

    private static void SaveCrashPoints(string directory, IReadOnlyList<TrialResult> results)
    {
        using var writer = new CrashPointCsvWriter(Path.Combine(directory, "crash_points.csv"));
        foreach (var result in results)
            writer.WriteResult(result);
    }

    private static void SaveConditions(string directory, Scenario scenario)
    {
        using var writer = new ConditionCsvWriter(Path.Combine(directory, "conditions.csv"));
        writer.WriteScenario(scenario);
    }

    private void SaveSummary(string directory, IReadOnlyList<TrialResult> results)
    {
        var summary = _aggregator.ComputeSummary(results);
        var region = _aggregator.ComputeConfidenceRegion95(results);
        _jsonWriter.Write(Path.Combine(directory, "summary.json"), summary, region);
    }

    private static void SaveTrajectories(string directory, IReadOnlyList<TrialResult> results)
    {
        using var writer = new TrajectoryCsvWriter(Path.Combine(directory, "trajectories.csv"));
        foreach (var result in results)
            writer.WriteTrajectory(result.TrialIndex, result.Trajectory);
    }

    private static string SanitizeForPath(string name) =>
        string.Concat(name.Select(c => char.IsLetterOrDigit(c) ? c : '_'));
}
