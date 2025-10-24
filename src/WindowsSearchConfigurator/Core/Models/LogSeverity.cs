namespace WindowsSearchConfigurator.Core.Models;

/// <summary>
/// Defines the severity levels for log entries.
/// </summary>
public enum LogSeverity
{
    /// <summary>
    /// General informational messages.
    /// </summary>
    Info = 0,

    /// <summary>
    /// Specific operations like registry reads/writes, API calls.
    /// </summary>
    Operation = 1,

    /// <summary>
    /// Non-critical issues or potential problems.
    /// </summary>
    Warning = 2,

    /// <summary>
    /// Errors that may impact functionality.
    /// </summary>
    Error = 3,

    /// <summary>
    /// Exceptions with full details and stack traces.
    /// </summary>
    Exception = 4
}
