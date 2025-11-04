using System.Diagnostics;
using System.Text;

namespace Snap.Engine.Logs;

public enum LogLevel
{
    Info,
    Warning,
    Error,
}

/// <summary>
/// Central logger that fans out entries to multiple sinks and provides utility methods.
/// </summary>
public sealed class Logger : TextWriter, IDisposable
{
    private readonly List<ILogSink> _sinks = new List<ILogSink>();
    private readonly Queue<string> _recentEntries;
    private readonly int _maxRecentEntries;
    private readonly object _syncLock = new object();
    private bool _disposed;

    public static Logger Instance { get; private set; }
    public override Encoding Encoding => Encoding.Default;
    public LogLevel Level { get; set; }

    public Logger(LogLevel minLevel = LogLevel.Info,
                  int maxRecentEntries = 100)
    {
        if (Instance != null)
            throw new InvalidOperationException("Logger already initialized.");
        Instance = this;
        Level = minLevel;
        _maxRecentEntries = maxRecentEntries;
        _recentEntries = new Queue<string>(maxRecentEntries);
    }

    /// <summary>Adds a log sink (e.g. console, file).</summary>
    public void AddSink(ILogSink sink)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(Logger));
        lock (_syncLock) { _sinks.Add(sink); }
    }

    public override void Write(char value)
    {
        if (_disposed) return;
        lock (_syncLock)
        {
            foreach (var s in _sinks) { s.RotateIfNeeded(1); s.Write(value); }
        }
    }

    public override void Write(string value)
    {
        if (_disposed) return;
        var bytes = Encoding.Default.GetByteCount(value);
        lock (_syncLock)
        {
            foreach (var s in _sinks) { s.RotateIfNeeded(bytes); s.Write(value); }
        }
    }

    public override void WriteLine(string value)
    {
        if (_disposed) return;
        var line = value;
        var bytes = Encoding.Default.GetByteCount(line + Environment.NewLine);
        lock (_syncLock)
        {
            foreach (var s in _sinks) { s.RotateIfNeeded(bytes); s.WriteLine(line); }
            _recentEntries.Enqueue(line);
            if (_recentEntries.Count > _maxRecentEntries) _recentEntries.Dequeue();
        }
    }

    public override void Flush()
    {
        if (_disposed) return;
        lock (_syncLock)
        { foreach (var s in _sinks) s.Flush(); }
    }

    public void Log(LogLevel level, string message)
    {
        if (_disposed || level < Level) return;
        string prefix = level switch { LogLevel.Warning => "[⚠️ WARNING]", LogLevel.Error => "[❌ ERROR]", _ => "[INFO]" };
        string entry = $"{DateTime.Now:HH:mm:ss} {prefix}: {message}";
        WriteLine(entry);
        Debug.WriteLine(entry);
    }

    public void LogException(Exception ex, LogLevel level = LogLevel.Error)
    {
        if (_disposed || level < Level) return;
        var seen = new HashSet<Exception>();
        var current = ex;
        while (current != null && !seen.Contains(current))
        {
            seen.Add(current);
            Log(level, $"Exception: {current.Message}");
            if (!string.IsNullOrEmpty(current.StackTrace)) Log(level, current.StackTrace);
            current = current.InnerException;
        }
    }

    public void LogFields(LogLevel level, params (string Key, object Value)[] fields)
    {
        if (_disposed || level < Level) return;
        var sb = new StringBuilder();
        for (int i = 0; i < fields.Length; i++)
        {
            var (Key, Value) = fields[i]; sb.Append(Key).Append('=').Append(Value);
            if (i < fields.Length - 1) sb.Append(' ');
        }
        Log(level, sb.ToString());
    }

    public string[] GetRecentEntries()
    {
        lock (_syncLock) { return _recentEntries.ToArray(); }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_disposed)
        {
            lock (_syncLock)
            { foreach (var s in _sinks) if (s is IDisposable d) d.Dispose(); _sinks.Clear(); _disposed = true; Instance = null; }
        }
        base.Dispose(disposing);
    }

    public void Dispose() => Dispose(true);
}