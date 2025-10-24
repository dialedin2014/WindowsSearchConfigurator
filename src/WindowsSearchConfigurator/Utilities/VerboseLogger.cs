using System;
using WindowsSearchConfigurator.Core.Interfaces;
using WindowsSearchConfigurator.Core.Models;

namespace WindowsSearchConfigurator.Utilities;

/// <summary>
/// Provides verbose logging functionality for diagnostic output to both console and file.
/// </summary>
public class VerboseLogger : IDisposable
{
    private bool _isVerboseEnabled;
    private IFileLogger? _fileLogger;
    private LogSession? _currentSession;
    private bool _isDisposed;

    /// <summary>
    /// Gets or sets a value indicating whether verbose logging is enabled.
    /// </summary>
    public bool IsEnabled
    {
        get => _isVerboseEnabled;
        set => _isVerboseEnabled = value;
    }

    /// <summary>
    /// Initializes file logging for the current session.
    /// </summary>
    /// <param name="fileLogger">The file logger instance.</param>
    /// <param name="logFilePath">Full path to the log file.</param>
    /// <param name="commandLine">The command line arguments.</param>
    /// <returns>True if file logging was initialized successfully.</returns>
    public bool InitializeFileLogging(IFileLogger fileLogger, string logFilePath, string commandLine)
    {
        _fileLogger = fileLogger;
        
        if (!_fileLogger.Initialize(logFilePath))
        {
            return false;
        }

        // Create session metadata
        _currentSession = new LogSession
        {
            SessionId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow,
            CommandLine = commandLine,
            UserName = Environment.UserName,
            WorkingDirectory = Environment.CurrentDirectory,
            WindowsVersion = GetWindowsVersion(),
            RuntimeVersion = Environment.Version.ToString(),
            Status = SessionStatus.InProgress
        };

        // Write session header
        _fileLogger.WriteSessionHeaderAsync(_currentSession).GetAwaiter().GetResult();

        return true;
    }

    /// <summary>
    /// Completes the logging session with exit code and status.
    /// </summary>
    /// <param name="exitCode">The process exit code.</param>
    public void CompleteSession(int exitCode)
    {
        if (_currentSession == null || _fileLogger == null)
        {
            return;
        }

        _currentSession.EndTime = DateTime.UtcNow;
        _currentSession.ExitCode = exitCode;
        _currentSession.Status = exitCode == 0 ? SessionStatus.Success : SessionStatus.Failed;

        _fileLogger.WriteSessionFooterAsync(_currentSession).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Writes a verbose message to the console and file if verbose mode is enabled.
    /// </summary>
    /// <param name="message">The message to write.</param>
    public void WriteLine(string message)
    {
        if (_isVerboseEnabled)
        {
            Console.WriteLine($"[VERBOSE] {message}");
            
            if (_fileLogger?.IsEnabled == true)
            {
                var entry = new LogEntry(
                    DateTime.UtcNow,
                    LogSeverity.Info,
                    "General",
                    message
                );
                _fileLogger.WriteEntryAsync(entry).GetAwaiter().GetResult();
            }
        }
    }

    /// <summary>
    /// Writes a verbose message with formatted arguments to the console and file if verbose mode is enabled.
    /// </summary>
    /// <param name="format">The format string.</param>
    /// <param name="args">The format arguments.</param>
    public void WriteLine(string format, params object[] args)
    {
        if (_isVerboseEnabled)
        {
            var message = string.Format(format, args);
            Console.WriteLine($"[VERBOSE] {message}");
            
            if (_fileLogger?.IsEnabled == true)
            {
                var entry = new LogEntry(
                    DateTime.UtcNow,
                    LogSeverity.Info,
                    "General",
                    message
                );
                _fileLogger.WriteEntryAsync(entry).GetAwaiter().GetResult();
            }
        }
    }

    /// <summary>
    /// Writes a verbose error message to the console and file if verbose mode is enabled.
    /// </summary>
    /// <param name="message">The error message to write.</param>
    public void WriteError(string message)
    {
        if (_isVerboseEnabled)
        {
            Console.Error.WriteLine($"[VERBOSE ERROR] {message}");
            
            if (_fileLogger?.IsEnabled == true)
            {
                var entry = new LogEntry(
                    DateTime.UtcNow,
                    LogSeverity.Error,
                    "General",
                    message
                );
                _fileLogger.WriteEntryAsync(entry).GetAwaiter().GetResult();
            }
        }
    }

    /// <summary>
    /// Writes an exception with full stack trace if verbose mode is enabled.
    /// </summary>
    /// <param name="ex">The exception to write.</param>
    public void WriteException(Exception ex)
    {
        if (_isVerboseEnabled)
        {
            Console.Error.WriteLine($"[VERBOSE EXCEPTION] {ex.GetType().Name}: {ex.Message}");
            Console.Error.WriteLine($"Stack Trace:\n{ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.Error.WriteLine($"Inner Exception: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
            }
            
            if (_fileLogger?.IsEnabled == true)
            {
                var message = $"{ex.GetType().Name}: {ex.Message}";
                if (ex.InnerException != null)
                {
                    message += $"\nInner Exception: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}";
                }
                
                var entry = new LogEntry(
                    DateTime.UtcNow,
                    LogSeverity.Exception,
                    "General",
                    message,
                    ex.StackTrace ?? string.Empty
                );
                _fileLogger.WriteEntryAsync(entry).GetAwaiter().GetResult();
            }
        }
    }

    /// <summary>
    /// Writes diagnostic information about the operation being performed.
    /// </summary>
    /// <param name="operation">The operation name.</param>
    /// <param name="details">The operation details.</param>
    public void WriteOperation(string operation, string details)
    {
        if (_isVerboseEnabled)
        {
            Console.WriteLine($"[VERBOSE OPERATION] {operation}: {details}");
            
            if (_fileLogger?.IsEnabled == true)
            {
                var entry = new LogEntry(
                    DateTime.UtcNow,
                    LogSeverity.Operation,
                    operation,
                    details
                );
                _fileLogger.WriteEntryAsync(entry).GetAwaiter().GetResult();
            }
        }
    }

    /// <summary>
    /// Gets the Windows version information.
    /// </summary>
    /// <returns>Windows version string.</returns>
    private static string GetWindowsVersion()
    {
        try
        {
            var version = Environment.OSVersion;
            return $"{version.VersionString} (Build {version.Version.Build})";
        }
        catch
        {
            return "Unknown";
        }
    }

    /// <summary>
    /// Disposes the verbose logger and associated resources.
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        if (_fileLogger != null)
        {
            _fileLogger.Dispose();
            _fileLogger = null;
        }

        GC.SuppressFinalize(this);
    }
}
