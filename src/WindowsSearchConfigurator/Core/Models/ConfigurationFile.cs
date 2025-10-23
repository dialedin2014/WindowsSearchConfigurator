namespace WindowsSearchConfigurator.Core.Models;

/// <summary>
/// A portable JSON representation of multiple index rules and extension settings that can be exported/imported.
/// </summary>
public class ConfigurationFile
{
    /// <summary>
    /// Gets or sets the schema version (e.g., "1.0").
    /// </summary>
    public required string Version { get; set; }

    /// <summary>
    /// Gets or sets when the configuration was exported.
    /// </summary>
    public DateTime ExportDate { get; set; }

    /// <summary>
    /// Gets or sets the user who exported the configuration (DOMAIN\User).
    /// </summary>
    public required string ExportedBy { get; set; }

    /// <summary>
    /// Gets or sets the computer where the export was performed.
    /// </summary>
    public required string MachineName { get; set; }

    /// <summary>
    /// Gets or sets the collection of index rules.
    /// </summary>
    public List<IndexRule> Rules { get; set; } = new();

    /// <summary>
    /// Gets or sets the collection of extension configurations.
    /// </summary>
    public List<FileExtensionSetting> ExtensionSettings { get; set; } = new();

    /// <summary>
    /// Gets or sets the SHA256 hash for integrity validation (optional).
    /// </summary>
    public string? Checksum { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationFile"/> class.
    /// </summary>
    public ConfigurationFile()
    {
        ExportDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationFile"/> class with specified values.
    /// </summary>
    /// <param name="version">The schema version.</param>
    /// <param name="exportedBy">The user performing the export.</param>
    /// <param name="machineName">The machine name.</param>
    [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
    public ConfigurationFile(string version, string exportedBy, string machineName)
    {
        Version = version;
        ExportedBy = exportedBy;
        MachineName = machineName;
        ExportDate = DateTime.UtcNow;
    }
}
