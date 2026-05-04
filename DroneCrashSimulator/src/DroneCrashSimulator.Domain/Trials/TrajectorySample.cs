using DroneCrashSimulator.Domain.Flight;

namespace DroneCrashSimulator.Domain.Trials;

public sealed record TrajectorySample(
    Position Position,
    Velocity Velocity,
    Attitude Attitude,
    double TimeSeconds);
