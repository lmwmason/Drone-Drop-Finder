using DroneCrashSimulator.Domain.Statistics;
using DroneCrashSimulator.Domain.Trials;

namespace DroneCrashSimulator.Application.Aggregation;

public sealed class StatisticsAggregator
{
    private const double ConfidenceLevel95 = 0.95;

    public DistributionSummary ComputeSummary(IReadOnlyList<TrialResult> results)
    {
        if (results.Count == 0)
            throw new ArgumentException("Results list must not be empty.", nameof(results));

        var crashPoints = results.Select(r => r.CrashPoint).ToList();
        return DistributionEstimator.Estimate(crashPoints);
    }

    public ConfidenceRegion ComputeConfidenceRegion95(IReadOnlyList<TrialResult> results)
    {
        if (results.Count == 0)
            throw new ArgumentException("Results list must not be empty.", nameof(results));

        var crashPoints = results.Select(r => r.CrashPoint).ToList();
        return DistributionEstimator.ComputeConfidenceRegion(crashPoints, ConfidenceLevel95);
    }
}
