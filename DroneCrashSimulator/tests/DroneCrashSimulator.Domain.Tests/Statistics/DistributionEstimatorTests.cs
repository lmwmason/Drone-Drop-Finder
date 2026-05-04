using DroneCrashSimulator.Domain.Statistics;
using DroneCrashSimulator.Domain.Trials;

namespace DroneCrashSimulator.Domain.Tests.Statistics;

public sealed class DistributionEstimatorTests
{
    [Fact]
    public void Estimate_WithSymmetricPoints_ProducesMeanNearOrigin()
    {
        var crashPoints = new[]
        {
            new CrashPoint(10.0, 5.0, Math.Sqrt(125), 0),
            new CrashPoint(-10.0, -5.0, Math.Sqrt(125), 0),
            new CrashPoint(10.0, -5.0, Math.Sqrt(125), 0),
            new CrashPoint(-10.0, 5.0, Math.Sqrt(125), 0)
        };

        var summary = DistributionEstimator.Estimate(crashPoints);

        Assert.Equal(0.0, summary.MeanXMeters, precision: 10);
        Assert.Equal(0.0, summary.MeanYMeters, precision: 10);
    }

    [Fact]
    public void Estimate_WithOffsetPoints_ProducesMeanAtOffset()
    {
        var crashPoints = Enumerable.Range(0, 100)
            .Select(_ => new CrashPoint(15.0, 20.0, 25.0, 0))
            .ToList();

        var summary = DistributionEstimator.Estimate(crashPoints);

        Assert.Equal(15.0, summary.MeanXMeters, precision: 6);
        Assert.Equal(20.0, summary.MeanYMeters, precision: 6);
    }

    [Fact]
    public void Estimate_WithSinglePoint_ProducesZeroStdDev()
    {
        var crashPoints = new[] { new CrashPoint(5.0, 3.0, Math.Sqrt(34), 0) };

        var summary = DistributionEstimator.Estimate(crashPoints);

        Assert.Equal(0.0, summary.StandardDeviationXMeters);
        Assert.Equal(0.0, summary.StandardDeviationYMeters);
    }

    [Fact]
    public void Estimate_WithEmptyList_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            DistributionEstimator.Estimate(Array.Empty<CrashPoint>()));
    }

    [Fact]
    public void ComputeConfidenceRegion_P95RadiusContains95PercentOfPoints()
    {
        const int totalPoints = 1000;
        var random = new Random(42);
        var crashPoints = Enumerable.Range(0, totalPoints)
            .Select(_ => new CrashPoint(
                random.NextGaussian(0, 10),
                random.NextGaussian(0, 10),
                0, 0))
            .ToList();

        var region = DistributionEstimator.ComputeConfidenceRegion(crashPoints, 0.95);

        var pointsInside = crashPoints.Count(p =>
            Math.Sqrt(
                (p.XMeters - region.CenterXMeters) * (p.XMeters - region.CenterXMeters) +
                (p.YMeters - region.CenterYMeters) * (p.YMeters - region.CenterYMeters))
            <= region.RadiusMeters);

        var fractionInside = (double)pointsInside / totalPoints;
        Assert.True(fractionInside >= 0.93 && fractionInside <= 0.97);
    }
}

file static class RandomExtensions
{
    public static double NextGaussian(this Random random, double mean, double stdDev)
    {
        var u1 = 1.0 - random.NextDouble();
        var u2 = 1.0 - random.NextDouble();
        var normal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
        return mean + stdDev * normal;
    }
}
