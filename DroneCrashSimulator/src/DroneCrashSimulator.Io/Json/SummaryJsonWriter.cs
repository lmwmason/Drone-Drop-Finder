using System.Text.Json;
using DroneCrashSimulator.Domain.Statistics;

namespace DroneCrashSimulator.Io.Json;

public sealed class SummaryJsonWriter
{
    private static readonly JsonSerializerOptions PrettyPrintOptions = new()
    {
        WriteIndented = true
    };

    public void Write(string filePath, DistributionSummary summary, ConfidenceRegion confidenceRegion)
    {
        var data = new
        {
            distribution = new
            {
                mean_x_meters = summary.MeanXMeters,
                mean_y_meters = summary.MeanYMeters,
                std_x_meters = summary.StandardDeviationXMeters,
                std_y_meters = summary.StandardDeviationYMeters,
                covariance = summary.CovarianceSquareMeters,
                median_distance_meters = summary.MedianDistanceMeters,
                p95_distance_meters = summary.Percentile95DistanceMeters,
                sample_count = summary.SampleCount
            },
            confidence_region_95 = new
            {
                center_x_meters = confidenceRegion.CenterXMeters,
                center_y_meters = confidenceRegion.CenterYMeters,
                radius_meters = confidenceRegion.RadiusMeters
            }
        };

        var json = JsonSerializer.Serialize(data, PrettyPrintOptions);
        File.WriteAllText(filePath, json);
    }
}
