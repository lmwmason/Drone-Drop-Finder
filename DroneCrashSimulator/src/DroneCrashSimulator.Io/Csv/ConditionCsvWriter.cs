using DroneCrashSimulator.Application.Batch;

namespace DroneCrashSimulator.Io.Csv;

public sealed class ConditionCsvWriter : IDisposable
{
    private readonly CsvWriter _csvWriter;

    public ConditionCsvWriter(string filePath)
    {
        _csvWriter = new CsvWriter(filePath);
        _csvWriter.WriteHeader(["scenario_name", "drone_name", "failure_mode",
            "trial_count"]);
    }

    public void WriteScenario(Scenario scenario)
    {
        _csvWriter.WriteRow([
            scenario.Name,
            scenario.Drone.Name,
            scenario.FailureMode.Name,
            scenario.TrialCount.ToString()
        ]);
    }

    public void Dispose() => _csvWriter.Dispose();
}
