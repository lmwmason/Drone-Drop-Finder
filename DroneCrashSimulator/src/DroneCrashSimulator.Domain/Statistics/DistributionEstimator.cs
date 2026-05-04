using DroneCrashSimulator.Domain.Trials;

namespace DroneCrashSimulator.Domain.Statistics;

public static class DistributionEstimator
{
    public static DistributionSummary Estimate(IReadOnlyList<CrashPoint> crashPoints)
    {
        if (crashPoints.Count == 0)
            throw new ArgumentException("At least one crash point is required.", nameof(crashPoints));

        var meanX = crashPoints.Average(p => p.XMeters);
        var meanY = crashPoints.Average(p => p.YMeters);

        var varianceX = ComputeVariance(crashPoints.Select(p => p.XMeters), meanX);
        var varianceY = ComputeVariance(crashPoints.Select(p => p.YMeters), meanY);
        var covariance = ComputeCovariance(crashPoints, meanX, meanY);

        var distances = crashPoints
            .Select(p => p.DistanceFromOriginMeters)
            .OrderBy(d => d)
            .ToList();

        return new DistributionSummary(
            MeanXMeters: meanX,
            MeanYMeters: meanY,
            StandardDeviationXMeters: Math.Sqrt(varianceX),
            StandardDeviationYMeters: Math.Sqrt(varianceY),
            CovarianceSquareMeters: covariance,
            MedianDistanceMeters: ComputePercentile(distances, 0.50),
            Percentile95DistanceMeters: ComputePercentile(distances, 0.95),
            SampleCount: crashPoints.Count);
    }

    public static ConfidenceRegion ComputeConfidenceRegion(
        IReadOnlyList<CrashPoint> crashPoints,
        double confidenceLevel)
    {
        if (crashPoints.Count == 0)
            throw new ArgumentException("At least one crash point is required.", nameof(crashPoints));

        var meanX = crashPoints.Average(p => p.XMeters);
        var meanY = crashPoints.Average(p => p.YMeters);

        var distances = crashPoints
            .Select(p => Math.Sqrt(
                (p.XMeters - meanX) * (p.XMeters - meanX) +
                (p.YMeters - meanY) * (p.YMeters - meanY)))
            .OrderBy(d => d)
            .ToList();

        var radius = ComputePercentile(distances, confidenceLevel);

        return new ConfidenceRegion(
            CenterXMeters: meanX,
            CenterYMeters: meanY,
            RadiusMeters: radius,
            ConfidenceLevel: confidenceLevel);
    }

    private static double ComputeVariance(IEnumerable<double> values, double mean)
    {
        var valueList = values.ToList();
        if (valueList.Count <= 1) return 0.0;
        return valueList.Sum(v => (v - mean) * (v - mean)) / (valueList.Count - 1);
    }

    private static double ComputeCovariance(
        IReadOnlyList<CrashPoint> points,
        double meanX,
        double meanY)
    {
        if (points.Count <= 1) return 0.0;
        return points.Sum(p => (p.XMeters - meanX) * (p.YMeters - meanY)) / (points.Count - 1);
    }

    private static double ComputePercentile(IList<double> sortedValues, double percentile)
    {
        var index = percentile * (sortedValues.Count - 1);
        var lowerIndex = (int)Math.Floor(index);
        var upperIndex = (int)Math.Ceiling(index);
        var fraction = index - lowerIndex;
        return sortedValues[lowerIndex] + fraction * (sortedValues[upperIndex] - sortedValues[lowerIndex]);
    }
}
