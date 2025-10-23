using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using WindowsSearchConfigurator.Commands;
using WindowsSearchConfigurator.Core.Interfaces;
using WindowsSearchConfigurator.Infrastructure;
using WindowsSearchConfigurator.Services;
using WindowsSearchConfigurator.Utilities;

namespace WindowsSearchConfigurator;

/// <summary>
/// Main entry point for the Windows Search Configurator application.
/// </summary>
public class Program
{
    /// <summary>
    /// Application entry point.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>Exit code.</returns>
    public static async Task<int> Main(string[] args)
    {
        try
        {
            // Check for verbose mode early
            bool verboseMode = args.Contains("--verbose") || args.Contains("-v");

            // US4: Check for COM registration flags
            bool autoRegisterCOM = args.Contains("--auto-register-com");
            bool noRegisterCOM = args.Contains("--no-register-com");

            // Validate mutual exclusivity of flags
            if (autoRegisterCOM && noRegisterCOM)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: --auto-register-com and --no-register-com are mutually exclusive.");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("You cannot specify both flags at the same time.");
                Console.WriteLine("Use --auto-register-com to automatically register if needed,");
                Console.WriteLine("or --no-register-com to fail immediately if not registered.");
                return 3; // Conflict error
            }

            // Setup dependency injection with verbose logger
            var services = ConfigureServices(verboseMode);
            var serviceProvider = services.BuildServiceProvider();

            var verboseLogger = serviceProvider.GetRequiredService<VerboseLogger>();
            verboseLogger.WriteLine("Windows Search Configurator starting...");
            verboseLogger.WriteLine($"Command-line arguments: {string.Join(" ", args)}");
            if (autoRegisterCOM)
            {
                verboseLogger.WriteLine("Flag detected: --auto-register-com");
            }
            if (noRegisterCOM)
            {
                verboseLogger.WriteLine("Flag detected: --no-register-com");
            }

            // Handle version option early
            if (args.Contains("--version"))
            {
                Console.WriteLine("Windows Search Configurator v1.0.0");
                Console.WriteLine("Copyright (c) 2025");
                Console.WriteLine("Target: Windows 10/11, Windows Server 2016+");
                Console.WriteLine($".NET Runtime: {Environment.Version}");
                return 0;
            }

            // Skip COM check for informational commands that don't require COM API
            bool isInfoCommand = args.Contains("--help") || args.Contains("-h") || args.Contains("-?");
            verboseLogger.WriteLine($"Informational command detected: {isInfoCommand}");

            // Check COM API registration before proceeding (US1: Detection and Notification)
            // Skip if this is just a help/info request
            if (!isInfoCommand)
            {
                verboseLogger.WriteLine("Checking COM API registration status...");
                var comDetector = serviceProvider.GetRequiredService<ICOMRegistrationDetector>();
                var registrationStatus = comDetector.GetRegistrationStatus();
            
                if (!registrationStatus.IsRegistered)
            {
                verboseLogger.WriteLine($"COM API not registered. CLSID exists: {registrationStatus.CLSIDExists}, DLL exists: {registrationStatus.DLLExists}, Validation: {registrationStatus.ValidationState}");
                
                // US4: Handle --no-register-com flag (fail immediately)
                if (noRegisterCOM)
                {
                    verboseLogger.WriteLine("--no-register-com flag specified. Exiting without registration.");
                    ConsoleFormatter.ShowCOMNotRegisteredError(registrationStatus);
                    Console.WriteLine();
                    Console.WriteLine("This application requires the COM API to function. Registration was not attempted");
                    Console.WriteLine("because --no-register-com flag was specified.");
                    Console.WriteLine();
                    Console.WriteLine("To register manually, run:");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("  regsvr32 \"%SystemRoot%\\System32\\SearchAPI.dll\"");
                    Console.ResetColor();
                    Console.WriteLine();
                    Console.WriteLine("Then restart WindowsSearchConfigurator.");
                    return 1;
                }
                
                // US4: Handle --auto-register-com flag (automatic registration without prompt)
                if (autoRegisterCOM)
                {
                    verboseLogger.WriteLine("--auto-register-com flag specified. Attempting automatic registration.");
                    Console.WriteLine("INFO: Microsoft Windows Search COM API is not registered.");
                    Console.WriteLine("Attempting automatic registration...");
                    Console.WriteLine();
                    
                    // Check for administrative privileges
                    var privCheckerAuto = serviceProvider.GetRequiredService<IPrivilegeChecker>();
                    if (!privCheckerAuto.IsAdministrator())
                    {
                        verboseLogger.WriteLine("User lacks administrative privileges for automatic registration.");
                        ConsoleFormatter.ShowElevationInstructions();
                        return 2; // Elevation needed
                    }
                    
                    verboseLogger.WriteLine("User has administrative privileges. Proceeding with automatic registration.");
                    
                    // Attempt registration
                    ConsoleFormatter.ShowCOMRegistrationInProgress();
                    var comRegistrationServiceAuto = serviceProvider.GetRequiredService<ICOMRegistrationService>();
                    var autoOptions = new Core.Models.RegistrationOptions
                    {
                        AutoRegister = true,
                        Silent = false,
                        TimeoutSeconds = 30
                    };
                    
                    var autoAttempt = await comRegistrationServiceAuto.RegisterCOMAsync(autoOptions);
                    verboseLogger.WriteLine($"Automatic registration attempt completed: {autoAttempt.Outcome}");
                    
                    if (autoAttempt.Outcome == Core.Models.RegistrationOutcome.Success)
                    {
                        ConsoleFormatter.ShowCOMRegistrationSuccess();
                        
                        // Validate registration
                        var newStatusAuto = comDetector.GetRegistrationStatus();
                        if (newStatusAuto.IsRegistered)
                        {
                            ConsoleFormatter.ShowCOMValidationSuccess();
                        }
                        else
                        {
                            verboseLogger.WriteLine("Warning: Registration reported success but validation failed.");
                            ConsoleFormatter.ShowCOMRegistrationError(autoAttempt);
                            Console.WriteLine();
                            ConsoleFormatter.ShowManualCOMRegistrationInstructions();
                            return 1;
                        }
                    }
                    else
                    {
                        ConsoleFormatter.ShowCOMRegistrationError(autoAttempt);
                        Console.WriteLine();
                        ConsoleFormatter.ShowManualCOMRegistrationInstructions();
                        return 1;
                    }
                }
                else
                {
                    // US2-US3: Interactive registration workflow (no flags specified)
                    ConsoleFormatter.ShowCOMNotRegisteredError(registrationStatus);
                    
                    // Prompt user for action
                    var choice = ConsoleFormatter.PromptCOMRegistration();
                    
                    if (choice == 'Q')
                    {
                        verboseLogger.WriteLine("User chose to quit without registering.");
                        return 1;
                    }
                    
                    if (choice == 'N')
                    {
                        verboseLogger.WriteLine("User chose to see manual registration instructions.");
                        ConsoleFormatter.ShowManualCOMRegistrationInstructions();
                        return 1;
                    }
                    
                    // User chose 'Y' - attempt registration
                    verboseLogger.WriteLine("User chose to attempt automatic registration.");
                    
                    // US3: Check for administrative privileges
                    var privChecker = serviceProvider.GetRequiredService<IPrivilegeChecker>();
                    if (!privChecker.IsAdministrator())
                    {
                        verboseLogger.WriteLine("User lacks administrative privileges.");
                        ConsoleFormatter.ShowElevationInstructions();
                        return 2; // Elevation needed
                    }
                    
                    verboseLogger.WriteLine("User has administrative privileges. Proceeding with registration.");
                    
                    // Attempt registration
                    ConsoleFormatter.ShowCOMRegistrationInProgress();
                    var comRegistrationService = serviceProvider.GetRequiredService<ICOMRegistrationService>();
                    var registrationOptions = new Core.Models.RegistrationOptions
                    {
                        AutoRegister = false, // Interactive mode
                        Silent = false,
                        TimeoutSeconds = 30
                    };
                    
                    var registrationAttempt = await comRegistrationService.RegisterCOMAsync(registrationOptions);
                    verboseLogger.WriteLine($"Registration attempt completed: {registrationAttempt.Outcome}");
                    
                    if (registrationAttempt.Outcome == Core.Models.RegistrationOutcome.Success)
                    {
                        ConsoleFormatter.ShowCOMRegistrationSuccess();
                        
                        // Validate registration
                        var newStatus = comDetector.GetRegistrationStatus();
                        if (newStatus.IsRegistered)
                        {
                            ConsoleFormatter.ShowCOMValidationSuccess();
                        }
                        else
                        {
                            verboseLogger.WriteLine("Warning: Registration reported success but validation failed.");
                            ConsoleFormatter.ShowCOMRegistrationError(registrationAttempt);
                            Console.WriteLine();
                            ConsoleFormatter.ShowManualCOMRegistrationInstructions();
                            return 1;
                        }
                    }
                    else
                    {
                        ConsoleFormatter.ShowCOMRegistrationError(registrationAttempt);
                        Console.WriteLine();
                        ConsoleFormatter.ShowManualCOMRegistrationInstructions();
                        return 1;
                    }
                }
                }
            
                verboseLogger.WriteLine("COM API is registered and functional.");
            } // End of if (!isInfoCommand)

            // Create root command
            var rootCommand = new RootCommand("Windows Search Configurator - Manage Windows Search index rules");
            
            // Add global verbose option
            var verboseOption = new Option<bool>(
                new[] { "--verbose", "-v" },
                description: "Enable verbose diagnostic output");
            rootCommand.AddGlobalOption(verboseOption);

            // US4: Add global COM registration options
            var autoRegisterOption = new Option<bool>(
                "--auto-register-com",
                description: "Automatically register COM API if not registered (requires admin privileges)");
            rootCommand.AddGlobalOption(autoRegisterOption);

            var noRegisterOption = new Option<bool>(
                "--no-register-com",
                description: "Do not attempt COM API registration, exit immediately if not registered");
            rootCommand.AddGlobalOption(noRegisterOption);

            // Register commands
            var searchIndexManager = serviceProvider.GetRequiredService<ISearchIndexManager>();
            var consoleFormatter = serviceProvider.GetRequiredService<ConsoleFormatter>();
            var privilegeChecker = serviceProvider.GetRequiredService<IPrivilegeChecker>();
            var pathValidator = serviceProvider.GetRequiredService<PathValidator>();
            var auditLogger = serviceProvider.GetRequiredService<IAuditLogger>();

            var configurationStore = serviceProvider.GetRequiredService<IConfigurationStore>();

            rootCommand.Add(ListCommand.Create(searchIndexManager, consoleFormatter));
            rootCommand.Add(AddCommand.Create(searchIndexManager, privilegeChecker, pathValidator, auditLogger));
            rootCommand.Add(RemoveCommand.Create(searchIndexManager, privilegeChecker, pathValidator, auditLogger));
            rootCommand.Add(ModifyCommand.Create(searchIndexManager, privilegeChecker, pathValidator, auditLogger));
            rootCommand.Add(SearchExtensionsCommand.Create(searchIndexManager, consoleFormatter));
            rootCommand.Add(ConfigureDepthCommand.Create(searchIndexManager, privilegeChecker, auditLogger));
            rootCommand.Add(ExportCommand.Create(configurationStore, auditLogger));
            rootCommand.Add(ImportCommand.Create(configurationStore, privilegeChecker, auditLogger));

            // Execute command
            return await rootCommand.InvokeAsync(args);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fatal error: {ex.Message}");
            
            // Show detailed error information in verbose mode
            bool verboseMode = args.Contains("--verbose") || args.Contains("-v");
            if (verboseMode)
            {
                Console.Error.WriteLine($"\nException Type: {ex.GetType().Name}");
                Console.Error.WriteLine($"Stack Trace:\n{ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.Error.WriteLine($"\nInner Exception: {ex.InnerException.GetType().Name}");
                    Console.Error.WriteLine($"Inner Message: {ex.InnerException.Message}");
                }
            }
            return 1;
        }
    }

    /// <summary>
    /// Configures the dependency injection container.
    /// </summary>
    /// <param name="verboseMode">Whether verbose logging is enabled.</param>
    /// <returns>The configured service collection.</returns>
    private static IServiceCollection ConfigureServices(bool verboseMode)
    {
        var services = new ServiceCollection();

        // Register verbose logger
        var verboseLogger = new VerboseLogger { IsEnabled = verboseMode };
        services.AddSingleton(verboseLogger);

        // Register core interfaces and implementations
        services.AddSingleton<IPrivilegeChecker, PrivilegeChecker>();
        services.AddSingleton<IAuditLogger, AuditLogger>();
        services.AddSingleton<PathValidator>();

        // Register COM registration services
        services.AddSingleton<ICOMRegistrationDetector, COMRegistrationDetector>();
        services.AddSingleton<ICOMRegistrationService, COMRegistrationService>();

        // Register infrastructure services
        services.AddSingleton<RegistryAccessor>();
        services.AddSingleton<ServiceStatusChecker>();
        services.AddSingleton<WindowsSearchInterop>();

        // Register search index manager
        services.AddSingleton<ISearchIndexManager, SearchIndexManager>();

        // Register utilities
        services.AddSingleton<ConsoleFormatter>();

        // Register configuration store
        services.AddSingleton<IConfigurationStore, ConfigurationStore>();

        return services;
    }
}
