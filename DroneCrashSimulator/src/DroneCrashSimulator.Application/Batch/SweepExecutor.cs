using DroneCrashSimulator.Application.Trials;
using DroneCrashSimulator.Domain.Drones;
using DroneCrashSimulator.Domain.Environment.Atmosphere;
using DroneCrashSimulator.Domain.Environment.Terrains;
using DroneCrashSimulator.Domain.Environment.Turbulence;
using DroneCrashSimulator.Domain.Environment.WindFields;
using DroneCrashSimulator.Domain.Environment;
using DroneCrashSimulator.Domain.Flight;
using DroneCrashSimulator.Physics.Simulation;

namespace DroneCrashSimulator.Application.Batch;

public sealed class SweepExecutor
{
    private const double TwoPi = 2.0 * Math.PI;
    private const double RadiansToDegrees = 180.0 / Math.PI;

    private readonly SweepConfiguration _config;
    private readonly IDroneModel _drone;
    private readonly TrialRunner _trialRunner;
    private readonly IReadOnlyList<SweepCombination> _combinations;
    private readonly TrialSeedGenerator _seedGenerator;

    private int _comboIndex;
    private int _trialInCombo;

    public SweepExecutor(SweepConfiguration config, IDroneModel drone)
    {
        _config = config;
        _drone = drone;
        _trialRunner = new TrialRunner(TimestepConfiguration.FastBatch);
        _combinations = BuildCombinations();
        _seedGenerator = TrialSeedGenerator.FromSystemTime();
    }

    public bool IsComplete => _comboIndex >= _combinations.Count;
    public int CompletedTrials => _comboIndex * _config.TrialsPerCombination + _trialInCombo;
    public SweepCombination? CurrentCombination =>
        _comboIndex < _combinations.Count ? _combinations[_comboIndex] : null;

    public SweepTrialOutput? AdvanceOneTrial()
    {
        if (IsComplete) return null;

        var combination = _combinations[_comboIndex];
        var seed = _seedGenerator.NextSeed();
        var random = new Random(seed);

        var windSpeed = random.NextDouble() * _config.MaxWindSpeedMetersPerSecond;
        var windAngle = random.NextDouble() * TwoPi;

        var environment = BuildEnvironment(windSpeed, windAngle);
        var initialState = BuildExactInitialState(combination);

        var result = _trialRunner.RunTrialFromFixedState(
            _trialInCombo, seed, _drone, environment, initialState);

        var output = new SweepTrialOutput(
            Combination: combination,
            Result: result,
            WindSpeedMetersPerSecond: windSpeed,
            WindBearingDegrees: windAngle * RadiansToDegrees);

        _trialInCombo++;
        if (_trialInCombo >= _config.TrialsPerCombination)
        {
            _trialInCombo = 0;
            _comboIndex++;
        }

        return output;
    }

    private static FlightState BuildExactInitialState(SweepCombination combination) =>
        new FlightState(
            Position: new Position(0.0, 0.0, combination.AltitudeMeters),
            Velocity: new Velocity(
                XMetersPerSecond: combination.HorizontalSpeedMetersPerSecond,
                YMetersPerSecond: 0.0,
                ZMetersPerSecond: combination.VerticalSpeedMetersPerSecond),
            Attitude: Attitude.Identity,
            AngularVelocityRadiansPerSecond: Velocity.Zero,
            TimeSeconds: 0.0);

    private static IEnvironment BuildEnvironment(double windSpeed, double windAngle) =>
        new StandardEnvironment(
            new StandardAtmosphere(),
            new FlatTerrain(),
            new UniformWindField(new Velocity(
                XMetersPerSecond: windSpeed * Math.Cos(windAngle),
                YMetersPerSecond: windSpeed * Math.Sin(windAngle),
                ZMetersPerSecond: 0.0)),
            new DrydenTurbulenceModel());

    private IReadOnlyList<SweepCombination> BuildCombinations()
    {
        var list = new List<SweepCombination>();
        var altLevels = _config.AltitudeLevels;
        var hSpeedLevels = _config.HorizontalSpeedLevels;
        var vSpeedLevels = _config.VerticalSpeedLevels;

        for (var ai = 0; ai < altLevels.Count; ai++)
        for (var hi = 0; hi < hSpeedLevels.Count; hi++)
        for (var vi = 0; vi < vSpeedLevels.Count; vi++)
            list.Add(new SweepCombination(
                AltitudeIndex: ai,
                HorizontalSpeedIndex: hi,
                VerticalSpeedIndex: vi,
                AltitudeMeters: altLevels[ai],
                HorizontalSpeedMetersPerSecond: hSpeedLevels[hi],
                VerticalSpeedMetersPerSecond: vSpeedLevels[vi]));

        return list;
    }
}
