namespace DroneCrashSimulator.Domain.Environment;

public sealed class StandardEnvironment : IEnvironment
{
    public StandardEnvironment(
        IAtmosphere atmosphere,
        ITerrain terrain,
        IWindField windField,
        ITurbulenceModel turbulenceModel)
    {
        Atmosphere = atmosphere;
        Terrain = terrain;
        WindField = windField;
        TurbulenceModel = turbulenceModel;
    }

    public IAtmosphere Atmosphere { get; }
    public ITerrain Terrain { get; }
    public IWindField WindField { get; }
    public ITurbulenceModel TurbulenceModel { get; }
}
