using DroneCrashSimulator.Domain.Constants;

namespace DroneCrashSimulator.Domain.Environment.Atmosphere;

public sealed class StandardAtmosphere : IAtmosphere
{
    public double GetAirDensityKilogramsPerCubicMeterAt(double altitudeMeters)
    {
        var temperatureKelvin = PhysicalConstants.SeaLevelTemperatureKelvin
            - PhysicalConstants.TemperatureLapseRateKelvinPerMeter * altitudeMeters;

        var temperatureRatio = temperatureKelvin / PhysicalConstants.SeaLevelTemperatureKelvin;

        var pressureRatio = Math.Pow(
            temperatureRatio,
            PhysicalConstants.StandardAtmosphereBarometricExponent);

        return PhysicalConstants.SeaLevelAirDensityKilogramsPerCubicMeter
            * pressureRatio
            / temperatureRatio;
    }

    public double GetGravityMetersPerSecondSquaredAt(double altitudeMeters)
    {
        return PhysicalConstants.StandardGravityMetersPerSecondSquared;
    }
}
