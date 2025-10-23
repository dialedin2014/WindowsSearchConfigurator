namespace WindowsSearchConfigurator.Core.Interfaces;

using WindowsSearchConfigurator.Core.Models;

/// <summary>
/// Defines operations for managing Windows Search index rules and extension settings.
/// </summary>
public interface ISearchIndexManager
{
    /// <summary>
    /// Retrieves all Windows Search index rules.
    /// </summary>
    /// <param name="includeSystemRules">Whether to include Windows default rules.</param>
    /// <returns>A collection of index rules.</returns>
    Task<OperationResult<IEnumerable<IndexRule>>> GetAllRulesAsync(bool includeSystemRules = false);

    /// <summary>
    /// Retrieves file extension indexing settings.
    /// </summary>
    /// <returns>A collection of file extension settings.</returns>
    Task<OperationResult<IEnumerable<FileExtensionSetting>>> GetExtensionSettingsAsync();

    /// <summary>
    /// Adds a new index rule to Windows Search.
    /// </summary>
    /// <param name="rule">The rule to add.</param>
    /// <returns>The result of the operation.</returns>
    Task<OperationResult> AddIndexRuleAsync(IndexRule rule);

    /// <summary>
    /// Removes an index rule from Windows Search.
    /// </summary>
    /// <param name="path">The path of the rule to remove.</param>
    /// <returns>The result of the operation.</returns>
    Task<OperationResult> RemoveIndexRuleAsync(string path);

    /// <summary>
    /// Modifies an existing index rule.
    /// </summary>
    /// <param name="rule">The updated rule.</param>
    /// <returns>The result of the operation.</returns>
    Task<OperationResult> ModifyIndexRuleAsync(IndexRule rule);

    /// <summary>
    /// Searches for file extensions matching a pattern.
    /// </summary>
    /// <param name="pattern">The wildcard pattern to match.</param>
    /// <returns>A collection of matching extension settings.</returns>
    Task<OperationResult<IEnumerable<FileExtensionSetting>>> SearchExtensionsAsync(string pattern);

    /// <summary>
    /// Sets the indexing depth for a file extension.
    /// </summary>
    /// <param name="extension">The file extension.</param>
    /// <param name="depth">The indexing depth.</param>
    /// <returns>The result of the operation.</returns>
    Task<OperationResult> SetExtensionDepthAsync(string extension, IndexingDepth depth);
}
