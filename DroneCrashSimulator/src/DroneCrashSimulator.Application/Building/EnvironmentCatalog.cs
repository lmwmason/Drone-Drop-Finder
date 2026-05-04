using DroneCrashSimulator.Domain.Environment;

namespace DroneCrashSimulator.Application.Building;

public sealed class EnvironmentCatalog
{
    private readonly Dictionary<string, IEnvironment> _environments = new();

    public EnvironmentCatalog Register(string name, IEnvironment environment)
    {
        _environments[name] = environment;
        return this;
    }

    public IEnvironment GetByName(string name)
    {
        if (_environments.TryGetValue(name, out var environment))
            return environment;
        throw new KeyNotFoundException($"Environment '{name}' is not registered.");
    }

    public IReadOnlyList<string> RegisteredNames => [.. _environments.Keys];
}
