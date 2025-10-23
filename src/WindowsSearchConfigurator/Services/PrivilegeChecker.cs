namespace WindowsSearchConfigurator.Services;

using System.Security.Principal;
using WindowsSearchConfigurator.Core.Interfaces;

/// <summary>
/// Provides privilege checking functionality for the application.
/// </summary>
public class PrivilegeChecker : IPrivilegeChecker
{
    /// <inheritdoc/>
    public bool IsAdministrator()
    {
        try
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public void RequireAdministrator()
    {
        if (!IsAdministrator())
        {
            throw new UnauthorizedAccessException(
                "This operation requires administrator privileges. " +
                "Please run the application as an administrator."
            );
        }
    }

    /// <inheritdoc/>
    public string GetCurrentUser()
    {
        return WindowsIdentity.GetCurrent().Name;
    }
}
