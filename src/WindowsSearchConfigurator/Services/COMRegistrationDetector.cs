using Microsoft.Win32;
using System.Runtime.InteropServices;
using WindowsSearchConfigurator.Core.Interfaces;
using WindowsSearchConfigurator.Core.Models;

namespace WindowsSearchConfigurator.Services;

/// <summary>
/// Detects COM API registration status for Windows Search.
/// </summary>
public class COMRegistrationDetector : ICOMRegistrationDetector
{
    private const string SEARCH_MANAGER_CLSID = "7D096C5F-AC08-4F1F-BEB7-5C22C517CE39";
    private static readonly Guid SearchManagerGuid = new Guid(SEARCH_MANAGER_CLSID);

    /// <inheritdoc/>
    public COMRegistrationStatus GetRegistrationStatus()
    {
        var status = new COMRegistrationStatus
        {
            DetectionTimestamp = DateTime.UtcNow,
            ValidationState = COMValidationState.NotChecked
        };

        try
        {
            // Check if CLSID exists in registry
            status.CLSIDExists = IsCLSIDRegistered(SearchManagerGuid);

            if (status.CLSIDExists)
            {
                // Get DLL path from registry
                status.DLLPath = GetDLLPath(SearchManagerGuid);

                if (!string.IsNullOrEmpty(status.DLLPath))
                {
                    // Expand environment variables like %SystemRoot%
                    var expandedPath = Environment.ExpandEnvironmentVariables(status.DLLPath);
                    status.DLLPath = expandedPath;
                    status.DLLExists = File.Exists(expandedPath);
                }

                // Attempt COM object validation
                status.ValidationState = ValidateCOMObject(SearchManagerGuid);
            }

            // IsRegistered is true only if all conditions are met
            status.IsRegistered = status.CLSIDExists && 
                                  status.DLLExists && 
                                  status.ValidationState == COMValidationState.Valid;
        }
        catch (Exception ex)
        {
            status.IsRegistered = false;
            status.ErrorMessage = $"Detection failed: {ex.Message}";
        }

        return status;
    }

    /// <inheritdoc/>
    public bool IsCLSIDRegistered(Guid clsid)
    {
        try
        {
            using var key = Registry.ClassesRoot.OpenSubKey($"CLSID\\{{{clsid}}}");
            return key != null;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public string? GetDLLPath(Guid clsid)
    {
        try
        {
            using var key = Registry.ClassesRoot.OpenSubKey($"CLSID\\{{{clsid}}}\\InprocServer32");
            return key?.GetValue(null) as string;
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public COMValidationState ValidateCOMObject(Guid clsid)
    {
        try
        {
            // Attempt to get COM type from CLSID
            var type = Type.GetTypeFromCLSID(clsid);
            if (type == null)
            {
                return COMValidationState.CLSIDNotFound;
            }

            // Attempt to instantiate the COM object
            var instance = Activator.CreateInstance(type);
            if (instance == null)
            {
                return COMValidationState.InstantiationFailed;
            }

            // Clean up COM object
            if (Marshal.IsComObject(instance))
            {
                Marshal.ReleaseComObject(instance);
            }

            return COMValidationState.Valid;
        }
        catch (COMException)
        {
            return COMValidationState.COMException;
        }
        catch (Exception)
        {
            return COMValidationState.UnknownError;
        }
    }
}
