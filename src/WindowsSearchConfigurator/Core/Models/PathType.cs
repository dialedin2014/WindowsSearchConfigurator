namespace WindowsSearchConfigurator.Core.Models;

/// <summary>
/// Specifies the type of file system path.
/// </summary>
public enum PathType
{
    /// <summary>
    /// Local path with drive letter (e.g., C:\folder or D:\path).
    /// </summary>
    Local = 0,

    /// <summary>
    /// UNC network path (e.g., \\server\share\path).
    /// </summary>
    UNC = 1,

    /// <summary>
    /// Relative path (e.g., .\subfolder or ..\parent).
    /// </summary>
    Relative = 2
}
