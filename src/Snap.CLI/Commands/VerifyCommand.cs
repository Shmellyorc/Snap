namespace Snap.CLI.Commands;

[Command("verify", Description = "Verify pack integrity")]
public sealed class VerifyCommand : PackerCommandBase
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
            
            Log($"Verifying pack: {PackFile}");
            
            using var reader = new PackReader(PackFile, key);
            
            var files = reader.GetAllFiles();
            int verified = 0;
            int failed = 0;
            
            using var progress = new ConsoleProgressBar(console, files.Count, "Verifying files");
            
            for (int i = 0; i < files.Count; i++)
            {
                try
                {
                    var data = reader.ReadFile(files[i]);
                    verified++;
                }
                catch (InvalidDataException)
                {
                    failed++;
                    LogWarning($"CRC mismatch: {files[i]}");
                }
                catch (Exception ex)
                {
                    failed++;
                    LogWarning($"Failed to read: {files[i]} - {ex.Message}");
                }
                
                progress.Update(i + 1, files[i]);
            }
            
            console.Output.WriteLine();
            console.Output.WriteLine(new string('-', 40));
            console.Output.WriteLine($"Verified: {verified}");
            console.Output.WriteLine($"Failed: {failed}");
            
            if (failed == 0)
            {
                console.Output.WriteLine("Pack integrity: VALID");
                Environment.Exit((int)ExitCode.Success);
            }
            else
            {
                console.Output.WriteLine("Pack integrity: CORRUPT");
                Environment.Exit((int)ExitCode.PackCorrupt);
            }
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
}

