namespace WindowsSearchConfigurator.Core.Interfaces;

using WindowsSearchConfigurator.Core.Models;

/// <summary>
/// Defines operations for importing and exporting configuration files.
/// </summary>
public interface IConfigurationStore
{
    /// <summary>
    /// Exports the current configuration to a JSON file.
    /// </summary>
    /// <param name="filePath">The path to the output file.</param>
    /// <param name="includeDefaults">Whether to include Windows default rules.</param>
    /// <param name="includeExtensions">Whether to include extension settings.</param>
    /// <returns>The result of the operation.</returns>
    Task<OperationResult> ExportAsync(string filePath, bool includeDefaults = false, bool includeExtensions = true);

    /// <summary>
    /// Imports configuration from a JSON file.
    /// </summary>
    /// <param name="filePath">The path to the input file.</param>
    /// <param name="merge">Whether to merge with existing rules or replace.</param>
    /// <param name="continueOnError">Whether to continue importing if individual rules fail.</param>
    /// <returns>The result of the operation with import statistics.</returns>
    Task<OperationResult<ImportResult>> ImportAsync(string filePath, bool merge = false, bool continueOnError = false);

    /// <summary>
    /// Validates a configuration file without applying changes.
    /// </summary>
    /// <param name="filePath">The path to the configuration file.</param>
    /// <returns>The result of the validation.</returns>
    Task<OperationResult<ValidationResult>> ValidateAsync(string filePath);
}

/// <summary>
/// Contains statistics about an import operation.
/// </summary>
public class ImportResult
{
    /// <summary>
    /// Gets or sets the number of rules successfully imported.
    /// </summary>
    public int RulesImported { get; set; }

    /// <summary>
    /// Gets or sets the number of rules that failed to import.
    /// </summary>
    public int RulesFailed { get; set; }

    /// <summary>
    /// Gets or sets the number of extension settings imported.
    /// </summary>
    public int ExtensionsImported { get; set; }

    /// <summary>
    /// Gets or sets the number of extension settings that failed to import.
    /// </summary>
    public int ExtensionsFailed { get; set; }

    /// <summary>
    /// Gets or sets the list of errors encountered during import.
    /// </summary>
    public List<string> Errors { get; set; } = new();
}
