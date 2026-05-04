using DroneCrashSimulator.Domain.Drones;
using DroneCrashSimulator.Domain.Drones.Models;

namespace DroneCrashSimulator.Application.Building;

public sealed class DroneCatalog
{
    private readonly Dictionary<string, IDroneModel> _drones = new();

    public DroneCatalog Register(IDroneModel drone)
    {
        _drones[drone.Name] = drone;
        return this;
    }

    public IDroneModel GetByName(string name)
    {
        if (_drones.TryGetValue(name, out var drone))
            return drone;
        throw new KeyNotFoundException($"Drone '{name}' is not registered.");
    }

    public IReadOnlyList<string> RegisteredNames => [.. _drones.Keys];

    public static DroneCatalog CreateDefault()
    {
        return new DroneCatalog().Register(new QuadcopterModel());
    }
}
