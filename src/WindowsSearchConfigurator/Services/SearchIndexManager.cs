using System.Runtime.InteropServices;
using WindowsSearchConfigurator.Core.Interfaces;
using WindowsSearchConfigurator.Core.Models;
using WindowsSearchConfigurator.Infrastructure;

namespace WindowsSearchConfigurator.Services;

/// <summary>
/// Implements Windows Search index management operations using COM API and Registry.
/// </summary>
public class SearchIndexManager : ISearchIndexManager
{
    private readonly RegistryAccessor _registryAccessor;
    private readonly ServiceStatusChecker _serviceStatusChecker;
    private readonly WindowsSearchInterop _searchInterop;
    private readonly IAuditLogger _auditLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchIndexManager"/> class.
    /// </summary>
    public SearchIndexManager(
        RegistryAccessor registryAccessor,
        ServiceStatusChecker serviceStatusChecker,
        WindowsSearchInterop searchInterop,
        IAuditLogger auditLogger)
    {
        _registryAccessor = registryAccessor ?? throw new ArgumentNullException(nameof(registryAccessor));
        _serviceStatusChecker = serviceStatusChecker ?? throw new ArgumentNullException(nameof(serviceStatusChecker));
        _searchInterop = searchInterop ?? throw new ArgumentNullException(nameof(searchInterop));
        _auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
    }

    /// <inheritdoc/>
    public async Task<OperationResult<IEnumerable<IndexRule>>> GetAllRulesAsync(bool includeSystemRules = false)
    {
        try
        {
            // Check Windows Search service status
            var serviceStatus = _serviceStatusChecker.IsWindowsSearchRunning();
            if (!serviceStatus)
            {
                return OperationResult<IEnumerable<IndexRule>>.Fail(
                    "Windows Search service is not running. Please start the 'WSearch' service.");
            }

            // Get rules from Windows Search COM API
            var rules = await Task.Run(() => _searchInterop.EnumerateScopeRules(includeSystemRules));
            
            _auditLogger.LogInfo($"Retrieved {rules.Count()} index rules (includeSystemRules: {includeSystemRules})");

            return OperationResult<IEnumerable<IndexRule>>.Ok(rules);
        }
        catch (COMException ex)
        {
            var errorMessage = ex.ErrorCode switch
            {
                unchecked((int)0x80040D03) => "Duplicate scope rule detected.",
                unchecked((int)0x80040D04) => "Scope rule not found.",
                unchecked((int)0x80070005) => "Access denied. Administrator privileges required.",
                _ => $"COM error accessing Windows Search: {ex.Message} (HRESULT: 0x{ex.ErrorCode:X8})"
            };

            _auditLogger.LogError($"Error retrieving rules: {errorMessage}", ex);
            return OperationResult<IEnumerable<IndexRule>>.Fail(errorMessage);
        }
        catch (Exception ex)
        {
            _auditLogger.LogError($"Unexpected error retrieving rules: {ex.Message}", ex);
            return OperationResult<IEnumerable<IndexRule>>.Fail($"Failed to retrieve index rules: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<OperationResult<IEnumerable<FileExtensionSetting>>> GetExtensionSettingsAsync()
    {
        try
        {
            // Read extension settings from Registry
            const string fileTypesKeyPath = @"SOFTWARE\Microsoft\Windows Search\Preferences\FileTypes";
            
            var settings = await Task.Run(() => _registryAccessor.EnumerateFileTypeSettings(fileTypesKeyPath));

            _auditLogger.LogInfo($"Retrieved {settings.Count()} file extension settings");

            return OperationResult<IEnumerable<FileExtensionSetting>>.Ok(settings);
        }
        catch (Exception ex)
        {
            _auditLogger.LogError($"Error retrieving extension settings: {ex.Message}");
            return OperationResult<IEnumerable<FileExtensionSetting>>.Fail(
                $"Failed to retrieve file extension settings: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<OperationResult> AddIndexRuleAsync(IndexRule rule)
    {
        try
        {
            // Check Windows Search service status
            var serviceStatus = _serviceStatusChecker.IsWindowsSearchRunning();
            if (!serviceStatus)
            {
                return OperationResult.Fail(
                    "Windows Search service is not running. Please start the 'WSearch' service.");
            }

            // Check for duplicate rule
            var existingRules = await GetAllRulesAsync(includeSystemRules: true);
            if (existingRules.Success)
            {
                var duplicate = existingRules.Value?.FirstOrDefault(r => 
                    string.Equals(r.Path, rule.Path, StringComparison.OrdinalIgnoreCase));
                
                if (duplicate != null)
                {
                    return OperationResult.Fail(
                        $"A rule for path '{rule.Path}' already exists. " +
                        "Use the modify command to update it or remove it first.");
                }
            }

            // Add rule using COM API
            await Task.Run(() => _searchInterop.AddUserScopeRule(rule));

            _auditLogger.LogRuleAdded(rule.Path, Environment.UserName); _auditLogger.LogInfo($"Added index rule: Path={rule.Path}, Type={rule.RuleType}, Recursive={rule.Recursive}");

            return OperationResult.Ok();
        }
        catch (COMException ex)
        {
            var errorMessage = ex.ErrorCode switch
            {
                unchecked((int)0x80040D03) => "A rule for this path already exists.",
                unchecked((int)0x80070005) => "Access denied. Administrator privileges required.",
                _ => $"COM error adding rule: {ex.Message} (HRESULT: 0x{ex.ErrorCode:X8})"
            };

            _auditLogger.LogError($"Error adding rule for '{rule.Path}': {errorMessage}");
            return OperationResult.Fail(errorMessage);
        }
        catch (Exception ex)
        {
            _auditLogger.LogError($"Unexpected error adding rule: {ex.Message}");
            return OperationResult.Fail($"Failed to add index rule: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<OperationResult> RemoveIndexRuleAsync(string path)
    {
        try
        {
            // Check Windows Search service status
            var serviceStatus = _serviceStatusChecker.IsWindowsSearchRunning();
            if (!serviceStatus)
            {
                return OperationResult.Fail(
                    "Windows Search service is not running. Please start the 'WSearch' service.");
            }

            // Remove rule using COM API
            await Task.Run(() => _searchInterop.RemoveScopeRule(path));

            _auditLogger.LogRuleRemoved(path, Environment.UserName);

            return OperationResult.Ok();
        }
        catch (COMException ex)
        {
            var errorMessage = ex.ErrorCode switch
            {
                unchecked((int)0x80040D04) => $"No index rule found for path '{path}'.",
                unchecked((int)0x80070005) => "Access denied. Administrator privileges required.",
                _ => $"COM error removing rule: {ex.Message} (HRESULT: 0x{ex.ErrorCode:X8})"
            };

            _auditLogger.LogError($"Error removing rule for '{path}': {errorMessage}");
            return OperationResult.Fail(errorMessage);
        }
        catch (Exception ex)
        {
            _auditLogger.LogError($"Unexpected error removing rule: {ex.Message}");
            return OperationResult.Fail($"Failed to remove index rule: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<OperationResult> ModifyIndexRuleAsync(IndexRule rule)
    {
        try
        {
            // Check Windows Search service status
            var serviceStatus = _serviceStatusChecker.IsWindowsSearchRunning();
            if (!serviceStatus)
            {
                return OperationResult.Fail(
                    "Windows Search service is not running. Please start the 'WSearch' service.");
            }

            // Verify rule exists
            var existingRules = await GetAllRulesAsync(includeSystemRules: true);
            if (existingRules.Success)
            {
                var existing = existingRules.Value?.FirstOrDefault(r => 
                    string.Equals(r.Path, rule.Path, StringComparison.OrdinalIgnoreCase));
                
                if (existing == null)
                {
                    return OperationResult.Fail(
                        $"No index rule found for path '{rule.Path}'. Use the add command to create a new rule.");
                }
            }

            // Modify by removing and re-adding (atomic operation)
            await Task.Run(() =>
            {
                _searchInterop.RemoveScopeRule(rule.Path);
                _searchInterop.AddUserScopeRule(rule);
            });

            _auditLogger.LogRuleModified(rule.Path, Environment.UserName, $"Type={rule.RuleType}, Recursive={rule.Recursive}"); _auditLogger.LogInfo($"Modified index rule: Path={rule.Path}, Type={rule.RuleType}, Recursive={rule.Recursive}");

            return OperationResult.Ok();
        }
        catch (COMException ex)
        {
            var errorMessage = ex.ErrorCode switch
            {
                unchecked((int)0x80040D04) => $"No index rule found for path '{rule.Path}'.",
                unchecked((int)0x80070005) => "Access denied. Administrator privileges required.",
                _ => $"COM error modifying rule: {ex.Message} (HRESULT: 0x{ex.ErrorCode:X8})"
            };

            _auditLogger.LogError($"Error modifying rule for '{rule.Path}': {errorMessage}");
            return OperationResult.Fail(errorMessage);
        }
        catch (Exception ex)
        {
            _auditLogger.LogError($"Unexpected error modifying rule: {ex.Message}");
            return OperationResult.Fail($"Failed to modify index rule: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<OperationResult<IEnumerable<FileExtensionSetting>>> SearchExtensionsAsync(string pattern)
    {
        try
        {
            // Get all extension settings
            var allSettings = await GetExtensionSettingsAsync();
            if (!allSettings.Success)
            {
                return OperationResult<IEnumerable<FileExtensionSetting>>.Fail(allSettings.Message!);
            }

            // Filter by pattern using WildcardMatcher
            var filtered = allSettings.Value!.Where(setting => 
                Utilities.WildcardMatcher.IsMatch(setting.Extension, pattern));

            _auditLogger.LogInfo(
                $"Searched extensions with pattern '{pattern}': {filtered.Count()} matches");

            return OperationResult<IEnumerable<FileExtensionSetting>>.Ok(filtered);
        }
        catch (Exception ex)
        {
            _auditLogger.LogError($"Error searching extensions: {ex.Message}");
            return OperationResult<IEnumerable<FileExtensionSetting>>.Fail(
                $"Failed to search extensions: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<OperationResult> SetExtensionDepthAsync(string extension, IndexingDepth depth)
    {
        try
        {
            // Ensure extension starts with a dot
            if (!extension.StartsWith("."))
            {
                extension = "." + extension;
            }

            // Set depth in Registry
            const string fileTypesKeyPath = @"SOFTWARE\Microsoft\Windows Search\Preferences\FileTypes";
            
            await Task.Run(() => _registryAccessor.SetFileTypeDepth(fileTypesKeyPath, extension, depth));

            _auditLogger.LogInfo(
                $"Set indexing depth for '{extension}' to {depth}");

            return OperationResult.Ok();
        }
        catch (UnauthorizedAccessException)
        {
            _auditLogger.LogError($"Access denied setting depth for '{extension}'");
            return OperationResult.Fail(
                "Access denied. Administrator privileges required to modify file extension settings.");
        }
        catch (Exception ex)
        {
            _auditLogger.LogError($"Error setting depth for '{extension}': {ex.Message}");
            return OperationResult.Fail(
                $"Failed to set indexing depth for '{extension}': {ex.Message}");
        }
    }
}
