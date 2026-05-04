using DroneCrashSimulator.Domain.Drones.Models;
using DroneCrashSimulator.Domain.Environment.Atmosphere;
using DroneCrashSimulator.Domain.Environment.Terrains;
using DroneCrashSimulator.Domain.Environment.Turbulence;
using DroneCrashSimulator.Domain.Environment.WindFields;
using DroneCrashSimulator.Domain.Environment;
using DroneCrashSimulator.Domain.Flight;
using DroneCrashSimulator.Domain.Forces.Contributors;
using DroneCrashSimulator.Domain.Forces;
using DroneCrashSimulator.Physics.Forces;
using DroneCrashSimulator.Physics.Simulation;

namespace DroneCrashSimulator.Physics.Tests.Simulation;

public sealed class PhysicsSimulationTests
{
    private static readonly QuadcopterModel Drone = new();
    private static readonly StandardEnvironment CalmEnvironment = new(
        new StandardAtmosphere(),
        new FlatTerrain(),
        UniformWindField.Calm,
        new DrydenTurbulenceModel());

    private static ForceApplicator BuildGravityOnlyApplicator() =>
        new ForceApplicator(
            new ForceAccumulator(new IForceContributor[] { new GravityContributor() }));

    [Fact]
    public void Advance_WithGravityOnly_DroneDescendsOverTime()
    {
        const double startAltitudeMeters = 50.0;
        var initialState = new FlightState(
            Position: new Position(0.0, 0.0, startAltitudeMeters),
            Velocity: Velocity.Zero,
            Attitude: Attitude.Identity,
            AngularVelocityRadiansPerSecond: Velocity.Zero,
            TimeSeconds: 0.0);

        using var simulation = PhysicsSimulation.Create(
            Drone, CalmEnvironment, initialState,
            BuildGravityOnlyApplicator(),
            TimestepConfiguration.Default);

        var step = simulation.Advance();

        Assert.True(step.Sample.Position.ZMeters < startAltitudeMeters,
            "Altitude should decrease under gravity");
    }

    [Fact]
    public void Advance_WithGravityOnly_EventuallyReachesTerrain()
    {
        const double startAltitudeMeters = 20.0;
        var initialState = new FlightState(
            Position: new Position(0.0, 0.0, startAltitudeMeters),
            Velocity: Velocity.Zero,
            Attitude: Attitude.Identity,
            AngularVelocityRadiansPerSecond: Velocity.Zero,
            TimeSeconds: 0.0);

        using var simulation = PhysicsSimulation.Create(
            Drone, CalmEnvironment, initialState,
            BuildGravityOnlyApplicator(),
            TimestepConfiguration.Default);

        var reachedGround = false;
        while (!simulation.HasExceededMaxDuration)
        {
            var step = simulation.Advance();
            if (!step.HasReachedTerrain) continue;
            reachedGround = true;
            break;
        }

        Assert.True(reachedGround, "Drone should reach the ground");
    }

    [Fact]
    public void Advance_WithNoForces_PositionRemainsConstant()
    {
        var initialState = new FlightState(
            Position: new Position(0.0, 0.0, 50.0),
            Velocity: Velocity.Zero,
            Attitude: Attitude.Identity,
            AngularVelocityRadiansPerSecond: Velocity.Zero,
            TimeSeconds: 0.0);

        var emptyApplicator = new ForceApplicator(
            new ForceAccumulator(Array.Empty<IForceContributor>()));

        using var simulation = PhysicsSimulation.Create(
            Drone, CalmEnvironment, initialState,
            emptyApplicator,
            TimestepConfiguration.Default);

        var step = simulation.Advance();

        Assert.Equal(50.0, step.Sample.Position.ZMeters, precision: 3);
    }
}
