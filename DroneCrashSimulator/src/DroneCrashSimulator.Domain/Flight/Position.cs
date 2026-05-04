namespace DroneCrashSimulator.Domain.Flight;

public readonly record struct Position(double XMeters, double YMeters, double ZMeters)
{
    public static readonly Position Origin = new(0.0, 0.0, 0.0);

    public double HorizontalDistanceFromOriginMeters =>
        Math.Sqrt(XMeters * XMeters + YMeters * YMeters);

    public double BearingFromOriginDegrees =>
        (Math.Atan2(YMeters, XMeters) * 180.0 / Math.PI + 360.0) % 360.0;
}
