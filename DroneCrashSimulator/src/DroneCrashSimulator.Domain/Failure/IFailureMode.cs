using DroneCrashSimulator.Domain.Flight;

namespace DroneCrashSimulator.Domain.Failure;

public interface IFailureMode
{
    string Name { get; }
    FlightState PerturbInitialState(FlightState stateAtFailure, Random random);
}
