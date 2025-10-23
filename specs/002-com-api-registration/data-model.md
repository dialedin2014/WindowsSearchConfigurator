# Data Model: COM API Registration Support

**Feature**: COM API Registration Support  
**Branch**: `002-com-api-registration`  
**Date**: 2025-10-22  
**Status**: Complete

## Overview

This document defines the domain model for COM API registration detection and management. The model captures registration state, registration attempts, validation results, and user decisions related to COM API availability.

## Domain Entities

### 1. COMRegistrationStatus

Represents the current state of the Microsoft.Search.Interop.CSearchManager COM API registration.

**Properties**:
- `IsRegistered` (bool): Whether the COM API is properly registered in the Windows Registry
- `CLSIDExists` (bool): Whether the CLSID key exists in HKEY_CLASSES_ROOT
- `DLLPath` (string, nullable): Path to the COM DLL (searchapi.dll) from InprocServer32 registry value
- `DLLExists` (bool): Whether the DLL file exists at the specified path
- `ValidationState` (COMValidationState enum): Result of COM object instantiation test
- `DetectionTimestamp` (DateTime): When the detection was performed
- `ErrorMessage` (string, nullable): Error details if detection failed

**Validation Rules**:
- `IsRegistered` is true only if `CLSIDExists` AND `DLLExists` AND `ValidationState == Valid`
- `DLLPath` must be validated using Environment.ExpandEnvironmentVariables() for %SystemRoot% etc.
- `DetectionTimestamp` must be in UTC
- If `CLSIDExists` is false, `DLLPath` should be null

**State Transitions**:
```
[Not Checked] → [Detection Performed] → [Registered | Not Registered | Detection Failed]
```

**Relationships**:
- None (value object, no dependencies)

---

### 2. COMValidationState (Enum)

Represents the result of attempting to instantiate the COM object.

**Values**:
- `NotChecked` (0): Validation has not been performed
- `Valid` (1): COM object successfully instantiated and disposed
- `CLSIDNotFound` (2): Type.GetTypeFromCLSID returned null
- `InstantiationFailed` (3): Activator.CreateInstance threw exception
- `COMException` (4): COM-specific exception during instantiation
- `UnknownError` (5): Unexpected error during validation

**Usage**:
```csharp
public enum COMValidationState
{
    NotChecked = 0,
    Valid = 1,
    CLSIDNotFound = 2,
    InstantiationFailed = 3,
    COMException = 4,
    UnknownError = 5
}
```

---

### 3. COMRegistrationAttempt

Represents a single attempt to register the COM API, including outcome and context.

**Properties**:
- `AttemptId` (Guid): Unique identifier for this registration attempt
- `Timestamp` (DateTime): When the registration attempt started (UTC)
- `Mode` (RegistrationMode enum): How registration was initiated
- `User` (string): Windows username who initiated registration
- `IsAdministrator` (bool): Whether the user had admin privileges
- `DLLPath` (string): Path to DLL being registered
- `RegistrationMethod` (string): Method used ("regsvr32", "manual", etc.)
- `Outcome` (RegistrationOutcome enum): Result of the registration attempt
- `ExitCode` (int, nullable): Exit code from regsvr32 (if applicable)
- `ErrorMessage` (string, nullable): Error details if registration failed
- `DurationMs` (long): Time taken for registration attempt in milliseconds
- `PostValidation` (COMValidationState enum): Validation result after registration

**Validation Rules**:
- `AttemptId` must be unique (new Guid)
- `Timestamp` must be in UTC
- `User` must not be null or empty
- `DLLPath` must be a valid file path
- If `Outcome == Success`, `PostValidation` must be `Valid`
- If `Outcome == Failed`, `ErrorMessage` must not be null
- `DurationMs` must be ≥ 0

**State Transitions**:
```
[Not Started] → [In Progress] → [Success | Failed | Timeout | Cancelled]
```

**Relationships**:
- Associated with one COMRegistrationStatus (the state that triggered the attempt)
- Logged to AuditLogger as multiple AuditEvent entries

---

### 4. RegistrationMode (Enum)

Represents how COM registration was initiated.

**Values**:
- `Interactive` (0): User responded to interactive prompt
- `Automatic` (1): Triggered by --auto-register-com flag
- `Manual` (2): User manually runs registration command (future: `wsc register-com`)
- `Declined` (3): User declined registration offer in interactive mode

**Usage**:
```csharp
public enum RegistrationMode
{
    Interactive = 0,
    Automatic = 1,
    Manual = 2,
    Declined = 3
}
```

---

### 5. RegistrationOutcome (Enum)

Represents the result of a registration attempt.

**Values**:
- `Success` (0): Registration completed successfully and validation passed
- `Failed` (1): Registration failed (regsvr32 returned non-zero exit code)
- `Timeout` (2): Registration process exceeded timeout threshold
- `InsufficientPrivileges` (3): User lacks administrative privileges
- `DLLNotFound` (4): DLL file not found at expected path
- `Cancelled` (5): User cancelled during interactive mode
- `ValidationFailed` (6): Registration appeared to succeed but validation failed

**Usage**:
```csharp
public enum RegistrationOutcome
{
    Success = 0,
    Failed = 1,
    Timeout = 2,
    InsufficientPrivileges = 3,
    DLLNotFound = 4,
    Cancelled = 5,
    ValidationFailed = 6
}
```

---

### 6. RegistrationOptions

Configuration options for controlling COM registration behavior.

**Properties**:
- `AutoRegister` (bool): Whether to automatically register without prompting (from --auto-register-com)
- `NoRegister` (bool): Whether to refuse registration and exit immediately (from --no-register-com)
- `Silent` (bool): Whether to suppress interactive prompts (for CI/CD)
- `TimeoutSeconds` (int): Maximum time to wait for regsvr32 process (default: 5)
- `DLLPath` (string, nullable): Override path to searchapi.dll (default: auto-detect)

**Validation Rules**:
- `AutoRegister` and `NoRegister` cannot both be true (mutually exclusive)
- `TimeoutSeconds` must be > 0 and ≤ 60
- If `DLLPath` is specified, it must point to an existing file
- If `Silent` is true, `AutoRegister` should typically also be true

**Default Values**:
```csharp
public class RegistrationOptions
{
    public bool AutoRegister { get; set; } = false;
    public bool NoRegister { get; set; } = false;
    public bool Silent { get; set; } = false;
    public int TimeoutSeconds { get; set; } = 5;
    public string? DLLPath { get; set; } = null;
}
```

---

## Service Interfaces

### ICOMRegistrationDetector

Responsible for detecting COM API registration status.

**Methods**:
- `COMRegistrationStatus GetRegistrationStatus()`: Checks current registration state
- `bool IsCLSIDRegistered(Guid clsid)`: Checks if specific CLSID is registered
- `string? GetDLLPath(Guid clsid)`: Retrieves DLL path from registry
- `COMValidationState ValidateCOMObject(Guid clsid)`: Attempts to instantiate COM object

**Error Handling**:
- Catches and wraps registry access exceptions (UnauthorizedAccessException, SecurityException)
- Returns status with `ErrorMessage` populated on failure
- Never throws exceptions to caller

---

### ICOMRegistrationService

Responsible for orchestrating COM API registration workflow.

**Methods**:
- `Task<RegistrationAttempt> RegisterCOMAsync(RegistrationOptions options)`: Performs registration
- `Task<bool> HandleMissingRegistration(string[] args)`: Startup integration point
- `void ShowRegistrationPrompt()`: Displays interactive prompt
- `void ShowManualInstructions()`: Displays manual registration steps
- `Task<bool> RequestElevation()`: Shows elevation guidance (does not auto-elevate)

**Error Handling**:
- All exceptions caught and converted to RegistrationOutcome values
- Logs all exceptions to AuditLogger
- Returns detailed RegistrationAttempt with outcome and error message
- Provides user-friendly console output via ConsoleFormatter

---

## Data Flow

### 1. Startup Detection Flow

```
Program.Main()
    ↓
COMRegistrationDetector.GetRegistrationStatus()
    ↓
├─ Registry: Check CLSID key
├─ File System: Verify DLL exists
├─ COM: Attempt instantiation
    ↓
COMRegistrationStatus
    ↓
if (!IsRegistered) → COMRegistrationService.HandleMissingRegistration()
```

### 2. Registration Flow

```
COMRegistrationService.HandleMissingRegistration()
    ↓
Parse RegistrationOptions from command-line args
    ↓
├─ NoRegister flag? → Exit with error
├─ AutoRegister flag? → Automatic registration
└─ Neither? → Interactive prompt
    ↓
Check IsAdministrator
    ↓
├─ Not admin? → Show elevation instructions → Exit
└─ Is admin? → Proceed
    ↓
Execute regsvr32.exe
    ↓
├─ Success? → Validate COM object
├─ Timeout? → Return timeout outcome
└─ Failed? → Return failure outcome
    ↓
COMRegistrationAttempt (logged to AuditLogger)
    ↓
Return success/failure to caller
```

### 3. Validation Flow

```
COMRegistrationService.RegisterCOMAsync()
    ↓
Registration completed
    ↓
COMRegistrationDetector.ValidateCOMObject()
    ↓
Type.GetTypeFromCLSID(clsid)
    ↓
Activator.CreateInstance(type)
    ↓
├─ Success? → COMValidationState.Valid
├─ CLSID not found? → COMValidationState.CLSIDNotFound
├─ COM exception? → COMValidationState.COMException
└─ Other error? → COMValidationState.UnknownError
    ↓
Update COMRegistrationAttempt.PostValidation
    ↓
Return final outcome
```

## Persistence

### Registry (Read-Only for Detection)

**Location**: `HKEY_CLASSES_ROOT\CLSID\{7D096C5F-AC08-4F1F-BEB7-5C22C517CE39}`

**Data Read**:
- CLSID key existence (indicates registration)
- InprocServer32\(Default) value (DLL path)

**No Writes**: Application never writes to registry directly (uses regsvr32.exe)

### Audit Log (Write-Only)

**Location**: Configured in existing AuditLogger (text file in application directory)

**Data Written**:
- COMRegistrationAttempt records (JSON serialized)
- Event type, timestamp, user, outcome, error details
- Correlation ID for related events

**Format**: JSON lines, one event per line

**Example**:
```json
{
  "EventType": "COMRegistrationAttemptStarted",
  "Timestamp": "2025-10-22T14:30:00Z",
  "User": "DOMAIN\\AdminUser",
  "AttemptId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "Mode": "Interactive",
  "IsAdministrator": true,
  "DLLPath": "C:\\Windows\\System32\\SearchAPI.dll"
}
```

## Error Scenarios

### Detection Errors

| Error | COMRegistrationStatus State | User Message |
|-------|----------------------------|--------------|
| Registry access denied | `ErrorMessage` populated | "Unable to check COM registration status (permission denied)" |
| CLSID not found | `IsRegistered = false`, `CLSIDExists = false` | "Microsoft Windows Search COM API is not registered" |
| DLL path invalid | `IsRegistered = false`, `DLLExists = false` | "COM API registered but DLL not found" |
| Validation failed | `IsRegistered = false`, `ValidationState != Valid` | "COM API registered but not functional" |

### Registration Errors

| Error | RegistrationOutcome | User Guidance |
|-------|---------------------|---------------|
| Not administrator | `InsufficientPrivileges` | "Run as Administrator to register COM API" |
| DLL not found | `DLLNotFound` | "SearchAPI.dll not found. Ensure Windows Search is installed." |
| regsvr32 timeout | `Timeout` | "Registration timed out. Check system load and try again." |
| regsvr32 failed | `Failed` | "Registration failed (exit code: {code}). See manual instructions." |
| Validation failed post-registration | `ValidationFailed` | "Registration completed but COM object not functional. Try manual registration." |

## Testing Data

### Test COMRegistrationStatus Objects

```csharp
// Fully registered
var registered = new COMRegistrationStatus
{
    IsRegistered = true,
    CLSIDExists = true,
    DLLPath = @"C:\Windows\System32\SearchAPI.dll",
    DLLExists = true,
    ValidationState = COMValidationState.Valid,
    DetectionTimestamp = DateTime.UtcNow,
    ErrorMessage = null
};

// Not registered
var notRegistered = new COMRegistrationStatus
{
    IsRegistered = false,
    CLSIDExists = false,
    DLLPath = null,
    DLLExists = false,
    ValidationState = COMValidationState.NotChecked,
    DetectionTimestamp = DateTime.UtcNow,
    ErrorMessage = null
};

// Registered but DLL missing (corrupted)
var corrupted = new COMRegistrationStatus
{
    IsRegistered = false,
    CLSIDExists = true,
    DLLPath = @"C:\Windows\System32\SearchAPI.dll",
    DLLExists = false,
    ValidationState = COMValidationState.NotChecked,
    DetectionTimestamp = DateTime.UtcNow,
    ErrorMessage = "DLL file not found"
};
```

### Test COMRegistrationAttempt Objects

```csharp
// Successful registration
var success = new COMRegistrationAttempt
{
    AttemptId = Guid.NewGuid(),
    Timestamp = DateTime.UtcNow,
    Mode = RegistrationMode.Interactive,
    User = "TestUser",
    IsAdministrator = true,
    DLLPath = @"C:\Windows\System32\SearchAPI.dll",
    RegistrationMethod = "regsvr32",
    Outcome = RegistrationOutcome.Success,
    ExitCode = 0,
    ErrorMessage = null,
    DurationMs = 1234,
    PostValidation = COMValidationState.Valid
};

// Failed due to insufficient privileges
var insufficientPrivileges = new COMRegistrationAttempt
{
    AttemptId = Guid.NewGuid(),
    Timestamp = DateTime.UtcNow,
    Mode = RegistrationMode.Automatic,
    User = "StandardUser",
    IsAdministrator = false,
    DLLPath = @"C:\Windows\System32\SearchAPI.dll",
    RegistrationMethod = "regsvr32",
    Outcome = RegistrationOutcome.InsufficientPrivileges,
    ExitCode = null,
    ErrorMessage = "Administrative privileges required",
    DurationMs = 50,
    PostValidation = COMValidationState.NotChecked
};
```

## Assumptions

- COM API CLSID is constant: `{7D096C5F-AC08-4F1F-BEB7-5C22C517CE39}`
- Default DLL path: `%SystemRoot%\System32\SearchAPI.dll`
- regsvr32.exe is available in %SystemRoot%\System32
- Registration timeout of 5 seconds is sufficient for most systems
- Validation via object instantiation is sufficient (no need to call actual methods)
- Single registration attempt per application run (no retry logic)
- User environment has access to read HKEY_CLASSES_ROOT registry hive

## References

- [COM Registration Documentation](https://learn.microsoft.com/en-us/windows/win32/com/registering-com-applications)
- [CLSID Structure](https://learn.microsoft.com/en-us/windows/win32/com/clsid-key-hklm)
- [Type.GetTypeFromCLSID Method](https://learn.microsoft.com/en-us/dotnet/api/system.type.gettypefromclsid)
- [Process Class](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.process)
