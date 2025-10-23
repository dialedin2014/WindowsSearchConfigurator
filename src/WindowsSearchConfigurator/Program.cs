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

            // Setup dependency injection with verbose logger
            var services = ConfigureServices(verboseMode);
            var serviceProvider = services.BuildServiceProvider();

            var verboseLogger = serviceProvider.GetRequiredService<VerboseLogger>();
            verboseLogger.WriteLine("Windows Search Configurator starting...");
            verboseLogger.WriteLine($"Command-line arguments: {string.Join(" ", args)}");

            // Handle version option early
            if (args.Contains("--version"))
            {
                Console.WriteLine("Windows Search Configurator v1.0.0");
                Console.WriteLine("Copyright (c) 2025");
                Console.WriteLine("Target: Windows 10/11, Windows Server 2016+");
                Console.WriteLine($".NET Runtime: {Environment.Version}");
                return 0;
            }

            // Check COM API registration before proceeding (US1: Detection and Notification)
            verboseLogger.WriteLine("Checking COM API registration status...");
            var comDetector = serviceProvider.GetRequiredService<ICOMRegistrationDetector>();
            var registrationStatus = comDetector.GetRegistrationStatus();
            
            if (!registrationStatus.IsRegistered)
            {
                verboseLogger.WriteLine($"COM API not registered. CLSID exists: {registrationStatus.CLSIDExists}, DLL exists: {registrationStatus.DLLExists}, Validation: {registrationStatus.ValidationState}");
                ConsoleFormatter.ShowCOMNotRegisteredError(registrationStatus);
                
                // For now (US1 only), we just exit with an error
                // US2-US4 will add registration options here
                Console.WriteLine();
                Console.WriteLine("Please ensure Windows Search is installed and the COM API is registered.");
                return 1;
            }
            
            verboseLogger.WriteLine("COM API is registered and functional.");

            // Create root command
            var rootCommand = new RootCommand("Windows Search Configurator - Manage Windows Search index rules");
            
            // Add global verbose option
            var verboseOption = new Option<bool>(
                new[] { "--verbose", "-v" },
                description: "Enable verbose diagnostic output");
            rootCommand.AddGlobalOption(verboseOption);

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
