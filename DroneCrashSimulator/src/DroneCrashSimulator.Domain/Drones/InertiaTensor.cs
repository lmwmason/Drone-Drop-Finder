namespace DroneCrashSimulator.Domain.Drones;

public sealed record InertiaTensor(
    double IxxKilogramSquareMeters,
    double IyyKilogramSquareMeters,
    double IzzKilogramSquareMeters);
