using DroneCrashSimulator.Domain.Constants;
using DroneCrashSimulator.Domain.Drones.Models;
using DroneCrashSimulator.Domain.Environment.Atmosphere;
using DroneCrashSimulator.Domain.Environment.Terrains;
using DroneCrashSimulator.Domain.Environment.Turbulence;
using DroneCrashSimulator.Domain.Environment.WindFields;
using DroneCrashSimulator.Domain.Environment;
using DroneCrashSimulator.Domain.Flight;
using DroneCrashSimulator.Domain.Forces.Contributors;

namespace DroneCrashSimulator.Domain.Tests.Forces;

public sealed class GravityContributorTests
{
    private static readonly GravityContributor Contributor = new();
    private static readonly QuadcopterModel Drone = new();
    private static readonly StandardEnvironment Environment = new(
        new StandardAtmosphere(),
        new FlatTerrain(),
        UniformWindField.Calm,
        new DrydenTurbulenceModel());

    [Fact]
    public void ComputeForce_AtSeaLevel_ProducesDownwardForceEqualToMassTimesGravity()
    {
        var state = new FlightState(Position.Origin, Velocity.Zero, Attitude.Identity, Velocity.Zero, 0.0);

        var force = Contributor.ComputeForce(state, Drone, Environment);

        var expectedForceNewtons = Drone.MassKilograms * PhysicalConstants.StandardGravityMetersPerSecondSquared;
        Assert.Equal(0.0, force.XNewtons);
        Assert.Equal(0.0, force.YNewtons);
        Assert.Equal(-expectedForceNewtons, force.ZNewtons, precision: 6);
    }

    [Fact]
    public void ComputeForce_ProducesNoTorque()
    {
        var state = new FlightState(Position.Origin, Velocity.Zero, Attitude.Identity, Velocity.Zero, 0.0);

        var force = Contributor.ComputeForce(state, Drone, Environment);

        Assert.Equal(0.0, force.TorqueXNewtonMeters);
        Assert.Equal(0.0, force.TorqueYNewtonMeters);
        Assert.Equal(0.0, force.TorqueZNewtonMeters);
    }
}
