using DroneCrashSimulator.Application.Aggregation;
using DroneCrashSimulator.Application.Batch;
using DroneCrashSimulator.Application.Building;
using DroneCrashSimulator.Application.Trials;
using DroneCrashSimulator.Domain.Environment.Atmosphere;
using DroneCrashSimulator.Domain.Environment.Terrains;
using DroneCrashSimulator.Domain.Environment.Turbulence;
using DroneCrashSimulator.Domain.Environment.WindFields;
using DroneCrashSimulator.Domain.Environment;
using DroneCrashSimulator.Domain.Flight;
using DroneCrashSimulator.Io.Archive;
using DroneCrashSimulator.Io.CommandLine;
using DroneCrashSimulator.Io.Json;
using DroneCrashSimulator.Physics.Simulation;

namespace DroneCrashSimulator.App.Modes;

public sealed class HeadlessBatchMode
{
    private readonly DroneCatalog _drones;
    private readonly FailureModeCatalog _failureModes;

    public HeadlessBatchMode(DroneCatalog drones, FailureModeCatalog failureModes)
    {
        _drones = drones;
        _failureModes = failureModes;
    }

    public void Run(CommandLineOptions options)
    {
        Console.WriteLine($"Starting batch mode: {options.TrialCount} trials per scenario");

        var archiveDirectory = ArchiveDirectoryFactory.CreateTimestampedDirectory(
            options.OutputDirectory);
        Console.WriteLine($"Output: {archiveDirectory}");

        var matrix = BuildScenarioMatrix(options);
        var collector = new ResultsCollector();
        var trialRunner = new TrialRunner(TimestepConfiguration.Default);
        var batchRunner = new BatchRunner(trialRunner, collector);
        var aggregator = new StatisticsAggregator();
        var archive = new ResultsArchive(new SummaryJsonWriter(), aggregator);

        var allResults = batchRunner.RunMatrix(matrix);
        foreach (var (scenarioName, results) in allResults)
        {
            var scenario = matrix.Scenarios.First(s => s.Name == scenarioName);
            archive.SaveScenarioResults(archiveDirectory, scenario, results, saveTrajectories: false);
            Console.WriteLine($"  {scenarioName}: {results.Count} trials done");
        }

        Console.WriteLine("Batch complete.");
    }

    private ScenarioMatrix BuildScenarioMatrix(CommandLineOptions options)
    {
        var drone = _drones.GetByName(_drones.RegisteredNames[0]);
        var failureMode = _failureModes.GetByName(_failureModes.RegisteredNames[0]);

        var windSpeeds = new[] { 0.0, options.WindSpeedMetersPerSecond };
        var scenarios = windSpeeds.Select(wind =>
            new ScenarioBuilder()
                .WithName($"Wind_{wind:F0}mps_Alt_{options.AltitudeMeters:F0}m")
                .WithDrone(drone)
                .WithEnvironment(new StandardEnvironment(
                    new StandardAtmosphere(),
                    new FlatTerrain(),
                    new UniformWindField(new Velocity(wind, 0.0, 0.0)),
                    new DrydenTurbulenceModel()))
                .WithFailureMode(failureMode)
                .WithFailurePosition(new Position(0.0, 0.0, options.AltitudeMeters))
                .WithTrialCount(options.TrialCount)
                .Build())
            .ToList();

        return new ScenarioMatrix(scenarios);
    }
}
