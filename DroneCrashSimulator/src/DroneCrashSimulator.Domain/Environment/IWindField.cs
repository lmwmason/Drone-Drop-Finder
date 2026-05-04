using DroneCrashSimulator.Domain.Flight;

namespace DroneCrashSimulator.Domain.Environment;

public interface IWindField
{
    Velocity GetWindVelocityAt(Position position);
}
