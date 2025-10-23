namespace WindowsSearchConfigurator.Core.Models;

/// <summary>
/// Represents the indexing depth configuration for a specific file extension.
/// </summary>
public class FileExtensionSetting
{
    /// <summary>
    /// Gets or sets the file extension (e.g., .txt, .log).
    /// </summary>
    public required string Extension { get; set; }

    /// <summary>
    /// Gets or sets the indexing depth for this extension.
    /// </summary>
    public required IndexingDepth IndexingDepth { get; set; }

    /// <summary>
    /// Gets or sets whether this is a Windows default setting.
    /// </summary>
    public bool IsDefaultSetting { get; set; }

    /// <summary>
    /// Gets or sets when this setting was last modified.
    /// </summary>
    public DateTime ModifiedDate { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileExtensionSetting"/> class.
    /// </summary>
    public FileExtensionSetting()
    {
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileExtensionSetting"/> class with specified values.
    /// </summary>
    /// <param name="extension">The file extension.</param>
    /// <param name="indexingDepth">The indexing depth.</param>
    /// <param name="isDefaultSetting">Whether this is a default setting.</param>
    public FileExtensionSetting(string extension, IndexingDepth indexingDepth, bool isDefaultSetting = false)
    {
        Extension = extension;
        IndexingDepth = indexingDepth;
        IsDefaultSetting = isDefaultSetting;
        ModifiedDate = DateTime.UtcNow;
    }
}
