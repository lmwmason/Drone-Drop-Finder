namespace DroneCrashSimulator.Domain.Statistics;

public sealed record DistributionSummary(
    double MeanXMeters,
    double MeanYMeters,
    double StandardDeviationXMeters,
    double StandardDeviationYMeters,
    double CovarianceSquareMeters,
    double MedianDistanceMeters,
    double Percentile95DistanceMeters,
    int SampleCount);
