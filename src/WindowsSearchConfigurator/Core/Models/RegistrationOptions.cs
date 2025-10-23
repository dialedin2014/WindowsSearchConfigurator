namespace WindowsSearchConfigurator.Core.Models;

/// <summary>
/// Configuration options for controlling COM registration behavior.
/// </summary>
public class RegistrationOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to automatically register without prompting (from --auto-register-com).
    /// </summary>
    public bool AutoRegister { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to refuse registration and exit immediately (from --no-register-com).
    /// </summary>
    public bool NoRegister { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to suppress interactive prompts (for CI/CD).
    /// </summary>
    public bool Silent { get; set; }

    /// <summary>
    /// Gets or sets the maximum time to wait for regsvr32 process (default: 5 seconds).
    /// </summary>
    public int TimeoutSeconds { get; set; } = 5;

    /// <summary>
    /// Gets or sets the override path to searchapi.dll (default: auto-detect).
    /// </summary>
    public string? DLLPath { get; set; }

    /// <summary>
    /// Validates that the options are valid and consistent.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when options are invalid or conflicting.</exception>
    public void Validate()
    {
        if (AutoRegister && NoRegister)
        {
            throw new ArgumentException("AutoRegister and NoRegister cannot both be true (mutually exclusive).");
        }

        if (TimeoutSeconds <= 0 || TimeoutSeconds > 60)
        {
            throw new ArgumentException("TimeoutSeconds must be between 1 and 60.");
        }

        if (!string.IsNullOrEmpty(DLLPath) && !File.Exists(DLLPath))
        {
            throw new ArgumentException($"Specified DLL path does not exist: {DLLPath}");
        }
    }
}
