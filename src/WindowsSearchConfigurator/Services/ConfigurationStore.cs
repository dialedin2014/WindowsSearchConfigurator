using System.Text.Json;
using System.Text.Json.Serialization;
using WindowsSearchConfigurator.Core.Interfaces;
using WindowsSearchConfigurator.Core.Models;

namespace WindowsSearchConfigurator.Services;

/// <summary>
/// Implements configuration import/export functionality using JSON serialization.
/// </summary>
public class ConfigurationStore : IConfigurationStore
{
    private readonly ISearchIndexManager _searchIndexManager;
    private readonly IAuditLogger _auditLogger;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationStore"/> class.
    /// </summary>
    public ConfigurationStore(
        ISearchIndexManager searchIndexManager,
        IAuditLogger auditLogger)
    {
        _searchIndexManager = searchIndexManager ?? throw new ArgumentNullException(nameof(searchIndexManager));
        _auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
    }

    /// <inheritdoc/>
    public async Task<OperationResult> ExportAsync(
        string filePath,
        bool includeDefaults = false,
        bool includeExtensions = true)
    {
        try
        {
            // Get all rules
            var rulesResult = await _searchIndexManager.GetAllRulesAsync(includeDefaults);
            if (!rulesResult.Success)
            {
                return OperationResult.Fail($"Failed to retrieve rules: {rulesResult.Message}");
            }

            var rules = rulesResult.Value?.ToList() ?? new List<IndexRule>();

            // Get extension settings if requested
            List<FileExtensionSetting>? extensions = null;
            if (includeExtensions)
            {
                var extensionsResult = await _searchIndexManager.GetExtensionSettingsAsync();
                if (extensionsResult.Success)
                {
                    extensions = extensionsResult.Value?.ToList();
                }
            }

            // Create configuration file model
            var config = new ConfigurationFile
            {
                Version = "1.0",
                ExportDate = DateTime.Now,
                ExportedBy = Environment.UserDomainName + "\\" + Environment.UserName,
                MachineName = Environment.MachineName,
                Rules = rules,
                ExtensionSettings = extensions ?? new List<FileExtensionSetting>()
            };

            // Serialize to JSON
            var json = JsonSerializer.Serialize(config, _jsonOptions);

            // Write to file
            await File.WriteAllTextAsync(filePath, json);

            _auditLogger.LogInfo($"Exported configuration to '{filePath}': {rules.Count} rules, " +
                                $"{config.ExtensionSettings.Count} extension settings");

            return OperationResult.Ok();
        }
        catch (UnauthorizedAccessException ex)
        {
            _auditLogger.LogError($"Access denied exporting to '{filePath}': {ex.Message}");
            return OperationResult.Fail($"Access denied: {ex.Message}");
        }
        catch (IOException ex)
        {
            _auditLogger.LogError($"I/O error exporting to '{filePath}': {ex.Message}");
            return OperationResult.Fail($"File error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _auditLogger.LogError($"Unexpected error exporting configuration: {ex.Message}", ex);
            return OperationResult.Fail($"Export failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<OperationResult<ImportResult>> ImportAsync(
        string filePath,
        bool merge = false,
        bool continueOnError = false)
    {
        try
        {
            // Read and parse JSON file
            if (!File.Exists(filePath))
            {
                return OperationResult<ImportResult>.Fail($"Configuration file not found: '{filePath}'");
            }

            var json = await File.ReadAllTextAsync(filePath);
            var config = JsonSerializer.Deserialize<ConfigurationFile>(json, _jsonOptions);

            if (config == null)
            {
                return OperationResult<ImportResult>.Fail("Failed to parse configuration file");
            }

            // Validate version
            if (config.Version != "1.0")
            {
                return OperationResult<ImportResult>.Fail(
                    $"Unsupported configuration version: {config.Version}. Expected: 1.0");
            }

            var result = new ImportResult
            {
                RulesImported = 0,
                RulesFailed = 0,
                ExtensionsImported = 0,
                ExtensionsFailed = 0,
                Errors = new List<string>()
            };

            // Import rules
            if (config.Rules != null && config.Rules.Any())
            {
                foreach (var rule in config.Rules)
                {
                    try
                    {
                        // Mark as imported source
                        rule.Source = RuleSource.Imported;
                        rule.CreatedDate = DateTime.Now;
                        rule.ModifiedDate = DateTime.Now;

                        var addResult = await _searchIndexManager.AddIndexRuleAsync(rule);
                        
                        if (addResult.Success)
                        {
                            result.RulesImported++;
                        }
                        else
                        {
                            result.RulesFailed++;
                            result.Errors.Add($"Rule '{rule.Path}': {addResult.Message}");
                            
                            if (!continueOnError)
                            {
                                return new OperationResult<ImportResult>
                                {
                                    Success = false,
                                    Message = $"Failed to import rule for '{rule.Path}': {addResult.Message}",
                                    Value = result
                                };
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        result.RulesFailed++;
                        result.Errors.Add($"Rule '{rule.Path}': {ex.Message}");
                        
                        if (!continueOnError)
                        {
                            return new OperationResult<ImportResult>
                            {
                                Success = false,
                                Message = $"Error importing rule for '{rule.Path}': {ex.Message}",
                                Value = result
                            };
                        }
                    }
                }
            }

            // Import extension settings
            if (config.ExtensionSettings != null && config.ExtensionSettings.Any())
            {
                foreach (var setting in config.ExtensionSettings)
                {
                    try
                    {
                        // Skip default settings (read-only)
                        if (setting.IsDefaultSetting)
                        {
                            continue;
                        }

                        var setResult = await _searchIndexManager.SetExtensionDepthAsync(
                            setting.Extension,
                            setting.IndexingDepth);
                        
                        if (setResult.Success)
                        {
                            result.ExtensionsImported++;
                        }
                        else
                        {
                            result.ExtensionsFailed++;
                            result.Errors.Add($"Extension '{setting.Extension}': {setResult.Message}");
                            
                            if (!continueOnError)
                            {
                                return new OperationResult<ImportResult>
                                {
                                    Success = false,
                                    Message = $"Failed to import extension '{setting.Extension}': {setResult.Message}",
                                    Value = result
                                };
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        result.ExtensionsFailed++;
                        result.Errors.Add($"Extension '{setting.Extension}': {ex.Message}");
                        
                        if (!continueOnError)
                        {
                            return new OperationResult<ImportResult>
                            {
                                Success = false,
                                Message = $"Error importing extension '{setting.Extension}': {ex.Message}",
                                Value = result
                            };
                        }
                    }
                }
            }

            _auditLogger.LogInfo(
                $"Imported configuration from '{filePath}': " +
                $"{result.RulesImported} rules imported, {result.RulesFailed} rules failed, " +
                $"{result.ExtensionsImported} extensions imported, {result.ExtensionsFailed} extensions failed");

            return OperationResult<ImportResult>.Ok(result);
        }
        catch (JsonException ex)
        {
            _auditLogger.LogError($"Invalid JSON in configuration file '{filePath}': {ex.Message}");
            return OperationResult<ImportResult>.Fail($"Invalid configuration file format: {ex.Message}");
        }
        catch (Exception ex)
        {
            _auditLogger.LogError($"Unexpected error importing configuration: {ex.Message}", ex);
            return OperationResult<ImportResult>.Fail($"Import failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<OperationResult<ValidationResult>> ValidateAsync(string filePath)
    {
        try
        {
            // Read and parse JSON file
            if (!File.Exists(filePath))
            {
                return OperationResult<ValidationResult>.Fail($"Configuration file not found: '{filePath}'");
            }

            var json = await File.ReadAllTextAsync(filePath);
            var config = JsonSerializer.Deserialize<ConfigurationFile>(json, _jsonOptions);

            if (config == null)
            {
                var validationResult = ValidationResult.Failure("Failed to parse configuration file");
                return new OperationResult<ValidationResult>
                {
                    Success = false,
                    Message = "Invalid configuration file",
                    Value = validationResult
                };
            }

            var issues = new List<string>();

            // Validate version
            if (config.Version != "1.0")
            {
                issues.Add($"Unsupported version: {config.Version}");
            }

            // Validate rules
            if (config.Rules != null)
            {
                foreach (var rule in config.Rules)
                {
                    if (string.IsNullOrWhiteSpace(rule.Path))
                    {
                        issues.Add("Rule with empty path found");
                    }

                    if (rule.Path != null && rule.Path.Length > 260)
                    {
                        issues.Add($"Path exceeds MAX_PATH (260 chars): {rule.Path}");
                    }
                }
            }

            // Validate extensions
            if (config.ExtensionSettings != null)
            {
                foreach (var setting in config.ExtensionSettings)
                {
                    if (string.IsNullOrWhiteSpace(setting.Extension))
                    {
                        issues.Add("Extension setting with empty extension found");
                    }
                    else if (!setting.Extension.StartsWith("."))
                    {
                        issues.Add($"Invalid extension format (must start with dot): {setting.Extension}");
                    }
                }
            }

            var validation = issues.Count == 0
                ? ValidationResult.Success(filePath)
                : ValidationResult.Failure(string.Join("; ", issues));

            _auditLogger.LogInfo($"Validated configuration file '{filePath}': {issues.Count} issues found");

            return OperationResult<ValidationResult>.Ok(validation);
        }
        catch (JsonException ex)
        {
            _auditLogger.LogError($"Invalid JSON in configuration file '{filePath}': {ex.Message}");
            var validationResult = ValidationResult.Failure($"Invalid JSON: {ex.Message}");
            return new OperationResult<ValidationResult>
            {
                Success = false,
                Message = "Validation failed",
                Value = validationResult
            };
        }
        catch (Exception ex)
        {
            _auditLogger.LogError($"Unexpected error validating configuration: {ex.Message}", ex);
            return OperationResult<ValidationResult>.Fail($"Validation failed: {ex.Message}");
        }
    }
}
