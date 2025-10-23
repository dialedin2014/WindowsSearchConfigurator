namespace WindowsSearchConfigurator.Core.Models;

/// <summary>
/// Specifies what type of items a filter pattern applies to.
/// </summary>
public enum FilterTarget
{
    /// <summary>
    /// Pattern matches file extensions (e.g., *.txt).
    /// </summary>
    FileExtension = 0,

    /// <summary>
    /// Pattern matches file names (e.g., readme.*).
    /// </summary>
    FileName = 1,

    /// <summary>
    /// Pattern matches subfolder names (e.g., node_modules).
    /// </summary>
    Subfolder = 2
}
