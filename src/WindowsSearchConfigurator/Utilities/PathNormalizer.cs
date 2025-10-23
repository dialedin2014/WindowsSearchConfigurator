namespace WindowsSearchConfigurator.Utilities;

/// <summary>
/// Provides path normalization and handling utilities.
/// </summary>
public class PathNormalizer
{
    /// <summary>
    /// Normalizes a path by removing relative segments and ensuring consistent format.
    /// </summary>
    /// <param name="path">The path to normalize.</param>
    /// <returns>The normalized path.</returns>
    public static string Normalize(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));
        }

        // Convert to absolute path if relative
        if (!Path.IsPathRooted(path) && !path.StartsWith(@"\\"))
        {
            path = Path.GetFullPath(path);
        }

        // Normalize separators to backslash for Windows
        path = path.Replace('/', '\\');

        // Remove trailing backslash (except for root paths)
        path = path.TrimEnd('\\');

        // Ensure root paths keep their trailing backslash
        if (path.Length == 2 && path[1] == ':')
        {
            path += "\\";
        }

        // Handle UNC paths
        if (path.StartsWith(@"\\"))
        {
            // Ensure exactly two backslashes at the start
            path = @"\\" + path.TrimStart('\\');
        }

        return path;
    }

    /// <summary>
    /// Converts a path to an absolute path.
    /// </summary>
    /// <param name="path">The path to convert.</param>
    /// <param name="basePath">Optional base path for resolving relative paths.</param>
    /// <returns>The absolute path.</returns>
    public static string ToAbsolute(string path, string? basePath = null)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));
        }

        // Already absolute
        if (Path.IsPathRooted(path) || path.StartsWith(@"\\"))
        {
            return Normalize(path);
        }

        // Use provided base path or current directory
        var baseDir = string.IsNullOrWhiteSpace(basePath) 
            ? Directory.GetCurrentDirectory() 
            : basePath;

        var combined = Path.Combine(baseDir, path);
        return Normalize(Path.GetFullPath(combined));
    }

    /// <summary>
    /// Checks if a path is a UNC path.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns>True if the path is a UNC path; otherwise, false.</returns>
    public static bool IsUncPath(string path)
    {
        return !string.IsNullOrWhiteSpace(path) && 
               (path.StartsWith(@"\\") || path.StartsWith("//"));
    }

    /// <summary>
    /// Checks if a path is a local path with a drive letter.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns>True if the path is a local path; otherwise, false.</returns>
    public static bool IsLocalPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        return Path.IsPathRooted(path) && !IsUncPath(path);
    }

    /// <summary>
    /// Checks if a path is a relative path.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns>True if the path is relative; otherwise, false.</returns>
    public static bool IsRelativePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        return !Path.IsPathRooted(path) && !IsUncPath(path);
    }

    /// <summary>
    /// Combines multiple path segments into a single normalized path.
    /// </summary>
    /// <param name="paths">The path segments to combine.</param>
    /// <returns>The combined normalized path.</returns>
    public static string Combine(params string[] paths)
    {
        if (paths == null || paths.Length == 0)
        {
            throw new ArgumentException("At least one path segment is required.", nameof(paths));
        }

        var combined = Path.Combine(paths);
        return Normalize(combined);
    }

    /// <summary>
    /// Gets the parent directory of a path.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>The parent directory path, or null if the path is a root.</returns>
    public static string? GetParent(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        var normalized = Normalize(path);
        var parent = Path.GetDirectoryName(normalized);
        
        return string.IsNullOrEmpty(parent) ? null : Normalize(parent);
    }
}
