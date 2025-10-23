namespace WindowsSearchConfigurator.Core.Interfaces;

/// <summary>
/// Defines operations for audit logging.
/// </summary>
public interface IAuditLogger
{
    /// <summary>
    /// Logs an informational message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="details">Optional additional details.</param>
    void LogInfo(string message, string? details = null);

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="details">Optional additional details.</param>
    void LogWarning(string message, string? details = null);

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="exception">Optional exception details.</param>
    void LogError(string message, Exception? exception = null);

    /// <summary>
    /// Logs a rule addition operation.
    /// </summary>
    /// <param name="path">The path that was added.</param>
    /// <param name="user">The user who performed the operation.</param>
    void LogRuleAdded(string path, string user);

    /// <summary>
    /// Logs a rule removal operation.
    /// </summary>
    /// <param name="path">The path that was removed.</param>
    /// <param name="user">The user who performed the operation.</param>
    void LogRuleRemoved(string path, string user);

    /// <summary>
    /// Logs a rule modification operation.
    /// </summary>
    /// <param name="path">The path that was modified.</param>
    /// <param name="user">The user who performed the operation.</param>
    /// <param name="changes">Description of the changes made.</param>
    void LogRuleModified(string path, string user, string changes);

    /// <summary>
    /// Logs a configuration export operation.
    /// </summary>
    /// <param name="filePath">The file path where configuration was exported.</param>
    /// <param name="user">The user who performed the operation.</param>
    /// <param name="ruleCount">Number of rules exported.</param>
    void LogConfigurationExported(string filePath, string user, int ruleCount);

    /// <summary>
    /// Logs a configuration import operation.
    /// </summary>
    /// <param name="filePath">The file path from which configuration was imported.</param>
    /// <param name="user">The user who performed the operation.</param>
    /// <param name="successCount">Number of rules successfully imported.</param>
    /// <param name="failCount">Number of rules that failed to import.</param>
    void LogConfigurationImported(string filePath, string user, int successCount, int failCount);
}
