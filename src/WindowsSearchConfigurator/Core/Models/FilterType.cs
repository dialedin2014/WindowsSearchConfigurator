namespace WindowsSearchConfigurator.Core.Models;

/// <summary>
/// Specifies whether a file type filter includes or excludes matching items.
/// </summary>
public enum FilterType
{
    /// <summary>
    /// Include items matching the filter pattern.
    /// </summary>
    Include = 0,

    /// <summary>
    /// Exclude items matching the filter pattern.
    /// </summary>
    Exclude = 1
}
