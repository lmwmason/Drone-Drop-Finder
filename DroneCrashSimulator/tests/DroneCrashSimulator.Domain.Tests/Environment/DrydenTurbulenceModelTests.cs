using DroneCrashSimulator.Domain.Environment.Turbulence;
using DroneCrashSimulator.Domain.Flight;

namespace DroneCrashSimulator.Domain.Tests.Environment;

public sealed class DrydenTurbulenceModelTests
{
    [Fact]
    public void SampleTurbulenceVelocity_WithSameSeed_ProducesIdenticalSequence()
    {
        const int seed = 42;
        var model1 = new DrydenTurbulenceModel();
        var model2 = new DrydenTurbulenceModel();
        model1.InitializeForTrial(seed);
        model2.InitializeForTrial(seed);

        var state = new FlightState(Position.Origin, new Velocity(0.0, 0.0, -10.0), Attitude.Identity, Velocity.Zero, 0.0);

        var velocity1 = model1.SampleTurbulenceVelocity(state);
        var velocity2 = model2.SampleTurbulenceVelocity(state);

        Assert.Equal(velocity1, velocity2);
    }

    [Fact]
    public void SampleTurbulenceVelocity_WithDifferentSeeds_ProducesDifferentSequences()
    {
        var model1 = new DrydenTurbulenceModel();
        var model2 = new DrydenTurbulenceModel();
        model1.InitializeForTrial(1);
        model2.InitializeForTrial(2);

        var state = new FlightState(Position.Origin, new Velocity(0.0, 0.0, -10.0), Attitude.Identity, Velocity.Zero, 0.0);
        var samples1 = Enumerable.Range(0, 10).Select(_ => model1.SampleTurbulenceVelocity(state)).ToList();
        var samples2 = Enumerable.Range(0, 10).Select(_ => model2.SampleTurbulenceVelocity(state)).ToList();

        Assert.False(samples1.SequenceEqual(samples2), "Different seeds should produce different samples");
    }

    [Fact]
    public void SampleTurbulenceVelocity_ProducesFiniteValues()
    {
        var model = new DrydenTurbulenceModel();
        model.InitializeForTrial(123);
        var state = new FlightState(Position.Origin, new Velocity(0.0, 0.0, -15.0), Attitude.Identity, Velocity.Zero, 1.0);

        for (var i = 0; i < 100; i++)
        {
            var velocity = model.SampleTurbulenceVelocity(state);
            Assert.True(double.IsFinite(velocity.XMetersPerSecond));
            Assert.True(double.IsFinite(velocity.YMetersPerSecond));
            Assert.True(double.IsFinite(velocity.ZMetersPerSecond));
        }
    }
}
