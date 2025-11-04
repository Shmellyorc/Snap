namespace Snap.Engine.Logs;

/// <summary>
/// Console log sink: writes to standard console output.
/// </summary>
public class ConsoleLogSink : ILogSink
{
    public void Write(char value) => Console.Write(value);
    public void Write(string text) => Console.Write(text);
    public void WriteLine(string text) => Console.WriteLine(text);
    public void Flush() => Console.Out.Flush();
    public void RotateIfNeeded(long upcomingBytes) { /* no-op for console */ }
}