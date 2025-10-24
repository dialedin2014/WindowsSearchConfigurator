using System.CommandLine;
using WindowsSearchConfigurator.Core.Interfaces;
using WindowsSearchConfigurator.Core.Models;
using WindowsSearchConfigurator.Services;
using WindowsSearchConfigurator.Utilities;

namespace WindowsSearchConfigurator.Commands;

/// <summary>
/// Handles the 'add' command to add folders to the Windows Search index.
/// </summary>
public static class AddCommand
{
    /// <summary>
    /// Creates the 'add' command with its options.
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
        var command = new Command("add", "Add a location to the Windows Search index");

        // Required argument
        var pathArgument = new Argument<string>(
            "path",
            "Path to add to the index (local or UNC path)");

        // Options
        var nonRecursiveOption = new Option<bool>(
            new[] { "--non-recursive", "-nr" },
            getDefaultValue: () => false,
            description: "Index only the specified folder, not subfolders");

        var includeOption = new Option<string[]?>(
            new[] { "--include", "-i" },
            description: "File type patterns to include (e.g., *.cs,*.txt)")
        {
            AllowMultipleArgumentsPerToken = true
        };

        var excludeFilesOption = new Option<string[]?>(
            new[] { "--exclude-files", "-ef" },
            description: "File name patterns to exclude (e.g., *.tmp,*.log)")
        {
            AllowMultipleArgumentsPerToken = true
        };

        var excludeFoldersOption = new Option<string[]?>(
            new[] { "--exclude-folders", "-ed" },
            description: "Subfolder names to exclude (e.g., bin,obj,node_modules)")
        {
            AllowMultipleArgumentsPerToken = true
        };

        var typeOption = new Option<RuleType>(
            new[] { "--type", "-t" },
            getDefaultValue: () => RuleType.Include,
            description: "Rule type: Include or Exclude");

        command.AddArgument(pathArgument);
        command.AddOption(nonRecursiveOption);
        command.AddOption(includeOption);
        command.AddOption(excludeFilesOption);
        command.AddOption(excludeFoldersOption);
        command.AddOption(typeOption);

        command.SetHandler(async (path, nonRecursive, include, excludeFiles, excludeFolders, ruleType) =>
        {
            await ExecuteAsync(
                searchIndexManager,
                privilegeChecker,
                pathValidator,
                auditLogger,
                verboseLogger,
                path,
                !nonRecursive,
                include,
                excludeFiles,
                excludeFolders,
                ruleType);
        }, pathArgument, nonRecursiveOption, includeOption, excludeFilesOption, excludeFoldersOption, typeOption);

        return command;
    }

    /// <summary>
    /// Executes the add command.
    /// </summary>
    private static async Task ExecuteAsync(
        ISearchIndexManager searchIndexManager,
        IPrivilegeChecker privilegeChecker,
        PathValidator pathValidator,
        IAuditLogger auditLogger,
        VerboseLogger verboseLogger,
        string path,
        bool recursive,
        string[]? includePatterns,
        string[]? excludeFilePatterns,
        string[]? excludeFolderPatterns,
        RuleType ruleType)
    {
        try
        {
            verboseLogger.WriteLine("AddCommand: Executing add command");
            verboseLogger.WriteOperation("AddCommand", $"Path: {path}, Recursive: {recursive}, Type: {ruleType}");

            // Check for administrator privileges (T051)
            verboseLogger.WriteLine("AddCommand: Checking administrator privileges");
            if (!privilegeChecker.IsAdministrator())
            {
                verboseLogger.WriteError("Add command failed: Insufficient privileges");
                Console.Error.WriteLine("Error: Administrator privileges required to add index rules.");
                Console.Error.WriteLine("Please restart the application as administrator.");
                auditLogger.LogError("Add command failed: Insufficient privileges");
                Environment.Exit(4); // Exit code 4: Insufficient privileges
                return;
            }
            verboseLogger.WriteLine("AddCommand: Privilege check passed");

            // Validate the path (T052)
            verboseLogger.WriteLine($"AddCommand: Validating path: {path}");
            var validation = pathValidator.ValidatePath(path);
            if (!validation.IsValid)
            {
                verboseLogger.WriteError($"Path validation failed: {validation.ErrorMessage}");
                Console.Error.WriteLine($"Error: {validation.ErrorMessage}");
                auditLogger.LogError($"Add command failed: Invalid path '{path}' - {validation.ErrorMessage}");
                Environment.Exit(3); // Exit code 3: Invalid input
                return;
            }

            // Use normalized path
            var normalizedPath = validation.NormalizedValue!;
            verboseLogger.WriteLine($"AddCommand: Path normalized to: {normalizedPath}");

            // Display warning if path doesn't exist
            if (!Directory.Exists(normalizedPath) && !File.Exists(normalizedPath))
            {
                Console.WriteLine($"Warning: Path '{normalizedPath}' does not currently exist.");
                Console.WriteLine("The rule will be added, but indexing will not occur until the path exists.");
            }

            // Build file type filters
            var filters = new List<FileTypeFilter>();

            if (includePatterns != null && includePatterns.Length > 0)
            {
                foreach (var pattern in includePatterns)
                {
                    // Split by comma if multiple patterns in one argument
                    var splitPatterns = pattern.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var p in splitPatterns)
                    {
                        filters.Add(new FileTypeFilter
                        {
                            Pattern = p.Trim(),
                            FilterType = FilterType.Include,
                            AppliesTo = FilterTarget.FileExtension
                        });
                    }
                }
            }

            if (excludeFilePatterns != null && excludeFilePatterns.Length > 0)
            {
                foreach (var pattern in excludeFilePatterns)
                {
                    var splitPatterns = pattern.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var p in splitPatterns)
                    {
                        filters.Add(new FileTypeFilter
                        {
                            Pattern = p.Trim(),
                            FilterType = FilterType.Exclude,
                            AppliesTo = FilterTarget.FileName
                        });
                    }
                }
            }

            // Build excluded subfolders list
            var excludedSubfolders = new List<string>();
            if (excludeFolderPatterns != null && excludeFolderPatterns.Length > 0)
            {
                foreach (var pattern in excludeFolderPatterns)
                {
                    var splitPatterns = pattern.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    excludedSubfolders.AddRange(splitPatterns.Select(p => p.Trim()));
                }
            }

            // Create the index rule
            var rule = new IndexRule
            {
                Id = Guid.NewGuid(),
                Path = normalizedPath,
                RuleType = ruleType,
                Recursive = recursive,
                FileTypeFilters = filters,
                ExcludedSubfolders = excludedSubfolders,
                IsUserDefined = true,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                Source = RuleSource.User
            };

            // Display summary before adding
            Console.WriteLine($"Adding index rule:");
            Console.WriteLine($"  Path:      {normalizedPath}");
            Console.WriteLine($"  Type:      {ruleType}");
            Console.WriteLine($"  Recursive: {recursive}");
            
            if (filters.Count > 0)
            {
                Console.WriteLine($"  Filters:   {filters.Count} filter(s)");
                foreach (var filter in filters)
                {
                    Console.WriteLine($"    - {filter.FilterType} {filter.AppliesTo}: {filter.Pattern}");
                }
            }

            if (excludedSubfolders.Count > 0)
            {
                Console.WriteLine($"  Excluded:  {string.Join(", ", excludedSubfolders)}");
            }

            Console.WriteLine();

            // Add the rule (T056: error handling is in SearchIndexManager)
            verboseLogger.WriteOperation("AddCommand", $"Adding index rule for: {normalizedPath}");
            var result = await searchIndexManager.AddIndexRuleAsync(rule);

            if (result.Success)
            {
                verboseLogger.WriteLine($"AddCommand: Successfully added index rule for '{normalizedPath}'");
                Console.WriteLine($"✓ Successfully added index rule for '{normalizedPath}'");
                Console.WriteLine("Windows Search will begin indexing this location.");
                Environment.Exit(0); // Exit code 0: Success
            }
            else
            {
                verboseLogger.WriteError($"AddCommand failed: {result.Message}");
                Console.Error.WriteLine($"Error: {result.Message}");
                Environment.Exit(2); // Exit code 2: Operation failed
            }
        }
        catch (Exception ex)
        {
            verboseLogger.WriteException(ex);
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            auditLogger.LogError($"Add command failed with exception: {ex.Message}", ex);
            Environment.Exit(1); // Exit code 1: General error
        }
    }
}
