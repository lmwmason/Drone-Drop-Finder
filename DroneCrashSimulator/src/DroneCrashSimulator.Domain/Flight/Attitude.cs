namespace DroneCrashSimulator.Domain.Flight;

public readonly record struct Attitude(double W, double X, double Y, double Z)
{
    public static readonly Attitude Identity = new(1.0, 0.0, 0.0, 0.0);

    public double NormSquared => W * W + X * X + Y * Y + Z * Z;

    public Attitude Normalized()
    {
        var magnitude = Math.Sqrt(NormSquared);
        return new Attitude(W / magnitude, X / magnitude, Y / magnitude, Z / magnitude);
    }

    public static Attitude FromAxisAngle(double axisX, double axisY, double axisZ, double angleRadians)
    {
        var halfAngle = angleRadians * 0.5;
        var sinHalf = Math.Sin(halfAngle);
        return new Attitude(
            W: Math.Cos(halfAngle),
            X: axisX * sinHalf,
            Y: axisY * sinHalf,
            Z: axisZ * sinHalf).Normalized();
    }
}
