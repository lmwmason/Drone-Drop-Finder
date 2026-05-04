using DroneCrashSimulator.Domain.Drones;
using DroneCrashSimulator.Domain.Environment;
using DroneCrashSimulator.Domain.Flight;
using DroneCrashSimulator.Physics.Bodies;

namespace DroneCrashSimulator.Physics.Forces;

public sealed class ForceApplicator
{
    private readonly ForceAccumulator _accumulator;

    public ForceApplicator(ForceAccumulator accumulator)
    {
        _accumulator = accumulator;
    }

    public void ApplyAccumulatedForces(
        RigidBodyAdapter body,
        FlightState state,
        IDroneModel drone,
        IEnvironment environment,
        double dtSeconds)
    {
        var totalForce = _accumulator.AccumulateForces(state, drone, environment);

        body.ApplyForceImpulse(
            totalForce.XNewtons,
            totalForce.YNewtons,
            totalForce.ZNewtons,
            drone.MassKilograms,
            dtSeconds);

        body.ApplyTorqueImpulse(
            totalForce.TorqueXNewtonMeters,
            totalForce.TorqueYNewtonMeters,
            totalForce.TorqueZNewtonMeters,
            drone.InertiaTensor,
            dtSeconds);
    }
}
