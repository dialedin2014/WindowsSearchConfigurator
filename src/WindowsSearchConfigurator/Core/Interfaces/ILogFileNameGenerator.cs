namespace WindowsSearchConfigurator.Core.Interfaces;

/// <summary>
/// Interface for generating unique log file names.
/// </summary>
public interface ILogFileNameGenerator
{
    /// <summary>
    /// Generates a unique log file name with timestamp and GUID.
    /// </summary>
    /// <returns>Log file name (not full path).</returns>
    string GenerateFileName();
}
