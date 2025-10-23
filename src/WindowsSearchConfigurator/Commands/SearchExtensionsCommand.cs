using System.CommandLine;
using WindowsSearchConfigurator.Core.Interfaces;
using WindowsSearchConfigurator.Core.Models;
using WindowsSearchConfigurator.Utilities;

namespace WindowsSearchConfigurator.Commands;

/// <summary>
/// Handles the 'search-extensions' command to search for file extensions with wildcards.
/// </summary>
public static class SearchExtensionsCommand
{
    /// <summary>
    /// Creates the 'search-extensions' command with its options.
    /// </summary>
    /// <param name="searchIndexManager">The search index manager service.</param>
    /// <param name="consoleFormatter">The console output formatter.</param>
    /// <returns>The configured command.</returns>
    public static Command Create(
        ISearchIndexManager searchIndexManager,
        ConsoleFormatter consoleFormatter)
    {
        var command = new Command("search-extensions", "Search for file extensions and their indexing settings");

        // Optional argument
        var patternArgument = new Argument<string>(
            "pattern",
            getDefaultValue: () => "*",
            description: "Wildcard pattern to match extensions (e.g., *.log, *.*x)");

        // Options
        var formatOption = new Option<OutputFormat>(
            new[] { "--format", "-f" },
            getDefaultValue: () => OutputFormat.Table,
            description: "Output format (table, json, or csv)");

        var depthFilterOption = new Option<IndexingDepth?>(
            new[] { "--depth", "-d" },
            description: "Filter by indexing depth (NotIndexed, PropertiesOnly, or PropertiesAndContents)");

        command.AddArgument(patternArgument);
        command.AddOption(formatOption);
        command.AddOption(depthFilterOption);

        command.SetHandler(async (pattern, format, depthFilter) =>
        {
            await ExecuteAsync(searchIndexManager, consoleFormatter, pattern, format, depthFilter);
        }, patternArgument, formatOption, depthFilterOption);

        return command;
    }

    /// <summary>
    /// Executes the search-extensions command.
    /// </summary>
    private static async Task ExecuteAsync(
        ISearchIndexManager searchIndexManager,
        ConsoleFormatter consoleFormatter,
        string pattern,
        OutputFormat format,
        IndexingDepth? depthFilter)
    {
        try
        {
            // Search extensions using pattern (T073)
            var result = await searchIndexManager.SearchExtensionsAsync(pattern);

            if (!result.Success)
            {
                Console.Error.WriteLine($"Error: {result.Message}");
                Environment.Exit(2); // Exit code 2: Operation failed
                return;
            }

            var extensions = result.Value ?? Enumerable.Empty<FileExtensionSetting>();

            // Apply depth filter if specified
            if (depthFilter.HasValue)
            {
                extensions = extensions.Where(e => e.IndexingDepth == depthFilter.Value);
            }

            var extensionsList = extensions.ToList();

            if (extensionsList.Count == 0)
            {
                Console.WriteLine($"No extensions found matching pattern '{pattern}'");
                if (depthFilter.HasValue)
                {
                    Console.WriteLine($"with indexing depth '{depthFilter.Value}'");
                }
                return;
            }

            // Format and display output
            switch (format)
            {
                case OutputFormat.Table:
                    consoleFormatter.FormatExtensionsAsTable(extensionsList);
                    break;
                case OutputFormat.Json:
                    consoleFormatter.FormatExtensionsAsJson(extensionsList);
                    break;
                case OutputFormat.Csv:
                    consoleFormatter.FormatExtensionsAsCsv(extensionsList);
                    break;
                default:
                    Console.Error.WriteLine($"Unknown output format: {format}");
                    Environment.Exit(3); // Exit code 3: Invalid input
                    return;
            }

            Environment.Exit(0); // Exit code 0: Success
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            Environment.Exit(1); // Exit code 1: General error
        }
    }
}
