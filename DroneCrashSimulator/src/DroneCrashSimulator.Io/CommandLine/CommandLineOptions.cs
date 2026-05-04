namespace DroneCrashSimulator.Io.CommandLine;

public sealed record CommandLineOptions(
    ExecutionMode Mode,
    int TrialCount,
    double WindSpeedMetersPerSecond,
    double AltitudeMeters,
    string OutputDirectory);
