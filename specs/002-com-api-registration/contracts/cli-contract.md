# CLI Contract: COM API Registration Support

**Feature**: COM API Registration Support  
**Branch**: `002-com-api-registration`  
**Date**: 2025-10-22  
**Status**: Complete

## Overview

This document defines the command-line interface contract for COM API registration functionality. It specifies command-line flags, exit codes, console output formats, and behavior in different modes (interactive, automatic, silent).

## Global Options

These options apply to all commands and are checked during application startup before any command execution.

### --auto-register-com

**Type**: Flag (boolean, no value required)

**Purpose**: Automatically register the COM API if not registered, without prompting for user confirmation. Requires administrative privileges.

**Behavior**:
- If COM API is registered: No action, proceed with command
- If COM API is not registered and user is admin: Attempt registration automatically
- If COM API is not registered and user is not admin: Display elevation instructions and exit with code 2

**Exit Codes**:
- 0: COM API was already registered or successfully registered
- 1: Registration was attempted but failed
- 2: Registration required admin privileges (elevation needed)

**Example Usage**:
```powershell
# Automatically register if needed, then list index rules
WindowsSearchConfigurator.exe --auto-register-com list

# Use in CI/CD pipeline
WindowsSearchConfigurator.exe --auto-register-com export --output config.json
```

**Conflicts**:
- Mutually exclusive with `--no-register-com`
- If both specified, exit with error and help message

---

### --no-register-com

**Type**: Flag (boolean, no value required)

**Purpose**: Do not attempt COM API registration. Exit immediately with error if COM API is not registered.

**Behavior**:
- If COM API is registered: Proceed with command
- If COM API is not registered: Display error message and exit immediately

**Exit Codes**:
- 0: COM API was registered, command executed successfully
- 1: COM API was not registered (fail-fast)

**Example Usage**:
```powershell
# Fail immediately if COM not registered (CI/CD pre-check)
WindowsSearchConfigurator.exe --no-register-com list

# Validate environment before deployment
WindowsSearchConfigurator.exe --no-register-com --help
```

**Conflicts**:
- Mutually exclusive with `--auto-register-com`
- If both specified, exit with error and help message

---

## Console Output

### Detection Messages

#### COM API Registered (Success Path)

No message displayed. Application proceeds silently to command execution.

---

#### COM API Not Registered (Interactive Mode - No Flags)

```
ERROR: Microsoft Windows Search COM API is not registered.

The Windows Search Configurator requires this API to function. The COM API is typically
installed with Windows Search but may not be registered on this system.

Would you like to attempt automatic registration? (requires administrative privileges)

Options:
  [Y] Yes - Attempt automatic registration now
  [N] No  - Show manual registration instructions
  [Q] Quit - Exit without registering

Enter your choice (Y/N/Q):
```

**User Input Handling**:
- `Y` or `y` or `yes`: Proceed to privilege check and registration
- `N` or `n` or `no`: Display manual instructions (see below)
- `Q` or `q` or `quit`: Exit with code 1
- Any other input: Re-prompt with "Invalid choice. Please enter Y, N, or Q:"

---

#### COM API Not Registered (Automatic Mode - --auto-register-com)

```
INFO: Microsoft Windows Search COM API is not registered.
Attempting automatic registration...
```

Then proceeds to registration (see Registration Messages below).

---

#### COM API Not Registered (No Register Mode - --no-register-com)

```
ERROR: Microsoft Windows Search COM API is not registered.

This application requires the COM API to function. Registration was not attempted
because --no-register-com flag was specified.

To register manually, run:
  regsvr32 "%SystemRoot%\System32\SearchAPI.dll"

Then restart WindowsSearchConfigurator.
```

Exit with code 1.

---

### Privilege Check Messages

#### User Has Admin Privileges

No message displayed. Proceed to registration.

---

#### User Lacks Admin Privileges

```
ERROR: Administrative privileges required for COM API registration.

To register the COM API, you must run WindowsSearchConfigurator as Administrator.

How to run as Administrator:
  1. Right-click on WindowsSearchConfigurator.exe
  2. Select "Run as administrator"
  3. Try your command again

Or use this command in an elevated prompt:
  WindowsSearchConfigurator.exe --auto-register-com [your command]

Alternatively, you can register manually:
  1. Open Command Prompt as Administrator
  2. Run: regsvr32 "%SystemRoot%\System32\SearchAPI.dll"
  3. Run WindowsSearchConfigurator again
```

Exit with code 2.

---

### Registration Messages

#### Registration In Progress

```
Registering COM API...
```

Shows spinner or progress indicator (if applicable).

---

#### Registration Succeeded

```
SUCCESS: COM API registered successfully.

Validating registration...
SUCCESS: COM API is functional.

Continuing with your command...
```

Then proceeds to execute the user's command.

---

#### Registration Failed - Generic Error

```
ERROR: COM API registration failed.

Registration attempt completed but returned an error (exit code: 3).

To register manually:
  1. Open Command Prompt as Administrator
  2. Run: regsvr32 "%SystemRoot%\System32\SearchAPI.dll"
  3. Restart WindowsSearchConfigurator

For troubleshooting assistance, see the log file at:
  %USERPROFILE%\.windowssearchconfigurator\audit.log
```

Exit with code 1.

---

#### Registration Failed - DLL Not Found

```
ERROR: COM API DLL not found.

The Windows Search COM API DLL (SearchAPI.dll) was not found at the expected location:
  C:\Windows\System32\SearchAPI.dll

This may indicate that Windows Search is not installed on this system.

To resolve:
  1. Verify Windows Search is installed (Control Panel > Programs > Turn Windows features on or off)
  2. If installed, check if SearchAPI.dll exists in System32 folder
  3. If missing, repair or reinstall Windows Search

For more information, see: https://support.microsoft.com/windows-search
```

Exit with code 1.

---

#### Registration Failed - Timeout

```
ERROR: COM API registration timed out.

The registration process did not complete within the expected time (5 seconds).

Possible causes:
  - System is under heavy load
  - Antivirus software is blocking the operation
  - Disk I/O issues

Please try again. If the problem persists, try manual registration:
  1. Open Command Prompt as Administrator
  2. Run: regsvr32 "%SystemRoot%\System32\SearchAPI.dll"
  3. Wait for confirmation message
  4. Restart WindowsSearchConfigurator
```

Exit with code 1.

---

#### Registration Succeeded But Validation Failed

```
WARNING: COM API registration completed but validation failed.

The registration process completed successfully, but the COM object could not be instantiated.

This may indicate:
  - Incomplete registration
  - DLL version mismatch
  - Corrupted Windows Search installation

Recommended actions:
  1. Restart your computer
  2. Try manual registration:
     regsvr32 "%SystemRoot%\System32\SearchAPI.dll"
  3. If problem persists, repair Windows Search installation

For troubleshooting assistance, see the log file at:
  %USERPROFILE%\.windowssearchconfigurator\audit.log
```

Exit with code 1.

---

### Manual Instructions (User Selected "No" in Interactive Mode)

```
Manual COM API Registration Instructions

To register the Windows Search COM API manually:

  1. Open Command Prompt as Administrator
     - Press Windows key
     - Type "cmd"
     - Right-click "Command Prompt"
     - Select "Run as administrator"

  2. Run the registration command:
     regsvr32 "%SystemRoot%\System32\SearchAPI.dll"

  3. Wait for confirmation dialog:
     - If successful: "DllRegisterServer in SearchAPI.dll succeeded"
     - If failed: Note the error message for troubleshooting

  4. Close the Command Prompt

  5. Run WindowsSearchConfigurator again

For troubleshooting:
  - Verify Windows Search service is installed
  - Check that SearchAPI.dll exists in C:\Windows\System32
  - Ensure you have administrative privileges
  - Review Windows Event Viewer for errors

For more help, see: [documentation URL]
```

Exit with code 1.

---

## Exit Codes

| Code | Meaning | Scenario |
|------|---------|----------|
| 0 | Success | COM API registered (or was already registered) and command executed successfully |
| 1 | COM registration declined/failed | User declined registration, registration failed, or --no-register-com used when COM not registered |
| 2 | Elevation required | User lacks admin privileges and registration was requested |
| 3 | Invalid arguments | Conflicting flags (--auto-register-com and --no-register-com both specified) |

---

## Behavior Matrix

| Scenario | Interactive (No Flags) | --auto-register-com | --no-register-com |
|----------|------------------------|---------------------|-------------------|
| COM registered, user is admin | Proceed silently | Proceed silently | Proceed silently |
| COM registered, user is not admin | Proceed silently | Proceed silently | Proceed silently |
| COM not registered, user is admin | Prompt user (Y/N/Q) | Auto-register, then proceed | Show error, exit code 1 |
| COM not registered, user is not admin | Prompt user → Show elevation instructions → Exit code 2 | Show elevation instructions, exit code 2 | Show error, exit code 1 |
| User selects "Yes" in prompt | Check privileges, register, proceed | N/A | N/A |
| User selects "No" in prompt | Show manual instructions, exit code 1 | N/A | N/A |
| User selects "Quit" in prompt | Exit code 1 | N/A | N/A |
| Registration succeeds | Validate, then proceed | Validate, then proceed | N/A |
| Registration fails | Show error, exit code 1 | Show error, exit code 1 | N/A |
| Both flags specified | Show error, exit code 3 | Show error, exit code 3 | Show error, exit code 3 |

---

## Help Text Integration

The global options should appear in all command help outputs.

### Root Command Help

```
WindowsSearchConfigurator.exe --help

Description:
  Manage Windows Search index rules and configuration

Usage:
  WindowsSearchConfigurator [options] [command]

Options:
  --auto-register-com     Automatically register COM API if not registered (requires admin)
  --no-register-com       Exit with error if COM API is not registered (do not attempt registration)
  -h, --help              Show help and usage information
  --version               Show version information

Commands:
  list        List current Windows Search index rules
  add         Add folder or file type to the index
  remove      Remove folder or file type from the index
  modify      Modify existing index rule settings
  export      Export index configuration to JSON file
  import      Import index configuration from JSON file
  search      Search for file extension indexing settings
  configure   Configure indexing depth for file extensions

Notes:
  - Most commands require administrative privileges to modify index settings
  - The --auto-register-com and --no-register-com options are mutually exclusive
  - COM API registration is required for all operations
```

---

### Command-Specific Help

```
WindowsSearchConfigurator.exe list --help

Description:
  List current Windows Search index rules

Usage:
  WindowsSearchConfigurator [options] list [command-options]

Command Options:
  --format <json|table>   Output format (default: table)
  -v, --verbose           Show detailed rule information

Global Options:
  --auto-register-com     Automatically register COM API if not registered (requires admin)
  --no-register-com       Exit with error if COM API is not registered
  -h, --help              Show help and usage information

Examples:
  WindowsSearchConfigurator list
  WindowsSearchConfigurator --auto-register-com list --format json
  WindowsSearchConfigurator list --verbose
```

---

## Error Messages

### Conflicting Flags Error

```
ERROR: Conflicting options specified.

The --auto-register-com and --no-register-com options are mutually exclusive.
Please specify only one of these options, or neither to use interactive mode.

Usage:
  WindowsSearchConfigurator [--auto-register-com | --no-register-com] [command]

For more information, run:
  WindowsSearchConfigurator --help
```

Exit with code 3.

---

## Logging

All COM registration activity is logged to the audit log with these event types:

```
[2025-10-22T14:30:00Z] INFO: COM registration check started
[2025-10-22T14:30:00Z] WARN: COM API not registered (CLSID not found)
[2025-10-22T14:30:05Z] INFO: User selected 'Yes' for automatic registration (Mode: Interactive)
[2025-10-22T14:30:05Z] INFO: Privilege check passed (User: DOMAIN\AdminUser, IsAdmin: true)
[2025-10-22T14:30:05Z] INFO: Starting COM registration (Method: regsvr32, DLL: C:\Windows\System32\SearchAPI.dll)
[2025-10-22T14:30:06Z] INFO: regsvr32 completed (ExitCode: 0, Duration: 1234ms)
[2025-10-22T14:30:06Z] INFO: Validating COM object instantiation
[2025-10-22T14:30:06Z] INFO: COM validation succeeded (State: Valid)
[2025-10-22T14:30:06Z] INFO: COM registration completed successfully (AttemptId: a1b2c3d4-...)
```

---

## Testing Contract

### Contract Test Cases

1. **Test: --auto-register-com flag is recognized**
   - Input: `["--auto-register-com", "list"]`
   - Expected: Flag parsed correctly, AutoRegister = true

2. **Test: --no-register-com flag is recognized**
   - Input: `["--no-register-com", "list"]`
   - Expected: Flag parsed correctly, NoRegister = true

3. **Test: Conflicting flags return error**
   - Input: `["--auto-register-com", "--no-register-com", "list"]`
   - Expected: Exit code 3, error message displayed

4. **Test: Interactive prompt accepts Y/N/Q**
   - Input: User types "Y", "N", or "Q"
   - Expected: Appropriate action taken based on input

5. **Test: Invalid interactive input re-prompts**
   - Input: User types "xyz"
   - Expected: "Invalid choice" message, re-prompt

6. **Test: Exit codes are correct**
   - Scenarios: Test all combinations from Behavior Matrix
   - Expected: Correct exit code for each scenario

7. **Test: Help text includes COM flags**
   - Input: `["--help"]`
   - Expected: Both --auto-register-com and --no-register-com documented

8. **Test: COM registration messages are displayed**
   - Scenarios: Success, failure, timeout, DLL not found
   - Expected: Appropriate message for each scenario

---

## Accessibility Considerations

- All prompts use simple, clear language
- Error messages avoid technical jargon where possible
- Manual instructions include step-by-step guidance
- Exit codes allow automated tooling to detect failure states
- Console output is screen-reader friendly (no ASCII art, clear structure)

---

## Localization Considerations (Future)

While initial implementation is English-only, the design supports future localization:

- All user-facing strings should be defined as constants (not inline)
- Error codes are numeric and language-independent
- Exit codes are universal
- Structured logging uses English keywords but includes metadata for translation

---

## References

- [System.CommandLine Documentation](https://github.com/dotnet/command-line-api/blob/main/docs/Your-first-app-with-System-CommandLine.md)
- [Console Application Best Practices](https://learn.microsoft.com/en-us/dotnet/standard/commandline/)
- [Exit Code Conventions](https://tldp.org/LDP/abs/html/exitcodes.html)
