namespace Snap.CLI.Commands;

[Command("pack", Description = "Create a pack file")]
public sealed class PackCommand : PackerCommandBase
{
    [CommandOption("input", 'i', Description = "Input directory or file", IsRequired = true)]
    public string Input { get; set; }

    [CommandOption("output", 'o', Description = "Output pack file path", IsRequired = true)]
    public string Output { get; set; }

    [CommandOption("encrypt", Description = "Enable encryption")]
    public bool Encrypt { get; set; }

    [CommandOption("compression", Description = "Compression level (0-9)")]
    public int CompressionLevel { get; set; } = 6;

    [CommandOption("no-adaptive", Description = "Disable adaptive compression")]
    public bool NoAdaptive { get; set; }

    [CommandOption("no-embed-manifest", Description = "Don't embed manifest in pack")]
    public bool NoEmbedManifest { get; set; }

    [CommandOption("exclude", Description = "Exclude patterns (semicolon separated)")]
    public string Exclude { get; set; }

    [CommandOption("include-only", Description = "Only include patterns (semicolon separated)")]
    public string IncludeOnly { get; set; }

    public override async ValueTask ExecuteAsync(IConsole console)
    {
        await Task.CompletedTask;

        try
        {
            if (!Directory.Exists(Input) && !File.Exists(Input))
            {
                LogError($"Input not found: {Input}");
                Environment.Exit((int)ExitCode.FileNotFound);
            }

            var excludePatterns = string.IsNullOrEmpty(Exclude)
                ? Array.Empty<string>()
                : Exclude.Split(';', StringSplitOptions.RemoveEmptyEntries);

            var includePatterns = string.IsNullOrEmpty(IncludeOnly)
                ? Array.Empty<string>()
                : IncludeOnly.Split(';', StringSplitOptions.RemoveEmptyEntries);

            var compression = (CompressionLevel)CompressionLevel;

            var options = new PackOptions
            {
                Encrypt = Encrypt,
                CompressionLevel = compression,
                AdaptiveCompression = !NoAdaptive,
                EmbedManifest = !NoEmbedManifest
            };

            Log($"Packing from: {Input}");
            Log($"Output: {Output}");
            Log($"Encryption: {(Encrypt ? "Enabled" : "Disabled")}");
            Log($"Compression: {CompressionLevel}");
            Log($"Adaptive compression: {!NoAdaptive}");
            Log($"Embed manifest: {!NoEmbedManifest}");

            using var writer = new PackWriter();

            if (File.Exists(Input))
            {
                // Single file
                byte[] data = File.ReadAllBytes(Input);
                string virtualPath = Path.GetFileName(Input);
                writer.AddFile(virtualPath, data);
                Log($"Added file: {virtualPath}");
            }
            else
            {
                // Directory
                var files = GetFilesWithPatterns(Input, includePatterns, excludePatterns);
                int total = files.Count;
                int current = 0;

                using var progress = new ConsoleProgressBar(console, total, "Adding files");

                foreach (var file in files)
                {
                    string relativePath = Path.GetRelativePath(Input, file);
                    byte[] data = File.ReadAllBytes(file);
                    writer.AddFile(relativePath, data);

                    current++;
                    progress.Update(current, relativePath);
                }

                progress.Complete($"✓ Added {total} files");
            }

            Log("Writing pack file...");

            // Generate random key if encryption is enabled
            if (Encrypt)
            {
                var key = new byte[32];
                using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
                rng.GetBytes(key);
                options.Passkey = key;
                SaveKey(Output, key);
            }

            writer.Write(Output, options);

            Log($"Pack created successfully: {Output}");
            console.Output.WriteLine($"\nPack completed! Output: {Output}");

            Environment.Exit((int)ExitCode.Success);
        }
        catch (Exception ex)
        {
            LogError(ex.Message);
            if (Verbose)
                LogError(ex.StackTrace);
            Environment.Exit((int)ExitCode.GeneralError);
        }
    }

    private List<string> GetFilesWithPatterns(string directory, string[] includePatterns, string[] excludePatterns)
    {
        var allFiles = Directory.GetFiles(directory, "*", SearchOption.AllDirectories).ToList();

        if (includePatterns.Length > 0)
        {
            allFiles = allFiles.Where(file =>
                includePatterns.Any(pattern => WildcardMatch(file, pattern, directory))).ToList();
        }

        if (excludePatterns.Length > 0)
        {
            allFiles = allFiles.Where(file =>
                !excludePatterns.Any(pattern => WildcardMatch(file, pattern, directory))).ToList();
        }

        return allFiles;
    }

    private bool WildcardMatch(string filePath, string pattern, string baseDirectory)
    {
        // Get relative path from base directory
        string relativePath = Path.GetRelativePath(baseDirectory, filePath).Replace('\\', '/');
        string fileName = Path.GetFileName(filePath);

        // Convert pattern to use forward slashes
        pattern = pattern.Replace('\\', '/');

        // Handle ** recursive wildcard
        if (pattern.Contains("**"))
        {
            string patternWithoutStars = pattern.Replace("**", "");
            if (patternWithoutStars.StartsWith("/"))
                patternWithoutStars = patternWithoutStars.Substring(1);
            if (patternWithoutStars.EndsWith("/"))
                patternWithoutStars = patternWithoutStars.Substring(0, patternWithoutStars.Length - 1);

            return relativePath.Contains(patternWithoutStars, StringComparison.OrdinalIgnoreCase) ||
                   relativePath.StartsWith(patternWithoutStars, StringComparison.OrdinalIgnoreCase);
        }

        // Handle directory wildcard (e.g., "Maps/backups/*")
        if (pattern.Contains('/') && pattern.EndsWith("/*"))
        {
            string dirPattern = pattern.Substring(0, pattern.Length - 2);
            return relativePath.StartsWith(dirPattern, StringComparison.OrdinalIgnoreCase);
        }

        // Handle extension wildcard (e.g., "*.png")
        if (pattern.StartsWith("*."))
        {
            string ext = pattern.Substring(1);
            return fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase);
        }

        // Handle exact file name match
        if (!pattern.Contains('/') && !pattern.Contains('*'))
        {
            return string.Equals(fileName, pattern, StringComparison.OrdinalIgnoreCase);
        }

        // Handle partial path match (e.g., "backups/Map.ldtk")
        return relativePath.Contains(pattern, StringComparison.OrdinalIgnoreCase);
    }
}

