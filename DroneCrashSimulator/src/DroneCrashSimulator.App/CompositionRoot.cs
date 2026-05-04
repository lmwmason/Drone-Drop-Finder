using DroneCrashSimulator.App.Bootstrapping;
using DroneCrashSimulator.App.Modes;
using DroneCrashSimulator.Application.Building;

namespace DroneCrashSimulator.App;

public sealed class CompositionRoot
{
    public static (InteractiveMode interactive, HeadlessBatchMode headless) Build()
    {
        var drones = DroneRegistration.RegisterAll(new DroneCatalog());
        var environments = EnvironmentRegistration.RegisterDefaults(new EnvironmentCatalog());
        var failureModes = FailureModeRegistration.RegisterAll(new FailureModeCatalog());

        var interactive = new InteractiveMode();
        var headless = new HeadlessBatchMode(drones, failureModes);

        return (interactive, headless);
    }
}
