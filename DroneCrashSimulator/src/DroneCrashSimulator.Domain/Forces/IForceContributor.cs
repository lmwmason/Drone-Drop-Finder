using DroneCrashSimulator.Domain.Drones;
using DroneCrashSimulator.Domain.Environment;
using DroneCrashSimulator.Domain.Flight;

namespace DroneCrashSimulator.Domain.Forces;

public interface IForceContributor
{
    AppliedForce ComputeForce(FlightState state, IDroneModel drone, IEnvironment environment);
}
