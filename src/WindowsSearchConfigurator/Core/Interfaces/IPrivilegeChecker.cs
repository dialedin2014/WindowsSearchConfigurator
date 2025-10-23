namespace WindowsSearchConfigurator.Core.Interfaces;

/// <summary>
/// Defines operations for checking user privileges.
/// </summary>
public interface IPrivilegeChecker
{
    /// <summary>
    /// Checks if the current user has administrator privileges.
    /// </summary>
    /// <returns>True if the user is an administrator; otherwise, false.</returns>
    bool IsAdministrator();

    /// <summary>
    /// Throws an exception if the current user does not have administrator privileges.
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user is not an administrator.</exception>
    void RequireAdministrator();

    /// <summary>
    /// Gets the current user identity in DOMAIN\Username format.
    /// </summary>
    /// <returns>The current user identity.</returns>
    string GetCurrentUser();
}
