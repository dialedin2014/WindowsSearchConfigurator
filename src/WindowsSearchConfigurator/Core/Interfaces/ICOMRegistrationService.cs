using WindowsSearchConfigurator.Core.Models;

namespace WindowsSearchConfigurator.Core.Interfaces;

/// <summary>
/// Provides COM API registration orchestration functionality.
/// </summary>
public interface ICOMRegistrationService
{
    /// <summary>
    /// Performs COM API registration based on provided options.
    /// </summary>
    /// <param name="options">The registration options.</param>
    /// <returns>A task representing the registration attempt with outcome.</returns>
    Task<COMRegistrationAttempt> RegisterCOMAsync(RegistrationOptions options);

    /// <summary>
    /// Handles missing COM registration during application startup.
    /// </summary>
    /// <param name="args">Command-line arguments for parsing registration flags.</param>
    /// <returns>A task representing whether the issue was resolved (true) or application should exit (false).</returns>
    Task<bool> HandleMissingRegistration(string[] args);

    /// <summary>
    /// Displays an interactive prompt for registration confirmation.
    /// </summary>
    /// <returns>The user's choice (Accept, Decline, Quit).</returns>
    string ShowRegistrationPrompt();

    /// <summary>
    /// Displays manual registration instructions.
    /// </summary>
    void ShowManualInstructions();

    /// <summary>
    /// Shows elevation guidance when user lacks admin privileges.
    /// </summary>
    void ShowElevationInstructions();
}
