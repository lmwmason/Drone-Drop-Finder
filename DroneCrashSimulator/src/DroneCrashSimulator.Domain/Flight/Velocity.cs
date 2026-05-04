namespace DroneCrashSimulator.Domain.Flight;

public readonly record struct Velocity(
    double XMetersPerSecond,
    double YMetersPerSecond,
    double ZMetersPerSecond)
{
    public static readonly Velocity Zero = new(0.0, 0.0, 0.0);

    public double MagnitudeMetersPerSecond =>
        Math.Sqrt(
            XMetersPerSecond * XMetersPerSecond +
            YMetersPerSecond * YMetersPerSecond +
            ZMetersPerSecond * ZMetersPerSecond);

    public static Velocity operator +(Velocity a, Velocity b) =>
        new(a.XMetersPerSecond + b.XMetersPerSecond,
            a.YMetersPerSecond + b.YMetersPerSecond,
            a.ZMetersPerSecond + b.ZMetersPerSecond);

    public static Velocity operator -(Velocity a, Velocity b) =>
        new(a.XMetersPerSecond - b.XMetersPerSecond,
            a.YMetersPerSecond - b.YMetersPerSecond,
            a.ZMetersPerSecond - b.ZMetersPerSecond);

    public static Velocity operator *(Velocity v, double scalar) =>
        new(v.XMetersPerSecond * scalar,
            v.YMetersPerSecond * scalar,
            v.ZMetersPerSecond * scalar);

    public static Velocity operator *(double scalar, Velocity v) => v * scalar;
}
