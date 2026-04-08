namespace Snap.CLI.Commands;

[Command("info", Description = "Show pack information")]
public sealed class InfoCommand : PackerCommandBase
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

            using var reader = new PackReader(PackFile, key);

            var files = reader.GetAllFiles();
            ulong totalCompressed = 0;
            ulong totalUncompressed = 0;
            int compressedCount = 0;

            foreach (var file in files)
            {
                var info = reader.GetFileInfo(file);
                if (info.HasValue)
                {
                    var entry = info.Value;
                    totalCompressed += entry.CompressedSize;
                    totalUncompressed += entry.UncompressedSize;
                    if (entry.IsCompressed)
                        compressedCount++;
                }
            }

            var fileInfo = new FileInfo(PackFile);

            console.Output.WriteLine(new string('=', 50));
            console.Output.WriteLine($"Pack File: {Path.GetFileName(PackFile)}");
            console.Output.WriteLine(new string('-', 50));
            console.Output.WriteLine($"File size: {FormatSize((ulong)fileInfo.Length)}");
            console.Output.WriteLine($"Total files: {files.Count}");
            console.Output.WriteLine($"Compressed files: {compressedCount}");
            console.Output.WriteLine($"Uncompressed files: {files.Count - compressedCount}");
            console.Output.WriteLine($"Total compressed data: {FormatSize(totalCompressed)}");
            console.Output.WriteLine($"Total uncompressed data: {FormatSize(totalUncompressed)}");

            if (totalUncompressed > 0)
            {
                double ratio = (1 - (double)totalCompressed / totalUncompressed) * 100;
                console.Output.WriteLine($"Compression ratio: {ratio:F1}%");
                console.Output.WriteLine($"Space saved: {FormatSize(totalUncompressed - totalCompressed)}");
            }

            console.Output.WriteLine(new string('=', 50));

            Environment.Exit((int)ExitCode.Success);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("encrypted"))
        {
            LogError("Pack is encrypted. Provide key with --key-file to see full info");
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

    private static string FormatSize(ulong bytes)
    {
        string[] sizes = ["B", "KB", "MB", "GB"];
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
