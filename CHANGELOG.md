# Changelog

All notable changes to the Windows Search Configurator project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.1.0] - 2025-10-23

### Added

#### Verbose Diagnostic Logging (Feature 004)

- **Global `--verbose` Flag**: Enable comprehensive diagnostic logging with `--verbose` or `-v` flag on any command
- **Automatic Log Files**: Logs written to uniquely-named files (`WindowsSearchConfigurator_YYYYMMDD_HHMMSS_GUID.log`) in executable directory
- **Session Metadata**: Complete session tracking with:
  - Unique session ID (GUID)
  - Start/end timestamps (ISO 8601 UTC format)
  - Command-line arguments captured
  - User context (username, domain, working directory)
  - System information (Windows version, .NET runtime)
  - Exit code and final status (Success/Failed/Aborted)
  - Total session duration
- **Five Severity Levels**:
  - INFO - General informational messages
  - OPERATION - Key operations being performed
  - WARNING - Non-critical issues
  - ERROR - Operation-preventing errors
  - EXCEPTION - Unhandled exceptions with full stack traces
- **Comprehensive Command Logging**: All 8 commands now log:
  - Operation start with parameters
  - Privilege checks and validation steps
  - Key decision points (user confirmations, filters applied)
  - Success/failure outcomes with details
  - Exception handling with stack traces
- **Thread-Safe File I/O**: Async file operations with SemaphoreSlim synchronization
- **Graceful Degradation**: File logging failures don't prevent tool operation - continues with console output only
- **Dual Output Mode**: When `--verbose` enabled, output goes to both console (INFO/OPERATION/ERROR) and log file (all severities)

**Logging Coverage**:
- `AddCommand`: 8 diagnostic logging points
- `RemoveCommand`: 7 diagnostic logging points
- `ListCommand`: 9 diagnostic logging points
- `ModifyCommand`: 12 diagnostic logging points
- `ConfigureDepthCommand`: 8 diagnostic logging points
- `SearchExtensionsCommand`: 7 diagnostic logging points
- `ExportCommand`: 7 diagnostic logging points
- `ImportCommand`: 11 diagnostic logging points

**Core Components**:
- `LogSeverity` enum - Five severity levels
- `LogEntry` record - Individual log entry with validation
- `SessionStatus` enum - Session state tracking
- `LogSession` class - Complete session metadata with duration calculation
- `IFileLogger` interface - File logging abstraction
- `ILogFileNameGenerator` interface - Filename generation abstraction
- `FileLogger` class - Thread-safe async file writer
- `LogFileNameGenerator` class - Unique filename generation with timestamps and GUIDs
- Enhanced `VerboseLogger` - Dual output with session management and IDisposable support

**Use Cases**:
- **Troubleshooting**: Capture detailed diagnostics when operations fail
- **Auditing**: Complete trail of administrative actions
- **Debugging**: See exact filter patterns, validation steps, and decision points
- **Performance Analysis**: Timestamps identify slow operations
- **Automated Deployments**: Generate logs for centralized review

**Performance**:
- Minimal overhead (<5% in typical scenarios)
- Async I/O prevents blocking
- Log files: 5-50 KB per execution
- SemaphoreSlim ensures thread safety without performance degradation

**Test Coverage**: 42 new tests (229 total tests passing)
- `LogEntryTests` - Validation, immutability, severity handling
- `LogSessionTests` - Duration calculation, status tracking
- `LogFileNameGeneratorTests` - Unique names, format validation
- `FileLoggerTests` - Initialization, error handling, disposal
- `FileLoggerWriteTests` - Entry writing, session headers/footers
- `VerboseLoggerTests` - Dual output, session lifecycle, integration

### Added

#### COM API Registration Support (Feature 002)

- **Automatic COM API Detection**: Tool now detects if the Microsoft.Search.Interop.CSearchManager COM API is registered before executing commands
- **Interactive Registration**: When COM API is not registered, users are offered an option to attempt automatic registration with clear guidance
- **Privilege-Aware Registration**: System checks for administrative privileges before attempting registration and provides elevation instructions for standard users
- **Non-Interactive Modes**: Added command-line flags for automation scenarios:
  - `--auto-register-com`: Automatically register COM API without prompts (requires admin)
  - `--no-register-com`: Fail immediately if COM API not registered (useful for CI/CD pre-checks)
- **Clear Error Messages**: Non-technical error messages explain the issue and provide actionable next steps
- **Manual Registration Instructions**: Users who decline automatic registration see step-by-step manual registration guidance
- **Registration Validation**: After successful registration, tool validates that COM object can be instantiated
- **Performance Optimized**: COM detection completes in under 500ms, registration in under 5 seconds
- **Audit Logging**: All COM registration attempts are logged for troubleshooting and compliance

**User Stories Implemented**:
- US1: Detection and Notification (P1) - Clear error messages when COM API missing
- US2: Interactive Registration Offer (P2) - User can accept, decline, or quit
- US3: Privilege-Aware Guidance (P2) - Checks admin rights and shows elevation instructions
- US4: Non-Interactive Support (P3) - Automation flags for CI/CD scenarios

**Exit Codes**:
- `0`: Success - command executed successfully
- `1`: Error - COM not registered or registration failed
- `2`: Elevation Required - admin privileges needed for registration
- `3`: Conflict - both `--auto-register-com` and `--no-register-com` specified

**Technical Details**:
- Registry-based CLSID detection (HKEY_CLASSES_ROOT)
- Process execution via `regsvr32.exe` for registration
- COM object instantiation validation
- Comprehensive test coverage (unit, integration, contract tests)

### Changed

- Application startup now includes COM API registration check before command execution
- Help and version commands (`--help`, `--version`) now work without COM API registration

### Fixed

- N/A (new feature)

## [1.0.0] - 2025-10-22

### Added

- Initial release of Windows Search Configurator
- Command-line interface for managing Windows Search index rules
- Support for adding, removing, and modifying index rules
- File extension indexing depth configuration
- Import/export functionality for configuration backup
- Comprehensive logging and audit trail
- Multi-format output (table, JSON, CSV)

### Requirements

- .NET 8.0 or later
- Windows 10/11 or Windows Server 2016+
- Windows Search service installed and running
- Administrative privileges for modification operations

---

## Version History

- **[Unreleased]**: COM API Registration Support (Feature 002)
- **[1.0.0]**: Initial release

[unreleased]: https://github.com/yourusername/WindowsSearchConfigurator/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/yourusername/WindowsSearchConfigurator/releases/tag/v1.0.0
