using DroneCrashSimulator.Domain.Failure;
using DroneCrashSimulator.Domain.Failure.Modes;

namespace DroneCrashSimulator.Application.Building;

public sealed class FailureModeCatalog
{
    private readonly Dictionary<string, IFailureMode> _modes = new();

    public FailureModeCatalog Register(IFailureMode mode)
    {
        _modes[mode.Name] = mode;
        return this;
    }

    public IFailureMode GetByName(string name)
    {
        if (_modes.TryGetValue(name, out var mode))
            return mode;
        throw new KeyNotFoundException($"Failure mode '{name}' is not registered.");
    }

    public IReadOnlyList<string> RegisteredNames => [.. _modes.Keys];

    public static FailureModeCatalog CreateDefault()
    {
        return new FailureModeCatalog().Register(new TotalThrustLossMode());
    }
}
