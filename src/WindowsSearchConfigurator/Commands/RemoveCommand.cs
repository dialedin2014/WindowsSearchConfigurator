using System.CommandLine;
using WindowsSearchConfigurator.Core.Interfaces;
using WindowsSearchConfigurator.Services;
using WindowsSearchConfigurator.Utilities;

namespace WindowsSearchConfigurator.Commands;

/// <summary>
/// Handles the 'remove' command to remove folders from the Windows Search index.
/// </summary>
public static class RemoveCommand
{
    /// <summary>
    /// Creates the 'remove' command with its options.
    /// </summary>
    /// <param name="searchIndexManager">The search index manager service.</param>
    /// <param name="privilegeChecker">The privilege checker service.</param>
    /// <param name="pathValidator">The path validator service.</param>
    /// <param name="auditLogger">The audit logger service.</param>
    /// <param name="verboseLogger">The verbose logger service.</param>
    /// <returns>The configured command.</returns>
    public static Command Create(
        ISearchIndexManager searchIndexManager,
        IPrivilegeChecker privilegeChecker,
        PathValidator pathValidator,
        IAuditLogger auditLogger,
        VerboseLogger verboseLogger)
    {
        var command = new Command("remove", "Remove a location from the Windows Search index");

        // Required argument
        var pathArgument = new Argument<string>(
            "path",
            "Path to remove from the index");

        // Options
        var forceOption = new Option<bool>(
            new[] { "--force", "-f", "--no-confirm" },
            getDefaultValue: () => false,
            description: "Skip confirmation prompt");

        command.AddArgument(pathArgument);
        command.AddOption(forceOption);

        command.SetHandler(async (path, force) =>
        {
            await ExecuteAsync(
                searchIndexManager,
                privilegeChecker,
                pathValidator,
                auditLogger,
                verboseLogger,
                path,
                force);
        }, pathArgument, forceOption);

        return command;
    }

    /// <summary>
    /// Executes the remove command.
    /// </summary>
    private static async Task ExecuteAsync(
        ISearchIndexManager searchIndexManager,
        IPrivilegeChecker privilegeChecker,
        PathValidator pathValidator,
        IAuditLogger auditLogger,
        VerboseLogger verboseLogger,
        string path,
        bool force)
    {
        try
        {
            verboseLogger.WriteLine("RemoveCommand: Executing remove command");
            verboseLogger.WriteOperation("RemoveCommand", $"Path: {path}, Force: {force}");

            // Check for administrator privileges (T059)
            verboseLogger.WriteLine("RemoveCommand: Checking administrator privileges");
            if (!privilegeChecker.IsAdministrator())
            {
                verboseLogger.WriteError("Remove command failed: Insufficient privileges");
                Console.Error.WriteLine("Error: Administrator privileges required to remove index rules.");
                Console.Error.WriteLine("Please restart the application as administrator.");
                auditLogger.LogError("Remove command failed: Insufficient privileges");
                Environment.Exit(4); // Exit code 4: Insufficient privileges
                return;
            }
            verboseLogger.WriteLine("RemoveCommand: Privilege check passed");

            // Validate and normalize the path
            verboseLogger.WriteLine($"RemoveCommand: Validating path: {path}");
            var validation = pathValidator.ValidatePath(path);
            if (!validation.IsValid)
            {
                verboseLogger.WriteError($"Path validation failed: {validation.ErrorMessage}");
                Console.Error.WriteLine($"Error: {validation.ErrorMessage}");
                auditLogger.LogError($"Remove command failed: Invalid path '{path}' - {validation.ErrorMessage}");
                Environment.Exit(3); // Exit code 3: Invalid input
                return;
            }

            var normalizedPath = validation.NormalizedValue!;
            verboseLogger.WriteLine($"RemoveCommand: Path normalized to: {normalizedPath}");

            // Confirmation prompt (T060) - skippable with --force
            if (!force)
            {
                Console.WriteLine($"Are you sure you want to remove the index rule for:");
                Console.WriteLine($"  {normalizedPath}");
                Console.WriteLine();
                Console.WriteLine("This will stop Windows Search from indexing this location.");
                Console.Write("Continue? (y/N): ");
                
                var response = Console.ReadLine()?.Trim().ToLowerInvariant();
                if (response != "y" && response != "yes")
                {
                    Console.WriteLine("Operation cancelled.");
                    Environment.Exit(0);
                    return;
                }
                Console.WriteLine();
            }

            // Remove the rule (T061: error handling for non-existent rule is in SearchIndexManager)
            verboseLogger.WriteOperation("RemoveCommand", $"Removing index rule for: {normalizedPath}");
            var result = await searchIndexManager.RemoveIndexRuleAsync(normalizedPath);

            if (result.Success)
            {
                verboseLogger.WriteLine($"RemoveCommand: Successfully removed index rule for '{normalizedPath}'");
                Console.WriteLine($"✓ Successfully removed index rule for '{normalizedPath}'");
                Console.WriteLine("Windows Search will no longer index this location.");
                
                // Audit logging (T062) is already done in SearchIndexManager
                Environment.Exit(0); // Exit code 0: Success
            }
            else
            {
                verboseLogger.WriteError($"RemoveCommand failed: {result.Message}");
                Console.Error.WriteLine($"Error: {result.Message}");
                Environment.Exit(2); // Exit code 2: Operation failed
            }
        }
        catch (Exception ex)
        {
            verboseLogger.WriteException(ex);
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            auditLogger.LogError($"Remove command failed with exception: {ex.Message}", ex);
            Environment.Exit(1); // Exit code 1: General error
        }
    }
}
