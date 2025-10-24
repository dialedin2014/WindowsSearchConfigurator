# Implementation Plan: Verbose Diagnostic Logging

**Branch**: `004-verbose-diagnostic-logging` | **Date**: October 23, 2025 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/004-verbose-diagnostic-logging/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Enhance the Windows Search Configurator tool to write verbose diagnostic information to log files when a verbose flag is specified. The tool already has a VerboseLogger class that writes to console; this feature extends it to also write timestamped, detailed diagnostic logs to files in the executable's directory. This enables users to troubleshoot issues independently by capturing registry operations, COM API calls, errors with stack traces, and operation timing information.

## Technical Context

**Language/Version**: C# / .NET 8.0 (LTS)  
**Primary Dependencies**: System.CommandLine (2.0.0-beta4), Microsoft.Extensions.DependencyInjection (9.0.10), System.IO for file operations  
**Storage**: Text log files written to executable directory using StreamWriter  
**Testing**: NUnit 4.x (test framework), NUnit3TestAdapter (test runner), Microsoft.NET.Test.Sdk  
**Target Platform**: Windows 10/11 and Windows Server 2016+  
**Project Type**: Single console application with existing CLI architecture  
**Performance Goals**: <100ms overhead for file logging operations per command execution  
**Constraints**: Must handle write failures gracefully without crashing; log files limited to reasonable size (spec suggests 100MB per session assumption)  
**Scale/Scope**: Expected single-user tool with typical sessions generating 1-10MB log files; must handle concurrent executions with unique filenames

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### I. Automated Testing ✓
- **Status**: PASS
- **Plan**: Unit tests for file logging, integration tests for verbose flag handling, contract tests for log file format
- **Justification**: New logging functionality will have comprehensive test coverage across all test categories

### II. Windows API Safety ✓
- **Status**: PASS
- **Plan**: File I/O operations will include proper error handling and validation; existing Windows Search API calls already properly handled
- **Justification**: File operations will check permissions, handle write failures, and provide clear error messages

### III. User Configuration Control ✓
- **Status**: PASS
- **Plan**: Verbose logging is opt-in via explicit command-line flag; no automatic behavior changes
- **Justification**: Users must explicitly request verbose logging; no silent modifications

### IV. Clear Interface Design ✓
- **Status**: PASS
- **Plan**: Command-line flag follows standard conventions (--verbose, -v); log files are human-readable text format
- **Justification**: Standard verbose flag pattern; clear log format; helpful error messages for write failures

### V. Documentation and Maintainability ✓
- **Status**: PASS
- **Plan**: Log file format, location, and naming convention will be documented; code will include XML documentation
- **Justification**: Specification includes complete documentation requirements; implementation will follow existing code documentation patterns

### VI. Incremental Implementation ✓
- **Status**: PASS
- **Plan**: Implementation broken into batches of 4-6 files maximum per phase
- **Justification**: Tasks will be organized into digestible batches focusing on: (1) file logging infrastructure, (2) integration with existing VerboseLogger, (3) command integration, (4) error handling

### VII. Source Control Discipline ✓
- **Status**: PASS
- **Plan**: Feature branch 004-verbose-diagnostic-logging; frequent commits with conventional commit messages; GitHub CLI for operations
- **Justification**: Already on feature branch; will use conventional commits like `feat(logging): add file writer`, `test(logging): add unit tests for log file creation`

**OVERALL STATUS**: ✅ ALL GATES PASS - Ready to proceed with Phase 0 research

---

## Post-Phase 1 Constitution Re-Check

**Date**: October 23, 2025  
**Status**: ✅ ALL GATES STILL PASS

### Review Summary
After completing research and design phases, all constitution principles remain satisfied:

1. **Automated Testing** ✅: Comprehensive test strategy defined in quickstart.md covering unit, integration, and contract tests
2. **Windows API Safety** ✅: File I/O error handling documented in research.md; existing Windows Search API calls already properly handled
3. **User Configuration Control** ✅: Opt-in via --verbose flag; no silent behavior changes
4. **Clear Interface Design** ✅: Standard verbose flag convention; human-readable log format specified in contracts
5. **Documentation and Maintainability** ✅: Complete documentation generated (research, data-model, contracts, quickstart)
6. **Incremental Implementation** ✅: Implementation organized into 4 phases with 4-6 files per phase
7. **Source Control Discipline** ✅: On feature branch; conventional commit strategy documented

**No new violations identified during design phase. Ready for Phase 2 task breakdown.**

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/
└── WindowsSearchConfigurator/
    ├── Program.cs              # Entry point - already handles --verbose flag
    ├── Commands/               # Command implementations - integrate file logging
    ├── Core/
    │   ├── Interfaces/         # New: IFileLogger interface
    │   └── Models/             # New: LogEntry, LogSession models
    ├── Services/               # Existing services - add logging calls
    └── Utilities/
        └── VerboseLogger.cs    # Extend to write to files

tests/
├── WindowsSearchConfigurator.UnitTests/
│   ├── Core/                   # New: LogEntry, LogSession tests
│   ├── Utilities/              # New: VerboseLogger file writing tests
│   └── Services/               # Update existing tests for logging
├── WindowsSearchConfigurator.IntegrationTests/
│   └── [New: Verbose logging integration tests]
└── WindowsSearchConfigurator.ContractTests/
    └── [New: Log file format contract tests]
```

**Structure Decision**: Single console application project (existing). This feature extends the existing `VerboseLogger` utility class to write to both console and file. New interfaces and models will be added to Core layer following existing patterns. Existing test project structure will be used with new test files added for logging functionality.

## Complexity Tracking

**Status**: No violations - Complexity Tracking not required

All constitution principles satisfied without exceptions. No complex architectural patterns needed.

---

## Plan Execution Summary

### Completed Phases

✅ **Phase 0: Research** (Complete)
- Generated `research.md` with all technical decisions
- Resolved file logging approach (StreamWriter with async)
- Resolved log file naming convention (timestamp + GUID)
- Resolved executable directory detection (AppContext.BaseDirectory)
- Resolved error handling strategy (graceful degradation)
- Resolved timestamp format (ISO 8601 UTC with milliseconds)
- Resolved log file format structure
- Resolved session metadata requirements
- Resolved thread safety approach (SemaphoreSlim)
- Resolved integration strategy with existing VerboseLogger
- No external dependencies required

✅ **Phase 1: Design & Contracts** (Complete)
- Generated `data-model.md` with entities:
  - LogEntry (timestamp, severity, component, message, stack trace)
  - LogSeverity enum (Info, Operation, Warning, Error, Exception)
  - LogSession (session metadata with header/footer)
  - SessionStatus enum (InProgress, Success, Failed, Aborted)
  - LogFileMetadata (file information)
  - IFileLogger interface
  - ILogFileNameGenerator interface
- Generated `contracts/log-file-format.md`:
  - File naming pattern specification
  - Log file structure (header, entries, footer)
  - Format validation rules
  - Contract test requirements
  - Sample valid log file
- Generated `quickstart.md`:
  - Developer guide for implementation
  - Usage examples and best practices
  - Testing strategy
  - Configuration and error handling
  - Common pitfalls and solutions
- Updated agent context (copilot-instructions.md)
- Re-checked constitution compliance: ✅ ALL PASS

### Next Steps

📋 **Phase 2: Task Breakdown** (Next action - NOT done by /speckit.plan)
- Run `/speckit.tasks` command to generate `tasks.md`
- Organize implementation into incremental batches (4-6 files max)
- Define test-first approach for each batch
- Set up task dependencies and ordering

🔨 **Phase 3: Implementation** (After tasks.md created)
- Run `/speckit.implement` command for each task batch
- Follow task sequence from tasks.md
- Verify tests pass after each batch
- Make frequent commits with conventional commit messages

### Artifacts Generated

| File | Status | Description |
|------|--------|-------------|
| `plan.md` | ✅ Complete | This implementation plan |
| `research.md` | ✅ Complete | Technical research and decisions |
| `data-model.md` | ✅ Complete | Entity definitions and validation rules |
| `contracts/log-file-format.md` | ✅ Complete | Log file format specification |
| `quickstart.md` | ✅ Complete | Developer implementation guide |
| `tasks.md` | ⏳ Pending | Task breakdown (run /speckit.tasks) |

### Branch Information

**Branch**: `004-verbose-diagnostic-logging`  
**Status**: Feature branch created (from setup-plan script)  
**Git Status**: Plan artifacts committed

### Key Decisions Summary

1. **File Logging**: StreamWriter with async operations, UTF-8 encoding
2. **Naming**: `WindowsSearchConfigurator_{timestamp}_{guid}.log`
3. **Location**: Executable directory via `AppContext.BaseDirectory`
4. **Format**: Structured text with session header/footer
5. **Timestamps**: ISO 8601 UTC with millisecond precision
6. **Thread Safety**: SemaphoreSlim for concurrent writes
7. **Error Handling**: Graceful degradation (disable file, continue console)
8. **Integration**: Extend existing VerboseLogger class
9. **Testing**: Unit, integration, and contract tests
10. **Dependencies**: None (built-in .NET libraries only)

---

**Plan Status**: ✅ COMPLETE (Phase 0 & Phase 1)  
**Ready For**: Phase 2 - Task Breakdown (`/speckit.tasks` command)  
**Constitution Compliance**: ✅ ALL GATES PASS  
**Last Updated**: October 23, 2025
