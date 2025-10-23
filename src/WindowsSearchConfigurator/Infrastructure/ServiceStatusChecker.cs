namespace WindowsSearchConfigurator.Infrastructure;

using System.ServiceProcess;

/// <summary>
/// Provides functionality to check Windows Search service status.
/// </summary>
public class ServiceStatusChecker
{
    private const string WINDOWS_SEARCH_SERVICE_NAME = "WSearch";

    /// <summary>
    /// Checks if the Windows Search service is running.
    /// </summary>
    /// <returns>True if the service is running; otherwise, false.</returns>
    public bool IsWindowsSearchRunning()
    {
        try
        {
            using var service = new ServiceController(WINDOWS_SEARCH_SERVICE_NAME);
            return service.Status == ServiceControllerStatus.Running;
        }
        catch (InvalidOperationException)
        {
            // Service not found
            return false;
        }
        catch (Exception)
        {
            // Other errors (permissions, etc.)
            return false;
        }
    }

    /// <summary>
    /// Gets the current status of the Windows Search service.
    /// </summary>
    /// <returns>The service status, or null if the service is not found.</returns>
    public ServiceControllerStatus? GetWindowsSearchStatus()
    {
        try
        {
            using var service = new ServiceController(WINDOWS_SEARCH_SERVICE_NAME);
            return service.Status;
        }
        catch (InvalidOperationException)
        {
            // Service not found
            return null;
        }
        catch (Exception)
        {
            // Other errors
            return null;
        }
    }

    /// <summary>
    /// Checks if the Windows Search service exists on this system.
    /// </summary>
    /// <returns>True if the service exists; otherwise, false.</returns>
    public bool DoesWindowsSearchServiceExist()
    {
        try
        {
            using var service = new ServiceController(WINDOWS_SEARCH_SERVICE_NAME);
            // Try to access a property to verify the service exists
            var _ = service.Status;
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        catch (Exception)
        {
            // If we get other exceptions, the service likely exists but we can't access it
            return true;
        }
    }

    /// <summary>
    /// Gets a user-friendly status message for the Windows Search service.
    /// </summary>
    /// <returns>A status message describing the service state.</returns>
    public string GetStatusMessage()
    {
        if (!DoesWindowsSearchServiceExist())
        {
            return "Windows Search service is not installed on this system.";
        }

        var status = GetWindowsSearchStatus();
        if (status == null)
        {
            return "Unable to determine Windows Search service status.";
        }

        return status switch
        {
            ServiceControllerStatus.Running => "Windows Search service is running.",
            ServiceControllerStatus.Stopped => "Windows Search service is stopped. Start the service to enable indexing.",
            ServiceControllerStatus.Paused => "Windows Search service is paused.",
            ServiceControllerStatus.StartPending => "Windows Search service is starting...",
            ServiceControllerStatus.StopPending => "Windows Search service is stopping...",
            ServiceControllerStatus.ContinuePending => "Windows Search service is resuming...",
            ServiceControllerStatus.PausePending => "Windows Search service is pausing...",
            _ => $"Windows Search service status: {status}"
        };
    }
}
