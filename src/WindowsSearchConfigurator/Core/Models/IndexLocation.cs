namespace WindowsSearchConfigurator.Core.Models;

/// <summary>
/// Represents a folder path (local or network) that is subject to indexing rules.
/// </summary>
public class IndexLocation
{
    /// <summary>
    /// Gets or sets the normalized absolute path.
    /// </summary>
    public required string FullPath { get; set; }

    /// <summary>
    /// Gets or sets the type of path (Local, UNC, or Relative).
    /// </summary>
    public required PathType PathType { get; set; }

    /// <summary>
    /// Gets or sets whether the location currently exists.
    /// </summary>
    public bool Exists { get; set; }

    /// <summary>
    /// Gets or sets whether the current user can read the path.
    /// </summary>
    public bool IsAccessible { get; set; }

    /// <summary>
    /// Gets or sets the drive/volume label (only for local paths).
    /// </summary>
    public string? VolumeLabel { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexLocation"/> class.
    /// </summary>
    public IndexLocation()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexLocation"/> class with a specified path.
    /// </summary>
    /// <param name="fullPath">The full path to the location.</param>
    /// <param name="pathType">The type of path.</param>
    public IndexLocation(string fullPath, PathType pathType)
    {
        FullPath = fullPath;
        PathType = pathType;
    }

    /// <summary>
    /// Determines the path type from a path string.
    /// </summary>
    /// <param name="path">The path to analyze.</param>
    /// <returns>The determined <see cref="PathType"/>.</returns>
    public static PathType DeterminePathType(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be null or whitespace.", nameof(path));
        }

        if (path.StartsWith(@"\\") || path.StartsWith("//"))
        {
            return PathType.UNC;
        }

        if (Path.IsPathRooted(path))
        {
            return PathType.Local;
        }

        return PathType.Relative;
    }
}
