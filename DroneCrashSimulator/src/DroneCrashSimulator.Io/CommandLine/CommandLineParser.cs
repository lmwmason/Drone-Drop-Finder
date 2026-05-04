namespace DroneCrashSimulator.Io.CommandLine;

public static class CommandLineParser
{
    private const int DefaultTrialCount = 500;
    private const double DefaultWindSpeedMetersPerSecond = 5.0;
    private const double DefaultAltitudeMeters = 100.0;
    private const string DefaultOutputDirectory = "results";

    public static CommandLineOptions Parse(string[] args)
    {
        var mode = args.Contains("--batch")
            ? ExecutionMode.HeadlessBatch
            : ExecutionMode.Interactive;

        var trialCount = ParseIntOption(args, "--trials", DefaultTrialCount);
        var windSpeed = ParseDoubleOption(args, "--wind", DefaultWindSpeedMetersPerSecond);
        var altitude = ParseDoubleOption(args, "--altitude", DefaultAltitudeMeters);
        var outputDir = ParseStringOption(args, "--output", DefaultOutputDirectory);

        return new CommandLineOptions(mode, trialCount, windSpeed, altitude, outputDir);
    }

    private static int ParseIntOption(string[] args, string flag, int defaultValue)
    {
        var index = Array.IndexOf(args, flag);
        if (index >= 0 && index + 1 < args.Length && int.TryParse(args[index + 1], out var value))
            return value;
        return defaultValue;
    }

    private static double ParseDoubleOption(string[] args, string flag, double defaultValue)
    {
        var index = Array.IndexOf(args, flag);
        if (index >= 0 && index + 1 < args.Length && double.TryParse(args[index + 1], out var value))
            return value;
        return defaultValue;
    }

    private static string ParseStringOption(string[] args, string flag, string defaultValue)
    {
        var index = Array.IndexOf(args, flag);
        if (index >= 0 && index + 1 < args.Length)
            return args[index + 1];
        return defaultValue;
    }
}
