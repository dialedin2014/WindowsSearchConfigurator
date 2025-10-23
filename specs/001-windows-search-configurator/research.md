# Research: Windows Search Configurator

**Feature**: Windows Search Index Rules Manager  
**Date**: 2025-10-22  
**Phase**: 0 - Outline & Research

## Research Tasks

This document consolidates research findings for technical decisions and best practices needed for implementation.

---

## 1. Windows Search API Integration

### Decision
Use **Windows Search Manager COM API** (ISearchManager, ISearchCatalogManager, ISearchCrawlScopeManager) as the primary interface for managing index rules.

### Rationale
- **Official supported API**: Microsoft's documented approach for programmatic search index management
- **Comprehensive functionality**: Supports all required operations (add/remove scope rules, enumerate rules, configure filters)
- **Version compatibility**: Available and stable across Windows 10/11 and Server 2016+
- **Type safety**: COM interop provides strongly-typed interfaces through runtime callable wrappers (RCW)

### Alternatives Considered
1. **Registry-only approach**
   - Rejected: Fragile, undocumented schema changes between Windows versions, requires deep Windows internals knowledge
   - Registry is used supplementarily for reading certain configuration details not exposed via COM

2. **WMI (Windows Management Instrumentation)**
   - Rejected: Limited Windows Search management capabilities, primarily for querying not configuration
   - WMI is used supplementarily for service status checking (Win32_Service class)

3. **PowerShell cmdlets (Search-Index module)**
   - Rejected: Requires PowerShell runtime, not pure .NET solution, limits control and error handling
   - Cmdlets are reference implementations but not suitable for programmatic use

### Implementation Approach
```csharp
// COM interop in C#
using Microsoft.Search.Interop;

// Key interfaces:
// - ISearchManager: Entry point to Windows Search
// - ISearchCatalogManager: Manages catalog operations
// - ISearchCrawlScopeManager: Manages scope rules (indexed locations)
// - ISearchRoot: Represents a search scope root
```

### Key API Operations Mapping

| Feature | COM Interface | Method |
|---------|---------------|--------|
| List index rules | ISearchCrawlScopeManager | EnumerateScopeRules() |
| Add location | ISearchCrawlScopeManager | AddDefaultScopeRule() or AddUserScopeRule() |
| Remove location | ISearchCrawlScopeManager | RemoveScopeRule() |
| Check service status | WMI Win32_Service | Query for "WSearch" service |
| Get indexing depth | Registry | HKLM\SOFTWARE\Microsoft\Windows Search\CrawlScopeManager |

### Error Handling Patterns
- Check HRESULT return codes from COM calls
- Handle COMException with specific error codes:
  - `0x80040D03`: Scope rule already exists
  - `0x80040D04`: Scope rule not found
  - `0x80070005`: Access denied (needs elevation)
- Graceful degradation when Windows Search service is stopped

### References
- [Microsoft Search Interop documentation](https://learn.microsoft.com/en-us/windows/win32/search/-search-3x-wds-extidx-csm)
- [ISearchCrawlScopeManager interface](https://learn.microsoft.com/en-us/windows/win32/api/searchapi/nn-searchapi-isearchcrawlscopemanager)

---

## 2. File Extension Indexing Depth Configuration

### Decision
Use **Windows Search IFilter configuration** via Registry to manage indexing depth (Properties only vs Properties and Contents).

### Rationale
- Registry path `HKLM\SOFTWARE\Microsoft\Windows Search\Preferences\FileTypes` stores per-extension settings
- Each extension has a DWORD value indicating indexing behavior:
  - `0`: Not indexed
  - `1`: Properties only
  - `2`: Properties and Contents (full-text indexing)
- This is the native Windows Search configuration mechanism

### Implementation Approach
```csharp
// Registry access for file extension settings
using Microsoft.Win32;

const string FileTypesKeyPath = @"SOFTWARE\Microsoft\Windows Search\Preferences\FileTypes";

// Read extension settings
using (var key = Registry.LocalMachine.OpenSubKey(FileTypesKeyPath))
{
    var value = key.GetValue(".txt"); // Returns DWORD
    // 0 = Not indexed, 1 = Properties only, 2 = Properties and Contents
}

// Write extension settings (requires admin)
using (var key = Registry.LocalMachine.OpenSubKey(FileTypesKeyPath, writable: true))
{
    key.SetValue(".log", 2, RegistryValueKind.DWord); // Full-text indexing
}
```

### Wildcard Search Strategy
- Enumerate all subkey values under FileTypes registry key
- Apply wildcard pattern matching against extension names
- Use `System.Text.RegularExpressions` to convert wildcard patterns to regex:
  - `*.*` → match all extensions
  - `*.log` → match `.log`
  - `*.*x` → match extensions ending in `x` (e.g., `.docx`, `.xlsx`)

### References
- [File Types registry configuration](https://learn.microsoft.com/en-us/windows/win32/search/-search-3x-wds-extidx-csm-filetypes)

---

## 3. Privilege Elevation and Security

### Decision
Implement **privilege checking without forced UAC elevation** - detect admin status and provide clear guidance when elevation is needed.

### Rationale
- FR-005 requires read operations work for standard users
- Only modification operations need admin privileges
- Forcing elevation on startup would block read-only scenarios
- Better UX to check on-demand and guide user to elevate if needed

### Implementation Approach
```csharp
using System.Security.Principal;

public class PrivilegeChecker : IPrivilegeChecker
{
    public bool IsRunningAsAdministrator()
    {
        using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
        {
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
    
    public void EnsureAdministrator(string operation)
    {
        if (!IsRunningAsAdministrator())
        {
            throw new UnauthorizedAccessException(
                $"Operation '{operation}' requires administrator privileges. " +
                "Please restart the application as administrator.");
        }
    }
}
```

### Best Practices
- Check privileges at operation start, not application start
- Provide actionable error messages with instructions to elevate
- Log privilege check failures to audit log
- Support `--no-confirm` flag for automated scenarios (must still be elevated)

### References
- [WindowsPrincipal.IsInRole](https://learn.microsoft.com/en-us/dotnet/api/system.security.principal.windowsprincipal.isinrole)

---

## 4. Command-Line Interface Design

### Decision
Use **System.CommandLine** library for CLI parsing and command structure.

### Rationale
- Modern Microsoft-recommended CLI framework for .NET
- Built-in support for:
  - Subcommands (list, add, remove, etc.)
  - Options and flags (`--no-confirm`, `--recursive`)
  - Help generation
  - Tab completion support
  - Type-safe argument binding
- Better than manual `args[]` parsing or third-party libraries

### Command Structure
```
WindowsSearchConfigurator.exe <command> [options]

Commands:
  list                    List all index rules
  add <path>              Add a location to the index
  remove <path>           Remove a location from the index
  modify <path>           Modify an existing index rule
  export <file>           Export rules to JSON file
  import <file>           Import rules from JSON file
  search-extensions       Search for file extensions
  configure-depth <ext>   Configure indexing depth for extension

Global Options:
  --no-confirm            Skip confirmation prompts
  --help                  Show help information
  --version               Show version information

Add Options:
  --non-recursive         Index only specified folder, not subfolders
  --include <patterns>    File type patterns to include (e.g., *.log,*.txt)
  --exclude <patterns>    Subfolders to exclude (e.g., node_modules,.git)
```

### Implementation Pattern
```csharp
using System.CommandLine;

var rootCommand = new RootCommand("Windows Search Index Configuration Manager");

var listCommand = new Command("list", "List all configured index rules");
listCommand.SetHandler(async () => await ListCommandHandler.ExecuteAsync());

var addCommand = new Command("add", "Add a location to the Windows Search index");
var pathArgument = new Argument<string>("path", "Path to add to index");
var recursiveOption = new Option<bool>("--non-recursive", "Index only specified folder");
addCommand.AddArgument(pathArgument);
addCommand.AddOption(recursiveOption);
addCommand.SetHandler(async (path, nonRecursive) => 
    await AddCommandHandler.ExecuteAsync(path, !nonRecursive), 
    pathArgument, recursiveOption);

rootCommand.AddCommand(listCommand);
rootCommand.AddCommand(addCommand);

return await rootCommand.InvokeAsync(args);
```

### References
- [System.CommandLine documentation](https://learn.microsoft.com/en-us/dotnet/standard/commandline/)
- [Command-line best practices](https://learn.microsoft.com/en-us/dotnet/standard/commandline/syntax-concepts-and-parser)

---

## 5. JSON Configuration Schema

### Decision
Use **System.Text.Json** with custom schema for import/export functionality.

### Rationale
- Modern, high-performance JSON library included in .NET
- No external dependencies (aligns with FR-019)
- Source generation support for AOT compilation scenarios
- Better performance than Newtonsoft.Json for most scenarios

### JSON Schema Design
```json
{
  "$schema": "https://example.com/windows-search-config-v1.schema.json",
  "version": "1.0",
  "exportDate": "2025-10-22T10:30:00Z",
  "exportedBy": "DOMAIN\\Username",
  "rules": [
    {
      "path": "D:\\Projects",
      "ruleType": "include",
      "recursive": true,
      "fileTypeFilters": ["*.cs", "*.csproj", "*.sln"],
      "excludedSubfolders": ["bin", "obj", "node_modules"],
      "createdDate": "2025-10-20T14:22:00Z"
    },
    {
      "path": "\\\\server\\share\\documents",
      "ruleType": "exclude",
      "recursive": false,
      "fileTypeFilters": [],
      "excludedSubfolders": [],
      "createdDate": "2025-10-21T09:15:00Z"
    }
  ],
  "extensionSettings": [
    {
      "extension": ".log",
      "indexingDepth": "PropertiesOnly",
      "modifiedDate": "2025-10-22T08:00:00Z"
    },
    {
      "extension": ".cs",
      "indexingDepth": "PropertiesAndContents",
      "modifiedDate": "2025-10-22T08:00:00Z"
    }
  ]
}
```

### Validation Strategy
- JSON schema validation on import
- Verify all paths exist before applying rules
- Validate enum values (ruleType, indexingDepth)
- Check for duplicate rules
- Partial import support (continue on errors, report failures)

### Implementation Pattern
```csharp
using System.Text.Json;
using System.Text.Json.Serialization;

public class ConfigurationFile
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0";
    
    [JsonPropertyName("exportDate")]
    public DateTime ExportDate { get; set; }
    
    [JsonPropertyName("rules")]
    public List<IndexRuleDto> Rules { get; set; } = new();
    
    [JsonPropertyName("extensionSettings")]
    public List<ExtensionSettingDto> ExtensionSettings { get; set; } = new();
}

var options = new JsonSerializerOptions 
{ 
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    Converters = { new JsonStringEnumConverter() }
};

// Export
string json = JsonSerializer.Serialize(config, options);

// Import
var config = JsonSerializer.Deserialize<ConfigurationFile>(json, options);
```

### References
- [System.Text.Json documentation](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/how-to)

---

## 6. Audit Logging Strategy

### Decision
Implement **structured text logging** to files in user profile or application directory.

### Rationale
- Simple, reliable, no dependencies
- Human-readable for quick troubleshooting
- Machine-parseable for audit analysis
- No elevated privileges needed to write to user profile
- Aligns with FR-015 requirements

### Log Format
```
Timestamp: 2025-10-22 10:30:45.123
User: DOMAIN\Username
Operation: AddIndexRule
Parameters: Path=D:\Projects, Recursive=true, Filters=*.cs;*.csproj
Result: Success
Details: Added scope rule for D:\Projects with 2 file type filters
---
```

### Implementation Approach
```csharp
public class AuditLogger : IAuditLogger
{
    private readonly string _logFilePath;
    
    public AuditLogger()
    {
        // Try application directory first, fall back to user profile
        var appDir = AppContext.BaseDirectory;
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        
        _logFilePath = TryCreateLogFile(appDir) 
            ?? CreateLogFile(Path.Combine(userProfile, ".windowssearchconfig"));
    }
    
    public void LogOperation(string operation, string parameters, bool success, string details)
    {
        var entry = $@"
Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}
User: {Environment.UserDomainName}\{Environment.UserName}
Operation: {operation}
Parameters: {parameters}
Result: {(success ? "Success" : "Failure")}
Details: {details}
---
";
        File.AppendAllText(_logFilePath, entry);
    }
}
```

### Best Practices
- Log rotation: Create new file daily (audit-YYYY-MM-DD.log)
- Include full context: user, timestamp, operation, parameters, outcome
- Log both successes and failures
- Handle write failures gracefully (don't block operations)
- Consider file locking for concurrent access

### References
- [Environment.GetFolderPath](https://learn.microsoft.com/en-us/dotnet/api/system.environment.getfolderpath)

---

## 7. Path Validation and Normalization

### Decision
Use **System.IO.Path** with custom validation for UNC paths, relative paths, and MAX_PATH constraints.

### Rationale
- Built-in path handling is robust and well-tested
- Need custom logic for:
  - UNC path validation (\\\\server\\share format)
  - Relative path resolution against current directory
  - MAX_PATH (260 character) validation per SC-006
  - Special character and Unicode handling

### Implementation Approach
```csharp
public class PathValidator
{
    public ValidationResult Validate(string path)
    {
        // Check empty/null
        if (string.IsNullOrWhiteSpace(path))
            return ValidationResult.Failure("Path cannot be empty");
        
        // Normalize
        string normalized;
        try
        {
            normalized = Path.GetFullPath(path);
        }
        catch (Exception ex)
        {
            return ValidationResult.Failure($"Invalid path format: {ex.Message}");
        }
        
        // Check MAX_PATH
        if (normalized.Length > 260)
            return ValidationResult.Failure(
                $"Path exceeds maximum length of 260 characters (current: {normalized.Length})");
        
        // Validate UNC path format if applicable
        if (normalized.StartsWith(@"\\"))
        {
            if (!IsValidUncPath(normalized))
                return ValidationResult.Failure("Invalid UNC path format");
        }
        
        // Check if path exists
        bool exists = Directory.Exists(normalized) || File.Exists(normalized);
        if (!exists)
            return ValidationResult.Warning($"Path does not exist: {normalized}");
        
        return ValidationResult.Success(normalized);
    }
    
    private bool IsValidUncPath(string path)
    {
        // \\server\share format
        var parts = path.Substring(2).Split('\\', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2; // At least server and share
    }
}
```

### Edge Cases
- Paths with Unicode characters: Supported via .NET's Unicode string handling
- Paths with spaces: Quoted or unquoted handling in CLI
- Network drives: Mapped drives treated as local paths
- Symbolic links/junctions: Resolved to target path

### References
- [Path.GetFullPath](https://learn.microsoft.com/en-us/dotnet/api/system.io.path.getfullpath)
- [MAX_PATH limitation](https://learn.microsoft.com/en-us/windows/win32/fileio/maximum-file-path-limitation)

---

## 8. Testing Strategy

### Decision
Three-tier testing approach: Unit → Integration → Contract tests.

### Rationale
- Aligns with Constitution principle I (Automated Testing)
- Unit tests: Fast, isolated, test business logic
- Integration tests: Verify Windows API interactions
- Contract tests: Ensure CLI interface stability

### Testing Framework Stack
```xml
<ItemGroup>
  <!-- Unit testing -->
  <PackageReference Include="NUnit" Version="4.0.1" />
  <PackageReference Include="NUnit3TestAdapter" Version="4.5.0" />
  
  <!-- Mocking -->
  <PackageReference Include="Moq" Version="4.20.0" />
  
  <!-- Integration testing helpers -->
  <PackageReference Include="FluentAssertions" Version="6.12.0" />
</ItemGroup>
```

### Test Organization

**Unit Tests** (~70% coverage target)
- Mock all Windows API calls (ISearchIndexManager, IConfigurationStore, etc.)
- Test command handlers with mocked dependencies
- Test path validation logic
- Test wildcard matching algorithms
- Test JSON serialization/deserialization
- Test privilege checking logic

**Integration Tests** (~20% coverage target)
- Require Windows Search service running
- Test actual COM API calls
- Test registry read/write operations
- Test file I/O for config and logs
- Setup/teardown: Create test scope rules, clean up after tests
- Run on dedicated test machine or CI environment with Windows

**Contract Tests** (~10% coverage target)
- Test CLI command parsing
- Test help text generation
- Test error message formatting
- Verify command option combinations
- Ensure backward compatibility of CLI interface

### Test Naming Convention
```csharp
// Pattern: MethodName_Scenario_ExpectedBehavior
public class AddCommandHandlerTests
{
    [Fact]
    public void Execute_ValidPath_AddsRuleSuccessfully() { }
    
    [Fact]
    public void Execute_PathTooLong_ReturnsValidationError() { }
    
    [Fact]
    public void Execute_WithoutAdminPrivileges_ThrowsUnauthorizedException() { }
}
```

### Continuous Integration Considerations
- Unit tests: Run on every commit
- Integration tests: Run on Windows-based CI agents only
- Contract tests: Run on every commit
- Test result reporting via JUnit XML format

### References
- [NUnit documentation](https://docs.nunit.org/)
- [Moq documentation](https://github.com/moq/moq4)

---

## 9. Error Handling Patterns

### Decision
Use **typed exceptions** with rich context and user-friendly messages.

### Rationale
- FR-007 requires clear error messages with failure reasons
- SC-004 requires error messages indicate problem and suggested resolution in 100% of cases
- Typed exceptions enable specific catch blocks and better error handling

### Exception Hierarchy
```csharp
// Base exception
public class SearchConfigurationException : Exception
{
    public string UserMessage { get; }
    public string SuggestedAction { get; }
    
    public SearchConfigurationException(string userMessage, string suggestedAction, Exception innerException = null)
        : base(userMessage, innerException)
    {
        UserMessage = userMessage;
        SuggestedAction = suggestedAction;
    }
}

// Specific exceptions
public class PrivilegeRequiredException : SearchConfigurationException
{
    public PrivilegeRequiredException(string operation)
        : base(
            $"Operation '{operation}' requires administrator privileges.",
            "Right-click the application and select 'Run as administrator'.")
    { }
}

public class SearchServiceUnavailableException : SearchConfigurationException
{
    public SearchServiceUnavailableException()
        : base(
            "Windows Search service is not running.",
            "Start the Windows Search service using Services.msc or run: net start wsearch")
    { }
}

public class PathValidationException : SearchConfigurationException
{
    public PathValidationException(string path, string reason)
        : base(
            $"Path '{path}' is invalid: {reason}",
            "Verify the path exists and is accessible.")
    { }
}
```

### Error Handling Flow
```csharp
try
{
    privilegeChecker.EnsureAdministrator("add index rule");
    serviceStatus.EnsureRunning();
    
    var validatedPath = pathValidator.Validate(path);
    indexManager.AddRule(validatedPath);
    
    auditLogger.Log("AddRule", path, success: true);
    Console.WriteLine($"✓ Successfully added {path} to index");
}
catch (SearchConfigurationException ex)
{
    auditLogger.Log("AddRule", path, success: false, ex.Message);
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"✗ Error: {ex.UserMessage}");
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"→ {ex.SuggestedAction}");
    Console.ResetColor();
    return 1; // Exit code for failure
}
catch (COMException ex)
{
    // Handle unexpected COM errors
    auditLogger.Log("AddRule", path, success: false, ex.ToString());
    Console.WriteLine($"✗ Windows Search API error: {ex.Message} (HRESULT: 0x{ex.ErrorCode:X})");
    return 1;
}
```

### Best Practices
- Always provide user-friendly message + suggested action
- Log full exception details (including stack trace) to audit log
- Use colored console output for visual distinction
- Return appropriate exit codes (0 = success, 1 = error)
- Never swallow exceptions silently

### References
- [Exception handling best practices](https://learn.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions)

---

## Research Complete

All technical decisions documented with rationale, alternatives considered, and implementation approaches defined. Ready to proceed to Phase 1: Design & Contracts.
