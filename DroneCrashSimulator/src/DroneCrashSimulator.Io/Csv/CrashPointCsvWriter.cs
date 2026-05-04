using DroneCrashSimulator.Domain.Flight;
using DroneCrashSimulator.Domain.Trials;

namespace DroneCrashSimulator.Io.Csv;

public sealed class CrashPointCsvWriter : IDisposable
{
    private readonly CsvWriter _csvWriter;
    private readonly GpsPosition? _originGps;

    public CrashPointCsvWriter(string filePath, GpsPosition? originGps = null)
    {
        _csvWriter = new CsvWriter(filePath);
        _originGps = originGps;

        if (originGps is not null)
            _csvWriter.WriteHeader([
                "trial_index", "seed",
                "x_meters", "y_meters", "distance_meters", "bearing_degrees",
                "crash_lat", "crash_lon"]);
        else
            _csvWriter.WriteHeader([
                "trial_index", "seed",
                "x_meters", "y_meters", "distance_meters", "bearing_degrees"]);
    }

    public void WriteResult(TrialResult result)
    {
        var p = result.CrashPoint;
        var baseColumns = new[]
        {
            result.TrialIndex.ToString(),
            result.Seed.ToString(),
            p.XMeters.ToString("F4"),
            p.YMeters.ToString("F4"),
            p.DistanceFromOriginMeters.ToString("F4"),
            p.BearingDegrees.ToString("F2")
        };

        if (_originGps is null)
        {
            _csvWriter.WriteRow(baseColumns);
            return;
        }

        var crashGps = GpsConverter.LocalOffsetToGps(p.XMeters, p.YMeters, _originGps);
        _csvWriter.WriteRow([
            ..baseColumns,
            crashGps.LatitudeDegrees.ToString("F7"),
            crashGps.LongitudeDegrees.ToString("F7")
        ]);
    }

    public void Flush() => _csvWriter.Flush();

    public void Dispose() => _csvWriter.Dispose();
}
