using DroneCrashSimulator.Application.Building;
using DroneCrashSimulator.Domain.Failure.Modes;

namespace DroneCrashSimulator.App.Bootstrapping;

public static class FailureModeRegistration
{
    public static FailureModeCatalog RegisterAll(FailureModeCatalog catalog)
    {
        catalog.Register(new TotalThrustLossMode());
        return catalog;
    }
}
