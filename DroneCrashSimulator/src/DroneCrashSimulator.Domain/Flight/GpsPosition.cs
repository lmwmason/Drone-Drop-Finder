namespace DroneCrashSimulator.Domain.Flight;

public sealed record GpsPosition(
    double LatitudeDegrees,
    double LongitudeDegrees,
    double AltitudeMeters);
