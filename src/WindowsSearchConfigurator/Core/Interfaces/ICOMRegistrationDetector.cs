using WindowsSearchConfigurator.Core.Models;

namespace WindowsSearchConfigurator.Core.Interfaces;

/// <summary>
/// Provides COM API registration detection functionality.
/// </summary>
public interface ICOMRegistrationDetector
{
    /// <summary>
    /// Checks the current registration state of the COM API.
    /// </summary>
    /// <returns>The current COM registration status.</returns>
    COMRegistrationStatus GetRegistrationStatus();

    /// <summary>
    /// Checks if a specific CLSID is registered.
    /// </summary>
    /// <param name="clsid">The CLSID to check.</param>
    /// <returns>True if the CLSID is registered; otherwise, false.</returns>
    bool IsCLSIDRegistered(Guid clsid);

    /// <summary>
    /// Retrieves the DLL path for a specific CLSID from the registry.
    /// </summary>
    /// <param name="clsid">The CLSID to look up.</param>
    /// <returns>The DLL path if found; otherwise, null.</returns>
    string? GetDLLPath(Guid clsid);

    /// <summary>
    /// Attempts to instantiate a COM object to validate registration.
    /// </summary>
    /// <param name="clsid">The CLSID to validate.</param>
    /// <returns>The validation state.</returns>
    COMValidationState ValidateCOMObject(Guid clsid);
}
