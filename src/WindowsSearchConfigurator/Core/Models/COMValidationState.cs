namespace WindowsSearchConfigurator.Core.Models;

/// <summary>
/// Represents the result of attempting to instantiate a COM object.
/// </summary>
public enum COMValidationState
{
    /// <summary>
    /// Validation has not been performed.
    /// </summary>
    NotChecked = 0,

    /// <summary>
    /// COM object successfully instantiated and disposed.
    /// </summary>
    Valid = 1,

    /// <summary>
    /// Type.GetTypeFromCLSID returned null.
    /// </summary>
    CLSIDNotFound = 2,

    /// <summary>
    /// Activator.CreateInstance threw an exception.
    /// </summary>
    InstantiationFailed = 3,

    /// <summary>
    /// COM-specific exception occurred during instantiation.
    /// </summary>
    COMException = 4,

    /// <summary>
    /// Unexpected error during validation.
    /// </summary>
    UnknownError = 5
}
