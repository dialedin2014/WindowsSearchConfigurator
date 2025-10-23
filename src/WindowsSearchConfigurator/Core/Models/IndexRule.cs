namespace WindowsSearchConfigurator.Core.Models;

/// <summary>
/// Represents a configuration entry that specifies whether a location should be indexed by Windows Search.
/// </summary>
public class IndexRule
{
    /// <summary>
    /// Gets or sets the unique identifier for the rule.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the absolute path to the indexed location.
    /// </summary>
    public required string Path { get; set; }

    /// <summary>
    /// Gets or sets whether to include or exclude this location.
    /// </summary>
    public required RuleType RuleType { get; set; }

    /// <summary>
    /// Gets or sets whether to index subfolders.
    /// </summary>
    public bool Recursive { get; set; } = true;

    /// <summary>
    /// Gets or sets the file patterns to include/exclude.
    /// </summary>
    public List<FileTypeFilter> FileTypeFilters { get; set; } = new();

    /// <summary>
    /// Gets or sets the subfolder patterns to skip.
    /// </summary>
    public List<string> ExcludedSubfolders { get; set; } = new();

    /// <summary>
    /// Gets or sets whether this is a user-configured rule.
    /// </summary>
    public bool IsUserDefined { get; set; } = true;

    /// <summary>
    /// Gets or sets when the rule was created.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Gets or sets when the rule was last modified.
    /// </summary>
    public DateTime ModifiedDate { get; set; }

    /// <summary>
    /// Gets or sets where the rule originated.
    /// </summary>
    public required RuleSource Source { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexRule"/> class.
    /// </summary>
    public IndexRule()
    {
        Id = Guid.NewGuid();
        CreatedDate = DateTime.UtcNow;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexRule"/> class with specified values.
    /// </summary>
    /// <param name="path">The path to index.</param>
    /// <param name="ruleType">The rule type (Include or Exclude).</param>
    /// <param name="source">The rule source.</param>
    [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
    public IndexRule(string path, RuleType ruleType, RuleSource source)
    {
        Id = Guid.NewGuid();
        Path = path;
        RuleType = ruleType;
        Source = source;
        CreatedDate = DateTime.UtcNow;
        ModifiedDate = DateTime.UtcNow;
    }
}
