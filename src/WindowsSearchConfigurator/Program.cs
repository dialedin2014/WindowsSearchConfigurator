using Microsoft.Extensions.DependencyInjection;
using WindowsSearchConfigurator.Core.Interfaces;
using WindowsSearchConfigurator.Services;
using WindowsSearchConfigurator.Infrastructure;

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
    public static int Main(string[] args)
    {
        try
        {
            // Setup dependency injection
            var services = ConfigureServices();
            var serviceProvider = services.BuildServiceProvider();

            // Display startup message
            Console.WriteLine("Windows Search Configurator v1.0.0");
            Console.WriteLine("Foundation established successfully.");
            Console.WriteLine();
            Console.WriteLine("Commands will be added in Phase 3+ (User Stories):");
            Console.WriteLine("  - list: View current index rules");
            Console.WriteLine("  - add: Add new index rules");
            Console.WriteLine("  - remove: Remove index rules");
            Console.WriteLine("  - modify: Modify existing rules");
            Console.WriteLine("  - search-extensions: Search file extensions");
            Console.WriteLine("  - configure-depth: Configure indexing depth");
            Console.WriteLine("  - export: Export configuration to JSON");
            Console.WriteLine("  - import: Import configuration from JSON");
            Console.WriteLine();
            Console.WriteLine("Phase 2 (Foundational) is complete!");
            Console.WriteLine("All core models, interfaces, services, and infrastructure are ready.");

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fatal error: {ex.Message}");
            if (args.Contains("--verbose") || args.Contains("-v"))
            {
                Console.Error.WriteLine(ex.StackTrace);
            }
            return 1;
        }
    }

    /// <summary>
    /// Configures the dependency injection container.
    /// </summary>
    /// <returns>The configured service collection.</returns>
    private static IServiceCollection ConfigureServices()
    {
        var services = new ServiceCollection();

        // Register core interfaces and implementations
        services.AddSingleton<IPrivilegeChecker, PrivilegeChecker>();
        services.AddSingleton<IAuditLogger, AuditLogger>();
        services.AddSingleton<PathValidator>();

        // Register infrastructure services
        services.AddSingleton<RegistryAccessor>();
        services.AddSingleton<ServiceStatusChecker>();
        services.AddSingleton<WindowsSearchInterop>();

        // TODO: Register ISearchIndexManager and IConfigurationStore implementations
        // when they are created in later phases
        // services.AddSingleton<ISearchIndexManager, SearchIndexManager>();
        // services.AddSingleton<IConfigurationStore, ConfigurationStore>();

        return services;
    }
}
