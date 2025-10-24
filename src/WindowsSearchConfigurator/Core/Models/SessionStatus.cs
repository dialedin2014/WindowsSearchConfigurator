namespace WindowsSearchConfigurator.Core.Models;

/// <summary>
/// Defines the possible states of a logging session.
/// </summary>
public enum SessionStatus
{
    /// <summary>
    /// Session is currently active.
    /// </summary>
    InProgress = 0,

    /// <summary>
    /// Session completed successfully (exit code 0).
    /// </summary>
    Success = 1,

    /// <summary>
    /// Session completed with error (non-zero exit code).
    /// </summary>
    Failed = 2,

    /// <summary>
    /// Session terminated abnormally (e.g., Ctrl+C).
    /// </summary>
    Aborted = 3
}
