using System.CommandLine;
using WindowsSearchConfigurator.Core.Interfaces;
using WindowsSearchConfigurator.Core.Models;
using WindowsSearchConfigurator.Services;

namespace WindowsSearchConfigurator.Commands;

/// <summary>
/// Handles the 'configure-depth' command to configure indexing depth for file extensions.
/// </summary>
public static class ConfigureDepthCommand
{
    /// <summary>
    /// Creates the 'configure-depth' command with its options.
    /// </summary>
    /// <param name="searchIndexManager">The search index manager service.</param>
    /// <param name="privilegeChecker">The privilege checker service.</param>
    /// <param name="auditLogger">The audit logger service.</param>
    /// <returns>The configured command.</returns>
    public static Command Create(
        ISearchIndexManager searchIndexManager,
        IPrivilegeChecker privilegeChecker,
        IAuditLogger auditLogger)
    {
        var command = new Command("configure-depth", "Configure indexing depth for a file extension");

        // Required arguments
        var extensionArgument = new Argument<string>(
            "extension",
            "File extension (e.g., .txt, .log)");

        var depthArgument = new Argument<IndexingDepth>(
            "depth",
            "Indexing depth: NotIndexed, PropertiesOnly, or PropertiesAndContents");

        command.AddArgument(extensionArgument);
        command.AddArgument(depthArgument);

        command.SetHandler(async (extension, depth) =>
        {
            await ExecuteAsync(searchIndexManager, privilegeChecker, auditLogger, extension, depth);
        }, extensionArgument, depthArgument);

        return command;
    }

    /// <summary>
    /// Executes the configure-depth command.
    /// </summary>
    private static async Task ExecuteAsync(
        ISearchIndexManager searchIndexManager,
        IPrivilegeChecker privilegeChecker,
        IAuditLogger auditLogger,
        string extension,
        IndexingDepth depth)
    {
        try
        {
            // Check for administrator privileges (T075)
            if (!privilegeChecker.IsAdministrator())
            {
                Console.Error.WriteLine("Error: Administrator privileges required to configure extension settings.");
                Console.Error.WriteLine("Please restart the application as administrator.");
                auditLogger.LogError("Configure-depth command failed: Insufficient privileges");
                Environment.Exit(4); // Exit code 4: Insufficient privileges
                return;
            }

            // Ensure extension starts with a dot
            if (!extension.StartsWith("."))
            {
                extension = "." + extension;
            }

            // Validate extension format
            if (extension.Length < 2 || extension.Contains(" ") || extension.Contains("\\") || extension.Contains("/"))
            {
                Console.Error.WriteLine($"Error: Invalid file extension format: '{extension}'");
                Console.Error.WriteLine("Extension must start with a dot and contain only valid characters.");
                Environment.Exit(3); // Exit code 3: Invalid input
                return;
            }

            // Display what will be changed
            Console.WriteLine($"Configuring indexing depth for extension: {extension}");
            Console.WriteLine($"New depth: {depth}");
            Console.WriteLine();

            string depthDescription = depth switch
            {
                IndexingDepth.NotIndexed => "Files will not be indexed at all",
                IndexingDepth.PropertiesOnly => "Only file metadata (name, size, dates) will be indexed",
                IndexingDepth.PropertiesAndContents => "Both metadata and full-text content will be indexed",
                _ => "Unknown depth setting"
            };

            Console.WriteLine($"Effect: {depthDescription}");
            Console.WriteLine();

            // Set the extension depth (T074)
            var result = await searchIndexManager.SetExtensionDepthAsync(extension, depth);

            if (result.Success)
            {
                Console.WriteLine($"✓ Successfully configured indexing depth for '{extension}'");
                Console.WriteLine("The change will take effect for newly indexed or modified files.");
                
                // Audit logging (T077) is already done in SearchIndexManager
                Environment.Exit(0); // Exit code 0: Success
            }
            else
            {
                Console.Error.WriteLine($"Error: {result.Message}");
                Environment.Exit(2); // Exit code 2: Operation failed
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            auditLogger.LogError($"Configure-depth command failed with exception: {ex.Message}", ex);
            Environment.Exit(1); // Exit code 1: General error
        }
    }
}
