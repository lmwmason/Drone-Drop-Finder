using DroneCrashSimulator.Domain.Trials;

namespace DroneCrashSimulator.Physics.Simulation;

public sealed record SimulationStep(
    TrajectorySample Sample,
    bool HasReachedTerrain);
