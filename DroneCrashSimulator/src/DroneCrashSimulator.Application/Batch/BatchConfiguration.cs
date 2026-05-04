namespace DroneCrashSimulator.Application.Batch;

public sealed record BatchConfiguration(
    string OutputDirectoryPath,
    bool SaveTrajectories,
    bool SaveSummaryJson);
