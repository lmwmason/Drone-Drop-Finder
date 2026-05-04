using DroneCrashSimulator.Domain.Drones;
using DroneCrashSimulator.Domain.Environment;
using DroneCrashSimulator.Domain.Failure;
using DroneCrashSimulator.Domain.Flight;
using DroneCrashSimulator.Domain.Forces;
using DroneCrashSimulator.Domain.Forces.Contributors;
using DroneCrashSimulator.Domain.Trials;
using DroneCrashSimulator.Physics.Forces;
using DroneCrashSimulator.Physics.Simulation;

namespace DroneCrashSimulator.Application.Trials;

public sealed class TrialRunner
{
    private readonly TimestepConfiguration _timestepConfig;
    private readonly InitialConditionsSampler _conditionsSampler;

    public TrialRunner(TimestepConfiguration timestepConfig)
    {
        _timestepConfig = timestepConfig;
        _conditionsSampler = new InitialConditionsSampler();
    }

    public TrialResult RunTrial(
        int trialIndex,
        int seed,
        IDroneModel drone,
        IEnvironment environment,
        IFailureMode failureMode,
        TrialConfiguration configuration)
    {
        var random = new Random(seed);
        environment.TurbulenceModel.InitializeForTrial(seed);

        var sampledInitialState = _conditionsSampler.SampleInitialState(
            configuration.FailurePosition.ZMeters,
            configuration.CruiseSpeedMetersPerSecond,
            random);

        var perturbedState = failureMode.PerturbInitialState(sampledInitialState, random);
        return SimulateFromState(trialIndex, seed, drone, environment, perturbedState);
    }

    public TrialResult RunTrialFromFixedState(
        int trialIndex,
        int seed,
        IDroneModel drone,
        IEnvironment environment,
        FlightState exactInitialState)
    {
        environment.TurbulenceModel.InitializeForTrial(seed);
        return SimulateFromState(trialIndex, seed, drone, environment, exactInitialState);
    }

    private TrialResult SimulateFromState(
        int trialIndex,
        int seed,
        IDroneModel drone,
        IEnvironment environment,
        FlightState initialState)
    {
        var forceApplicator = new ForceApplicator(
            new ForceAccumulator(BuildForceContributors()));

        var samples = new List<TrajectorySample>();

        using var simulation = PhysicsSimulation.Create(
            drone, environment, initialState, forceApplicator, _timestepConfig);

        while (!simulation.HasExceededMaxDuration)
        {
            var step = simulation.Advance();
            samples.Add(step.Sample);
            if (step.HasReachedTerrain) break;
        }

        var lastSample = samples[^1];
        var crashPoint = new CrashPoint(
            XMeters: lastSample.Position.XMeters,
            YMeters: lastSample.Position.YMeters,
            DistanceFromOriginMeters: lastSample.Position.HorizontalDistanceFromOriginMeters,
            BearingDegrees: lastSample.Position.BearingFromOriginDegrees);

        return new TrialResult
        {
            Trajectory = new Trajectory(samples),
            CrashPoint = crashPoint,
            TrialIndex = trialIndex,
            Seed = seed
        };
    }

    private static IForceContributor[] BuildForceContributors() =>
    [
        new GravityContributor(),
        new AerodynamicDragContributor(),
        new WindContributor()
    ];
}
