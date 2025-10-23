# Research: COM API Registration Support

**Feature**: COM API Registration Support  
**Branch**: `002-com-api-registration`  
**Date**: 2025-10-22  
**Status**: Complete

## Overview

This document consolidates research findings for implementing COM API registration detection and automatic registration functionality in WindowsSearchConfigurator. The research covers COM registration detection mechanisms, registration methods, privilege handling, and integration patterns.

## Research Tasks

### 1. COM Registration Detection Methods

**Task**: Research how to detect if Microsoft.Search.Interop.CSearchManager COM API is registered

**Decision**: Use Registry-based detection with CLSID lookup

**Rationale**:
- COM registration stores CLSID entries in `HKEY_CLASSES_ROOT\CLSID\{7D096C5F-AC08-4F1F-BEB7-5C22C517CE39}` (CSearchManager CLSID)
- Registry approach is faster than attempting COM instantiation and catching exceptions
- Provides ability to distinguish between "not registered" vs "DLL missing" scenarios
- Can verify both CLSID and ProgID registration status

**Implementation Approach**:
```csharp
// Check CLSID key existence
bool isRegistered = Registry.ClassesRoot.OpenSubKey(@"CLSID\{7D096C5F-AC08-4F1F-BEB7-5C22C517CE39}") != null;

// Check InprocServer32 to verify DLL path
var clsidKey = Registry.ClassesRoot.OpenSubKey(@"CLSID\{7D096C5F-AC08-4F1F-BEB7-5C22C517CE39}\InprocServer32");
string dllPath = clsidKey?.GetValue(null) as string;
bool dllExists = !string.IsNullOrEmpty(dllPath) && File.Exists(Environment.ExpandEnvironmentVariables(dllPath));
```

**Alternatives Considered**:
1. **COM Instantiation Attempt**: Try to create COM object and catch exceptions
   - Rejected: Slower, less informative error messages, harder to distinguish error types
2. **WMI Query**: Use Win32_ClassicCOMClassSetting WMI class
   - Rejected: Heavier weight, requires additional dependencies, slower performance
3. **Type.GetTypeFromCLSID()**: Use .NET reflection to check COM type availability
   - Rejected: Creates unnecessary object instantiation overhead, less control over error handling

**Best Practices**:
- Always check both CLSID existence and DLL file path validity
- Handle registry access exceptions gracefully (permission denied, key not found)
- Cache detection result during application lifetime to avoid repeated checks
- Validate COM object instantiation after registration to ensure functional state

---

### 2. COM Registration Methods

**Task**: Research best practices for programmatically registering COM DLLs in C#

**Decision**: Use System.Diagnostics.Process to invoke regsvr32.exe with proper error handling

**Rationale**:
- regsvr32.exe is the standard Windows utility for COM DLL registration
- Works consistently across all Windows versions (10, 11, Server 2016+)
- Provides clear exit codes (0 = success, non-zero = failure)
- Handles all registry modifications and DLL entry point calls automatically
- Does not require P/Invoke or complex COM interop code

**Implementation Approach**:
```csharp
var processInfo = new ProcessStartInfo
{
    FileName = "regsvr32.exe",
    Arguments = "/s \"C:\\Windows\\System32\\SearchAPI.dll\"", // /s = silent mode
    UseShellExecute = false,
    CreateNoWindow = true,
    RedirectStandardError = true,
    RedirectStandardOutput = true,
    Verb = "runas" // Request elevation if needed
};

using (var process = Process.Start(processInfo))
{
    process.WaitForExit(5000); // 5 second timeout
    bool success = process.ExitCode == 0;
}
```

**Alternatives Considered**:
1. **P/Invoke to DllRegisterServer**: Directly call DLL's registration entry point
   - Rejected: Requires manual registry modifications, error-prone, platform-specific
2. **COM Registration API (ITypeLib)**: Use COM APIs for registration
   - Rejected: More complex, requires deep COM knowledge, same admin requirements
3. **PowerShell Register-COM cmdlet**: Shell out to PowerShell
   - Rejected: Additional dependency, slower, PowerShell may not be available in all environments

**Best Practices**:
- Always use silent mode (/s) to avoid modal dialogs blocking automation
- Set reasonable timeout (5 seconds) to prevent hung processes
- Capture and log both stdout and stderr for troubleshooting
- Check exit code to determine success (0) vs failure (non-zero)
- Verify registration succeeded by re-checking registry after regsvr32 completes

---

### 3. Administrative Privilege Detection

**Task**: Research how to detect if application is running with administrative privileges

**Decision**: Use WindowsIdentity and WindowsPrincipal to check Administrator role membership

**Rationale**:
- Standard .NET approach using System.Security.Principal namespace
- Works consistently across all Windows versions
- No P/Invoke required
- Provides accurate privilege detection for UAC-enabled systems
- Already partially implemented in existing PrivilegeChecker service

**Implementation Approach**:
```csharp
using System.Security.Principal;

public static bool IsAdministrator()
{
    using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
    {
        WindowsPrincipal principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
}
```

**Alternatives Considered**:
1. **Token Elevation Check**: Use GetTokenInformation P/Invoke
   - Rejected: Requires P/Invoke, more complex, WindowsPrincipal sufficient for needs
2. **Registry Write Test**: Attempt to write to HKLM and catch exceptions
   - Rejected: Side effects, slower, not a clean detection method
3. **File System Permission Check**: Try to access admin-only directories
   - Rejected: Unreliable, side effects, not the right semantic check

**Best Practices**:
- Check privileges before attempting privileged operations
- Provide clear guidance on how to elevate (Run as Administrator)
- Cache privilege check result (doesn't change during application lifetime)
- Include privilege status in error messages when operations fail

---

### 4. Non-Interactive Mode Design

**Task**: Research patterns for supporting both interactive and non-interactive (CI/CD) modes

**Decision**: Use command-line flags with clear semantics: --auto-register-com and --no-register-com

**Rationale**:
- Command-line flags are standard for controlling automation behavior
- Explicit flags prevent unintended behavior in scripts
- Follows existing WindowsSearchConfigurator CLI patterns (using System.CommandLine)
- Provides clear opt-in/opt-out semantics
- Enables fail-fast behavior in CI/CD pipelines

**Implementation Approach**:
```csharp
// Global options added to root command
var autoRegisterOption = new Option<bool>(
    "--auto-register-com",
    description: "Automatically register COM API if not registered (requires admin privileges)"
);

var noRegisterOption = new Option<bool>(
    "--no-register-com", 
    description: "Exit with error if COM API is not registered (do not attempt registration)"
);

// Priority: --no-register-com > --auto-register-com > interactive prompt
if (noRegisterOption.Value)
{
    // Exit immediately if COM not registered
}
else if (autoRegisterOption.Value)
{
    // Attempt automatic registration without prompt
}
else
{
    // Interactive prompt
}
```

**Alternatives Considered**:
1. **Environment Variables**: Use WINDOWSSEARCH_AUTO_REGISTER=true
   - Rejected: Less discoverable, harder to document, not standard for CLI tools
2. **Configuration File**: Use JSON config to control registration behavior
   - Rejected: Overkill for single boolean setting, less transparent in scripts
3. **Single Flag with Enum**: --com-registration-mode=auto|manual|skip
   - Rejected: More complex, harder to use in shell scripts (string values vs boolean flags)

**Best Practices**:
- Make flags mutually exclusive (validate only one is specified)
- Default to interactive mode when no flags provided
- Return appropriate exit codes (0 = success, 1 = COM not registered and user declined, 2 = registration failed)
- Document flags prominently in --help output
- Log which mode is active for troubleshooting

---

### 5. Error Handling and User Guidance

**Task**: Research best practices for providing actionable error messages and manual registration instructions

**Decision**: Use tiered error messaging with progressively detailed guidance

**Rationale**:
- Users need different levels of detail depending on context (quick scan vs troubleshooting)
- Clear error messages reduce support burden
- Manual instructions provide fallback when automation fails
- Consistent with WindowsSearchConfigurator's existing error handling patterns

**Implementation Approach**:
```csharp
// Level 1: Clear error message
Console.WriteLine("ERROR: Microsoft Windows Search COM API is not registered.");
Console.WriteLine("The application requires this API to function.");

// Level 2: Offer automatic solution
Console.WriteLine("\nWould you like to attempt automatic registration? (requires admin privileges)");
Console.WriteLine("Enter 'yes' to register, or 'no' for manual instructions: ");

// Level 3: Manual fallback instructions
Console.WriteLine("\nTo manually register the COM API:");
Console.WriteLine("1. Open Command Prompt as Administrator");
Console.WriteLine("2. Run: regsvr32 \"%SystemRoot%\\System32\\SearchAPI.dll\"");
Console.WriteLine("3. Restart WindowsSearchConfigurator");
Console.WriteLine("\nFor troubleshooting, see: [documentation URL]");
```

**Alternatives Considered**:
1. **Error Codes Only**: Use numeric error codes (e.g., ERROR_COM_NOT_REGISTERED = 1001)
   - Rejected: Not user-friendly, requires documentation lookup
2. **Stack Traces**: Show detailed technical error information
   - Rejected: Confusing for end users, violates SC-004 (no cryptic errors)
3. **Silent Failure**: Attempt COM operations and fail with generic error
   - Rejected: Violates FR-002 (clear notification), poor user experience

**Best Practices**:
- Start with clear, non-technical explanation of the problem
- Provide immediate action options (automated fix vs manual steps)
- Include manual instructions as fallback
- Log detailed technical information for troubleshooting without showing to user
- Link to documentation for complex scenarios

---

### 6. Registration Validation

**Task**: Research how to validate COM API is functional after registration

**Decision**: Attempt COM object instantiation after registration to verify functionality

**Rationale**:
- Registry presence doesn't guarantee functional COM object
- Instantiation test catches corrupted DLLs, version mismatches, and incomplete registration
- Provides high confidence that subsequent operations will succeed
- Allows detection of partial registration scenarios

**Implementation Approach**:
```csharp
// After registration attempt, validate functionality
try
{
    Type searchManagerType = Type.GetTypeFromCLSID(
        new Guid("7D096C5F-AC08-4F1F-BEB7-5C22C517CE39")
    );
    
    if (searchManagerType == null)
    {
        return ValidationResult.NotRegistered;
    }
    
    object searchManager = Activator.CreateInstance(searchManagerType);
    
    if (searchManager == null)
    {
        return ValidationResult.RegistrationInvalid;
    }
    
    // Clean up
    if (searchManager is IDisposable disposable)
    {
        disposable.Dispose();
    }
    
    return ValidationResult.Valid;
}
catch (COMException ex)
{
    return ValidationResult.RegistrationInvalid;
}
```

**Alternatives Considered**:
1. **Registry Check Only**: Trust registry presence as proof of registration
   - Rejected: Doesn't catch corrupted/incompatible DLLs
2. **Full API Test**: Call actual search methods to validate
   - Rejected: Overkill, requires search service running, slower
3. **DLL Signature Verification**: Validate DLL file signature
   - Rejected: Doesn't test registration, only file integrity

**Best Practices**:
- Perform validation after each registration attempt
- Dispose COM objects properly to avoid resource leaks
- Distinguish between "not registered" and "registered but broken" states
- Provide specific error messages for validation failures
- Consider caching validation result to avoid repeated expensive checks

---

### 7. Integration with Existing Architecture

**Task**: Research how to integrate COM detection into existing WindowsSearchConfigurator startup flow

**Decision**: Add COM detection as first step in Program.cs Main(), before command parsing

**Rationale**:
- Fail-fast principle: detect problems before user invests time in command
- All commands require COM API, so check applies universally
- Consistent user experience across all commands
- Minimal code changes to existing command handlers
- Centralizes COM handling logic in one place

**Implementation Approach**:
```csharp
// In Program.cs Main()
public static async Task<int> Main(string[] args)
{
    // Step 1: Check COM API registration (new)
    var comDetector = new COMRegistrationDetector();
    var registrationStatus = comDetector.GetRegistrationStatus();
    
    if (!registrationStatus.IsRegistered)
    {
        var registrationService = new COMRegistrationService(comDetector);
        bool resolved = await registrationService.HandleMissingRegistration(args);
        
        if (!resolved)
        {
            return 1; // Exit if user declined or registration failed
        }
    }
    
    // Step 2: Continue with normal command parsing and execution
    var rootCommand = BuildRootCommand();
    return await rootCommand.InvokeAsync(args);
}
```

**Alternatives Considered**:
1. **Per-Command Check**: Add COM detection to each command handler
   - Rejected: Code duplication, inconsistent behavior, more maintenance
2. **Lazy Initialization**: Check COM when first API call is made
   - Rejected: Late failure, poor user experience (command appears to work then fails)
3. **Dependency Injection Container**: Register COM check as startup service
   - Rejected: Overkill for single check, complicates simple startup flow

**Best Practices**:
- Check COM API before parsing commands (fail-fast)
- Cache detection result to avoid repeated checks
- Integrate with existing logging infrastructure
- Respect existing command-line parsing flow (System.CommandLine)
- Minimize changes to existing command handlers

---

### 8. Testing Strategy

**Task**: Research testing approaches for COM registration functionality

**Decision**: Three-tier testing strategy: Unit tests with mocks, integration tests with real COM, contract tests for CLI

**Rationale**:
- Unit tests enable fast, reliable testing without admin privileges or COM dependencies
- Integration tests validate real-world behavior on actual Windows systems
- Contract tests ensure CLI flags work correctly in automation scenarios
- Follows existing WindowsSearchConfigurator testing patterns (NUnit + Moq)
- Enables test-first development approach

**Implementation Approach**:

**Unit Tests** (WindowsSearchConfigurator.UnitTests):
```csharp
[TestFixture]
public class COMRegistrationDetectorTests
{
    [Test]
    public void GetRegistrationStatus_WhenCLSIDExists_ReturnsRegistered()
    {
        // Mock registry to return CLSID key
        var mockRegistry = new Mock<IRegistryAccessor>();
        mockRegistry.Setup(r => r.KeyExists(@"CLSID\{...}")).Returns(true);
        
        var detector = new COMRegistrationDetector(mockRegistry.Object);
        var status = detector.GetRegistrationStatus();
        
        Assert.That(status.IsRegistered, Is.True);
    }
}
```

**Integration Tests** (WindowsSearchConfigurator.IntegrationTests):
```csharp
[TestFixture]
[RequiresAdmin] // Custom attribute to skip when not admin
public class COMRegistrationIntegrationTests
{
    [Test]
    public async Task RegisterCOM_WhenNotRegistered_SuccessfullyRegisters()
    {
        // Note: May require unregistering COM first in test setup
        var service = new COMRegistrationService();
        var result = await service.RegisterCOMAsync();
        
        Assert.That(result.Success, Is.True);
        
        // Validate registration
        var detector = new COMRegistrationDetector();
        Assert.That(detector.GetRegistrationStatus().IsRegistered, Is.True);
    }
}
```

**Contract Tests** (WindowsSearchConfigurator.ContractTests):
```csharp
[TestFixture]
public class COMRegistrationContractTests
{
    [Test]
    public void AutoRegisterFlag_IsRecognized()
    {
        var args = new[] { "--auto-register-com", "list" };
        var parseResult = ParseArguments(args);
        
        Assert.That(parseResult.HasOption("--auto-register-com"), Is.True);
    }
}
```

**Alternatives Considered**:
1. **Integration Tests Only**: Skip unit tests, only test with real COM
   - Rejected: Requires admin privileges, slow, hard to test error conditions
2. **Manual Testing Only**: No automated tests for COM registration
   - Rejected: Violates Constitution Principle I (automated testing required)
3. **Mocking Framework for COM**: Mock COM objects directly
   - Rejected: COM mocking is complex, better to abstract registry access

**Best Practices**:
- Mock registry access for unit tests (not COM objects)
- Use [RequiresAdmin] attribute to skip integration tests when appropriate
- Test both success and failure paths
- Test edge cases: partial registration, missing DLL, permission denied
- Run integration tests on clean VM snapshots when possible
- Document test requirements (admin privileges, Windows Search installed)

---

### 9. Logging and Audit Trail

**Task**: Research how to integrate COM registration logging with existing audit infrastructure

**Decision**: Extend existing AuditLogger service with COM registration events

**Rationale**:
- WindowsSearchConfigurator already has AuditLogger infrastructure
- Consistent logging format and location
- Meets FR-012 requirement (log all registration attempts)
- Enables troubleshooting and compliance auditing
- Minimal code changes to integrate

**Implementation Approach**:
```csharp
// Add new log event types to AuditLogger
public enum AuditEventType
{
    // ... existing event types ...
    COMRegistrationDetected,
    COMRegistrationAttemptStarted,
    COMRegistrationSucceeded,
    COMRegistrationFailed,
    COMValidationSucceeded,
    COMValidationFailed
}

// Usage in COMRegistrationService
_auditLogger.LogEvent(new AuditEvent
{
    EventType = AuditEventType.COMRegistrationAttemptStarted,
    Timestamp = DateTime.UtcNow,
    User = Environment.UserName,
    Details = "Attempting COM API registration",
    Metadata = new Dictionary<string, string>
    {
        ["DLLPath"] = dllPath,
        ["IsAdmin"] = isAdmin.ToString(),
        ["Mode"] = mode.ToString() // Interactive, Automatic, etc.
    }
});
```

**Alternatives Considered**:
1. **Separate COM Log File**: Create COM-specific log file
   - Rejected: Fragments audit trail, user confusion about multiple log locations
2. **Windows Event Log**: Write to Windows Event Log
   - Rejected: Requires admin privileges, not consistent with existing approach
3. **No Logging**: Only show console output
   - Rejected: Violates FR-012, loses troubleshooting capability

**Best Practices**:
- Log all registration attempts (success and failure)
- Include context: user, timestamp, mode (interactive/auto)
- Log technical details (DLL path, exit codes, error messages)
- Log validation results after registration
- Include correlation IDs for related events
- Respect existing log rotation and retention policies

---

## Summary of Key Decisions

| Area | Decision | Key Dependencies |
|------|----------|------------------|
| COM Detection | Registry-based CLSID lookup | Microsoft.Win32.Registry |
| Registration Method | regsvr32.exe via Process | System.Diagnostics.Process |
| Privilege Detection | WindowsPrincipal.IsInRole | System.Security.Principal |
| Non-Interactive Mode | --auto-register-com / --no-register-com flags | System.CommandLine |
| Error Handling | Tiered messaging with manual fallback | Existing ConsoleFormatter |
| Validation | COM object instantiation test | System.Runtime.InteropServices |
| Integration Point | Program.cs Main() startup | Existing architecture |
| Testing Strategy | Unit (mocked) + Integration (real) + Contract | NUnit, Moq, FluentAssertions |
| Logging | Extend existing AuditLogger | Existing AuditLogger service |

## Compliance with Constitution

All research findings comply with Windows Search Configurator Constitution v1.3.1:

- **Automated Testing**: Three-tier testing strategy defined (unit, integration, contract)
- **Windows API Safety**: Comprehensive error handling for all registry/COM operations
- **User Configuration Control**: Explicit user approval required for registration
- **Clear Interface Design**: Clear error messages, helpful guidance, discoverable flags
- **Documentation**: Quickstart updates, XML documentation, troubleshooting guide
- **Incremental Implementation**: Feature decomposable into 4 batches of 4-6 files each
- **Source Control Discipline**: GitHub workflow, conventional commits, branch strategy

## References

- [COM Registration Documentation](https://learn.microsoft.com/en-us/windows/win32/com/registering-com-applications)
- [regsvr32.exe Usage](https://learn.microsoft.com/en-us/windows-server/administration/windows-commands/regsvr32)
- [WindowsIdentity Class](https://learn.microsoft.com/en-us/dotnet/api/system.security.principal.windowsidentity)
- [Process Class](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.process)
- [System.CommandLine](https://github.com/dotnet/command-line-api)
- Windows Search COM API CLSID: `{7D096C5F-AC08-4F1F-BEB7-5C22C517CE39}`
