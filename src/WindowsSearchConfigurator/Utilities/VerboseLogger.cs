using System;

namespace WindowsSearchConfigurator.Utilities;

/// <summary>
/// Provides verbose logging functionality for diagnostic output.
/// </summary>
public class VerboseLogger
{
    private bool _isVerboseEnabled;

    /// <summary>
    /// Gets or sets a value indicating whether verbose logging is enabled.
    /// </summary>
    public bool IsEnabled
    {
        get => _isVerboseEnabled;
        set => _isVerboseEnabled = value;
    }

    /// <summary>
    /// Writes a verbose message to the console if verbose mode is enabled.
    /// </summary>
    /// <param name="message">The message to write.</param>
    public void WriteLine(string message)
    {
        if (_isVerboseEnabled)
        {
            Console.WriteLine($"[VERBOSE] {message}");
        }
    }

    /// <summary>
    /// Writes a verbose message with formatted arguments to the console if verbose mode is enabled.
    /// </summary>
    /// <param name="format">The format string.</param>
    /// <param name="args">The format arguments.</param>
    public void WriteLine(string format, params object[] args)
    {
        if (_isVerboseEnabled)
        {
            Console.WriteLine($"[VERBOSE] {string.Format(format, args)}");
        }
    }

    /// <summary>
    /// Writes a verbose error message to the console if verbose mode is enabled.
    /// </summary>
    /// <param name="message">The error message to write.</param>
    public void WriteError(string message)
    {
        if (_isVerboseEnabled)
        {
            Console.Error.WriteLine($"[VERBOSE ERROR] {message}");
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
        }
    }
}
