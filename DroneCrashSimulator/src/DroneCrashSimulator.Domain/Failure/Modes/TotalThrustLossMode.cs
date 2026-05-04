using DroneCrashSimulator.Domain.Flight;

namespace DroneCrashSimulator.Domain.Failure.Modes;

public sealed class TotalThrustLossMode : IFailureMode
{
    private const double MaxInitialAngularPerturbationRadiansPerSecond = 0.5;
    private const double MaxInitialAttitudePerturbationRadians = 0.1;

    public string Name => "Total Thrust Loss";

    public FlightState PerturbInitialState(FlightState stateAtFailure, Random random)
    {
        var perturbedAttitude = PerturbAttitude(stateAtFailure.Attitude, random);
        var perturbedAngularVelocity = SampleSmallAngularVelocity(random);

        return stateAtFailure with
        {
            Attitude = perturbedAttitude,
            AngularVelocityRadiansPerSecond = perturbedAngularVelocity
        };
    }

    private static Attitude PerturbAttitude(Attitude original, Random random)
    {
        var axisX = random.NextDouble() * 2.0 - 1.0;
        var axisY = random.NextDouble() * 2.0 - 1.0;
        var axisZ = random.NextDouble() * 2.0 - 1.0;
        var axisLength = Math.Sqrt(axisX * axisX + axisY * axisY + axisZ * axisZ);

        if (axisLength < double.Epsilon) return original;

        var angle = (random.NextDouble() * 2.0 - 1.0) * MaxInitialAttitudePerturbationRadians;
        var perturbation = Attitude.FromAxisAngle(
            axisX / axisLength,
            axisY / axisLength,
            axisZ / axisLength,
            angle);

        return MultiplyQuaternions(perturbation, original);
    }

    private static Attitude MultiplyQuaternions(Attitude a, Attitude b) =>
        new Attitude(
            W: a.W * b.W - a.X * b.X - a.Y * b.Y - a.Z * b.Z,
            X: a.W * b.X + a.X * b.W + a.Y * b.Z - a.Z * b.Y,
            Y: a.W * b.Y - a.X * b.Z + a.Y * b.W + a.Z * b.X,
            Z: a.W * b.Z + a.X * b.Y - a.Y * b.X + a.Z * b.W).Normalized();

    private static Velocity SampleSmallAngularVelocity(Random random) =>
        new Velocity(
            XMetersPerSecond: (random.NextDouble() * 2.0 - 1.0) * MaxInitialAngularPerturbationRadiansPerSecond,
            YMetersPerSecond: (random.NextDouble() * 2.0 - 1.0) * MaxInitialAngularPerturbationRadiansPerSecond,
            ZMetersPerSecond: (random.NextDouble() * 2.0 - 1.0) * MaxInitialAngularPerturbationRadiansPerSecond);
}
