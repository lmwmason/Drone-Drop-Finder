using DroneCrashSimulator.Domain.Flight;

namespace DroneCrashSimulator.Application.Trials;

public sealed class InitialConditionsSampler
{
    private const double TwoPi = 2.0 * Math.PI;

    public FlightState SampleInitialState(
        double altitudeMeters,
        double cruiseSpeedMetersPerSecond,
        Random random)
    {
        var bearingRadians = random.NextDouble() * TwoPi;
        var speedFraction = 0.3 + random.NextDouble() * 0.7;
        var speed = cruiseSpeedMetersPerSecond * speedFraction;

        var horizontalVelocity = new Velocity(
            XMetersPerSecond: speed * Math.Cos(bearingRadians),
            YMetersPerSecond: speed * Math.Sin(bearingRadians),
            ZMetersPerSecond: 0.0);

        var tiltAngleRadians = (random.NextDouble() - 0.5) * 0.3;
        var tiltAxisAngle = random.NextDouble() * TwoPi;
        var attitude = Attitude.FromAxisAngle(
            Math.Cos(tiltAxisAngle),
            Math.Sin(tiltAxisAngle),
            0.0,
            tiltAngleRadians);

        return new FlightState(
            Position: new Position(0.0, 0.0, altitudeMeters),
            Velocity: horizontalVelocity,
            Attitude: attitude,
            AngularVelocityRadiansPerSecond: Velocity.Zero,
            TimeSeconds: 0.0);
    }
}
