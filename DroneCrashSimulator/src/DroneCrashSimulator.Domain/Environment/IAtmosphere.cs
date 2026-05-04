namespace DroneCrashSimulator.Domain.Environment;

public interface IAtmosphere
{
    double GetAirDensityKilogramsPerCubicMeterAt(double altitudeMeters);
    double GetGravityMetersPerSecondSquaredAt(double altitudeMeters);
}
