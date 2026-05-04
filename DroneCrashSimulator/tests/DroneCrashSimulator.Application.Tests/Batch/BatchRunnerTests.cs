using DroneCrashSimulator.Application.Aggregation;
using DroneCrashSimulator.Application.Batch;
using DroneCrashSimulator.Application.Building;
using DroneCrashSimulator.Application.Trials;
using DroneCrashSimulator.Domain.Drones.Models;
using DroneCrashSimulator.Domain.Environment.Atmosphere;
using DroneCrashSimulator.Domain.Environment.Terrains;
using DroneCrashSimulator.Domain.Environment.Turbulence;
using DroneCrashSimulator.Domain.Environment.WindFields;
using DroneCrashSimulator.Domain.Environment;
using DroneCrashSimulator.Domain.Failure.Modes;
using DroneCrashSimulator.Domain.Flight;
using DroneCrashSimulator.Physics.Simulation;

namespace DroneCrashSimulator.Application.Tests.Batch;

public sealed class BatchRunnerTests
{
    private static readonly QuadcopterModel Drone = new();
    private static readonly StandardEnvironment CalmEnvironment = new(
        new StandardAtmosphere(),
        new FlatTerrain(),
        UniformWindField.Calm,
        new DrydenTurbulenceModel());
    private static readonly TotalThrustLossMode FailureMode = new();

    private static Scenario BuildScenario(int trialCount) =>
        new ScenarioBuilder()
            .WithName("Test Scenario")
            .WithDrone(Drone)
            .WithEnvironment(CalmEnvironment)
            .WithFailureMode(FailureMode)
            .WithFailurePosition(new Position(0.0, 0.0, 30.0))
            .WithTrialCount(trialCount)
            .Build();

    [Fact]
    public void RunScenario_ProducesCorrectNumberOfResults()
    {
        const int trialCount = 3;
        var runner = new BatchRunner(
            new TrialRunner(TimestepConfiguration.Default),
            new ResultsCollector());

        var results = runner.RunScenario(BuildScenario(trialCount));

        Assert.Equal(trialCount, results.Count);
    }

    [Fact]
    public void RunScenario_AllResultsHaveValidCrashPoints()
    {
        var runner = new BatchRunner(
            new TrialRunner(TimestepConfiguration.Default),
            new ResultsCollector());

        var results = runner.RunScenario(BuildScenario(5));

        Assert.All(results, result =>
        {
            Assert.True(double.IsFinite(result.CrashPoint.XMeters));
            Assert.True(double.IsFinite(result.CrashPoint.YMeters));
            Assert.True(result.CrashPoint.DistanceFromOriginMeters >= 0.0);
        });
    }
}
