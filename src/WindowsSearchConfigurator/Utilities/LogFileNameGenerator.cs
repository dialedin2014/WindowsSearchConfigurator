namespace WindowsSearchConfigurator.Utilities;

/// <summary>
/// Generates unique log file names with timestamp and GUID for the Windows Search Configurator.
/// </summary>
public class LogFileNameGenerator : Core.Interfaces.ILogFileNameGenerator
{
    /// <summary>
    /// Generates a unique log file name with timestamp and GUID.
    /// Pattern: WindowsSearchConfigurator_{yyyyMMdd_HHmmss}_{guid}.log
    /// </summary>
    /// <returns>Log file name (not full path).</returns>
    public string GenerateFileName()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var guidPart = Guid.NewGuid().ToString("N")[..6]; // First 6 characters
        return $"WindowsSearchConfigurator_{timestamp}_{guidPart}.log";
    }
}
