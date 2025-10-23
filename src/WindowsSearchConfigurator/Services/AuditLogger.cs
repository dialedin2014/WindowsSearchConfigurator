namespace WindowsSearchConfigurator.Services;

using WindowsSearchConfigurator.Core.Interfaces;

/// <summary>
/// Provides file-based audit logging functionality.
/// </summary>
public class AuditLogger : IAuditLogger
{
    private readonly string _logFilePath;
    private readonly object _lockObject = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditLogger"/> class.
    /// </summary>
    /// <param name="logFilePath">The path to the log file. If null, uses default location.</param>
    public AuditLogger(string? logFilePath = null)
    {
        if (string.IsNullOrWhiteSpace(logFilePath))
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var logDir = Path.Combine(appDataPath, "WindowsSearchConfigurator", "Logs");
            Directory.CreateDirectory(logDir);
            _logFilePath = Path.Combine(logDir, $"audit_{DateTime.Now:yyyyMMdd}.log");
        }
        else
        {
            _logFilePath = logFilePath;
            var directory = Path.GetDirectoryName(_logFilePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }

    /// <inheritdoc/>
    public void LogInfo(string message, string? details = null)
    {
        WriteLog("INFO", message, details);
    }

    /// <inheritdoc/>
    public void LogWarning(string message, string? details = null)
    {
        WriteLog("WARN", message, details);
    }

    /// <inheritdoc/>
    public void LogError(string message, Exception? exception = null)
    {
        var details = exception != null ? $"{exception.Message}\n{exception.StackTrace}" : null;
        WriteLog("ERROR", message, details);
    }

    /// <inheritdoc/>
    public void LogRuleAdded(string path, string user)
    {
        WriteLog("INFO", $"Rule added: {path}", $"User: {user}");
    }

    /// <inheritdoc/>
    public void LogRuleRemoved(string path, string user)
    {
        WriteLog("INFO", $"Rule removed: {path}", $"User: {user}");
    }

    /// <inheritdoc/>
    public void LogRuleModified(string path, string user, string changes)
    {
        WriteLog("INFO", $"Rule modified: {path}", $"User: {user}\nChanges: {changes}");
    }

    /// <inheritdoc/>
    public void LogConfigurationExported(string filePath, string user, int ruleCount)
    {
        WriteLog("INFO", $"Configuration exported to: {filePath}", $"User: {user}\nRules: {ruleCount}");
    }

    /// <inheritdoc/>
    public void LogConfigurationImported(string filePath, string user, int successCount, int failCount)
    {
        WriteLog("INFO", $"Configuration imported from: {filePath}", 
            $"User: {user}\nSuccessful: {successCount}\nFailed: {failCount}");
    }

    private void WriteLog(string level, string message, string? details = null)
    {
        try
        {
            lock (_lockObject)
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var logEntry = $"[{timestamp}] [{level}] {message}";
                
                if (!string.IsNullOrWhiteSpace(details))
                {
                    logEntry += $"\n  Details: {details}";
                }
                
                logEntry += Environment.NewLine;

                File.AppendAllText(_logFilePath, logEntry);
            }
        }
        catch (Exception ex)
        {
            // If logging fails, write to console as fallback
            Console.Error.WriteLine($"Failed to write to audit log: {ex.Message}");
        }
    }
}
