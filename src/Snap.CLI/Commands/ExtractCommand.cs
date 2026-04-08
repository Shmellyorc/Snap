namespace Snap.CLI.Commands;

[Command("extract", Description = "Extract files from pack")]
public sealed class ExtractCommand : PackerCommandBase
{
    [CommandOption("pack", 'p', Description = "Pack file path", IsRequired = true)]
    public string PackFile { get; set; }

    [CommandOption("output", 'o', Description = "Output directory", IsRequired = true)]
    public string OutputDir { get; set; }

    [CommandOption("file", 'f', Description = "Specific file to extract (can be used multiple times)")]
    public string[] Files { get; set; }

    public override async ValueTask ExecuteAsync(IConsole console)
    {
        await Task.CompletedTask;

        try
        {
            if (!File.Exists(PackFile))
            {
                LogError($"Pack file not found: {PackFile}");
                Environment.Exit((int)ExitCode.FileNotFound);
            }

            var key = LoadKey();

            Log($"Extracting from pack: {PackFile}");
            Log($"Output directory: {OutputDir}");

            Directory.CreateDirectory(OutputDir);

            using var reader = new PackReader(PackFile, key);

            var filesToExtract = Files != null && Files.Length > 0
                ? Files.ToList()
                : reader.GetAllFiles();

            int total = filesToExtract.Count;
            int current = 0;

            using var progress = new ConsoleProgressBar(console, total, "Extracting files");

            foreach (var virtualPath in filesToExtract)
            {
                if (!reader.HasFile(virtualPath))
                {
                    LogWarning($"File not found in pack: {virtualPath}");
                    continue;
                }

                var data = reader.ReadFile(virtualPath);
                var fullPath = Path.Combine(OutputDir, virtualPath);
                var directory = Path.GetDirectoryName(fullPath);

                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);

                File.WriteAllBytes(fullPath, data);

                current++;
                progress.Update(current, virtualPath);
            }

            console.Output.WriteLine($"\nExtracted {current} files to: {OutputDir}");
            Environment.Exit((int)ExitCode.Success);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("encrypted"))
        {
            LogError("Pack is encrypted. Please provide key file with --key-file");
            Environment.Exit((int)ExitCode.KeyMissing);
        }
        catch (Exception ex)
        {
            LogError(ex.Message);

            if (Verbose)
                LogError(ex.StackTrace);

            Environment.Exit((int)ExitCode.GeneralError);
        }
    }
}

