namespace Snap.CLI.Commands;

public abstract class PackerCommandBase : ICommand
{
    [CommandOption("verbose", Description = "Show detailed output")]
    public bool Verbose { get; set; }

    [CommandOption("key-file", Description = "Path to key file")]
    public string KeyFile { get; set; }

	public event EventHandler CanExecuteChanged;

	protected void Log(string message)
    {
        if (Verbose)
            Console.WriteLine($"[INFO] {message}");
    }

    protected void LogError(string message)
    {
        Console.Error.WriteLine($"[ERROR] {message}");
    }

    protected void LogWarning(string message)
    {
        Console.WriteLine($"[WARN] {message}");
    }

    protected byte[] LoadKey()
    {
        if (string.IsNullOrEmpty(KeyFile))
            return null;

        if (!File.Exists(KeyFile))
        {
            LogWarning($"Key file not found: {KeyFile}");
            return null;
        }

        return File.ReadAllBytes(KeyFile);
    }

    protected void SaveKey(string outputPath, byte[] key)
    {
        string keyPath = string.IsNullOrEmpty(KeyFile)
            ? outputPath + ".key"
            : KeyFile;

        File.WriteAllBytes(keyPath, key);
        Log($"Key saved to: {keyPath}");
    }

    public abstract ValueTask ExecuteAsync(IConsole console);
}
