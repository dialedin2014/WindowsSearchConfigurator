# Quickstart: Verbose Diagnostic Logging

**Feature**: 004-verbose-diagnostic-logging  
**Date**: October 23, 2025

## Overview

This quickstart guide helps developers understand and implement the verbose diagnostic logging feature for Windows Search Configurator. The feature extends the existing VerboseLogger to write detailed diagnostic information to timestamped log files in the executable's directory.

## What This Feature Does

When users run the tool with the `--verbose` or `-v` flag, the tool will:
1. Create a timestamped log file in the executable's directory
2. Write detailed diagnostic information to both console and file
3. Capture registry operations, COM API calls, errors, and timing information
4. Generate a structured log with session header, entries, and footer

## Key Components

### 1. Core Models (`Core/Models/`)

**LogEntry.cs** - Represents a single log event
```csharp
public record LogEntry(
    DateTime Timestamp,
    LogSeverity Severity,
    string Component,
    string Message,
    string? StackTrace = null
);
```

**LogSeverity.cs** - Enum for severity levels
```csharp
public enum LogSeverity
{
    Info = 0,
    Operation = 1,
    Warning = 2,
    Error = 3,
    Exception = 4
}
```

**LogSession.cs** - Represents a complete logging session
```csharp
public class LogSession
{
    public Guid SessionId { get; init; }
    public DateTime StartTime { get; init; }
    public DateTime? EndTime { get; set; }
    public string CommandLine { get; init; }
    public string UserName { get; init; }
    public string WorkingDirectory { get; init; }
    public string WindowsVersion { get; init; }
    public string RuntimeVersion { get; init; }
    public int? ExitCode { get; set; }
    public SessionStatus Status { get; set; }
    public TimeSpan? Duration => EndTime.HasValue ? EndTime.Value - StartTime : null;
}
```

**SessionStatus.cs** - Enum for session states
```csharp
public enum SessionStatus
{
    InProgress = 0,
    Success = 1,
    Failed = 2,
    Aborted = 3
}
```

### 2. Core Interfaces (`Core/Interfaces/`)

**IFileLogger.cs** - Interface for file logging operations
```csharp
public interface IFileLogger : IDisposable
{
    bool Initialize(string logFilePath);
    Task WriteEntryAsync(LogEntry entry);
    Task WriteSessionHeaderAsync(LogSession session);
    Task WriteSessionFooterAsync(LogSession session);
    bool IsEnabled { get; }
}
```

### 3. Utilities (`Utilities/`)

**FileLogger.cs** - Implements IFileLogger
- Uses StreamWriter for async file operations
- Thread-safe with SemaphoreSlim
- Graceful error handling (disables file logging on failures)
- UTF-8 encoding

**LogFileNameGenerator.cs** - Generates unique log filenames
- Pattern: `WindowsSearchConfigurator_{timestamp}_{guid}.log`
- Timestamp: `yyyyMMdd_HHmmss`
- GUID: First 6 characters

**VerboseLogger.cs** - Enhanced to support dual output
- Existing: Console output
- New: File output via IFileLogger
- Manages LogSession lifecycle
- Coordinates session header/footer writing

## Implementation Phases

### Phase 1: Core Infrastructure (4-6 files)
1. Create LogEntry, LogSeverity models
2. Create LogSession, SessionStatus models
3. Create IFileLogger interface
4. Add unit tests for models

### Phase 2: File Logging Implementation (4-6 files)
1. Implement FileLogger class
2. Implement LogFileNameGenerator class
3. Enhance VerboseLogger with file support
4. Add unit tests for FileLogger and VerboseLogger

### Phase 3: Integration (4-6 files)
1. Update Program.cs to initialize file logging
2. Update dependency injection configuration
3. Add integration tests
4. Add contract tests for log format

### Phase 4: Error Handling & Polish (4-6 files)
1. Add comprehensive error handling
2. Add logging to existing services
3. Update documentation
4. Final testing and validation

## Usage Examples

### For Developers: Adding Logging to Services

**Before** (existing):
```csharp
public class MyService
{
    private readonly VerboseLogger _logger;
    
    public void DoSomething()
    {
        _logger.WriteLine("Doing something...");
        // operation
        _logger.WriteLine("Done!");
    }
}
```

**After** (no changes needed!):
```csharp
// Existing code works as-is
// When verbose mode is enabled with file logging,
// output goes to both console and file automatically
```

### For Developers: Initializing File Logging

**Program.cs changes**:
```csharp
var services = ConfigureServices(verboseMode);
var serviceProvider = services.BuildServiceProvider();

var verboseLogger = serviceProvider.GetRequiredService<VerboseLogger>();

// NEW: Initialize file logging if verbose mode enabled
if (verboseMode)
{
    var logFileName = new LogFileNameGenerator().GenerateFileName();
    var logFilePath = Path.Combine(AppContext.BaseDirectory, logFileName);
    verboseLogger.InitializeFileLogging(logFilePath, commandLine: string.Join(" ", args));
}

verboseLogger.WriteLine("Windows Search Configurator starting...");
```

### For End Users

**Without verbose logging**:
```powershell
WindowsSearchConfigurator.exe add C:\Data
```

**With verbose logging**:
```powershell
WindowsSearchConfigurator.exe add C:\Data --verbose
```

**Result**: Log file created at:
```
C:\Path\To\Exe\WindowsSearchConfigurator_20251023_143055_a3f8d2.log
```

## Log File Example

```
================================ LOG SESSION START ================================
Session ID: a3f8d2c4-1234-5678-9abc-def012345678
Start Time: 2025-10-23T14:30:55.000Z
Command: WindowsSearchConfigurator.exe add C:\Data --verbose
User: DOMAIN\jsmith
Working Directory: C:\Users\jsmith\Desktop
Windows Version: Windows 11 22H2 (Build 22621.2134)
.NET Runtime: 8.0.0
===================================================================================
[2025-10-23T14:30:55.123Z] [INFO] Startup: Windows Search Configurator starting...
[2025-10-23T14:30:55.125Z] [INFO] Startup: Command-line arguments: add C:\Data --verbose
[2025-10-23T14:30:55.130Z] [OPERATION] RegistryAccessor: Reading key HKLM\SOFTWARE\...
[2025-10-23T14:30:55.145Z] [INFO] ServiceStatusChecker: Windows Search service is running
[2025-10-23T14:30:55.200Z] [OPERATION] SearchIndexManager: Adding path C:\Data to index
================================= LOG SESSION END =================================
End Time: 2025-10-23T14:31:05.234Z
Duration: 10.234 seconds
Exit Code: 0
Status: SUCCESS
===================================================================================
```

## Testing Strategy

### Unit Tests
- LogEntry creation and validation
- LogSession lifecycle and duration calculation
- FileLogger initialization and write operations
- LogFileNameGenerator format validation
- VerboseLogger dual-output coordination
- Error handling scenarios (permissions, disk full)

### Integration Tests
- End-to-end verbose logging with actual commands
- Log file creation in executable directory
- Session header/footer writing
- Concurrent execution scenarios

### Contract Tests
- Log file naming pattern validation
- Log file format compliance
- UTF-8 encoding verification
- Timestamp format validation
- Header/footer structure validation

## Configuration (Dependency Injection)

**Add to ConfigureServices in Program.cs**:
```csharp
services.AddSingleton<IFileLogger, FileLogger>();
services.AddSingleton<ILogFileNameGenerator, LogFileNameGenerator>();
services.AddSingleton<VerboseLogger>(); // Already exists
```

## Error Handling

### Permission Denied
- Catch `UnauthorizedAccessException`
- Log warning to console
- Disable file logging
- Continue with console-only output

### Disk Full
- Catch `IOException` during writes
- Log warning to console
- Disable file logging
- Continue with console-only output

### Invalid Path
- Validate path in Initialize()
- Return false if invalid
- Fall back to console-only logging

## Performance Considerations

- **Async I/O**: All file writes use async operations to prevent blocking
- **Buffering**: StreamWriter provides automatic buffering
- **Flush**: Flush after each write to ensure data persisted
- **Thread Safety**: SemaphoreSlim ensures thread-safe writes
- **Overhead Goal**: <100ms per session for file operations

## Best Practices

1. **Always check VerboseLogger.IsEnabled** before expensive string formatting
2. **Use structured messages**: `"{Component}: {Action} {Details}"`
3. **Log operations, not results**: Log what you're doing, then log the outcome
4. **Include context**: Registry keys, file paths, COM API methods
5. **Use appropriate severity**:
   - Info: General progress
   - Operation: Specific actions (registry, API)
   - Warning: Non-critical issues
   - Error: Failures that impact functionality
   - Exception: Full exception details with stack trace

## Common Pitfalls

❌ **Don't**: Format expensive strings when verbose is disabled
```csharp
// BAD: String interpolation happens even if disabled
logger.WriteLine($"Processing {ExpensiveOperation()}");
```

✅ **Do**: Check IsEnabled first
```csharp
// GOOD: ExpensiveOperation() only called if verbose enabled
if (logger.IsEnabled)
{
    logger.WriteLine($"Processing {ExpensiveOperation()}");
}
```

❌ **Don't**: Log sensitive data
```csharp
// BAD: Credentials in log file
logger.WriteLine($"Connecting with password: {password}");
```

✅ **Do**: Sanitize or omit sensitive data
```csharp
// GOOD: No credentials logged
logger.WriteLine("Connecting to service...");
```

## Next Steps

1. Review data-model.md for complete entity definitions
2. Review contracts/log-file-format.md for format specification
3. Implement Phase 1 (Core Infrastructure)
4. Run unit tests after each phase
5. Review plan.md for complete task breakdown

## Questions?

- See **research.md** for technical decision rationale
- See **data-model.md** for complete data structures
- See **contracts/log-file-format.md** for format specification
- See **spec.md** for original requirements and user scenarios

---

**Quickstart Version**: 1.0  
**Last Updated**: October 23, 2025
