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
    /// <param name="verboseLogger">The verbose logger service.</param>
    /// <returns>The configured command.</returns>
    public static Command Create(ISearchIndexManager searchIndexManager, ConsoleFormatter consoleFormatter, VerboseLogger verboseLogger)
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
            await ExecuteAsync(searchIndexManager, consoleFormatter, verboseLogger, format, showDefaults, filter);
        }, formatOption, showDefaultsOption, filterOption);

        return command;
    }

    /// <summary>
    /// Executes the list command.
    /// </summary>
    private static async Task ExecuteAsync(
        ISearchIndexManager searchIndexManager,
        ConsoleFormatter consoleFormatter,
        VerboseLogger verboseLogger,
        OutputFormat format,
        bool showDefaults,
        string? filter)
    {
        try
        {
            verboseLogger.WriteLine("ListCommand: Executing list command");
            verboseLogger.WriteOperation("ListCommand", $"Format: {format}, ShowDefaults: {showDefaults}, Filter: {filter ?? "(none)"}");

            // Retrieve all rules
            verboseLogger.WriteLine($"ListCommand: Retrieving index rules (ShowDefaults={showDefaults})");
            var result = await searchIndexManager.GetAllRulesAsync(showDefaults);

            if (!result.Success)
            {
                verboseLogger.WriteError($"Failed to retrieve rules: {result.Message}");
                Console.Error.WriteLine($"Error: {result.Message}");
                Environment.Exit(2); // Exit code 2: Operation failed
                return;
            }
            verboseLogger.WriteLine("ListCommand: Successfully retrieved rules");

            var rules = result.Value ?? Enumerable.Empty<Core.Models.IndexRule>();
            verboseLogger.WriteLine($"ListCommand: Retrieved {rules.Count()} rules");

            // Apply filter if specified
            if (!string.IsNullOrWhiteSpace(filter))
            {
                verboseLogger.WriteLine($"ListCommand: Applying filter pattern: {filter}");
                rules = rules.Where(r => WildcardMatcher.IsMatch(r.Path, filter!));
            }

            var rulesList = rules.ToList();
            verboseLogger.WriteLine($"ListCommand: Displaying {rulesList.Count} rules after filtering");

            if (rulesList.Count == 0)
            {
                verboseLogger.WriteLine("ListCommand: No rules to display");
                Console.WriteLine("No index rules found.");
                return;
            }

            // Format and display output
            verboseLogger.WriteLine($"ListCommand: Formatting output as {format}");
            switch (format)
            {
                case OutputFormat.Table:
                    consoleFormatter.FormatRulesAsTable(rulesList);
                    verboseLogger.WriteLine("ListCommand: Table output completed");
                    break;
                case OutputFormat.Json:
                    consoleFormatter.FormatRulesAsJson(rulesList);
                    verboseLogger.WriteLine("ListCommand: JSON output completed");
                    break;
                case OutputFormat.Csv:
                    consoleFormatter.FormatRulesAsCsv(rulesList);
                    verboseLogger.WriteLine("ListCommand: CSV output completed");
                    break;
                default:
                    verboseLogger.WriteError($"Unknown output format: {format}");
                    Console.Error.WriteLine($"Unknown output format: {format}");
                    Environment.Exit(1); // Exit code 1: Invalid arguments
                    break;
            }
        }
        catch (Exception ex)
        {
            verboseLogger.WriteException(ex);
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
