using DroneCrashSimulator.Domain.Flight;

namespace DroneCrashSimulator.Domain.Environment;

public interface ITerrain
{
    double GetElevationMetersAt(double xMeters, double yMeters);
    bool IsAboveTerrain(Position position);
}
