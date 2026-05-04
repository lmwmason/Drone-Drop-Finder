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

namespace DroneCrashSimulator.Application.Tests.Trials;

public sealed class TrialRunnerTests
{
    private static readonly QuadcopterModel Drone = new();
    private static readonly TotalThrustLossMode FailureMode = new();
    private static readonly StandardEnvironment CalmEnvironment = new(
        new StandardAtmosphere(),
        new FlatTerrain(),
        UniformWindField.Calm,
        new DrydenTurbulenceModel());
    private static readonly TrialConfiguration DefaultConfiguration = new(
        FailurePosition: new Position(0.0, 0.0, 50.0),
        FailureVelocity: Velocity.Zero,
        TrialCount: 1);

    [Fact]
    public void RunTrial_ProducesNonNullResult()
    {
        var runner = new TrialRunner(TimestepConfiguration.Default);

        var result = runner.RunTrial(0, 42, Drone, CalmEnvironment, FailureMode, DefaultConfiguration);

        Assert.NotNull(result);
        Assert.NotNull(result.Trajectory);
        Assert.NotNull(result.CrashPoint);
    }

    [Fact]
    public void RunTrial_TrajectoryStartsAtFailureAltitude()
    {
        var runner = new TrialRunner(TimestepConfiguration.Default);

        var result = runner.RunTrial(0, 42, Drone, CalmEnvironment, FailureMode, DefaultConfiguration);

        var firstSample = result.Trajectory.First;
        Assert.Equal(50.0, firstSample.Position.ZMeters, precision: 1);
    }

    [Fact]
    public void RunTrial_CrashPointAtOrNearGroundLevel()
    {
        var runner = new TrialRunner(TimestepConfiguration.Default);

        var result = runner.RunTrial(0, 42, Drone, CalmEnvironment, FailureMode, DefaultConfiguration);

        var lastSample = result.Trajectory.Last;
        Assert.True(lastSample.Position.ZMeters <= 0.5,
            "Final position should be at or near ground");
    }

    [Fact]
    public void RunTrial_WithSameSeed_ProducesSameCrashPoint()
    {
        var runner = new TrialRunner(TimestepConfiguration.Default);

        var result1 = runner.RunTrial(0, 99, Drone, CalmEnvironment, FailureMode, DefaultConfiguration);
        var result2 = runner.RunTrial(0, 99, Drone, CalmEnvironment, FailureMode, DefaultConfiguration);

        Assert.Equal(result1.CrashPoint.XMeters, result2.CrashPoint.XMeters, precision: 3);
        Assert.Equal(result1.CrashPoint.YMeters, result2.CrashPoint.YMeters, precision: 3);
    }
}
