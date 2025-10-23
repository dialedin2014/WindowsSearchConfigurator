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
            // Setup dependency injection
            var services = ConfigureServices();
            var serviceProvider = services.BuildServiceProvider();

            // Handle version option early
            if (args.Contains("--version"))
            {
                Console.WriteLine("Windows Search Configurator v1.0.0");
                Console.WriteLine("Copyright (c) 2025");
                Console.WriteLine("Target: Windows 10/11, Windows Server 2016+");
                Console.WriteLine($".NET Runtime: {Environment.Version}");
                return 0;
            }

            // Create root command
            var rootCommand = new RootCommand("Windows Search Configurator - Manage Windows Search index rules");

            // Register commands
            var searchIndexManager = serviceProvider.GetRequiredService<ISearchIndexManager>();
            var consoleFormatter = serviceProvider.GetRequiredService<ConsoleFormatter>();

            rootCommand.Add(ListCommand.Create(searchIndexManager, consoleFormatter));

            // Execute command
            return await rootCommand.InvokeAsync(args);
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

        // Register search index manager
        services.AddSingleton<ISearchIndexManager, SearchIndexManager>();

        // Register utilities
        services.AddSingleton<ConsoleFormatter>();

        // TODO: Register IConfigurationStore implementation when created
        // services.AddSingleton<IConfigurationStore, ConfigurationStore>();

        return services;
    }
}
