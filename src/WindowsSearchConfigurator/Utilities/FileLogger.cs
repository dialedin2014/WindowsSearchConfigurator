using System.Text;
using WindowsSearchConfigurator.Core.Interfaces;
using WindowsSearchConfigurator.Core.Models;

namespace WindowsSearchConfigurator.Utilities;

/// <summary>
/// Implements file logging operations with thread-safe async writes.
/// </summary>
public class FileLogger : IFileLogger
{
    private StreamWriter? _fileWriter;
    private readonly SemaphoreSlim _fileLock = new(1, 1);
    private bool _isDisposed;

    /// <summary>
    /// Gets a value indicating whether file logging is currently active.
    /// </summary>
    public bool IsEnabled => _fileWriter != null;

    /// <summary>
    /// Initializes the file logger with the specified log file path.
    /// </summary>
    /// <param name="logFilePath">Full path to the log file.</param>
    /// <returns>True if initialization succeeded, false otherwise.</returns>
    public bool Initialize(string logFilePath)
    {
        if (string.IsNullOrWhiteSpace(logFilePath))
        {
            return false;
        }

        try
        {
            // Attempt to create the log file
            _fileWriter = new StreamWriter(logFilePath, append: false, encoding: Encoding.UTF8)
            {
                AutoFlush = false // We'll flush manually for better control
            };
            return true;
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"WARNING: Cannot create log file (access denied): {logFilePath}");
            Console.WriteLine("Verbose output will be written to console only.");
            Console.WriteLine($"Details: {ex.Message}");
            _fileWriter = null;
            return false;
        }
        catch (IOException ex)
        {
            Console.WriteLine($"WARNING: Cannot create log file (I/O error): {logFilePath}");
            Console.WriteLine("Verbose output will be written to console only.");
            Console.WriteLine($"Details: {ex.Message}");
            _fileWriter = null;
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WARNING: Cannot create log file: {logFilePath}");
            Console.WriteLine("Verbose output will be written to console only.");
            Console.WriteLine($"Details: {ex.Message}");
            _fileWriter = null;
            return false;
        }
    }

    /// <summary>
    /// Writes a log entry to the file asynchronously.
    /// </summary>
    /// <param name="entry">The log entry to write.</param>
    public async Task WriteEntryAsync(LogEntry entry)
    {
        if (_fileWriter == null || _isDisposed)
        {
            return;
        }

        await _fileLock.WaitAsync();
        try
        {
            var timestamp = entry.Timestamp.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'");
            var severityStr = entry.Severity.ToString().ToUpperInvariant();
            var line = $"[{timestamp}] [{severityStr}] {entry.Component}: {entry.Message}";
            
            await _fileWriter.WriteLineAsync(line);
            
            if (!string.IsNullOrEmpty(entry.StackTrace))
            {
                await _fileWriter.WriteLineAsync("Stack Trace:");
                await _fileWriter.WriteLineAsync(entry.StackTrace);
            }
            
            await _fileWriter.FlushAsync();
        }
        catch (Exception ex)
        {
            // Disable file logging on write failure
            Console.WriteLine($"WARNING: Failed to write to log file: {ex.Message}");
            Console.WriteLine("File logging disabled. Continuing with console output only.");
            await DisableFileLoggingAsync();
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// Writes the session header to the log file.
    /// </summary>
    /// <param name="session">The session metadata.</param>
    public async Task WriteSessionHeaderAsync(LogSession session)
    {
        if (_fileWriter == null || _isDisposed)
        {
            return;
        }

        await _fileLock.WaitAsync();
        try
        {
            await _fileWriter.WriteLineAsync("================================ LOG SESSION START ================================");
            await _fileWriter.WriteLineAsync($"Session ID: {session.SessionId}");
            await _fileWriter.WriteLineAsync($"Start Time: {session.StartTime:yyyy-MM-dd'T'HH:mm:ss.fff'Z'}");
            await _fileWriter.WriteLineAsync($"Command: {session.CommandLine}");
            await _fileWriter.WriteLineAsync($"User: {session.UserName}");
            await _fileWriter.WriteLineAsync($"Working Directory: {session.WorkingDirectory}");
            await _fileWriter.WriteLineAsync($"Windows Version: {session.WindowsVersion}");
            await _fileWriter.WriteLineAsync($".NET Runtime: {session.RuntimeVersion}");
            await _fileWriter.WriteLineAsync("===================================================================================");
            await _fileWriter.FlushAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WARNING: Failed to write session header: {ex.Message}");
            await DisableFileLoggingAsync();
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// Writes the session footer to the log file.
    /// </summary>
    /// <param name="session">The session metadata.</param>
    public async Task WriteSessionFooterAsync(LogSession session)
    {
        if (_fileWriter == null || _isDisposed)
        {
            return;
        }

        await _fileLock.WaitAsync();
        try
        {
            var durationStr = session.Duration.HasValue 
                ? $"{session.Duration.Value.TotalSeconds:F3} seconds" 
                : "N/A";
            var statusStr = session.Status.ToString().ToUpperInvariant();
            
            await _fileWriter.WriteLineAsync("================================= LOG SESSION END =================================");
            await _fileWriter.WriteLineAsync($"End Time: {session.EndTime:yyyy-MM-dd'T'HH:mm:ss.fff'Z'}");
            await _fileWriter.WriteLineAsync($"Duration: {durationStr}");
            await _fileWriter.WriteLineAsync($"Exit Code: {session.ExitCode ?? -1}");
            await _fileWriter.WriteLineAsync($"Status: {statusStr}");
            await _fileWriter.WriteLineAsync("===================================================================================");
            await _fileWriter.FlushAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WARNING: Failed to write session footer: {ex.Message}");
            await DisableFileLoggingAsync();
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// Disables file logging by closing and disposing the file writer.
    /// </summary>
    private async Task DisableFileLoggingAsync()
    {
        if (_fileWriter != null)
        {
            try
            {
                await _fileWriter.FlushAsync();
                _fileWriter.Dispose();
            }
            catch
            {
                // Ignore errors during cleanup
            }
            finally
            {
                _fileWriter = null;
            }
        }
    }

    /// <summary>
    /// Disposes the file logger and releases resources.
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        _fileLock.Wait();
        try
        {
            if (_fileWriter != null)
            {
                _fileWriter.Flush();
                _fileWriter.Dispose();
                _fileWriter = null;
            }
        }
        finally
        {
            _fileLock.Release();
            _fileLock.Dispose();
        }

        GC.SuppressFinalize(this);
    }
}
