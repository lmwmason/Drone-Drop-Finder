using DroneCrashSimulator.Domain.Flight;

namespace DroneCrashSimulator.Domain.Environment.Terrains;

public sealed class FlatTerrain : ITerrain
{
    private readonly double _elevationMeters;

    public FlatTerrain(double elevationMeters = 0.0)
    {
        _elevationMeters = elevationMeters;
    }

    public double GetElevationMetersAt(double xMeters, double yMeters) => _elevationMeters;

    public bool IsAboveTerrain(Position position) =>
        position.ZMeters > GetElevationMetersAt(position.XMeters, position.YMeters);
}
