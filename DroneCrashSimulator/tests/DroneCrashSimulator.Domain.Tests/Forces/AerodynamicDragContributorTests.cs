using DroneCrashSimulator.Domain.Drones.Models;
using DroneCrashSimulator.Domain.Environment.Atmosphere;
using DroneCrashSimulator.Domain.Environment.Terrains;
using DroneCrashSimulator.Domain.Environment.Turbulence;
using DroneCrashSimulator.Domain.Environment.WindFields;
using DroneCrashSimulator.Domain.Environment;
using DroneCrashSimulator.Domain.Flight;
using DroneCrashSimulator.Domain.Forces;
using DroneCrashSimulator.Domain.Forces.Contributors;

namespace DroneCrashSimulator.Domain.Tests.Forces;

public sealed class AerodynamicDragContributorTests
{
    private static readonly AerodynamicDragContributor Contributor = new();
    private static readonly QuadcopterModel Drone = new();
    private static readonly StandardEnvironment CalmEnvironment = new(
        new StandardAtmosphere(),
        new FlatTerrain(),
        UniformWindField.Calm,
        new DrydenTurbulenceModel());

    [Fact]
    public void ComputeForce_WithZeroVelocity_ProducesZeroForce()
    {
        var state = new FlightState(Position.Origin, Velocity.Zero, Attitude.Identity, Velocity.Zero, 0.0);

        var force = Contributor.ComputeForce(state, Drone, CalmEnvironment);

        Assert.Equal(AppliedForce.Zero, force);
    }

    [Fact]
    public void ComputeForce_WithDownwardVelocity_ProducesUpwardDrag()
    {
        var downwardVelocity = new Velocity(0.0, 0.0, -20.0);
        var state = new FlightState(Position.Origin, downwardVelocity, Attitude.Identity, Velocity.Zero, 0.0);

        var force = Contributor.ComputeForce(state, Drone, CalmEnvironment);

        Assert.True(force.ZNewtons > 0.0, "Drag should oppose downward motion (be positive Z)");
        Assert.Equal(0.0, force.XNewtons, precision: 6);
        Assert.Equal(0.0, force.YNewtons, precision: 6);
    }

    [Fact]
    public void ComputeForce_DragIncreasesWithSpeed()
    {
        var slowVelocity = new Velocity(0.0, 0.0, -10.0);
        var fastVelocity = new Velocity(0.0, 0.0, -20.0);
        var slowState = new FlightState(Position.Origin, slowVelocity, Attitude.Identity, Velocity.Zero, 0.0);
        var fastState = new FlightState(Position.Origin, fastVelocity, Attitude.Identity, Velocity.Zero, 0.0);

        var slowDrag = Math.Abs(Contributor.ComputeForce(slowState, Drone, CalmEnvironment).ZNewtons);
        var fastDrag = Math.Abs(Contributor.ComputeForce(fastState, Drone, CalmEnvironment).ZNewtons);

        Assert.True(fastDrag > slowDrag, "Faster speed should produce more drag");
    }
}
