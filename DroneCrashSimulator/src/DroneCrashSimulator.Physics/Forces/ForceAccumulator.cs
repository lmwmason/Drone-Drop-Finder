using DroneCrashSimulator.Domain.Drones;
using DroneCrashSimulator.Domain.Environment;
using DroneCrashSimulator.Domain.Flight;
using DroneCrashSimulator.Domain.Forces;

namespace DroneCrashSimulator.Physics.Forces;

public sealed class ForceAccumulator
{
    private readonly IReadOnlyList<IForceContributor> _contributors;

    public ForceAccumulator(IReadOnlyList<IForceContributor> contributors)
    {
        _contributors = contributors;
    }

    public AppliedForce AccumulateForces(
        FlightState state,
        IDroneModel drone,
        IEnvironment environment)
    {
        var total = AppliedForce.Zero;
        foreach (var contributor in _contributors)
            total += contributor.ComputeForce(state, drone, environment);
        return total;
    }
}
