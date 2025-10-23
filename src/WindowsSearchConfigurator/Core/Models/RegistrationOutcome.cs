namespace WindowsSearchConfigurator.Core.Models;

/// <summary>
/// Represents the result of a COM registration attempt.
/// </summary>
public enum RegistrationOutcome
{
    /// <summary>
    /// Registration completed successfully and validation passed.
    /// </summary>
    Success = 0,

    /// <summary>
    /// Registration failed (regsvr32 returned non-zero exit code).
    /// </summary>
    Failed = 1,

    /// <summary>
    /// Registration process exceeded timeout threshold.
    /// </summary>
    Timeout = 2,

    /// <summary>
    /// User lacks administrative privileges.
    /// </summary>
    InsufficientPrivileges = 3,

    /// <summary>
    /// DLL file not found at expected path.
    /// </summary>
    DLLNotFound = 4,

    /// <summary>
    /// User cancelled during interactive mode.
    /// </summary>
    Cancelled = 5,

    /// <summary>
    /// Registration appeared to succeed but validation failed.
    /// </summary>
    ValidationFailed = 6
}
