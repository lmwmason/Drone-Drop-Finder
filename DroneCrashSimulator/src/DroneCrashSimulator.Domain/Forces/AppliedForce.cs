namespace DroneCrashSimulator.Domain.Forces;

public readonly record struct AppliedForce(
    double XNewtons,
    double YNewtons,
    double ZNewtons,
    double TorqueXNewtonMeters,
    double TorqueYNewtonMeters,
    double TorqueZNewtonMeters)
{
    public static readonly AppliedForce Zero = new(0.0, 0.0, 0.0, 0.0, 0.0, 0.0);

    public static AppliedForce operator +(AppliedForce a, AppliedForce b) =>
        new(a.XNewtons + b.XNewtons,
            a.YNewtons + b.YNewtons,
            a.ZNewtons + b.ZNewtons,
            a.TorqueXNewtonMeters + b.TorqueXNewtonMeters,
            a.TorqueYNewtonMeters + b.TorqueYNewtonMeters,
            a.TorqueZNewtonMeters + b.TorqueZNewtonMeters);
}
