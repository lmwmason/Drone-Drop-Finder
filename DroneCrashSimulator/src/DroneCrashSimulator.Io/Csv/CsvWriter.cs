namespace DroneCrashSimulator.Io.Csv;

public sealed class CsvWriter : IDisposable
{
    private readonly StreamWriter _writer;

    public CsvWriter(string filePath)
    {
        _writer = new StreamWriter(filePath, append: false);
    }

    public void WriteHeader(IEnumerable<string> columns)
    {
        _writer.WriteLine(string.Join(",", columns));
    }

    public void WriteRow(IEnumerable<string> values)
    {
        _writer.WriteLine(string.Join(",", values));
    }

    public void Flush() => _writer.Flush();

    public void Dispose() => _writer.Dispose();
}
