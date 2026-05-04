using DroneCrashSimulator.Domain.Drones;
using DroneCrashSimulator.Domain.Environment;
using DroneCrashSimulator.Domain.Flight;

namespace DroneCrashSimulator.Domain.Forces.Contributors;

public sealed class GravityContributor : IForceContributor
{
    public AppliedForce ComputeForce(FlightState state, IDroneModel drone, IEnvironment environment)
    {
        var gravityAccelerationMetersPerSecondSquared =
            environment.Atmosphere.GetGravityMetersPerSecondSquaredAt(state.Position.ZMeters);
        var gravitationalForceNewtons =
            drone.MassKilograms * gravityAccelerationMetersPerSecondSquared;

        return new AppliedForce(
            XNewtons: 0.0,
            YNewtons: 0.0,
            ZNewtons: -gravitationalForceNewtons,
            TorqueXNewtonMeters: 0.0,
            TorqueYNewtonMeters: 0.0,
            TorqueZNewtonMeters: 0.0);
    }
}
