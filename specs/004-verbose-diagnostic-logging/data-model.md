# Data Model: Verbose Diagnostic Logging

**Feature**: 004-verbose-diagnostic-logging  
**Date**: October 23, 2025  
**Status**: Draft

## Overview

This document defines the data structures and entities for the verbose diagnostic logging feature. The logging system captures detailed diagnostic information during tool execution and persists it to timestamped log files.

## Core Entities

### 1. LogEntry

Represents a single logged event with timestamp, severity level, component/source, and message text.

**Properties**:
- `Timestamp` (DateTime): UTC timestamp with millisecond precision when the entry was created
- `Severity` (LogSeverity enum): Level of the log entry (Info, Operation, Warning, Error, Exception)
- `Component` (string): Source component or service that generated the entry (e.g., "RegistryAccessor", "SearchIndexManager")
- `Message` (string): Human-readable log message content
- `StackTrace` (string?, optional): Stack trace information for exceptions

**Validation Rules**:
- `Timestamp` must be valid UTC DateTime
- `Component` must not be null or empty
- `Message` must not be null (can be empty string)
- `StackTrace` is required when Severity is Exception, otherwise optional

**State Transitions**: Immutable once created

**Relationships**:
- Part of a LogSession (many-to-one)

**Format Example**:
```
[2025-10-23T14:30:55.123Z] [INFO] Startup: Windows Search Configurator starting...
[2025-10-23T14:30:55.145Z] [OPERATION] RegistryAccessor: Reading key HKLM\...\CrawlScopeManager
[2025-10-23T14:30:55.890Z] [ERROR] SearchIndexManager: Failed to add path
```

### 2. LogSeverity (Enum)

Defines the severity levels for log entries.

**Values**:
- `Info` = 0: General informational messages
- `Operation` = 1: Specific operations like registry reads/writes, API calls
- `Warning` = 2: Non-critical issues or potential problems
- `Error` = 3: Errors that may impact functionality
- `Exception` = 4: Exceptions with full details and stack traces

**Usage**: Used to categorize log entries and enable filtering during troubleshooting

### 3. LogSession

Represents a complete execution of the tool with verbose logging, containing metadata and collection of log entries.

**Properties**:
- `SessionId` (Guid): Unique identifier for this logging session
- `StartTime` (DateTime): UTC timestamp when session started
- `EndTime` (DateTime?): UTC timestamp when session ended (null if not yet ended)
- `CommandLine` (string): Full command-line arguments that triggered this session
- `UserName` (string): Windows username executing the tool
- `WorkingDirectory` (string): Current working directory when tool was launched
- `WindowsVersion` (string): Windows version information (e.g., "Windows 11 22H2 Build 22621")
- `RuntimeVersion` (string): .NET runtime version (e.g., "8.0.0")
- `ExitCode` (int?): Process exit code (null if session not yet completed)
- `Status` (SessionStatus enum): Overall session status

**Validation Rules**:
- `SessionId` must be non-empty GUID
- `StartTime` must be valid UTC DateTime
- `EndTime` must be null or later than StartTime
- `CommandLine` must not be null
- `ExitCode` must be null or valid exit code (typically 0-255)

**State Transitions**:
1. Created with StartTime, Status = InProgress
2. Updated with EndTime, ExitCode, Status = Completed/Failed when session ends

**Calculated Properties**:
- `Duration` (TimeSpan?): EndTime - StartTime (null if session not ended)

**Relationships**:
- Contains multiple LogEntry instances (one-to-many)

### 4. SessionStatus (Enum)

Defines the possible states of a logging session.

**Values**:
- `InProgress` = 0: Session is currently active
- `Success` = 1: Session completed successfully (exit code 0)
- `Failed` = 2: Session completed with error (non-zero exit code)
- `Aborted` = 3: Session terminated abnormally (e.g., Ctrl+C)

### 5. LogFileMetadata

Represents information about a physical log file on disk.

**Properties**:
- `FilePath` (string): Full absolute path to the log file
- `FileName` (string): Log file name (e.g., "WindowsSearchConfigurator_20251023_143055_a3f8d2.log")
- `CreatedTime` (DateTime): UTC timestamp when file was created
- `FileSize` (long?): Size of log file in bytes (null if not yet written)

**Validation Rules**:
- `FilePath` must be valid absolute path
- `FileName` must match naming convention pattern
- `CreatedTime` must be valid UTC DateTime
- `FileSize` must be null or non-negative

**Format Convention**:
- Pattern: `WindowsSearchConfigurator_{timestamp}_{guid}.log`
- Timestamp format: `yyyyMMdd_HHmmss`
- GUID: First 6 characters (lowercase hex)
- Example: `WindowsSearchConfigurator_20251023_143055_a3f8d2.log`

## Entity Relationships

```
LogSession (1) ----contains----> (0..*) LogEntry
LogSession (1) ----persisted to----> (1) LogFileMetadata
```

## Data Flow

1. **Session Initialization**:
   - User executes tool with `--verbose` flag
   - `LogSession` created with metadata (command, user, timestamp, SessionId)
   - `LogFileMetadata` created with unique filename
   - Physical log file created in executable directory
   - Session header written to file

2. **During Execution**:
   - Components create `LogEntry` instances via VerboseLogger
   - Each entry formatted and written to both console and file
   - File writes are thread-safe (SemaphoreSlim)
   - I/O errors handled gracefully (disable file logging, continue console)

3. **Session Completion**:
   - `LogSession` updated with EndTime, ExitCode, Status
   - Session footer written to file
   - File handle closed and flushed
   - Final log file persisted on disk

## Validation Rules Summary

### LogEntry
- ✅ Timestamp must be valid UTC DateTime
- ✅ Severity must be valid LogSeverity enum value
- ✅ Component must not be null or empty string
- ✅ Message must not be null
- ✅ StackTrace required only when Severity = Exception

### LogSession
- ✅ SessionId must be non-empty GUID
- ✅ StartTime must be valid UTC DateTime
- ✅ EndTime must be null or >= StartTime
- ✅ CommandLine must not be null
- ✅ ExitCode must be null or 0-255
- ✅ Status must be valid SessionStatus enum value

### LogFileMetadata
- ✅ FilePath must be valid absolute path
- ✅ FileName must match pattern: `WindowsSearchConfigurator_{yyyyMMdd_HHmmss}_{6-char-guid}.log`
- ✅ CreatedTime must be valid UTC DateTime
- ✅ FileSize must be null or >= 0

## File Format Specification

### Session Header
```
================================ LOG SESSION START ================================
Session ID: {SessionId}
Start Time: {StartTime:yyyy-MM-ddTHH:mm:ss.fffZ}
Command: {CommandLine}
User: {UserName}
Working Directory: {WorkingDirectory}
Windows Version: {WindowsVersion}
.NET Runtime: {RuntimeVersion}
===================================================================================
```

### Log Entry
```
[{Timestamp:yyyy-MM-ddTHH:mm:ss.fffZ}] [{Severity}] {Component}: {Message}
{StackTrace if present}
```

### Session Footer
```
================================= LOG SESSION END =================================
End Time: {EndTime:yyyy-MM-ddTHH:mm:ss.fffZ}
Duration: {Duration.TotalSeconds:F3} seconds
Exit Code: {ExitCode}
Status: {Status}
===================================================================================
```

## Storage Considerations

**Location**: Executable directory (via `AppContext.BaseDirectory`)

**Naming Strategy**: Timestamp + GUID ensures uniqueness and sortability
- Chronological ordering by filename
- No collisions from concurrent executions
- Easy to identify recent vs. old logs

**Retention**: No automatic cleanup (out of scope per spec)
- Users manually manage log files
- Typical session < 10MB (assumption from spec)

**Encoding**: UTF-8 without BOM (standard for text files)

**Performance**: Async I/O with buffering via StreamWriter
- Minimal impact on tool execution (<100ms overhead goal)
- FlushAsync after each write ensures data persisted

**Error Handling**: 
- UnauthorizedAccessException → disable file logging, warn user, continue
- IOException (disk full) → disable file logging, warn user, continue
- Never crash application due to logging failures

## Interface Definitions

### IFileLogger

```csharp
public interface IFileLogger : IDisposable
{
    /// <summary>
    /// Initializes the file logger with the specified log file path.
    /// </summary>
    /// <param name="logFilePath">Full path to the log file.</param>
    /// <returns>True if initialization succeeded, false otherwise.</returns>
    bool Initialize(string logFilePath);
    
    /// <summary>
    /// Writes a log entry to the file asynchronously.
    /// </summary>
    /// <param name="entry">The log entry to write.</param>
    Task WriteEntryAsync(LogEntry entry);
    
    /// <summary>
    /// Writes the session header to the log file.
    /// </summary>
    /// <param name="session">The session metadata.</param>
    Task WriteSessionHeaderAsync(LogSession session);
    
    /// <summary>
    /// Writes the session footer to the log file.
    /// </summary>
    /// <param name="session">The session metadata.</param>
    Task WriteSessionFooterAsync(LogSession session);
    
    /// <summary>
    /// Gets a value indicating whether file logging is currently active.
    /// </summary>
    bool IsEnabled { get; }
}
```

### ILogFileNameGenerator

```csharp
public interface ILogFileNameGenerator
{
    /// <summary>
    /// Generates a unique log file name with timestamp and GUID.
    /// </summary>
    /// <returns>Log file name (not full path).</returns>
    string GenerateFileName();
}
```

## Implementation Notes

- `LogEntry` and `LogSession` should be simple immutable classes (records or classes with init-only properties)
- `LogSeverity` and `SessionStatus` enums should be in Core/Models
- `IFileLogger` interface in Core/Interfaces
- Concrete FileLogger implementation in Utilities
- VerboseLogger extended to use IFileLogger via dependency injection
- File writing must be thread-safe (use SemaphoreSlim)
- All file operations async to prevent blocking
- Graceful degradation on errors (log to console, warn user, continue execution)

---

**Data Model Complete**: Ready for contract generation and implementation.
