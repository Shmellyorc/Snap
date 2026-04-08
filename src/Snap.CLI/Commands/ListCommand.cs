namespace Snap.CLI.Commands;

[Command("list", Description = "List contents of a pack")]
public sealed class ListCommand : PackerCommandBase
{
    [CommandOption("pack", 'p', Description = "Pack file path", IsRequired = true)]
    public string PackFile { get; set; }

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

            Log($"Reading pack: {PackFile}");

            using var reader = new PackReader(PackFile, key);

            var files = reader.GetAllFiles();

            console.Output.WriteLine($"\nPack: {Path.GetFileName(PackFile)}");
            console.Output.WriteLine($"Total files: {files.Count}");
            console.Output.WriteLine(new string('-', 60));

            ulong totalCompressed = 0;
            ulong totalUncompressed = 0;

            foreach (var file in files)
            {
                var info = reader.GetFileInfo(file);
                if (info.HasValue)
                {
                    var entry = info.Value;
                    totalCompressed += entry.CompressedSize;
                    totalUncompressed += entry.UncompressedSize;

                    double ratio = entry.CompressedSize > 0
                        ? (1 - (double)entry.CompressedSize / entry.UncompressedSize) * 100
                        : 0;

                    string compressed = FormatSize(entry.CompressedSize);
                    string uncompressed = FormatSize(entry.UncompressedSize);
                    string ratioStr = entry.IsCompressed ? $"{ratio:F1}%" : "none";
                    string status = entry.IsCompressed ? "C" : " ";

                    console.Output.WriteLine($"[{status}] {file}");
                    console.Output.WriteLine($"      {uncompressed} -> {compressed} ({ratioStr})");
                }
            }

            console.Output.WriteLine(new string('-', 60));
            console.Output.WriteLine($"Total compressed: {FormatSize(totalCompressed)}");
            console.Output.WriteLine($"Total uncompressed: {FormatSize(totalUncompressed)}");

            if (totalUncompressed > 0)
            {
                double overallRatio = (1 - (double)totalCompressed / totalUncompressed) * 100;
                console.Output.WriteLine($"Overall savings: {overallRatio:F1}%");
            }

            Environment.Exit((int)ExitCode.Success);
        }
        catch (InvalidDataException ex)
        {
            LogError($"Pack corrupt: {ex.Message}");
            Environment.Exit((int)ExitCode.PackCorrupt);
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

    private string FormatSize(ulong bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}

