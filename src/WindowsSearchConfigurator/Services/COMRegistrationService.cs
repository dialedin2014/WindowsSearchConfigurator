using System.Diagnostics;
using System.Runtime.InteropServices;
using WindowsSearchConfigurator.Core.Interfaces;
using WindowsSearchConfigurator.Core.Models;
using WindowsSearchConfigurator.Utilities;

namespace WindowsSearchConfigurator.Services;

/// <summary>
/// Service for orchestrating COM API registration workflow.
/// </summary>
public class COMRegistrationService : ICOMRegistrationService
{
    private const string SEARCH_MANAGER_CLSID = "7D096C5F-AC08-4F1F-BEB7-5C22C517CE39";
    private static readonly Guid SearchManagerGuid = new Guid(SEARCH_MANAGER_CLSID);

    private readonly ICOMRegistrationDetector _detector;
    private readonly IPrivilegeChecker _privilegeChecker;
    private readonly VerboseLogger _verboseLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="COMRegistrationService"/> class.
    /// </summary>
    /// <param name="detector">The COM registration detector.</param>
    /// <param name="privilegeChecker">The privilege checker.</param>
    /// <param name="verboseLogger">The verbose logger.</param>
    public COMRegistrationService(
        ICOMRegistrationDetector detector,
        IPrivilegeChecker privilegeChecker,
        VerboseLogger verboseLogger)
    {
        _detector = detector ?? throw new ArgumentNullException(nameof(detector));
        _privilegeChecker = privilegeChecker ?? throw new ArgumentNullException(nameof(privilegeChecker));
        _verboseLogger = verboseLogger ?? throw new ArgumentNullException(nameof(verboseLogger));
    }

    /// <inheritdoc/>
    public async Task<COMRegistrationAttempt> RegisterCOMAsync(RegistrationOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        options.Validate();

        var attempt = new COMRegistrationAttempt
        {
            AttemptId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Mode = RegistrationMode.Manual,
            User = Environment.UserName,
            IsAdministrator = _privilegeChecker.IsAdministrator(),
            DLLPath = options.DLLPath ?? GetDefaultDLLPath()
        };

        _verboseLogger.WriteLine($"Starting COM registration attempt {attempt.AttemptId}");
        _verboseLogger.WriteLine($"User: {attempt.User} (Admin: {attempt.IsAdministrator})");
        _verboseLogger.WriteLine($"DLL Path: {attempt.DLLPath}");

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Check if already registered
            var status = _detector.GetRegistrationStatus();
            if (status.IsRegistered)
            {
                _verboseLogger.WriteLine("COM API already registered, skipping registration.");
                attempt.Outcome = RegistrationOutcome.Success;
                attempt.DurationMs = (int)stopwatch.ElapsedMilliseconds;
                attempt.PostValidation = COMValidationState.Valid;
                return attempt;
            }

            // Verify DLL exists
            if (!File.Exists(attempt.DLLPath))
            {
                _verboseLogger.WriteLine($"DLL not found: {attempt.DLLPath}");
                attempt.Outcome = RegistrationOutcome.DLLNotFound;
                attempt.ErrorMessage = $"DLL not found: {attempt.DLLPath}";
                attempt.DurationMs = (int)stopwatch.ElapsedMilliseconds;
                return attempt;
            }

            // Check privileges
            if (!attempt.IsAdministrator)
            {
                _verboseLogger.WriteLine("Insufficient privileges for registration.");
                attempt.Outcome = RegistrationOutcome.InsufficientPrivileges;
                attempt.ErrorMessage = "Administrator privileges required for COM registration.";
                attempt.DurationMs = (int)stopwatch.ElapsedMilliseconds;
                return attempt;
            }

            // Execute regsvr32
            _verboseLogger.WriteLine($"Executing: regsvr32 /s \"{attempt.DLLPath}\"");
            var result = await ExecuteRegsvr32Async(attempt.DLLPath, options.TimeoutSeconds, options.Silent);

            attempt.ExitCode = result.ExitCode;
            attempt.Outcome = result.ExitCode == 0 ? RegistrationOutcome.Success : RegistrationOutcome.Failed;

            if (result.ExitCode != 0)
            {
                attempt.ErrorMessage = $"regsvr32 failed with exit code {result.ExitCode}";
                if (!string.IsNullOrWhiteSpace(result.Error))
                {
                    attempt.ErrorMessage += $": {result.Error}";
                }
            }

            // Validate registration
            var postStatus = _detector.GetRegistrationStatus();
            attempt.PostValidation = postStatus.ValidationState;

            if (result.ExitCode == 0 && !postStatus.IsRegistered)
            {
                _verboseLogger.WriteLine("WARNING: regsvr32 succeeded but COM validation failed.");
                attempt.Outcome = RegistrationOutcome.ValidationFailed;
                attempt.ErrorMessage = "Registration appeared successful but COM object cannot be validated.";
            }

            stopwatch.Stop();
            attempt.DurationMs = (int)stopwatch.ElapsedMilliseconds;

            _verboseLogger.WriteLine($"Registration attempt completed: {attempt.Outcome} ({attempt.DurationMs}ms)");

            return attempt;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            attempt.DurationMs = (int)stopwatch.ElapsedMilliseconds;
            attempt.Outcome = RegistrationOutcome.Failed;
            attempt.ErrorMessage = $"Unexpected error: {ex.Message}";
            _verboseLogger.WriteLine($"Registration failed with exception: {ex}");
            return attempt;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> HandleMissingRegistration(string[] args)
    {
        _verboseLogger.WriteLine("Handling missing COM registration...");

        // Parse flags
        var options = ParseRegistrationOptions(args);

        // Auto-register mode
        if (options.AutoRegister)
        {
            _verboseLogger.WriteLine("Auto-register mode enabled.");
            var attempt = await RegisterCOMAsync(options);
            
            if (attempt.Outcome == RegistrationOutcome.Success)
            {
                ConsoleFormatter.ShowCOMRegistrationSuccess();
                return true;
            }
            else
            {
                ConsoleFormatter.ShowCOMRegistrationError(attempt);
                return false;
            }
        }

        // No-register mode
        if (options.NoRegister)
        {
            _verboseLogger.WriteLine("No-register mode enabled, exiting.");
            return false;
        }

        // Interactive mode
        var choice = ShowRegistrationPrompt();

        switch (choice.ToLowerInvariant())
        {
            case "a":
            case "accept":
                _verboseLogger.WriteLine("User accepted registration offer.");
                
                if (!_privilegeChecker.IsAdministrator())
                {
                    ShowElevationInstructions();
                    return false;
                }

                var registrationAttempt = await RegisterCOMAsync(options);
                
                if (registrationAttempt.Outcome == RegistrationOutcome.Success)
                {
                    ConsoleFormatter.ShowCOMRegistrationSuccess();
                    return true;
                }
                else
                {
                    ConsoleFormatter.ShowCOMRegistrationError(registrationAttempt);
                    return false;
                }

            case "d":
            case "decline":
                _verboseLogger.WriteLine("User declined registration offer.");
                ShowManualInstructions();
                return false;

            case "q":
            case "quit":
                _verboseLogger.WriteLine("User chose to quit.");
                return false;

            default:
                _verboseLogger.WriteLine($"Invalid choice: {choice}");
                Console.WriteLine("Invalid choice. Exiting.");
                return false;
        }
    }

    /// <inheritdoc/>
    public string ShowRegistrationPrompt()
    {
        Console.WriteLine();
        ConsoleFormatter.WriteColored("Windows Search COM API Registration Required", ConsoleColor.Yellow);
        Console.WriteLine();
        Console.WriteLine("The Windows Search COM API is not registered on this system.");
        Console.WriteLine("Without it, this tool cannot interact with the Windows Search service.");
        Console.WriteLine();
        Console.WriteLine("Would you like to attempt automatic registration?");
        Console.WriteLine();
        Console.WriteLine("  [A] Accept - Attempt automatic registration (requires admin privileges)");
        Console.WriteLine("  [D] Decline - Show manual registration instructions");
        Console.WriteLine("  [Q] Quit - Exit the application");
        Console.WriteLine();
        Console.Write("Your choice [A/D/Q]: ");
        
        var choice = Console.ReadLine()?.Trim() ?? "q";
        return choice;
    }

    /// <inheritdoc/>
    public void ShowManualInstructions()
    {
        Console.WriteLine();
        ConsoleFormatter.WriteColored("Manual COM Registration Instructions", ConsoleColor.Cyan);
        Console.WriteLine();
        Console.WriteLine("To manually register the Windows Search COM API:");
        Console.WriteLine();
        Console.WriteLine("1. Open an elevated Command Prompt (Run as Administrator)");
        Console.WriteLine();
        Console.WriteLine("2. Run the following command:");
        Console.WriteLine();
        
        var dllPath = GetDefaultDLLPath();
        ConsoleFormatter.WriteColored($"   regsvr32 \"{dllPath}\"", ConsoleColor.White);
        Console.WriteLine();
        Console.WriteLine();
        
        Console.WriteLine("3. You should see a success message: \"DllRegisterServer in [path] succeeded.\"");
        Console.WriteLine();
        Console.WriteLine("4. Re-run this tool to verify registration.");
        Console.WriteLine();
    }

    /// <inheritdoc/>
    public void ShowElevationInstructions()
    {
        Console.WriteLine();
        ConsoleFormatter.WriteColored("Administrator Privileges Required", ConsoleColor.Yellow);
        Console.WriteLine();
        Console.WriteLine("COM registration requires administrator privileges.");
        Console.WriteLine();
        Console.WriteLine("To complete registration:");
        Console.WriteLine();
        Console.WriteLine("1. Close this application");
        Console.WriteLine("2. Right-click on Command Prompt or PowerShell");
        Console.WriteLine("3. Select 'Run as Administrator'");
        Console.WriteLine("4. Navigate back to this tool's directory");
        Console.WriteLine("5. Re-run the tool - it will offer registration again");
        Console.WriteLine();
        Console.WriteLine("Alternatively, use manual registration (see manual instructions).");
        Console.WriteLine();
    }

    /// <summary>
    /// Parses command-line arguments for registration options.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>Parsed registration options.</returns>
    private RegistrationOptions ParseRegistrationOptions(string[] args)
    {
        var options = new RegistrationOptions();

        if (args == null)
        {
            return options;
        }

        foreach (var arg in args)
        {
            if (arg.Equals("--auto-register-com", StringComparison.OrdinalIgnoreCase))
            {
                options.AutoRegister = true;
            }
            else if (arg.Equals("--no-register-com", StringComparison.OrdinalIgnoreCase))
            {
                options.NoRegister = true;
            }
            else if (arg.Equals("--silent", StringComparison.OrdinalIgnoreCase))
            {
                options.Silent = true;
            }
        }

        return options;
    }

    /// <summary>
    /// Gets the default DLL path for Windows Search COM API.
    /// </summary>
    /// <returns>The default DLL path.</returns>
    private string GetDefaultDLLPath()
    {
        // Try to get from registry first
        var registryPath = _detector.GetDLLPath(SearchManagerGuid);
        if (!string.IsNullOrEmpty(registryPath))
        {
            return Environment.ExpandEnvironmentVariables(registryPath);
        }

        // Fall back to default location
        var systemRoot = Environment.GetEnvironmentVariable("SystemRoot") ?? @"C:\Windows";
        return Path.Combine(systemRoot, "System32", "SearchAPI.dll");
    }

    /// <summary>
    /// Executes regsvr32 to register the COM DLL.
    /// </summary>
    /// <param name="dllPath">Path to the DLL to register.</param>
    /// <param name="timeoutSeconds">Timeout in seconds.</param>
    /// <param name="silent">Whether to run in silent mode.</param>
    /// <returns>A task representing the process result.</returns>
    private async Task<(int ExitCode, string Error)> ExecuteRegsvr32Async(string dllPath, int timeoutSeconds, bool silent)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return (-1, "regsvr32 is only available on Windows.");
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "regsvr32.exe",
            Arguments = silent ? $"/s \"{dllPath}\"" : $"\"{dllPath}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        var errorOutput = string.Empty;

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                errorOutput += e.Data + Environment.NewLine;
            }
        };

        process.Start();
        process.BeginErrorReadLine();

        var completedInTime = await Task.Run(() => process.WaitForExit(timeoutSeconds * 1000));

        if (!completedInTime)
        {
            try
            {
                process.Kill();
            }
            catch
            {
                // Best effort kill
            }
            return (-1, $"Registration timed out after {timeoutSeconds} seconds.");
        }

        return (process.ExitCode, errorOutput);
    }
}
