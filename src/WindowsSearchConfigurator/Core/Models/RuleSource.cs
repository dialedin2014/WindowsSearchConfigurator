namespace WindowsSearchConfigurator.Core.Models;

/// <summary>
/// Indicates the origin of an index rule.
/// </summary>
public enum RuleSource
{
    /// <summary>
    /// Windows default rule created by the operating system.
    /// </summary>
    System = 0,

    /// <summary>
    /// User-created rule configured manually.
    /// </summary>
    User = 1,

    /// <summary>
    /// Rule imported from a configuration file.
    /// </summary>
    Imported = 2
}
