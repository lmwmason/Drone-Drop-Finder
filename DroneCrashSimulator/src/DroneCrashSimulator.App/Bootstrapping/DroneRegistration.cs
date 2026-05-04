using DroneCrashSimulator.Application.Building;
using DroneCrashSimulator.Domain.Drones.Models;

namespace DroneCrashSimulator.App.Bootstrapping;

public static class DroneRegistration
{
    public static DroneCatalog RegisterAll(DroneCatalog catalog)
    {
        catalog.Register(new QuadcopterModel());
        return catalog;
    }
}
