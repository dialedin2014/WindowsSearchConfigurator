# Research: Verbose Diagnostic Logging

**Feature**: 004-verbose-diagnostic-logging  
**Date**: October 23, 2025  
**Status**: Complete

## Overview

Research findings for implementing file-based verbose diagnostic logging in the Windows Search Configurator tool. The tool already has a VerboseLogger class that writes to console; this research addresses extending it to write timestamped log files to the executable directory.

## Key Research Areas

### 1. File Logging Best Practices in .NET

**Decision**: Use `StreamWriter` with asynchronous operations and proper disposal patterns

**Rationale**:
- `StreamWriter` provides buffered text writing with good performance
- Built-in support for UTF-8 encoding (requirement from spec)
- Async operations prevent blocking on I/O
- `using` statements ensure proper file handle cleanup
- No external dependencies needed (uses System.IO)

**Alternatives Considered**:
- **Serilog/NLog**: Rejected - spec excludes external logging frameworks, requires keeping implementation simple
- **FileStream directly**: Rejected - StreamWriter provides text writing conveniences like WriteLine and encoding
- **System.Diagnostics.TraceListener**: Rejected - more complex than needed for simple file appending

**Implementation Pattern**:
```csharp
// Thread-safe file writing with async support
private readonly SemaphoreSlim _fileLock = new(1, 1);
private StreamWriter? _fileWriter;

public async Task WriteToFileAsync(string message)
{
    if (_fileWriter == null) return;
    
    await _fileLock.WaitAsync();
    try
    {
        await _fileWriter.WriteLineAsync($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] {message}");
        await _fileWriter.FlushAsync();
    }
    finally
    {
        _fileLock.Release();
    }
}
```

### 2. Log File Naming Convention

**Decision**: `WindowsSearchConfigurator_{timestamp}_{guid}.log` format

**Rationale**:
- Timestamp ensures chronological ordering and human readability (FR-007, FR-015)
- GUID suffix prevents collisions from concurrent executions (edge case requirement)
- `.log` extension for standard text editor association
- Pattern: `WindowsSearchConfigurator_20251023_143055_a3f8d2.log`

**Alternatives Considered**:
- **Process ID suffix**: Rejected - PIDs can be reused, doesn't guarantee uniqueness
- **Sequential numbering**: Rejected - requires reading directory and maintaining state
- **Timestamp only**: Rejected - concurrent executions could collide

**Format Specification**:
- Timestamp format: `yyyyMMdd_HHmmss` (sortable, no special characters)
- GUID: First 6 characters (sufficient uniqueness for typical usage)
- Total example: `WindowsSearchConfigurator_20251023_143055_a3f8d2.log`

### 3. Executable Directory Detection

**Decision**: Use `AppContext.BaseDirectory` for reliable executable directory path

**Rationale**:
- `AppContext.BaseDirectory` returns the directory containing the application base directory
- Works correctly for both single-file and framework-dependent deployments
- Recommended by Microsoft for .NET Core/.NET 5+ applications
- More reliable than `Assembly.GetExecutingAssembly().Location` which can be empty in single-file scenarios

**Alternatives Considered**:
- **Environment.CurrentDirectory**: Rejected - returns working directory, not executable location
- **Assembly.GetExecutingAssembly().Location**: Rejected - can be empty string in single-file published apps
- **Process.GetCurrentProcess().MainModule.FileName**: Rejected - can be null, requires additional error handling

**Implementation**:
```csharp
string exeDirectory = AppContext.BaseDirectory;
string logFilePath = Path.Combine(exeDirectory, logFileName);
```

### 4. Handling Write Permission Failures

**Decision**: Graceful degradation with console warning; do not crash application

**Rationale**:
- Core functionality (Windows Search configuration) should not be blocked by logging failures
- User may be running in restricted environment where exe directory is read-only
- Clear error message enables user to diagnose permission issues (FR-010)

**Error Handling Strategy**:
```csharp
try
{
    // Attempt to create log file
    _fileWriter = new StreamWriter(logFilePath, append: false, encoding: Encoding.UTF8);
}
catch (UnauthorizedAccessException ex)
{
    Console.WriteLine($"WARNING: Cannot create log file (access denied): {logFilePath}");
    Console.WriteLine("Verbose output will be written to console only.");
    _fileWriter = null; // Disable file logging
}
catch (IOException ex)
{
    Console.WriteLine($"WARNING: Cannot create log file (I/O error): {ex.Message}");
    Console.WriteLine("Verbose output will be written to console only.");
    _fileWriter = null;
}
```

### 5. Log Entry Timestamp Format

**Decision**: ISO 8601 format with millisecond precision in UTC

**Rationale**:
- ISO 8601 is internationally recognized standard (FR-003)
- Millisecond precision enables correlation of rapid operations
- UTC avoids timezone ambiguity when troubleshooting across systems
- Format: `2025-10-23T14:30:55.123Z`

**Alternatives Considered**:
- **Local time**: Rejected - timezone issues when sharing logs
- **Unix timestamp**: Rejected - not human readable
- **Microsecond precision**: Rejected - unnecessary for this use case, adds noise

**Implementation**:
```csharp
DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'")
// Or simply: DateTime.UtcNow.ToString("o") for full ISO 8601
```

### 6. Log File Format Structure

**Decision**: Simple structured text format with severity levels

**Rationale**:
- Human-readable text format (FR-011)
- Easy to parse with standard text tools (grep, findstr)
- Severity levels enable filtering (INFO, OPERATION, ERROR, EXCEPTION)
- Structured but not overly complex (spec excludes JSON/XML)

**Format Specification**:
```
[YYYY-MM-DDTHH:mm:ss.fffZ] [SEVERITY] Component: Message
```

**Example Log Entries**:
```
[2025-10-23T14:30:55.123Z] [INFO] Startup: Windows Search Configurator starting...
[2025-10-23T14:30:55.125Z] [INFO] Startup: Command-line arguments: add C:\Data --verbose
[2025-10-23T14:30:55.130Z] [OPERATION] RegistryAccessor: Reading key HKLM\SOFTWARE\Microsoft\Windows Search\CrawlScopeManager
[2025-10-23T14:30:55.145Z] [OPERATION] RegistryAccessor: Value found: IndexerState=2
[2025-10-23T14:30:55.890Z] [ERROR] SearchIndexManager: Failed to add path
[2025-10-23T14:30:55.891Z] [EXCEPTION] System.UnauthorizedAccessException: Access denied
Stack Trace:
   at WindowsSearchConfigurator.Services.SearchIndexManager.AddPath(...)
```

### 7. Session Metadata Requirements

**Decision**: Write session header and footer with summary information

**Rationale**:
- Session boundaries clearly marked in log file (FR-008, FR-009, FR-014)
- Command execution context captured upfront
- Summary information aids in quick assessment of session outcome
- Duration calculation helps identify performance issues

**Session Header Format**:
```
================================ LOG SESSION START ================================
Session ID: a3f8d2c4-1234-5678-9abc-def012345678
Start Time: 2025-10-23T14:30:55.000Z
Command: WindowsSearchConfigurator.exe add C:\Data --verbose
User: DOMAIN\username
Working Directory: C:\Users\Admin
Windows Version: Windows 11 22H2 (Build 22621.2134)
.NET Runtime: 8.0.0
===================================================================================
```

**Session Footer Format**:
```
================================= LOG SESSION END =================================
End Time: 2025-10-23T14:31:05.234Z
Duration: 10.234 seconds
Exit Code: 0
Status: SUCCESS
===================================================================================
```

### 8. Thread Safety for Concurrent Writes

**Decision**: Use `SemaphoreSlim` for thread-safe file writing

**Rationale**:
- Multiple operations may log concurrently (async operations)
- `SemaphoreSlim` provides async-friendly locking mechanism
- Prevents interleaved writes that could corrupt log entries
- Low overhead compared to Monitor or lock()

**Implementation**:
```csharp
private readonly SemaphoreSlim _fileLock = new SemaphoreSlim(1, 1);

public async Task WriteLineAsync(string message)
{
    await _fileLock.WaitAsync();
    try
    {
        await _fileWriter.WriteLineAsync(message);
        await _fileWriter.FlushAsync();
    }
    finally
    {
        _fileLock.Release();
    }
}
```

### 9. Integration with Existing VerboseLogger

**Decision**: Extend existing `VerboseLogger` class to support dual output (console + file)

**Rationale**:
- Minimal code changes to existing codebase
- All existing verbose logging calls automatically write to file
- Single responsibility: VerboseLogger remains the logging facade
- Dependency injection already set up for VerboseLogger

**Integration Pattern**:
```csharp
public class VerboseLogger : IDisposable
{
    private bool _isVerboseEnabled;
    private FileLogger? _fileLogger;
    
    public void Initialize(bool verbose, string? logFilePath = null)
    {
        _isVerboseEnabled = verbose;
        if (verbose && !string.IsNullOrEmpty(logFilePath))
        {
            _fileLogger = new FileLogger(logFilePath);
        }
    }
    
    public void WriteLine(string message)
    {
        if (!_isVerboseEnabled) return;
        
        Console.WriteLine($"[VERBOSE] {message}");
        _fileLogger?.WriteAsync($"[INFO] {message}").Wait();
    }
    
    public void Dispose()
    {
        _fileLogger?.Dispose();
    }
}
```

### 10. Handling Disk Space Exhaustion

**Decision**: Catch `IOException` during writes and disable file logging gracefully

**Rationale**:
- Disk full scenarios should not crash the application
- Console output continues to work
- User receives clear warning about logging failure
- Aligns with FR-010 requirement for graceful failure handling

**Implementation**:
```csharp
try
{
    await _fileWriter.WriteLineAsync(message);
    await _fileWriter.FlushAsync();
}
catch (IOException ex)
{
    // Disk full or other I/O error
    Console.WriteLine($"WARNING: Log file write failed: {ex.Message}");
    Console.WriteLine("File logging has been disabled. Console output continues.");
    
    // Clean up and disable file logging
    _fileWriter?.Dispose();
    _fileWriter = null;
}
```

## Technology Decisions Summary

| Component | Technology/Approach | Rationale |
|-----------|-------------------|-----------|
| File I/O | StreamWriter with async | Built-in, performant, proper encoding support |
| Thread Safety | SemaphoreSlim | Async-friendly locking for concurrent writes |
| Encoding | UTF-8 | Universal compatibility, spec requirement |
| Timestamp | ISO 8601 UTC with ms | Standard format, unambiguous, precise |
| Naming | timestamp_guid.log | Unique, sortable, collision-resistant |
| Location Detection | AppContext.BaseDirectory | Reliable for all deployment scenarios |
| Error Handling | Graceful degradation | Non-blocking, user-friendly |
| Session Tracking | Header/footer format | Clear boundaries, summary info |

## Dependencies

**No new external dependencies required**

All functionality can be implemented using built-in .NET libraries:
- `System.IO` - File operations, StreamWriter
- `System.Threading` - SemaphoreSlim for thread safety
- `System.Text` - UTF-8 encoding
- `System` - DateTime, Guid

## Open Questions

None - all technical decisions resolved.

## References

- [StreamWriter Class Documentation](https://docs.microsoft.com/en-us/dotnet/api/system.io.streamwriter)
- [AppContext.BaseDirectory Documentation](https://docs.microsoft.com/en-us/dotnet/api/system.appcontext.basedirectory)
- [SemaphoreSlim Class Documentation](https://docs.microsoft.com/en-us/dotnet/api/system.threading.semaphoreslim)
- [ISO 8601 DateTime Format](https://en.wikipedia.org/wiki/ISO_8601)
- [.NET File I/O Best Practices](https://docs.microsoft.com/en-us/dotnet/standard/io/)

---

**Research Complete**: All NEEDS CLARIFICATION items resolved. Ready for Phase 1 design.
