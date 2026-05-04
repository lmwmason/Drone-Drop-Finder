using DroneCrashSimulator.Domain.Drones;
using DroneCrashSimulator.Domain.Environment;
using DroneCrashSimulator.Domain.Flight;
using DroneCrashSimulator.Domain.Trials;
using DroneCrashSimulator.Physics.Bepu;
using DroneCrashSimulator.Physics.Bodies;
using DroneCrashSimulator.Physics.Forces;
using BepuSim = BepuPhysics.Simulation;

namespace DroneCrashSimulator.Physics.Simulation;

public sealed class PhysicsSimulation : IDisposable
{
    private readonly BepuSim _bepuSimulation;
    private readonly BepuUtilities.Memory.BufferPool _bufferPool;
    private readonly RigidBodyAdapter _droneBody;
    private readonly ForceApplicator _forceApplicator;
    private readonly TimestepConfiguration _timestepConfig;
    private readonly IDroneModel _drone;
    private readonly IEnvironment _environment;
    private double _elapsedTimeSeconds;

    private PhysicsSimulation(
        BepuSim bepuSimulation,
        BepuUtilities.Memory.BufferPool bufferPool,
        RigidBodyAdapter droneBody,
        ForceApplicator forceApplicator,
        TimestepConfiguration timestepConfig,
        IDroneModel drone,
        IEnvironment environment)
    {
        _bepuSimulation = bepuSimulation;
        _bufferPool = bufferPool;
        _droneBody = droneBody;
        _forceApplicator = forceApplicator;
        _timestepConfig = timestepConfig;
        _drone = drone;
        _environment = environment;
        _elapsedTimeSeconds = 0.0;
    }

    public static PhysicsSimulation Create(
        IDroneModel drone,
        IEnvironment environment,
        FlightState initialState,
        ForceApplicator forceApplicator,
        TimestepConfiguration timestepConfig)
    {
        var bufferPool = BepuMemoryPool.Create();
        var bepuSimulation = BepuSimulationFactory.Create(bufferPool);
        var droneBody = RigidBodyAdapter.CreateFor(bepuSimulation, drone, initialState);

        return new PhysicsSimulation(
            bepuSimulation, bufferPool, droneBody,
            forceApplicator, timestepConfig, drone, environment);
    }

    public SimulationStep Advance()
    {
        var currentState = _droneBody.ReadCurrentFlightState(_elapsedTimeSeconds);

        _forceApplicator.ApplyAccumulatedForces(
            _droneBody, currentState, _drone, _environment, _timestepConfig.TimestepSeconds);

        _bepuSimulation.Timestep((float)_timestepConfig.TimestepSeconds);
        _elapsedTimeSeconds += _timestepConfig.TimestepSeconds;

        var sample = _droneBody.ReadCurrentSample(_elapsedTimeSeconds);
        var terrainElevation = _environment.Terrain
            .GetElevationMetersAt(sample.Position.XMeters, sample.Position.YMeters);
        var hasReachedTerrain = sample.Position.ZMeters <= terrainElevation;

        return new SimulationStep(sample, hasReachedTerrain);
    }

    public bool HasExceededMaxDuration =>
        _elapsedTimeSeconds >= _timestepConfig.MaxSimulationDurationSeconds;

    public void Dispose()
    {
        _bepuSimulation.Dispose();
        _bufferPool.Clear();
    }
}
