namespace DroneCrashSimulator.Application.Batch;

public sealed record SweepConfiguration(
    double AltitudeMinMeters,
    double AltitudeMaxMeters,
    int AltitudeLevelCount,
    double HorizontalSpeedMaxMetersPerSecond,
    int HorizontalSpeedLevelCount,
    double VerticalSpeedMinMetersPerSecond,
    double VerticalSpeedMaxMetersPerSecond,
    int VerticalSpeedLevelCount,
    int TrialsPerCombination,
    double MaxWindSpeedMetersPerSecond)
{
    public IReadOnlyList<double> AltitudeLevels => GenerateLevels(
        AltitudeMinMeters, AltitudeMaxMeters, AltitudeLevelCount);

    public IReadOnlyList<double> HorizontalSpeedLevels => GenerateLevels(
        0.0, HorizontalSpeedMaxMetersPerSecond, HorizontalSpeedLevelCount);

    public IReadOnlyList<double> VerticalSpeedLevels => GenerateLevels(
        VerticalSpeedMinMetersPerSecond, VerticalSpeedMaxMetersPerSecond, VerticalSpeedLevelCount);

    public int TotalCombinations =>
        AltitudeLevelCount * HorizontalSpeedLevelCount * VerticalSpeedLevelCount;

    public int TotalTrials => TotalCombinations * TrialsPerCombination;

    private static IReadOnlyList<double> GenerateLevels(double min, double max, int count)
    {
        if (count <= 1) return [min];
        var step = (max - min) / (count - 1);
        return Enumerable.Range(0, count).Select(i => min + i * step).ToList();
    }
}
