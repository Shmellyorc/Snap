using System.Text;

namespace Snap.Engine.Logs;

/// <summary>
/// File-based log sink with rolling by size and date-based filenames that auto-roll daily.
/// </summary>
public class FileLogSink : ILogSink, IDisposable
{
    private readonly string _logDirectory;
    private readonly long _maxFileSizeBytes;
    private readonly int _maxRollFiles;
    private long _currentSize;
    private StreamWriter _writer;

    private DateTime _currentDate;
    private string _fileBaseName;

    public FileLogSink(string logDirectory, long maxFileSizeBytes, int maxRollFiles)
    {
        _logDirectory = logDirectory;
        _maxFileSizeBytes = maxFileSizeBytes;
        _maxRollFiles = maxRollFiles;

        Directory.CreateDirectory(_logDirectory);
        _currentDate = DateTime.Now.Date;
        _fileBaseName = _currentDate.ToString("ddd-MMM-yyyy");
        OpenWriter();
    }

    private void OpenWriter()
    {
        if (_writer != null)
        {
            try { _writer.Flush(); } catch { /* swallow flush errors */ }
            try { _writer.Dispose(); } catch { /* swallow dispose errors */ }
        }
        string path = GetBasePath();
        _currentSize = File.Exists(path) ? new FileInfo(path).Length : 0;
        var fs = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read);
        _writer = new StreamWriter(fs, Encoding.Default) { AutoFlush = true };
    }

    private string GetBasePath() => Path.Combine(_logDirectory, _fileBaseName + ".log");
    private string GetRolledPath(int idx) => Path.Combine(_logDirectory, $"{_fileBaseName}-{idx}.log");

    private void CheckDateRollover()
    {
        var today = DateTime.Now.Date;
        if (today != _currentDate)
        {
            // Safely close old writer
            try { _writer.Close(); } catch { /* swallow close errors */ }
            try { _writer.Dispose(); } catch { /* swallow dispose errors */ }

            _currentDate = today;
            _fileBaseName = _currentDate.ToString("ddd-MMM-yyyy");
            OpenWriter();
        }
    }

    public void Write(char value)
    {
        CheckDateRollover();
        RotateIfNeeded(1);
        _writer.Write(value);
        _currentSize++;
    }

    public void Write(string text)
    {
        CheckDateRollover();
        var bytes = Encoding.Default.GetByteCount(text);
        RotateIfNeeded(bytes);
        _writer.Write(text);
        _currentSize += bytes;
    }

    public void WriteLine(string text)
    {
        CheckDateRollover();
        var line = text + Environment.NewLine;
        var bytes = Encoding.Default.GetByteCount(line);
        RotateIfNeeded(bytes);
        _writer.WriteLine(text);
        _currentSize += bytes;
    }

    public void Flush() => _writer.Flush();

    public void RotateIfNeeded(long upcomingBytes)
    {
        if (_currentSize + upcomingBytes <= _maxFileSizeBytes)
            return;

        // Safely close and dispose current writer
        try { _writer.Close(); } catch { /* swallow close errors */ }
        try { _writer.Dispose(); } catch { /* swallow dispose errors */ }

        for (int i = _maxRollFiles; i >= 1; i--)
        {
            string dst = GetRolledPath(i);
            string src = (i == 1) ? GetBasePath() : GetRolledPath(i - 1);
            try { if (File.Exists(dst)) File.Delete(dst); } catch { /* swallow delete errors */ }
            try { if (File.Exists(src)) File.Move(src, dst); } catch { /* swallow move errors */ }
        }

        OpenWriter();
    }

    public void Dispose()
    {
        try { _writer?.Flush(); } catch { }
        try { _writer?.Dispose(); } catch { }
    }
}