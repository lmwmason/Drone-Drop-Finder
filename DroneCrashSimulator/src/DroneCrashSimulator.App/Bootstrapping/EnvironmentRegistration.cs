using DroneCrashSimulator.Application.Building;
using DroneCrashSimulator.Domain.Environment;
using DroneCrashSimulator.Domain.Environment.Atmosphere;
using DroneCrashSimulator.Domain.Environment.Terrains;
using DroneCrashSimulator.Domain.Environment.Turbulence;
using DroneCrashSimulator.Domain.Environment.WindFields;
using DroneCrashSimulator.Domain.Flight;

namespace DroneCrashSimulator.App.Bootstrapping;

public static class EnvironmentRegistration
{
    public static EnvironmentCatalog RegisterDefaults(EnvironmentCatalog catalog)
    {
        catalog.Register("Calm / Flat", BuildEnvironment(Velocity.Zero));
        catalog.Register("5 m/s Wind / Flat", BuildEnvironment(new Velocity(5.0, 0.0, 0.0)));
        catalog.Register("10 m/s Wind / Flat", BuildEnvironment(new Velocity(10.0, 0.0, 0.0)));
        return catalog;
    }

    private static IEnvironment BuildEnvironment(Velocity windVelocity) =>
        new StandardEnvironment(
            new StandardAtmosphere(),
            new FlatTerrain(),
            new UniformWindField(windVelocity),
            new DrydenTurbulenceModel());
}
