using System.CommandLine;
using WindowsSearchConfigurator.Core.Interfaces;
using WindowsSearchConfigurator.Services;
using WindowsSearchConfigurator.Utilities;

namespace WindowsSearchConfigurator.Commands;

/// <summary>
/// Handles the 'import' command to import configuration from JSON file.
/// </summary>
public static class ImportCommand
{
    /// <summary>
    /// Creates the 'import' command with its options.
    /// </summary>
    /// <param name="configurationStore">The configuration store service.</param>
    /// <param name="privilegeChecker">The privilege checker service.</param>
    /// <param name="auditLogger">The audit logger service.</param>
    /// <param name="verboseLogger">The verbose logger service.</param>
    /// <returns>The configured command.</returns>
    public static Command Create(
        IConfigurationStore configurationStore,
        IPrivilegeChecker privilegeChecker,
        IAuditLogger auditLogger,
        VerboseLogger verboseLogger)
    {
        var command = new Command("import", "Import configuration from JSON file");

        // Required argument
        var fileArgument = new Argument<string>(
            "file",
            "Path to the input JSON file");

        // Options
        var mergeOption = new Option<bool>(
            new[] { "--merge", "-m" },
            getDefaultValue: () => false,
            description: "Merge with existing rules instead of replacing");

        var continueOnErrorOption = new Option<bool>(
            new[] { "--continue-on-error", "-c" },
            getDefaultValue: () => false,
            description: "Continue importing even if individual rules fail");

        var dryRunOption = new Option<bool>(
            new[] { "--dry-run", "-n" },
            getDefaultValue: () => false,
            description: "Validate configuration without applying changes");

        command.AddArgument(fileArgument);
        command.AddOption(mergeOption);
        command.AddOption(continueOnErrorOption);
        command.AddOption(dryRunOption);

        command.SetHandler(async (file, merge, continueOnError, dryRun) =>
        {
            await ExecuteAsync(
                configurationStore,
                privilegeChecker,
                auditLogger,
                verboseLogger,
                file,
                merge,
                continueOnError,
                dryRun);
        }, fileArgument, mergeOption, continueOnErrorOption, dryRunOption);

        return command;
    }

    /// <summary>
    /// Executes the import command.
    /// </summary>
    private static async Task ExecuteAsync(
        IConfigurationStore configurationStore,
        IPrivilegeChecker privilegeChecker,
        IAuditLogger auditLogger,
        VerboseLogger verboseLogger,
        string filePath,
        bool merge,
        bool continueOnError,
        bool dryRun)
    {
        try
        {
            verboseLogger.WriteLine("ImportCommand: Executing import command");
            verboseLogger.WriteOperation("ImportCommand", $"File: {filePath}, Merge: {merge}, ContinueOnError: {continueOnError}, DryRun: {dryRun}");

            // Check for administrator privileges (T086) - unless dry-run
            if (!dryRun)
            {
                verboseLogger.WriteLine("ImportCommand: Checking administrator privileges");
            }
            if (!dryRun && !privilegeChecker.IsAdministrator())
            {
                verboseLogger.WriteError("Import command failed: Insufficient privileges");
                Console.Error.WriteLine("Error: Administrator privileges required to import configuration.");
                Console.Error.WriteLine("Please restart the application as administrator.");
                Console.Error.WriteLine("(Use --dry-run to validate without applying changes)");
                auditLogger.LogError("Import command failed: Insufficient privileges");
                Environment.Exit(4); // Exit code 4: Insufficient privileges
                return;
            }
            if (!dryRun)
            {
                verboseLogger.WriteLine("ImportCommand: Privilege check passed");
            }

            // Check if file exists
            verboseLogger.WriteLine($"ImportCommand: Checking file existence: {filePath}");
            if (!File.Exists(filePath))
            {
                verboseLogger.WriteError($"File not found: {filePath}");
                Console.Error.WriteLine($"Error: Configuration file not found: '{filePath}'");
                Environment.Exit(3); // Exit code 3: Invalid input
                return;
            }
            verboseLogger.WriteLine("ImportCommand: File exists");

            // Dry-run mode (T088) - validate only
            if (dryRun)
            {
                verboseLogger.WriteLine("ImportCommand: Running in dry-run mode (validation only)");
                Console.WriteLine($"Validating configuration file: {filePath}");
                Console.WriteLine("(Dry-run mode - no changes will be applied)");
                Console.WriteLine();

                verboseLogger.WriteOperation("ImportCommand", "Validating configuration file");
                var validateResult = await configurationStore.ValidateAsync(filePath);
                
                if (validateResult.Success && validateResult.Value != null)
                {
                    if (validateResult.Value.IsValid)
                    {
                        verboseLogger.WriteLine("ImportCommand: Validation passed");
                        Console.WriteLine("✓ Configuration file is valid");
                        Console.WriteLine($"  Path: {validateResult.Value.NormalizedValue}");
                        Environment.Exit(0);
                    }
                    else
                    {
                        verboseLogger.WriteError($"Validation failed: {validateResult.Value.ErrorMessage}");
                        Console.Error.WriteLine("✗ Configuration file has validation errors:");
                        Console.Error.WriteLine($"  {validateResult.Value.ErrorMessage}");
                        Environment.Exit(3); // Exit code 3: Invalid input
                    }
                }
                else
                {
                    verboseLogger.WriteError($"Validation error: {validateResult.Message}");
                    Console.Error.WriteLine($"Error: {validateResult.Message}");
                    Environment.Exit(2); // Exit code 2: Operation failed
                }
                return;
            }

            // Display import summary
            verboseLogger.WriteLine("ImportCommand: Displaying import summary");
            Console.WriteLine($"Importing configuration from '{filePath}'...");
            Console.WriteLine($"  Merge mode: {merge}");
            Console.WriteLine($"  Continue on error: {continueOnError}");
            Console.WriteLine();

            if (!merge)
            {
                verboseLogger.WriteLine("ImportCommand: Replace mode - prompting for confirmation");
                Console.WriteLine("Warning: This will replace all existing user-defined rules.");
                Console.Write("Continue? (y/N): ");
                
                var response = Console.ReadLine()?.Trim().ToLowerInvariant();
                verboseLogger.WriteLine($"ImportCommand: User response: {response}");
                if (response != "y" && response != "yes")
                {
                    verboseLogger.WriteLine("ImportCommand: User cancelled operation");
                    Console.WriteLine("Operation cancelled.");
                    Environment.Exit(0);
                    return;
                }
                verboseLogger.WriteLine("ImportCommand: User confirmed import");
                Console.WriteLine();
            }

            // Import configuration (T083, T084, T085)
            verboseLogger.WriteOperation("ImportCommand", $"Importing configuration from: {filePath}");
            var result = await configurationStore.ImportAsync(
                filePath,
                merge,
                continueOnError);

            if (result.Success && result.Value != null)
            {
                var importResult = result.Value;
                verboseLogger.WriteLine($"ImportCommand: Import completed - Rules: {importResult.RulesImported} imported, {importResult.RulesFailed} failed; Extensions: {importResult.ExtensionsImported} imported, {importResult.ExtensionsFailed} failed");
                
                Console.WriteLine("Import completed:");
                Console.WriteLine($"  Rules imported:    {importResult.RulesImported}");
                Console.WriteLine($"  Rules failed:      {importResult.RulesFailed}");
                Console.WriteLine($"  Extensions imported: {importResult.ExtensionsImported}");
                Console.WriteLine($"  Extensions failed:   {importResult.ExtensionsFailed}");
                
                if (importResult.Errors.Count > 0)
                {
                    verboseLogger.WriteLine($"ImportCommand: {importResult.Errors.Count} errors encountered during import");
                    foreach (var error in importResult.Errors)
                    {
                        verboseLogger.WriteError($"Import error: {error}");
                    }
                    Console.WriteLine();
                    Console.WriteLine("Errors encountered:");
                    foreach (var error in importResult.Errors)
                    {
                        Console.WriteLine($"  - {error}");
                    }
                }

                if (importResult.RulesFailed > 0 || importResult.ExtensionsFailed > 0)
                {
                    verboseLogger.WriteError("Import completed with errors");
                    Console.WriteLine();
                    Console.WriteLine("⚠ Import completed with errors");
                    
                    // Audit logging (T089) is already done in ConfigurationStore
                    Environment.Exit(2); // Exit code 2: Operation failed (partial)
                }
                else
                {
                    verboseLogger.WriteLine("ImportCommand: All configuration imported successfully");
                    Console.WriteLine();
                    Console.WriteLine("✓ Successfully imported all configuration");
                    
                    // Audit logging (T089) is already done in ConfigurationStore
                    Environment.Exit(0); // Exit code 0: Success
                }
            }
            else
            {
                verboseLogger.WriteError($"Import failed: {result.Message}");
                Console.Error.WriteLine($"Error: {result.Message}");
                
                // Show partial results if available
                if (result.Value != null)
                {
                    var importResult = result.Value;
                    verboseLogger.WriteLine($"ImportCommand: Partial results - Rules imported: {importResult.RulesImported}, Rules failed: {importResult.RulesFailed}");
                    Console.Error.WriteLine($"  Rules imported before error: {importResult.RulesImported}");
                    Console.Error.WriteLine($"  Rules failed: {importResult.RulesFailed}");
                }
                
                Environment.Exit(2); // Exit code 2: Operation failed
            }
        }
        catch (Exception ex)
        {
            verboseLogger.WriteException(ex);
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            auditLogger.LogError($"Import command failed with exception: {ex.Message}", ex);
            Environment.Exit(1); // Exit code 1: General error
        }
    }
}
