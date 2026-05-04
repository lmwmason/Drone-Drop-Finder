using DroneCrashSimulator.Domain.Trials;

namespace DroneCrashSimulator.Visualization.Animation;

public sealed class SequentialAnimator
{
    private const int PauseFramesAfterLanding = 25;
    private const int TrailSamples = 20;

    private readonly IReadOnlyList<TrialResult> _results;
    private readonly int _samplesPerFrame;
    private int _currentTrialIndex;
    private int _currentSampleIndex;
    private int _pauseRemaining;
    private readonly List<CrashPoint> _landedPoints = new();

    public SequentialAnimator(IReadOnlyList<TrialResult> results)
    {
        _results = results;
        _samplesPerFrame = ComputeSamplesPerFrame(results.Count);
    }

    public bool IsComplete { get; private set; }
    public IReadOnlyList<CrashPoint> LandedPoints => _landedPoints;
    public int CurrentTrialIndex => _currentTrialIndex;

    public float Progress => _results.Count == 0
        ? 1.0f
        : ((float)_currentTrialIndex + (float)_currentSampleIndex /
            Math.Max(1, _results[Math.Min(_currentTrialIndex, _results.Count - 1)].Trajectory.Samples.Count))
            / _results.Count;

    public void Advance()
    {
        if (IsComplete) return;

        if (_pauseRemaining > 0)
        {
            _pauseRemaining--;
            return;
        }

        _currentSampleIndex += _samplesPerFrame;

        var samples = _results[_currentTrialIndex].Trajectory.Samples;
        if (_currentSampleIndex < samples.Count) return;

        _landedPoints.Add(_results[_currentTrialIndex].CrashPoint);
        _currentSampleIndex = 0;
        _pauseRemaining = PauseFramesAfterLanding;
        _currentTrialIndex++;

        if (_currentTrialIndex >= _results.Count)
            IsComplete = true;
    }

    public TrajectorySample? GetActiveDronePosition()
    {
        if (IsComplete || _pauseRemaining > 0 || _currentTrialIndex >= _results.Count)
            return null;

        var samples = _results[_currentTrialIndex].Trajectory.Samples;
        return samples[Math.Min(_currentSampleIndex, samples.Count - 1)];
    }

    public IReadOnlyList<TrajectorySample> GetActiveDroneTrail()
    {
        if (IsComplete || _pauseRemaining > 0 || _currentTrialIndex >= _results.Count)
            return Array.Empty<TrajectorySample>();

        var samples = _results[_currentTrialIndex].Trajectory.Samples;
        var end = Math.Min(_currentSampleIndex, samples.Count - 1);
        var start = Math.Max(0, end - TrailSamples);
        return samples.Skip(start).Take(end - start + 1).ToList();
    }

    private static int ComputeSamplesPerFrame(int trialCount) => trialCount switch
    {
        <= 15 => 2,
        <= 40 => 4,
        <= 80 => 7,
        <= 150 => 12,
        _ => 20
    };
}
