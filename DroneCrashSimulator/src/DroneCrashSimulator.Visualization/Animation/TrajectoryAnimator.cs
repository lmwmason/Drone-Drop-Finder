using DroneCrashSimulator.Domain.Trials;

namespace DroneCrashSimulator.Visualization.Animation;

public sealed class TrajectoryAnimator
{
    private const int SamplesAdvancedPerFrame = 4;
    private const int TrailLengthSamples = 15;

    private readonly IReadOnlyList<TrialResult> _results;
    private int _currentSampleIndex;

    public TrajectoryAnimator(IReadOnlyList<TrialResult> results)
    {
        _results = results;
        _currentSampleIndex = 0;
    }

    public bool IsComplete { get; private set; }

    public float Progress
    {
        get
        {
            if (_results.Count == 0) return 1.0f;
            var maxSamples = _results.Max(r => r.Trajectory.Samples.Count);
            return (float)_currentSampleIndex / maxSamples;
        }
    }

    public void Advance()
    {
        if (IsComplete) return;

        _currentSampleIndex += SamplesAdvancedPerFrame;

        var maxSamples = _results.Max(r => r.Trajectory.Samples.Count);
        if (_currentSampleIndex >= maxSamples)
        {
            _currentSampleIndex = maxSamples;
            IsComplete = true;
        }
    }

    public IReadOnlyList<TrajectorySample> GetCurrentPositions()
    {
        return _results
            .Select(r => r.Trajectory.Samples[
                Math.Min(_currentSampleIndex, r.Trajectory.Samples.Count - 1)])
            .ToList();
    }

    public IReadOnlyList<IReadOnlyList<TrajectorySample>> GetTrails()
    {
        return _results
            .Select(r =>
            {
                var endIndex = Math.Min(_currentSampleIndex, r.Trajectory.Samples.Count - 1);
                var startIndex = Math.Max(0, endIndex - TrailLengthSamples);
                return (IReadOnlyList<TrajectorySample>)r.Trajectory.Samples
                    .Skip(startIndex)
                    .Take(endIndex - startIndex + 1)
                    .ToList();
            })
            .ToList();
    }
}
