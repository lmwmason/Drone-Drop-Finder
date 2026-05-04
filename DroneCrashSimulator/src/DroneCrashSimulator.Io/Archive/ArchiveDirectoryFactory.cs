namespace DroneCrashSimulator.Io.Archive;

public static class ArchiveDirectoryFactory
{
    public static string CreateTimestampedDirectory(string baseDirectory)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var directoryPath = Path.Combine(baseDirectory, timestamp);
        Directory.CreateDirectory(directoryPath);
        return directoryPath;
    }
}
