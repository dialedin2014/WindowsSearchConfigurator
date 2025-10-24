using System.CommandLine;
using WindowsSearchConfigurator.Core.Interfaces;
using WindowsSearchConfigurator.Core.Models;
using WindowsSearchConfigurator.Services;
using WindowsSearchConfigurator.Utilities;

namespace WindowsSearchConfigurator.Commands;

/// <summary>
/// Handles the 'modify' command to modify existing Windows Search index rules.
/// </summary>
public static class ModifyCommand
{
    /// <summary>
    /// Creates the 'modify' command with its options.
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
        var command = new Command("modify", "Modify an existing Windows Search index rule");

        // Required argument
        var pathArgument = new Argument<string>(
            "path",
            "Path of the rule to modify");

        // Options
        var recursiveOption = new Option<bool?>(
            new[] { "--recursive", "-r" },
            description: "Set whether to index subfolders (true or false)");

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

        var typeOption = new Option<RuleType?>(
            new[] { "--type", "-t" },
            description: "Rule type: Include or Exclude");

        var forceOption = new Option<bool>(
            new[] { "--force", "-f", "--no-confirm" },
            getDefaultValue: () => false,
            description: "Skip confirmation prompt for destructive changes");

        command.AddArgument(pathArgument);
        command.AddOption(recursiveOption);
        command.AddOption(includeOption);
        command.AddOption(excludeFilesOption);
        command.AddOption(excludeFoldersOption);
        command.AddOption(typeOption);
        command.AddOption(forceOption);

        command.SetHandler(async (path, recursive, include, excludeFiles, excludeFolders, type, force) =>
        {
            await ExecuteAsync(
                searchIndexManager,
                privilegeChecker,
                pathValidator,
                auditLogger,
                verboseLogger,
                path,
                recursive,
                include,
                excludeFiles,
                excludeFolders,
                type,
                force);
        }, pathArgument, recursiveOption, includeOption, excludeFilesOption, excludeFoldersOption, typeOption, forceOption);

        return command;
    }

    /// <summary>
    /// Executes the modify command.
    /// </summary>
    private static async Task ExecuteAsync(
        ISearchIndexManager searchIndexManager,
        IPrivilegeChecker privilegeChecker,
        PathValidator pathValidator,
        IAuditLogger auditLogger,
        VerboseLogger verboseLogger,
        string path,
        bool? recursive,
        string[]? includePatterns,
        string[]? excludeFilePatterns,
        string[]? excludeFolderPatterns,
        RuleType? ruleType,
        bool force)
    {
        try
        {
            verboseLogger.WriteLine("ModifyCommand: Executing modify command");
            verboseLogger.WriteOperation("ModifyCommand", $"Path: {path}, Recursive: {recursive}, RuleType: {ruleType}, Force: {force}");

            // Check for administrator privileges (T066)
            verboseLogger.WriteLine("ModifyCommand: Checking administrator privileges");
            if (!privilegeChecker.IsAdministrator())
            {
                verboseLogger.WriteError("Modify command failed: Insufficient privileges");
                Console.Error.WriteLine("Error: Administrator privileges required to modify index rules.");
                Console.Error.WriteLine("Please restart the application as administrator.");
                auditLogger.LogError("Modify command failed: Insufficient privileges");
                Environment.Exit(4); // Exit code 4: Insufficient privileges
                return;
            }
            verboseLogger.WriteLine("ModifyCommand: Privilege check passed");

            // Validate and normalize the path
            verboseLogger.WriteLine($"ModifyCommand: Validating path: {path}");
            var validation = pathValidator.ValidatePath(path);
            if (!validation.IsValid)
            {
                verboseLogger.WriteError($"Path validation failed: {validation.ErrorMessage}");
                Console.Error.WriteLine($"Error: {validation.ErrorMessage}");
                auditLogger.LogError($"Modify command failed: Invalid path '{path}' - {validation.ErrorMessage}");
                Environment.Exit(3); // Exit code 3: Invalid input
                return;
            }

            var normalizedPath = validation.NormalizedValue!;
            verboseLogger.WriteLine($"ModifyCommand: Path normalized to: {normalizedPath}");

            // Check if at least one modification is specified
            verboseLogger.WriteLine("ModifyCommand: Validating modification parameters");
            if (!recursive.HasValue && includePatterns == null && excludeFilePatterns == null && 
                excludeFolderPatterns == null && !ruleType.HasValue)
            {
                verboseLogger.WriteError("No modification options specified");
                Console.Error.WriteLine("Error: At least one modification option must be specified.");
                Console.Error.WriteLine("Use --help to see available options.");
                Environment.Exit(3); // Exit code 3: Invalid input
                return;
            }
            verboseLogger.WriteLine("ModifyCommand: At least one modification parameter specified");

            // Retrieve existing rule (T067)
            verboseLogger.WriteLine($"ModifyCommand: Retrieving existing rule for: {normalizedPath}");
            var allRulesResult = await searchIndexManager.GetAllRulesAsync(includeSystemRules: true);
            if (!allRulesResult.Success)
            {
                verboseLogger.WriteError($"Failed to retrieve rules: {allRulesResult.Message}");
                Console.Error.WriteLine($"Error: {allRulesResult.Message}");
                Environment.Exit(2); // Exit code 2: Operation failed
                return;
            }

            var existingRule = allRulesResult.Value?.FirstOrDefault(r =>
                string.Equals(r.Path, normalizedPath, StringComparison.OrdinalIgnoreCase));

            if (existingRule == null)
            {
                verboseLogger.WriteError($"No rule found for path: {normalizedPath}");
                Console.Error.WriteLine($"Error: No index rule found for path '{normalizedPath}'.");
                Console.Error.WriteLine("Use the 'add' command to create a new rule.");
                Environment.Exit(2); // Exit code 2: Operation failed
                return;
            }
            verboseLogger.WriteLine($"ModifyCommand: Found existing rule (ID: {existingRule.Id})");

            // Build modified rule with changes
            var modifiedRule = new IndexRule
            {
                Id = existingRule.Id,
                Path = normalizedPath,
                RuleType = ruleType ?? existingRule.RuleType,
                Recursive = recursive ?? existingRule.Recursive,
                FileTypeFilters = new List<FileTypeFilter>(existingRule.FileTypeFilters),
                ExcludedSubfolders = new List<string>(existingRule.ExcludedSubfolders),
                IsUserDefined = existingRule.IsUserDefined,
                CreatedDate = existingRule.CreatedDate,
                ModifiedDate = DateTime.Now,
                Source = existingRule.Source
            };

            // Update filters if specified
            if (includePatterns != null)
            {
                verboseLogger.WriteLine($"ModifyCommand: Updating include patterns: {string.Join(", ", includePatterns)}");
                // Clear existing include filters and add new ones
                modifiedRule.FileTypeFilters = modifiedRule.FileTypeFilters
                    .Where(f => f.FilterType != FilterType.Include || f.AppliesTo != FilterTarget.FileExtension)
                    .ToList();

                foreach (var pattern in includePatterns)
                {
                    var splitPatterns = pattern.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var p in splitPatterns)
                    {
                        modifiedRule.FileTypeFilters.Add(new FileTypeFilter
                        {
                            Pattern = p.Trim(),
                            FilterType = FilterType.Include,
                            AppliesTo = FilterTarget.FileExtension
                        });
                    }
                }
                verboseLogger.WriteLine($"ModifyCommand: Added {modifiedRule.FileTypeFilters.Count(f => f.FilterType == FilterType.Include)} include filters");
            }

            if (excludeFilePatterns != null)
            {
                verboseLogger.WriteLine($"ModifyCommand: Updating exclude file patterns: {string.Join(", ", excludeFilePatterns)}");
                // Clear existing exclude file filters and add new ones
                modifiedRule.FileTypeFilters = modifiedRule.FileTypeFilters
                    .Where(f => f.FilterType != FilterType.Exclude || f.AppliesTo != FilterTarget.FileName)
                    .ToList();

                foreach (var pattern in excludeFilePatterns)
                {
                    var splitPatterns = pattern.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var p in splitPatterns)
                    {
                        modifiedRule.FileTypeFilters.Add(new FileTypeFilter
                        {
                            Pattern = p.Trim(),
                            FilterType = FilterType.Exclude,
                            AppliesTo = FilterTarget.FileName
                        });
                    }
                }
                verboseLogger.WriteLine($"ModifyCommand: Added {modifiedRule.FileTypeFilters.Count(f => f.FilterType == FilterType.Exclude && f.AppliesTo == FilterTarget.FileName)} exclude file filters");
            }

            // Update excluded subfolders if specified
            if (excludeFolderPatterns != null)
            {
                verboseLogger.WriteLine($"ModifyCommand: Updating excluded subfolders: {string.Join(", ", excludeFolderPatterns)}");
                modifiedRule.ExcludedSubfolders.Clear();
                foreach (var pattern in excludeFolderPatterns)
                {
                    var splitPatterns = pattern.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    modifiedRule.ExcludedSubfolders.AddRange(splitPatterns.Select(p => p.Trim()));
                }
                verboseLogger.WriteLine($"ModifyCommand: Set {modifiedRule.ExcludedSubfolders.Count} excluded subfolders");
            }

            // Display changes
            Console.WriteLine($"Modifying index rule for: {normalizedPath}");
            Console.WriteLine();
            Console.WriteLine("Current settings:");
            Console.WriteLine($"  Type:      {existingRule.RuleType}");
            Console.WriteLine($"  Recursive: {existingRule.Recursive}");
            Console.WriteLine($"  Filters:   {existingRule.FileTypeFilters.Count}");
            Console.WriteLine($"  Excluded:  {existingRule.ExcludedSubfolders.Count}");
            Console.WriteLine();
            Console.WriteLine("New settings:");
            Console.WriteLine($"  Type:      {modifiedRule.RuleType}");
            Console.WriteLine($"  Recursive: {modifiedRule.Recursive}");
            Console.WriteLine($"  Filters:   {modifiedRule.FileTypeFilters.Count}");
            Console.WriteLine($"  Excluded:  {modifiedRule.ExcludedSubfolders.Count}");
            Console.WriteLine();

            // Confirmation prompt (T068) for destructive changes
            if (!force && (ruleType.HasValue || recursive.HasValue))
            {
                verboseLogger.WriteLine("ModifyCommand: Prompting for confirmation (destructive change)");
                Console.Write("Continue with modification? (y/N): ");
                var response = Console.ReadLine()?.Trim().ToLowerInvariant();
                verboseLogger.WriteLine($"ModifyCommand: User response: {response}");
                if (response != "y" && response != "yes")
                {
                    verboseLogger.WriteLine("ModifyCommand: User cancelled operation");
                    Console.WriteLine("Operation cancelled.");
                    Environment.Exit(0);
                    return;
                }
                verboseLogger.WriteLine("ModifyCommand: User confirmed modification");
                Console.WriteLine();
            }

            // Modify the rule
            verboseLogger.WriteOperation("ModifyCommand", $"Modifying rule for: {normalizedPath}");
            var result = await searchIndexManager.ModifyIndexRuleAsync(modifiedRule);

            if (result.Success)
            {
                verboseLogger.WriteLine($"ModifyCommand: Successfully modified rule for {normalizedPath}");
                Console.WriteLine($"✓ Successfully modified index rule for '{normalizedPath}'");
                
                // Audit logging (T069) is already done in SearchIndexManager
                Environment.Exit(0); // Exit code 0: Success
            }
            else
            {
                verboseLogger.WriteError($"ModifyCommand failed: {result.Message}");
                Console.Error.WriteLine($"Error: {result.Message}");
                Environment.Exit(2); // Exit code 2: Operation failed
            }
        }
        catch (Exception ex)
        {
            verboseLogger.WriteException(ex);
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            auditLogger.LogError($"Modify command failed with exception: {ex.Message}", ex);
            Environment.Exit(1); // Exit code 1: General error
        }
    }
}
