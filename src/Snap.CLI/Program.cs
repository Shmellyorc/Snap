// # 1. Rebuild the CLI project
// dotnet build -c Release
//
// # 2. Pack the tool
// dotnet pack -c Release
//
// # 3. Update the globally installed tool
// dotnet tool update --global CryptEngine.CLI --version 1.0.0 --add-source ./bin/Release
//
// To re-install or install:
// dotnet tool uninstall --global CryptEngine.CLI
// dotnet tool install --global CryptEngine.CLI --add-source ./bin/Release
//
// To test:
// cryptpacker pack --input ./Content --output game.pak --encrypt --compression 9 --exclude "*.ase;*.aseprite;Maps/Map/backups/*"
// or to be more specific: cryptpacker pack --input ./Content --output game.pak --encrypt --compression 9 --exclude "*.ase;*.aseprite;*/backups/*"
//
// cryptpacker list --pack game.pak --key-file game.pak.key

public enum ExitCode
{
    Success = 0,
    GeneralError = 1,
    InvalidArguments = 2,
    FileNotFound = 3,
    PackCorrupt = 4,
    KeyMissing = 5,
    KeyInvalid = 6
}

public static class Program
{
    public static async Task<int> Main() =>
        await new CliApplicationBuilder()
            .AddCommands([
                typeof(PackCommand),
                typeof(ListCommand),
                typeof(VerifyCommand),
                typeof(ExtractCommand),
                typeof(InfoCommand),
            ])
            // .AddCommands<PackCommand, ListCommand, VerifyCommand, ExtractCommand, InfoCommand>()
            .Build()
            .RunAsync();
}
