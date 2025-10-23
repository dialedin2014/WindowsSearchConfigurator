namespace WindowsSearchConfigurator.Core.Models;

/// <summary>
/// Defines which file extensions, specific files, or subfolders should be included or excluded within an index rule.
/// </summary>
public class FileTypeFilter
{
    /// <summary>
    /// Gets or sets the wildcard pattern (e.g., *.txt, *.log, node_modules).
    /// </summary>
    public required string Pattern { get; set; }

    /// <summary>
    /// Gets or sets whether this filter includes or excludes matching items.
    /// </summary>
    public required FilterType FilterType { get; set; }

    /// <summary>
    /// Gets or sets what type of items this filter applies to.
    /// </summary>
    public required FilterTarget AppliesTo { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileTypeFilter"/> class.
    /// </summary>
    public FileTypeFilter()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileTypeFilter"/> class with specified values.
    /// </summary>
    /// <param name="pattern">The wildcard pattern.</param>
    /// <param name="filterType">The filter type (Include or Exclude).</param>
    /// <param name="appliesTo">The filter target (FileExtension, FileName, or Subfolder).</param>
    public FileTypeFilter(string pattern, FilterType filterType, FilterTarget appliesTo)
    {
        Pattern = pattern;
        FilterType = filterType;
        AppliesTo = appliesTo;
    }
}
