namespace DroneCrashSimulator.Domain.Flight;

public sealed record FlightState(
    Position Position,
    Velocity Velocity,
    Attitude Attitude,
    Velocity AngularVelocityRadiansPerSecond,
    double TimeSeconds);
