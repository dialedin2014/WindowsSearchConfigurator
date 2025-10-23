namespace WindowsSearchConfigurator.Services;

using WindowsSearchConfigurator.Core.Models;

/// <summary>
/// Provides path validation functionality with support for UNC, local, and relative paths.
/// </summary>
public class PathValidator
{
    private const int MAX_PATH_LENGTH = 260;

    /// <summary>
    /// Validates a file system path.
    /// </summary>
    /// <param name="path">The path to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> indicating success or failure.</returns>
    public ValidationResult ValidatePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return ValidationResult.Failure("Path cannot be null or empty.");
        }

        // Check for invalid characters
        var invalidChars = Path.GetInvalidPathChars();
        if (path.Any(c => invalidChars.Contains(c)))
        {
            return ValidationResult.Failure("Path contains invalid characters.");
        }

        // Determine path type
        var pathType = IndexLocation.DeterminePathType(path);

        // Validate based on path type
        switch (pathType)
        {
            case PathType.UNC:
                return ValidateUncPath(path);
            case PathType.Local:
                return ValidateLocalPath(path);
            case PathType.Relative:
                return ValidateRelativePath(path);
            default:
                return ValidationResult.Failure("Unknown path type.");
        }
    }

    /// <summary>
    /// Normalizes a path by resolving relative segments and ensuring consistent format.
    /// </summary>
    /// <param name="path">The path to normalize.</param>
    /// <returns>The normalized path.</returns>
    public string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));
        }

        var pathType = IndexLocation.DeterminePathType(path);

        if (pathType == PathType.Relative)
        {
            // Convert relative to absolute
            path = Path.GetFullPath(path);
        }

        // Remove trailing backslash (except for root paths like C:\)
        path = path.TrimEnd('\\', '/');
        
        // Ensure root paths keep their trailing backslash
        if (path.Length == 2 && path[1] == ':')
        {
            path += "\\";
        }

        return path;
    }

    private ValidationResult ValidateUncPath(string path)
    {
        // UNC path format: \\server\share\path
        if (!path.StartsWith(@"\\") && !path.StartsWith("//"))
        {
            return ValidationResult.Failure("UNC path must start with '\\\\'.");
        }

        var parts = path.Substring(2).Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
        {
            return ValidationResult.Failure("UNC path must include server and share name (\\\\server\\share).");
        }

        return CheckPathLength(path);
    }

    private ValidationResult ValidateLocalPath(string path)
    {
        // Local path should have a drive letter
        if (!Path.IsPathRooted(path))
        {
            return ValidationResult.Failure("Local path must be rooted (e.g., C:\\folder).");
        }

        try
        {
            // Try to get the full path to validate format
            var fullPath = Path.GetFullPath(path);
            return CheckPathLength(fullPath);
        }
        catch (Exception ex)
        {
            return ValidationResult.Failure($"Invalid path format: {ex.Message}");
        }
    }

    private ValidationResult ValidateRelativePath(string path)
    {
        try
        {
            // Convert to absolute path for validation
            var absolutePath = Path.GetFullPath(path);
            return CheckPathLength(absolutePath);
        }
        catch (Exception ex)
        {
            return ValidationResult.Failure($"Invalid relative path: {ex.Message}");
        }
    }

    private ValidationResult CheckPathLength(string path)
    {
        var normalizedPath = NormalizePath(path);
        
        if (normalizedPath.Length > MAX_PATH_LENGTH)
        {
            return ValidationResult.Failure(
                $"Path length ({normalizedPath.Length}) exceeds maximum allowed length ({MAX_PATH_LENGTH})."
            );
        }

        if (normalizedPath.Length > MAX_PATH_LENGTH - 12) // Windows recommendation
        {
            return ValidationResult.Warning(
                $"Path length ({normalizedPath.Length}) is close to maximum. Consider using a shorter path.",
                normalizedPath
            );
        }

        return ValidationResult.Success(normalizedPath);
    }

    /// <summary>
    /// Checks if a path exists and is accessible.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns>True if the path exists and is accessible; otherwise, false.</returns>
    public bool PathExistsAndAccessible(string path)
    {
        try
        {
            return Directory.Exists(path);
        }
        catch
        {
            return false;
        }
    }
}
