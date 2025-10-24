using System.CommandLine;
using WindowsSearchConfigurator.Core.Interfaces;
using WindowsSearchConfigurator.Services;
using WindowsSearchConfigurator.Utilities;

namespace WindowsSearchConfigurator.Commands;

/// <summary>
/// Handles the 'export' command to export configuration to JSON file.
/// </summary>
public static class ExportCommand
{
    /// <summary>
    /// Creates the 'export' command with its options.
    /// </summary>
    /// <param name="configurationStore">The configuration store service.</param>
    /// <param name="auditLogger">The audit logger service.</param>
    /// <param name="verboseLogger">The verbose logger service.</param>
    /// <returns>The configured command.</returns>
    public static Command Create(
        IConfigurationStore configurationStore,
        IAuditLogger auditLogger,
        VerboseLogger verboseLogger)
    {
        var command = new Command("export", "Export current configuration to JSON file");

        // Required argument
        var fileArgument = new Argument<string>(
            "file",
            "Path to the output JSON file");

        // Options
        var includeDefaultsOption = new Option<bool>(
            new[] { "--include-defaults", "-d" },
            getDefaultValue: () => false,
            description: "Include Windows system default rules in export");

        var includeExtensionsOption = new Option<bool>(
            new[] { "--include-extensions", "-e" },
            getDefaultValue: () => true,
            description: "Include file extension settings in export");

        var overwriteOption = new Option<bool>(
            new[] { "--overwrite", "-o" },
            getDefaultValue: () => false,
            description: "Overwrite existing file without prompting");

        command.AddArgument(fileArgument);
        command.AddOption(includeDefaultsOption);
        command.AddOption(includeExtensionsOption);
        command.AddOption(overwriteOption);

        command.SetHandler(async (file, includeDefaults, includeExtensions, overwrite) =>
        {
            await ExecuteAsync(
                configurationStore,
                auditLogger,
                verboseLogger,
                file,
                includeDefaults,
                includeExtensions,
                overwrite);
        }, fileArgument, includeDefaultsOption, includeExtensionsOption, overwriteOption);

        return command;
    }

    /// <summary>
    /// Executes the export command.
    /// </summary>
    private static async Task ExecuteAsync(
        IConfigurationStore configurationStore,
        IAuditLogger auditLogger,
        VerboseLogger verboseLogger,
        string filePath,
        bool includeDefaults,
        bool includeExtensions,
        bool overwrite)
    {
        try
        {
            verboseLogger.WriteLine("ExportCommand: Executing export command");
            verboseLogger.WriteOperation("ExportCommand", $"File: {filePath}, IncludeDefaults: {includeDefaults}, IncludeExtensions: {includeExtensions}, Overwrite: {overwrite}");

            // Check if file exists and prompt if overwrite not specified
            verboseLogger.WriteLine($"ExportCommand: Checking if file exists: {filePath}");
            if (File.Exists(filePath) && !overwrite)
            {
                verboseLogger.WriteLine("ExportCommand: File exists, prompting for overwrite confirmation");
                Console.WriteLine($"Warning: File '{filePath}' already exists.");
                Console.Write("Overwrite? (y/N): ");
                
                var response = Console.ReadLine()?.Trim().ToLowerInvariant();
                verboseLogger.WriteLine($"ExportCommand: User response: {response}");
                if (response != "y" && response != "yes")
                {
                    verboseLogger.WriteLine("ExportCommand: User cancelled operation");
                    Console.WriteLine("Operation cancelled.");
                    Environment.Exit(0);
                    return;
                }
                verboseLogger.WriteLine("ExportCommand: User confirmed overwrite");
                Console.WriteLine();
            }

            Console.WriteLine($"Exporting configuration to '{filePath}'...");
            Console.WriteLine($"  Include defaults: {includeDefaults}");
            Console.WriteLine($"  Include extensions: {includeExtensions}");
            Console.WriteLine();

            // Export configuration (T082)
            verboseLogger.WriteOperation("ExportCommand", $"Exporting configuration to: {filePath}");
            var result = await configurationStore.ExportAsync(
                filePath,
                includeDefaults,
                includeExtensions);

            if (result.Success)
            {
                verboseLogger.WriteLine($"ExportCommand: Successfully exported to {filePath}");
                Console.WriteLine($"✓ Successfully exported configuration to '{filePath}'");
                
                // Show file size
                var fileInfo = new FileInfo(filePath);
                verboseLogger.WriteLine($"ExportCommand: File size: {fileInfo.Length} bytes");
                Console.WriteLine($"  File size: {fileInfo.Length:N0} bytes");
                
                // Audit logging (T089) is already done in ConfigurationStore
                Environment.Exit(0); // Exit code 0: Success
            }
            else
            {
                verboseLogger.WriteError($"ExportCommand failed: {result.Message}");
                Console.Error.WriteLine($"Error: {result.Message}");
                Environment.Exit(2); // Exit code 2: Operation failed
            }
        }
        catch (Exception ex)
        {
            verboseLogger.WriteException(ex);
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            auditLogger.LogError($"Export command failed with exception: {ex.Message}", ex);
            Environment.Exit(1); // Exit code 1: General error
        }
    }
}
