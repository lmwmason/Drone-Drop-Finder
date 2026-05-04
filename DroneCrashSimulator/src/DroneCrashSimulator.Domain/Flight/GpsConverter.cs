namespace DroneCrashSimulator.Domain.Flight;

public static class GpsConverter
{
    private const double MetersPerDegreeLat = 111320.0;

    public static GpsPosition LocalOffsetToGps(
        double dxMeters,
        double dyMeters,
        GpsPosition origin)
    {
        var metersPerDegreeLon = MetersPerDegreeLat
            * Math.Cos(origin.LatitudeDegrees * Math.PI / 180.0);

        return new GpsPosition(
            LatitudeDegrees: origin.LatitudeDegrees + dyMeters / MetersPerDegreeLat,
            LongitudeDegrees: origin.LongitudeDegrees + dxMeters / metersPerDegreeLon,
            AltitudeMeters: 0.0);
    }
}
