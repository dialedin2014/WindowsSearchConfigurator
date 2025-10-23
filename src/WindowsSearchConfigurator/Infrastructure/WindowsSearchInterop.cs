using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using WindowsSearchConfigurator.Core.Models;

namespace WindowsSearchConfigurator.Infrastructure;

/// <summary>
/// Provides COM API interop for Windows Search operations.
/// </summary>
/// <remarks>
/// Windows Search uses the ISearchCrawlScopeManager COM interface for managing index rules.
/// COM interfaces are accessed dynamically to avoid complex PInvoke declarations.
/// </remarks>
[SupportedOSPlatform("windows")]
public class WindowsSearchInterop
{
    private const string SEARCH_MANAGER_PROGID = "SearchIndexer.CSearchManager";
    private const string SYSTEM_CATALOG = "SystemIndex";

    /// <summary>
    /// Checks if Windows Search COM APIs are available on this system.
    /// </summary>
    /// <returns>True if Windows Search APIs are available; otherwise, false.</returns>
    public bool IsAvailable()
    {
        try
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return false;
            }

            // Try to create the COM object
            var type = Type.GetTypeFromProgID(SEARCH_MANAGER_PROGID);
            return type != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Enumerates all scope rules from Windows Search.
    /// </summary>
    /// <param name="includeSystemRules">Whether to include system default rules.</param>
    /// <returns>A collection of index rules.</returns>
    public IEnumerable<IndexRule> EnumerateScopeRules(bool includeSystemRules)
    {
        var rules = new List<IndexRule>();

        try
        {
            dynamic? searchManager = Activator.CreateInstance(Type.GetTypeFromProgID(SEARCH_MANAGER_PROGID)!);
            dynamic? catalog = searchManager!.GetCatalog(SYSTEM_CATALOG);
            dynamic? scopeManager = catalog.GetCrawlScopeManager();

            // Get the scope rule enumerator
            dynamic? ruleEnumerator = scopeManager.EnumerateScopeRules();

            // Enumerate all rules
            while (true)
            {
                dynamic? rule = null;
                uint fetched = 0;

                // Fetch next rule
                ruleEnumerator.Next(1, out rule, out fetched);

                if (fetched == 0 || rule == null)
                {
                    break;
                }

                // Get rule properties
                string url = rule?.PatternOrURL ?? string.Empty;
                bool isIncluded = rule?.IsIncluded ?? false;
                bool isDefault = rule?.IsDefault ?? false;

                // Filter system rules if requested
                if (!includeSystemRules && isDefault)
                {
                    continue;
                }

                // Create IndexRule object
                var indexRule = new IndexRule
                {
                    Id = Guid.NewGuid(),
                    Path = url,
                    RuleType = isIncluded ? RuleType.Include : RuleType.Exclude,
                    Recursive = true, // Windows Search rules are typically recursive
                    Source = isDefault ? RuleSource.System : RuleSource.User,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now,
                    FileTypeFilters = new List<FileTypeFilter>(),
                    ExcludedSubfolders = new List<string>()
                };

                rules.Add(indexRule);

                // Release COM object
                Marshal.ReleaseComObject(rule);
            }

            // Cleanup COM objects
            if (ruleEnumerator != null)
            {
                Marshal.ReleaseComObject(ruleEnumerator);
            }
            if (scopeManager != null)
            {
                Marshal.ReleaseComObject(scopeManager);
            }
            if (catalog != null)
            {
                Marshal.ReleaseComObject(catalog);
            }
            if (searchManager != null)
            {
                Marshal.ReleaseComObject(searchManager);
            }
        }
        catch (COMException)
        {
            throw; // Rethrow COM exceptions for proper error handling
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to enumerate scope rules: {ex.Message}", ex);
        }

        return rules;
    }

    /// <summary>
    /// Adds a user-defined scope rule to Windows Search.
    /// </summary>
    /// <param name="rule">The rule to add.</param>
    public void AddUserScopeRule(IndexRule rule)
    {
        try
        {
            dynamic? searchManager = Activator.CreateInstance(Type.GetTypeFromProgID(SEARCH_MANAGER_PROGID)!);
            dynamic? catalog = searchManager!.GetCatalog(SYSTEM_CATALOG);
            dynamic? scopeManager = catalog.GetCrawlScopeManager();

            // Add the rule
            int isIncluded = rule.RuleType == RuleType.Include ? 1 : 0;
            scopeManager.AddUserScopeRule(rule.Path, isIncluded, true, 0);

            // Save changes
            scopeManager.SaveAll();

            // Cleanup COM objects
            if (scopeManager != null)
            {
                Marshal.ReleaseComObject(scopeManager);
            }
            if (catalog != null)
            {
                Marshal.ReleaseComObject(catalog);
            }
            if (searchManager != null)
            {
                Marshal.ReleaseComObject(searchManager);
            }
        }
        catch (COMException)
        {
            throw; // Rethrow COM exceptions for proper error handling
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to add scope rule: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Removes a scope rule from Windows Search.
    /// </summary>
    /// <param name="path">The path of the rule to remove.</param>
    public void RemoveScopeRule(string path)
    {
        try
        {
            dynamic? searchManager = Activator.CreateInstance(Type.GetTypeFromProgID(SEARCH_MANAGER_PROGID)!);
            dynamic? catalog = searchManager!.GetCatalog(SYSTEM_CATALOG);
            dynamic? scopeManager = catalog.GetCrawlScopeManager();

            // Remove the rule
            scopeManager.RemoveScopeRule(path);

            // Save changes
            scopeManager.SaveAll();

            // Cleanup COM objects
            if (scopeManager != null)
            {
                Marshal.ReleaseComObject(scopeManager);
            }
            if (catalog != null)
            {
                Marshal.ReleaseComObject(catalog);
            }
            if (searchManager != null)
            {
                Marshal.ReleaseComObject(searchManager);
            }
        }
        catch (COMException)
        {
            throw; // Rethrow COM exceptions for proper error handling
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to remove scope rule: {ex.Message}", ex);
        }
    }
}
