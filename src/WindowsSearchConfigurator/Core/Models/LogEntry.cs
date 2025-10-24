namespace WindowsSearchConfigurator.Core.Models;

/// <summary>
/// Represents a single logged event with timestamp, severity level, component/source, and message text.
/// </summary>
/// <param name="Timestamp">UTC timestamp with millisecond precision when the entry was created.</param>
/// <param name="Severity">Level of the log entry.</param>
/// <param name="Component">Source component or service that generated the entry.</param>
/// <param name="Message">Human-readable log message content.</param>
/// <param name="StackTrace">Stack trace information for exceptions (optional).</param>
public record LogEntry(
    DateTime Timestamp,
    LogSeverity Severity,
    string Component,
    string Message,
    string? StackTrace = null)
{
    /// <summary>
    /// Validates that the log entry is properly formed.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public void Validate()
    {
        if (Timestamp == default)
        {
            throw new ArgumentException("Timestamp must be a valid UTC DateTime.", nameof(Timestamp));
        }

        if (string.IsNullOrEmpty(Component))
        {
            throw new ArgumentException("Component must not be null or empty.", nameof(Component));
        }

        if (Message == null)
        {
            throw new ArgumentException("Message must not be null.", nameof(Message));
        }

        if (Severity == LogSeverity.Exception && string.IsNullOrEmpty(StackTrace))
        {
            throw new ArgumentException("StackTrace is required when Severity is Exception.", nameof(StackTrace));
        }
    }
}
