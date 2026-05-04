using DroneCrashSimulator.Domain.Flight;

namespace DroneCrashSimulator.Domain.Environment.WindFields;

public sealed class UniformWindField : IWindField
{
    private readonly Velocity _windVelocity;

    public UniformWindField(Velocity windVelocity)
    {
        _windVelocity = windVelocity;
    }

    public static UniformWindField Calm => new(Velocity.Zero);

    public Velocity GetWindVelocityAt(Position position) => _windVelocity;
}
