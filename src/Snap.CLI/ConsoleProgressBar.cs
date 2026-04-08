namespace Snap.CLI;

public sealed class ConsoleProgressBar : IDisposable
{
    private readonly IConsole _console;
    private readonly int _total;
    private int _current;
    private readonly string _message;
    private readonly int _barWidth;
    private int _lastPercent = -1;
    private string _lastFile = string.Empty;
    // private int _lastFileLength = 0;

    public ConsoleProgressBar(IConsole console, int total, string message, int barWidth = 40)
    {
        _console = console;
        _total = total;
        _message = message;
        _barWidth = barWidth;
        
        _console.Write($"{_message}: [");
        _console.Write(new string(' ', _barWidth));
        _console.Write("] 0%");
    }

    public void Update(int current, string fileName = null)
    {
        _current = current;
        double percent = _total > 0 ? (double)_current / _total : 1.0;
        int currentPercent = (int)(percent * 100);
        
        if (currentPercent == _lastPercent && fileName == _lastFile)
            return;
        
        _lastPercent = currentPercent;
        
        int filled = (int)(percent * _barWidth);
        
        // Save cursor position
        int left = _console.CursorLeft;
        int top = _console.CursorTop;
        
        // Move to start of bar
        _console.CursorLeft = _message.Length + 2;
        
        // Draw filled portion
        _console.Write(new string('█', filled));
        
        // Draw empty portion
        if (filled < _barWidth)
            _console.Write(new string('░', _barWidth - filled));
        
        // Draw percentage
        _console.Write($" {currentPercent}%");
        
        // Draw file name if provided
        if (!string.IsNullOrEmpty(fileName))
        {
            _console.Write($" - {fileName}");
            
            // Clear leftover characters from previous longer filename
            if (_lastFile.Length > fileName.Length)
            {
                int spaces = _lastFile.Length - fileName.Length;
                _console.Write(new string(' ', spaces));
            }
        }
        else if (_lastFile.Length > 0)
        {
            // Clear previous filename if no new one
            _console.Write(new string(' ', _lastFile.Length + 3));
        }
        
        _lastFile = fileName ?? string.Empty;
        
        // Restore cursor
        _console.CursorLeft = left;
        _console.CursorTop = top;
    }

    public void Complete(string message = null)
    {
        // Save cursor position
        int left = _console.CursorLeft;
        int top = _console.CursorTop;
        
        // Clear the entire line
        _console.CursorLeft = 0;
        _console.Write(new string(' ', _console.WindowWidth));
        
        // Write completion message
        _console.CursorLeft = 0;
        _console.Write(message ?? $"Added {_total} files");
        
        // Restore cursor to next line
        _console.CursorLeft = 0;
        _console.CursorTop = top + 1;
    }

    public void Dispose()
    {
        _console.WriteLine();
    }
}
