namespace WindowsSearchConfigurator.Core.Models;

/// <summary>
/// Specifies whether a location should be included or excluded from Windows Search indexing.
/// </summary>
public enum RuleType
{
    /// <summary>
    /// Include this location in the Windows Search index.
    /// </summary>
    Include = 0,

    /// <summary>
    /// Exclude this location from the Windows Search index.
    /// </summary>
    Exclude = 1
}
