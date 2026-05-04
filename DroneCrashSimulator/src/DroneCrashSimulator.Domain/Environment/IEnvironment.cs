namespace DroneCrashSimulator.Domain.Environment;

public interface IEnvironment
{
    IAtmosphere Atmosphere { get; }
    ITerrain Terrain { get; }
    IWindField WindField { get; }
    ITurbulenceModel TurbulenceModel { get; }
}
