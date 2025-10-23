namespace WindowsSearchConfigurator.Core.Models;

/// <summary>
/// Represents how COM registration was initiated.
/// </summary>
public enum RegistrationMode
{
    /// <summary>
    /// User responded to interactive prompt.
    /// </summary>
    Interactive = 0,

    /// <summary>
    /// Triggered by --auto-register-com flag.
    /// </summary>
    Automatic = 1,

    /// <summary>
    /// User manually runs registration command (future: wsc register-com).
    /// </summary>
    Manual = 2,

    /// <summary>
    /// User declined registration offer in interactive mode.
    /// </summary>
    Declined = 3
}
