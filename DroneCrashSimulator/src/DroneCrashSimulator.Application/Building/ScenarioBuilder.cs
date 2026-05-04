using DroneCrashSimulator.Application.Batch;
using DroneCrashSimulator.Application.Trials;
using DroneCrashSimulator.Domain.Drones;
using DroneCrashSimulator.Domain.Environment;
using DroneCrashSimulator.Domain.Failure;
using DroneCrashSimulator.Domain.Flight;

namespace DroneCrashSimulator.Application.Building;

public sealed class ScenarioBuilder
{
    private IDroneModel? _drone;
    private IEnvironment? _environment;
    private IFailureMode? _failureMode;
    private Position _failurePosition = new(0.0, 0.0, 100.0);
    private Velocity _failureVelocity = Velocity.Zero;
    private int _trialCount = 100;
    private string _name = "Unnamed";

    public ScenarioBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ScenarioBuilder WithDrone(IDroneModel drone)
    {
        _drone = drone;
        return this;
    }

    public ScenarioBuilder WithEnvironment(IEnvironment environment)
    {
        _environment = environment;
        return this;
    }

    public ScenarioBuilder WithFailureMode(IFailureMode mode)
    {
        _failureMode = mode;
        return this;
    }

    public ScenarioBuilder WithFailurePosition(Position position)
    {
        _failurePosition = position;
        return this;
    }

    public ScenarioBuilder WithFailureVelocity(Velocity velocity)
    {
        _failureVelocity = velocity;
        return this;
    }

    public ScenarioBuilder WithTrialCount(int count)
    {
        _trialCount = count;
        return this;
    }

    public Scenario Build()
    {
        if (_drone is null) throw new InvalidOperationException("Drone is required.");
        if (_environment is null) throw new InvalidOperationException("Environment is required.");
        if (_failureMode is null) throw new InvalidOperationException("Failure mode is required.");

        return new Scenario(
            Name: _name,
            Drone: _drone,
            Environment: _environment,
            FailureMode: _failureMode,
            TrialConfiguration: new TrialConfiguration(
                FailurePosition: _failurePosition,
                FailureVelocity: _failureVelocity,
                TrialCount: _trialCount),
            TrialCount: _trialCount);
    }
}
