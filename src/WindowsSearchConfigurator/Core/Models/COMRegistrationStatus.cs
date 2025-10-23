namespace WindowsSearchConfigurator.Core.Models;

/// <summary>
/// Represents the current state of the Microsoft.Search.Interop.CSearchManager COM API registration.
/// </summary>
public class COMRegistrationStatus
{
    /// <summary>
    /// Gets or sets a value indicating whether the COM API is properly registered.
    /// IsRegistered is true only if CLSIDExists AND DLLExists AND ValidationState == Valid.
    /// </summary>
    public bool IsRegistered { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the CLSID key exists in HKEY_CLASSES_ROOT.
    /// </summary>
    public bool CLSIDExists { get; set; }

    /// <summary>
    /// Gets or sets the path to the COM DLL (searchapi.dll) from InprocServer32 registry value.
    /// </summary>
    public string? DLLPath { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the DLL file exists at the specified path.
    /// </summary>
    public bool DLLExists { get; set; }

    /// <summary>
    /// Gets or sets the result of the COM object instantiation test.
    /// </summary>
    public COMValidationState ValidationState { get; set; }

    /// <summary>
    /// Gets or sets when the detection was performed (UTC).
    /// </summary>
    public DateTime DetectionTimestamp { get; set; }

    /// <summary>
    /// Gets or sets error details if detection failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
