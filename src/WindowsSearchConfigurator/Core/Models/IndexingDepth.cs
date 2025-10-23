namespace WindowsSearchConfigurator.Core.Models;

/// <summary>
/// Specifies the depth of indexing for file extensions.
/// </summary>
public enum IndexingDepth
{
    /// <summary>
    /// Extension is not indexed at all.
    /// </summary>
    NotIndexed = 0,

    /// <summary>
    /// Index only file metadata (name, size, dates, attributes).
    /// </summary>
    PropertiesOnly = 1,

    /// <summary>
    /// Index both metadata and full-text content of files.
    /// </summary>
    PropertiesAndContents = 2
}
