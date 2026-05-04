using DroneCrashSimulator.Domain.Flight;

namespace DroneCrashSimulator.Domain.Environment.Turbulence;

public sealed class DrydenTurbulenceModel : ITurbulenceModel
{
    private const double HorizontalIntensityMetersPerSecond = 1.5;
    private const double VerticalIntensityMetersPerSecond = 1.0;
    private const double HorizontalLengthScaleMeters = 200.0;
    private const double VerticalLengthScaleMeters = 50.0;
    private const double MinimumSpeedMetersPerSecond = 1.0;

    private Random _random = new();
    private double _stateU;
    private double _stateV;
    private double _stateW;

    public void InitializeForTrial(int seed)
    {
        _random = new Random(seed);
        _stateU = 0.0;
        _stateV = 0.0;
        _stateW = 0.0;
    }

    public Velocity SampleTurbulenceVelocity(FlightState state)
    {
        var speedMetersPerSecond = Math.Max(
            state.Velocity.MagnitudeMetersPerSecond,
            MinimumSpeedMetersPerSecond);

        var dtSeconds = 0.01;

        _stateU = UpdateGaussMarkovState(
            _stateU, HorizontalIntensityMetersPerSecond,
            HorizontalLengthScaleMeters, speedMetersPerSecond, dtSeconds);
        _stateV = UpdateGaussMarkovState(
            _stateV, HorizontalIntensityMetersPerSecond,
            HorizontalLengthScaleMeters, speedMetersPerSecond, dtSeconds);
        _stateW = UpdateGaussMarkovState(
            _stateW, VerticalIntensityMetersPerSecond,
            VerticalLengthScaleMeters, speedMetersPerSecond, dtSeconds);

        return new Velocity(_stateU, _stateV, _stateW);
    }

    private double UpdateGaussMarkovState(
        double currentState,
        double intensityMetersPerSecond,
        double lengthScaleMeters,
        double speedMetersPerSecond,
        double dtSeconds)
    {
        var timeConstantSeconds = lengthScaleMeters / speedMetersPerSecond;
        var decayFactor = 1.0 - dtSeconds / timeConstantSeconds;
        var noiseAmplitude = intensityMetersPerSecond
            * Math.Sqrt(2.0 * dtSeconds / timeConstantSeconds);
        var gaussianNoise = SampleStandardNormal();
        return decayFactor * currentState + noiseAmplitude * gaussianNoise;
    }

    private double SampleStandardNormal()
    {
        var u1 = 1.0 - _random.NextDouble();
        var u2 = 1.0 - _random.NextDouble();
        return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
    }
}
