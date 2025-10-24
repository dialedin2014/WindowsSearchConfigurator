namespace WindowsSearchConfigurator.Core.Models;

/// <summary>
/// Represents a complete execution of the tool with verbose logging, containing metadata and collection of log entries.
/// </summary>
public class LogSession
{
    /// <summary>
    /// Gets or initializes the unique identifier for this logging session.
    /// </summary>
    public Guid SessionId { get; init; }

    /// <summary>
    /// Gets or initializes the UTC timestamp when session started.
    /// </summary>
    public DateTime StartTime { get; init; }

    /// <summary>
    /// Gets or sets the UTC timestamp when session ended (null if not yet ended).
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Gets or initializes the full command-line arguments that triggered this session.
    /// </summary>
    public string CommandLine { get; init; } = string.Empty;

    /// <summary>
    /// Gets or initializes the Windows username executing the tool.
    /// </summary>
    public string UserName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or initializes the current working directory when tool was launched.
    /// </summary>
    public string WorkingDirectory { get; init; } = string.Empty;

    /// <summary>
    /// Gets or initializes the Windows version information.
    /// </summary>
    public string WindowsVersion { get; init; } = string.Empty;

    /// <summary>
    /// Gets or initializes the .NET runtime version.
    /// </summary>
    public string RuntimeVersion { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the process exit code (null if session not yet completed).
    /// </summary>
    public int? ExitCode { get; set; }

    /// <summary>
    /// Gets or sets the overall session status.
    /// </summary>
    public SessionStatus Status { get; set; }

    /// <summary>
    /// Gets the calculated duration of the session (EndTime - StartTime), or null if not yet completed.
    /// </summary>
    public TimeSpan? Duration => EndTime.HasValue ? EndTime.Value - StartTime : null;

    /// <summary>
    /// Validates that the log session is properly formed.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public void Validate()
    {
        if (SessionId == Guid.Empty)
        {
            throw new ArgumentException("SessionId must be a non-empty GUID.", nameof(SessionId));
        }

        if (StartTime == default)
        {
            throw new ArgumentException("StartTime must be a valid UTC DateTime.", nameof(StartTime));
        }

        if (EndTime.HasValue && EndTime.Value < StartTime)
        {
            throw new ArgumentException("EndTime must be null or later than StartTime.", nameof(EndTime));
        }

        if (CommandLine == null)
        {
            throw new ArgumentException("CommandLine must not be null.", nameof(CommandLine));
        }

        if (ExitCode.HasValue && (ExitCode.Value < 0 || ExitCode.Value > 255))
        {
            throw new ArgumentException("ExitCode must be null or 0-255.", nameof(ExitCode));
        }
    }
}
