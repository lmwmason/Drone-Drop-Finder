using DroneCrashSimulator.Domain.Flight;

namespace DroneCrashSimulator.Domain.Environment;

public interface ITurbulenceModel
{
    void InitializeForTrial(int seed);
    Velocity SampleTurbulenceVelocity(FlightState state);
}
