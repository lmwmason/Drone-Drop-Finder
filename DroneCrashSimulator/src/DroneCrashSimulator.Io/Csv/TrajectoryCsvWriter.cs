using DroneCrashSimulator.Domain.Trials;

namespace DroneCrashSimulator.Io.Csv;

public sealed class TrajectoryCsvWriter : IDisposable
{
    private readonly CsvWriter _csvWriter;

    public TrajectoryCsvWriter(string filePath)
    {
        _csvWriter = new CsvWriter(filePath);
        _csvWriter.WriteHeader(["trial_index", "time_s", "x_m", "y_m", "z_m",
            "vx_mps", "vy_mps", "vz_mps"]);
    }

    public void WriteTrajectory(int trialIndex, Trajectory trajectory)
    {
        foreach (var sample in trajectory.Samples)
        {
            _csvWriter.WriteRow([
                trialIndex.ToString(),
                sample.TimeSeconds.ToString("F3"),
                sample.Position.XMeters.ToString("F4"),
                sample.Position.YMeters.ToString("F4"),
                sample.Position.ZMeters.ToString("F4"),
                sample.Velocity.XMetersPerSecond.ToString("F4"),
                sample.Velocity.YMetersPerSecond.ToString("F4"),
                sample.Velocity.ZMetersPerSecond.ToString("F4")
            ]);
        }
    }

    public void Flush() => _csvWriter.Flush();

    public void Dispose() => _csvWriter.Dispose();
}
