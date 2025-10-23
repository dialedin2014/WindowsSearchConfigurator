namespace WindowsSearchConfigurator.Core.Models;

/// <summary>
/// Represents a single attempt to register the COM API, including outcome and context.
/// </summary>
public class COMRegistrationAttempt
{
    /// <summary>
    /// Gets or sets the unique identifier for this registration attempt.
    /// </summary>
    public Guid AttemptId { get; set; }

    /// <summary>
    /// Gets or sets when the registration attempt started (UTC).
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets how registration was initiated.
    /// </summary>
    public RegistrationMode Mode { get; set; }

    /// <summary>
    /// Gets or sets the Windows username who initiated registration.
    /// </summary>
    public string User { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the user had admin privileges.
    /// </summary>
    public bool IsAdministrator { get; set; }

    /// <summary>
    /// Gets or sets the path to the DLL being registered.
    /// </summary>
    public string DLLPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the method used for registration (e.g., "regsvr32", "manual").
    /// </summary>
    public string RegistrationMethod { get; set; } = "regsvr32";

    /// <summary>
    /// Gets or sets the result of the registration attempt.
    /// </summary>
    public RegistrationOutcome Outcome { get; set; }

    /// <summary>
    /// Gets or sets the exit code from regsvr32 (if applicable).
    /// </summary>
    public int? ExitCode { get; set; }

    /// <summary>
    /// Gets or sets error details if registration failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the time taken for registration attempt in milliseconds.
    /// </summary>
    public long DurationMs { get; set; }

    /// <summary>
    /// Gets or sets the validation result after registration.
    /// </summary>
    public COMValidationState PostValidation { get; set; }
}
