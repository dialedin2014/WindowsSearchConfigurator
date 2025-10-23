using System.CommandLine;
using WindowsSearchConfigurator.Core.Interfaces;
using WindowsSearchConfigurator.Utilities;

namespace WindowsSearchConfigurator.Commands;

/// <summary>
/// Handles the 'list' command to display Windows Search index rules.
/// </summary>
public static class ListCommand
{
    /// <summary>
    /// Creates the 'list' command with its options.
    /// </summary>
    /// <param name="searchIndexManager">The search index manager service.</param>
    /// <param name="consoleFormatter">The console output formatter.</param>
    /// <returns>The configured command.</returns>
    public static Command Create(ISearchIndexManager searchIndexManager, ConsoleFormatter consoleFormatter)
    {
        var command = new Command("list", "List all configured Windows Search index rules");

        // Options
        var formatOption = new Option<OutputFormat>(
            new[] { "--format", "-f" },
            getDefaultValue: () => OutputFormat.Table,
            description: "Output format (table, json, or csv)");

        var showDefaultsOption = new Option<bool>(
            new[] { "--show-defaults", "-d" },
            getDefaultValue: () => false,
            description: "Include Windows system default rules");

        var filterOption = new Option<string?>(
            new[] { "--filter", "-p" },
            description: "Filter rules by path pattern (supports wildcards)");

        command.Add(formatOption);
        command.Add(showDefaultsOption);
        command.Add(filterOption);

        command.SetHandler(async (format, showDefaults, filter) =>
        {
            await ExecuteAsync(searchIndexManager, consoleFormatter, format, showDefaults, filter);
        }, formatOption, showDefaultsOption, filterOption);

        return command;
    }

    /// <summary>
    /// Executes the list command.
    /// </summary>
    private static async Task ExecuteAsync(
        ISearchIndexManager searchIndexManager,
        ConsoleFormatter consoleFormatter,
        OutputFormat format,
        bool showDefaults,
        string? filter)
    {
        try
        {
            // Retrieve all rules
            var result = await searchIndexManager.GetAllRulesAsync(showDefaults);

            if (!result.Success)
            {
                Console.Error.WriteLine($"Error: {result.Message}");
                Environment.Exit(2); // Exit code 2: Operation failed
                return;
            }

            var rules = result.Value ?? Enumerable.Empty<Core.Models.IndexRule>();

            // Apply filter if specified
            if (!string.IsNullOrWhiteSpace(filter))
            {
                rules = rules.Where(r => WildcardMatcher.IsMatch(r.Path, filter!));
            }

            var rulesList = rules.ToList();

            if (rulesList.Count == 0)
            {
                Console.WriteLine("No index rules found.");
                return;
            }

            // Format and display output
            switch (format)
            {
                case OutputFormat.Table:
                    consoleFormatter.FormatRulesAsTable(rulesList);
                    break;
                case OutputFormat.Json:
                    consoleFormatter.FormatRulesAsJson(rulesList);
                    break;
                case OutputFormat.Csv:
                    consoleFormatter.FormatRulesAsCsv(rulesList);
                    break;
                default:
                    Console.Error.WriteLine($"Unknown output format: {format}");
                    Environment.Exit(1); // Exit code 1: Invalid arguments
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            Environment.Exit(5); // Exit code 5: Unexpected error
        }
    }
}

/// <summary>
/// Supported output formats for the list command.
/// </summary>
public enum OutputFormat
{
    /// <summary>
    /// Table format with Unicode box-drawing characters.
    /// </summary>
    Table,

    /// <summary>
    /// JSON format.
    /// </summary>
    Json,

    /// <summary>
    /// CSV format following RFC 4180.
    /// </summary>
    Csv
}
