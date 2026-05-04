using DroneCrashSimulator.Domain.Drones;
using DroneCrashSimulator.Domain.Environment;
using DroneCrashSimulator.Domain.Failure;
using DroneCrashSimulator.Application.Trials;

namespace DroneCrashSimulator.Application.Batch;

public sealed record Scenario(
    string Name,
    IDroneModel Drone,
    IEnvironment Environment,
    IFailureMode FailureMode,
    TrialConfiguration TrialConfiguration,
    int TrialCount);
