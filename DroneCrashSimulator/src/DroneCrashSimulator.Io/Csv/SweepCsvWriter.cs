using DroneCrashSimulator.Application.Batch;

namespace DroneCrashSimulator.Io.Csv;

public sealed class SweepCsvWriter : IDisposable
{
    private readonly CsvWriter _writer;

    public SweepCsvWriter(string filePath)
    {
        _writer = new CsvWriter(filePath);
        _writer.WriteHeader([
            "altitude_m",
            "h_speed_mps",
            "v_speed_mps",
            "trial_index",
            "wind_speed_mps",
            "wind_bearing_deg",
            "crash_x_m",
            "crash_y_m",
            "crash_distance_m",
            "crash_bearing_deg"
        ]);
    }

    public void WriteTrialResult(SweepTrialOutput output)
    {
        var c = output.Combination;
        var p = output.Result.CrashPoint;
        _writer.WriteRow([
            c.AltitudeMeters.ToString("F2"),
            c.HorizontalSpeedMetersPerSecond.ToString("F3"),
            c.VerticalSpeedMetersPerSecond.ToString("F3"),
            output.Result.TrialIndex.ToString(),
            output.WindSpeedMetersPerSecond.ToString("F4"),
            output.WindBearingDegrees.ToString("F2"),
            p.XMeters.ToString("F4"),
            p.YMeters.ToString("F4"),
            p.DistanceFromOriginMeters.ToString("F4"),
            p.BearingDegrees.ToString("F2")
        ]);
    }

    public void Flush() => _writer.Flush();
    public void Dispose() => _writer.Dispose();
}
