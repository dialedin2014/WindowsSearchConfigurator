using System.Runtime.Versioning;
using Microsoft.Win32;
using WindowsSearchConfigurator.Core.Models;

namespace WindowsSearchConfigurator.Infrastructure;

/// <summary>
/// Provides Registry access operations for Windows Search configuration.
/// </summary>
[SupportedOSPlatform("windows")]
public class RegistryAccessor
{
    private const string WINDOWS_SEARCH_KEY = @"SOFTWARE\Microsoft\Windows Search";
    private const string FILE_TYPES_KEY = @"SOFTWARE\Microsoft\Windows Search\Preferences\FileTypes";

    /// <summary>
    /// Reads file extension indexing depth from the Registry.
    /// </summary>
    /// <param name="extension">The file extension (e.g., .txt).</param>
    /// <returns>The indexing depth value (0=NotIndexed, 1=PropertiesOnly, 2=PropertiesAndContents), or null if not found.</returns>
    public int? ReadExtensionDepth(string extension)
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(FILE_TYPES_KEY, false);
            if (key == null)
            {
                return null;
            }

            var value = key.GetValue(extension);
            if (value is int intValue)
            {
                return intValue;
            }

            return null;
        }
        catch (UnauthorizedAccessException)
        {
            throw new UnauthorizedAccessException(
                "Access denied reading Windows Search registry. This operation requires administrator privileges."
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to read extension depth for {extension}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Writes file extension indexing depth to the Registry.
    /// </summary>
    /// <param name="extension">The file extension (e.g., .txt).</param>
    /// <param name="depth">The indexing depth value (0=NotIndexed, 1=PropertiesOnly, 2=PropertiesAndContents).</param>
    public void WriteExtensionDepth(string extension, int depth)
    {
        if (depth < 0 || depth > 2)
        {
            throw new ArgumentException("Depth must be 0 (NotIndexed), 1 (PropertiesOnly), or 2 (PropertiesAndContents).", nameof(depth));
        }

        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(FILE_TYPES_KEY, true);
            if (key == null)
            {
                throw new InvalidOperationException(
                    $"Registry key not found: {FILE_TYPES_KEY}. Windows Search may not be installed."
                );
            }

            key.SetValue(extension, depth, RegistryValueKind.DWord);
        }
        catch (UnauthorizedAccessException)
        {
            throw new UnauthorizedAccessException(
                "Access denied writing to Windows Search registry. This operation requires administrator privileges."
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to write extension depth for {extension}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Enumerates all file extensions configured in the Registry.
    /// </summary>
    /// <returns>A dictionary of extensions and their indexing depth values.</returns>
    public Dictionary<string, int> EnumerateExtensions()
    {
        var extensions = new Dictionary<string, int>();

        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(FILE_TYPES_KEY, false);
            if (key == null)
            {
                return extensions;
            }

            foreach (var valueName in key.GetValueNames())
            {
                var value = key.GetValue(valueName);
                if (value is int intValue)
                {
                    extensions[valueName] = intValue;
                }
            }

            return extensions;
        }
        catch (UnauthorizedAccessException)
        {
            throw new UnauthorizedAccessException(
                "Access denied reading Windows Search registry. This operation may require administrator privileges."
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to enumerate extensions: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Checks if the Windows Search registry keys exist.
    /// </summary>
    /// <returns>True if the keys exist; otherwise, false.</returns>
    public bool WindowsSearchRegistryExists()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(WINDOWS_SEARCH_KEY, false);
            return key != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Enumerates file type settings from the Registry.
    /// </summary>
    /// <param name="keyPath">The registry key path.</param>
    /// <returns>A collection of file extension settings.</returns>
    public IEnumerable<FileExtensionSetting> EnumerateFileTypeSettings(string keyPath)
    {
        var settings = new List<FileExtensionSetting>();

        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(keyPath, false);
            if (key == null)
            {
                return settings;
            }

            foreach (var valueName in key.GetValueNames())
            {
                var value = key.GetValue(valueName);
                if (value is int intValue && !string.IsNullOrWhiteSpace(valueName))
                {
                    var depth = intValue switch
                    {
                        0 => IndexingDepth.NotIndexed,
                        1 => IndexingDepth.PropertiesOnly,
                        2 => IndexingDepth.PropertiesAndContents,
                        _ => IndexingDepth.NotIndexed
                    };

                    settings.Add(new FileExtensionSetting
                    {
                        Extension = valueName,
                        IndexingDepth = depth,
                        IsDefaultSetting = false, // Cannot determine from registry alone
                        ModifiedDate = DateTime.Now
                    });
                }
            }

            return settings;
        }
        catch (UnauthorizedAccessException)
        {
            throw new UnauthorizedAccessException(
                "Access denied reading Windows Search registry. This operation may require administrator privileges."
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to enumerate file type settings: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Sets the indexing depth for a file extension.
    /// </summary>
    /// <param name="keyPath">The registry key path.</param>
    /// <param name="extension">The file extension.</param>
    /// <param name="depth">The indexing depth.</param>
    public void SetFileTypeDepth(string keyPath, string extension, IndexingDepth depth)
    {
        var depthValue = depth switch
        {
            IndexingDepth.NotIndexed => 0,
            IndexingDepth.PropertiesOnly => 1,
            IndexingDepth.PropertiesAndContents => 2,
            _ => throw new ArgumentException($"Invalid indexing depth: {depth}", nameof(depth))
        };

        WriteExtensionDepth(extension, depthValue);
    }
}
