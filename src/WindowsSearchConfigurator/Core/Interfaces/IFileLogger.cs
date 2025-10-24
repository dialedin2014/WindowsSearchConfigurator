using WindowsSearchConfigurator.Core.Models;

namespace WindowsSearchConfigurator.Core.Interfaces;

/// <summary>
/// Interface for file logging operations.
/// </summary>
public interface IFileLogger : IDisposable
{
    /// <summary>
    /// Initializes the file logger with the specified log file path.
    /// </summary>
    /// <param name="logFilePath">Full path to the log file.</param>
    /// <returns>True if initialization succeeded, false otherwise.</returns>
    bool Initialize(string logFilePath);

    /// <summary>
    /// Writes a log entry to the file asynchronously.
    /// </summary>
    /// <param name="entry">The log entry to write.</param>
    Task WriteEntryAsync(LogEntry entry);

    /// <summary>
    /// Writes the session header to the log file.
    /// </summary>
    /// <param name="session">The session metadata.</param>
    Task WriteSessionHeaderAsync(LogSession session);

    /// <summary>
    /// Writes the session footer to the log file.
    /// </summary>
    /// <param name="session">The session metadata.</param>
    Task WriteSessionFooterAsync(LogSession session);

    /// <summary>
    /// Gets a value indicating whether file logging is currently active.
    /// </summary>
    bool IsEnabled { get; }
}
