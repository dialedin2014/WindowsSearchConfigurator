# Tasks: Verbose Diagnostic Logging

**Feature**: 004-verbose-diagnostic-logging  
**Input**: Design documents from `/specs/004-verbose-diagnostic-logging/`  
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅, quickstart.md ✅

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure - no code changes needed (existing project)

- [X] T001 Verify existing project structure and dependencies per plan.md
- [X] T002 Review existing VerboseLogger implementation in src/WindowsSearchConfigurator/Utilities/VerboseLogger.cs
- [X] T003 Review existing test project structure in tests/ directories

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T004 [P] Create LogSeverity enum in src/WindowsSearchConfigurator/Core/Models/LogSeverity.cs
- [X] T005 [P] Create LogEntry record in src/WindowsSearchConfigurator/Core/Models/LogEntry.cs
- [X] T006 [P] Create SessionStatus enum in src/WindowsSearchConfigurator/Core/Models/SessionStatus.cs
- [X] T007 Create LogSession class in src/WindowsSearchConfigurator/Core/Models/LogSession.cs
- [X] T008 [P] Create IFileLogger interface in src/WindowsSearchConfigurator/Core/Interfaces/IFileLogger.cs
- [X] T009 [P] Create ILogFileNameGenerator interface in src/WindowsSearchConfigurator/Core/Interfaces/ILogFileNameGenerator.cs
- [X] T010 [P] Add unit tests for LogEntry in tests/WindowsSearchConfigurator.UnitTests/Core/Models/LogEntryTests.cs
- [X] T011 [P] Add unit tests for LogSession in tests/WindowsSearchConfigurator.UnitTests/Core/Models/LogSessionTests.cs
- [X] T012 [P] Add unit tests for LogSeverity enum in tests/WindowsSearchConfigurator.UnitTests/Core/Models/LogSeverityTests.cs
- [X] T013 [P] Add unit tests for SessionStatus enum in tests/WindowsSearchConfigurator.UnitTests/Core/Models/SessionStatusTests.cs

**Checkpoint**: Foundation ready - user story implementation can now begin ✅

---

## Phase 3: User Story 1 - Enable Verbose Logging for Troubleshooting (Priority: P1) 🎯 MVP

**Goal**: Enable users to run the tool with a verbose flag that writes detailed diagnostic information to a log file in the executable's directory, including registry operations, COM API calls, errors with stack traces, and timing information.

**Independent Test**: Run any command with `--verbose` flag and verify that a diagnostic log file is created in the executable's directory with detailed trace information including session header, log entries, and session footer.

### Implementation for User Story 1

- [X] T014 [P] [US1] Implement LogFileNameGenerator class in src/WindowsSearchConfigurator/Utilities/LogFileNameGenerator.cs
- [X] T015 [P] [US1] Implement FileLogger class with Initialize, WriteEntryAsync, WriteSessionHeaderAsync, WriteSessionFooterAsync methods in src/WindowsSearchConfigurator/Utilities/FileLogger.cs
- [X] T016 [US1] Enhance VerboseLogger class to integrate IFileLogger and manage dual output (console + file) in src/WindowsSearchConfigurator/Utilities/VerboseLogger.cs
- [X] T017 [US1] Add session management methods (InitializeSession, CompleteSession) to VerboseLogger in src/WindowsSearchConfigurator/Utilities/VerboseLogger.cs
- [X] T018 [P] [US1] Add unit tests for LogFileNameGenerator in tests/WindowsSearchConfigurator.UnitTests/Utilities/LogFileNameGeneratorTests.cs
- [X] T019 [P] [US1] Add unit tests for FileLogger initialization and error handling in tests/WindowsSearchConfigurator.UnitTests/Utilities/FileLoggerTests.cs
- [X] T020 [P] [US1] Add unit tests for FileLogger write operations in tests/WindowsSearchConfigurator.UnitTests/Utilities/FileLoggerWriteTests.cs
- [X] T021 [P] [US1] Add unit tests for enhanced VerboseLogger dual-output in tests/WindowsSearchConfigurator.UnitTests/Utilities/VerboseLoggerTests.cs

**Checkpoint**: At this point, file logging infrastructure is complete and tested ✅

---

## Phase 4: User Story 1 Integration (Priority: P1) 🎯 MVP

**Goal**: Wire up file logging to Program.cs and existing commands so verbose flag actually creates log files

**Independent Test**: Execute `WindowsSearchConfigurator.exe list --verbose` and verify log file created with valid format

### Implementation for User Story 1 Integration (Batch 2)

- [ ] T022 [US1] Update dependency injection in Program.cs to register IFileLogger, ILogFileNameGenerator, and VerboseLogger
- [ ] T023 [US1] Update Program.cs to initialize file logging when --verbose flag is present
- [ ] T024 [US1] Update Program.cs to complete logging session with exit code and status on shutdown
- [ ] T025 [P] [US1] Add logging calls to AddCommand in src/WindowsSearchConfigurator/Commands/AddCommand.cs for operation details
- [ ] T026 [P] [US1] Add logging calls to RemoveCommand in src/WindowsSearchConfigurator/Commands/RemoveCommand.cs for operation details
- [ ] T027 [P] [US1] Add logging calls to ListCommand in src/WindowsSearchConfigurator/Commands/ListCommand.cs for operation details

**Checkpoint**: At this point, verbose logging with file output is functional for basic commands

---

## Phase 5: User Story 1 Completion (Priority: P1) 🎯 MVP

**Goal**: Add comprehensive logging throughout the application and validate with integration/contract tests

**Independent Test**: Run multiple commands with verbose flag and verify all log files follow contract specification

### Implementation for User Story 1 Completion (Batch 3)

- [ ] T028 [P] [US1] Add logging calls to remaining commands (ModifyCommand, ConfigureDepthCommand, ExportCommand, ImportCommand, SearchExtensionsCommand) in src/WindowsSearchConfigurator/Commands/
- [ ] T029 [P] [US1] Add logging calls to RegistryAccessor for all registry read/write operations in src/WindowsSearchConfigurator/Infrastructure/RegistryAccessor.cs
- [ ] T030 [P] [US1] Add logging calls to WindowsSearchInterop for all COM API interactions in src/WindowsSearchConfigurator/Infrastructure/WindowsSearchInterop.cs
- [ ] T031 [P] [US1] Add logging calls to ServiceStatusChecker in src/WindowsSearchConfigurator/Infrastructure/ServiceStatusChecker.cs
- [ ] T032 [P] [US1] Add integration test for verbose flag with AddCommand in tests/WindowsSearchConfigurator.IntegrationTests/VerboseLoggingIntegrationTests.cs
- [ ] T033 [P] [US1] Add contract tests for log file naming pattern in tests/WindowsSearchConfigurator.ContractTests/LogFileFormatContractTests.cs
- [ ] T034 [P] [US1] Add contract tests for log file structure (header, entries, footer) in tests/WindowsSearchConfigurator.ContractTests/LogFileStructureContractTests.cs
- [ ] T035 [P] [US1] Add contract tests for timestamp format and validation in tests/WindowsSearchConfigurator.ContractTests/LogEntryFormatContractTests.cs

**Checkpoint**: At this point, User Story 1 should be fully functional and independently testable with comprehensive logging throughout the application

---

## Phase 6: User Story 2 - View Diagnostic History (Priority: P2)

**Goal**: Enable users to locate and review diagnostic log files from previous tool executions for troubleshooting recurring issues

**Independent Test**: Run the tool multiple times with verbose logging, then verify that multiple log files exist in the executable directory with unique names and can be opened with any text editor

### Implementation for User Story 2

- [ ] T036 [P] [US2] Add integration test for multiple verbose sessions creating unique log files in tests/WindowsSearchConfigurator.IntegrationTests/MultipleSessionsIntegrationTests.cs
- [ ] T037 [P] [US2] Add integration test verifying log files persist after tool completion in tests/WindowsSearchConfigurator.IntegrationTests/LogPersistenceIntegrationTests.cs
- [ ] T038 [US2] Add contract test for log file uniqueness across concurrent executions in tests/WindowsSearchConfigurator.ContractTests/LogFileUniquenessContractTests.cs
- [ ] T039 [US2] Update README.md with section on locating and interpreting log files in executable directory
- [ ] T040 [US2] Create example log file documentation showing typical troubleshooting scenarios in docs/LogFileExamples.md

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently - users can create logs and review them later

---

## Phase 7: User Story 3 - Control Logging Detail Level (Priority: P3)

**Goal**: Allow users to control when verbose logging occurs, running without the flag for normal operation and enabling verbose mode only when detailed diagnostics are needed

**Independent Test**: Run the same command with and without the `--verbose` flag and verify that no log file is created without the flag, and minimal overhead occurs

### Implementation for User Story 3

- [ ] T041 [P] [US3] Add integration test verifying no log file created without verbose flag in tests/WindowsSearchConfigurator.IntegrationTests/NoVerboseModeIntegrationTests.cs
- [ ] T042 [P] [US3] Add performance test measuring overhead of verbose logging (<100ms requirement) in tests/WindowsSearchConfigurator.IntegrationTests/VerboseLoggingPerformanceTests.cs
- [ ] T043 [US3] Update VerboseLogger to ensure minimal overhead when verbose mode is disabled
- [ ] T044 [US3] Add conditional logging checks in services to prevent expensive string operations when verbose is disabled
- [ ] T045 [US3] Update README.md with guidance on when to use verbose mode vs normal mode
- [ ] T046 [US3] Add examples to documentation showing performance comparison between verbose and normal modes

**Checkpoint**: All three user stories should now be independently functional with complete control over logging behavior

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Final improvements, edge case handling, and documentation updates

- [ ] T047 [P] Add error handling test for read-only executable directory in tests/WindowsSearchConfigurator.UnitTests/Utilities/FileLoggerErrorHandlingTests.cs
- [ ] T048 [P] Add error handling test for disk space exhaustion scenario in tests/WindowsSearchConfigurator.UnitTests/Utilities/FileLoggerDiskFullTests.cs
- [ ] T049 [P] Add error handling test for log file locked by another process in tests/WindowsSearchConfigurator.UnitTests/Utilities/FileLoggerLockedFileTests.cs
- [ ] T050 Add graceful degradation logic for all identified error scenarios in src/WindowsSearchConfigurator/Utilities/FileLogger.cs
- [ ] T051 [P] Update CHANGELOG.md with feature details and usage instructions
- [ ] T052 [P] Update README.md with complete verbose logging documentation and examples
- [ ] T053 Verify all requirements from spec.md are implemented (FR-001 through FR-015)
- [ ] T054 Run quickstart.md validation scenarios to ensure all examples work correctly
- [ ] T055 Perform code review focusing on error handling, thread safety, and performance
- [ ] T056 Final integration test run covering all user stories and edge cases

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately ✅
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Story 1 (Phases 3-5)**: Depends on Foundational phase completion
- **User Story 2 (Phase 6)**: Depends on User Story 1 completion (US1 provides the logs to review)
- **User Story 3 (Phase 7)**: Depends on User Story 1 completion (US1 provides the logging to control)
- **Polish (Phase 8)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories ✅
- **User Story 2 (P2)**: Requires User Story 1 complete (need logs to review) 🔗
- **User Story 3 (P3)**: Requires User Story 1 complete (need logging to control) 🔗

### Within Each User Story

**User Story 1** (split into 3 batches):
- Batch 1 (Phase 3): Models/Infrastructure first - can run in parallel
- Batch 2 (Phase 4): Integration with Program.cs - depends on Batch 1
- Batch 3 (Phase 5): Comprehensive logging + tests - depends on Batch 2

**User Story 2**: All tasks can run in parallel (documentation and tests)

**User Story 3**: Tests first, then implementation, then documentation

### Parallel Opportunities

- **Phase 2 (Foundational)**: Tasks T004-T013 marked [P] can run in parallel (10 tasks)
- **Phase 3 (US1 Batch 1)**: Tasks T014-T015, T018-T021 marked [P] can run in parallel (6 tasks)
- **Phase 4 (US1 Batch 2)**: Tasks T025-T027 marked [P] can run in parallel (3 tasks)
- **Phase 5 (US1 Batch 3)**: Tasks T028-T035 marked [P] can run in parallel (8 tasks)
- **Phase 6 (US2)**: Tasks T036-T037, T039-T040 marked [P] can run in parallel (4 tasks)
- **Phase 7 (US3)**: Tasks T041-T042, T045-T046 marked [P] can run in parallel (4 tasks)
- **Phase 8 (Polish)**: Tasks T047-T049, T051-T052 marked [P] can run in parallel (5 tasks)

---

## Parallel Example: Foundational Phase

```bash
# Launch all model enums in parallel:
Task: "Create LogSeverity enum in src/WindowsSearchConfigurator/Core/Models/LogSeverity.cs"
Task: "Create SessionStatus enum in src/WindowsSearchConfigurator/Core/Models/SessionStatus.cs"

# Launch all interfaces in parallel:
Task: "Create IFileLogger interface in src/WindowsSearchConfigurator/Core/Interfaces/IFileLogger.cs"
Task: "Create ILogFileNameGenerator interface in src/WindowsSearchConfigurator/Core/Interfaces/ILogFileNameGenerator.cs"

# Launch all unit tests in parallel after models are complete:
Task: "Add unit tests for LogEntry in tests/WindowsSearchConfigurator.UnitTests/Core/Models/LogEntryTests.cs"
Task: "Add unit tests for LogSession in tests/WindowsSearchConfigurator.UnitTests/Core/Models/LogSessionTests.cs"
Task: "Add unit tests for LogSeverity enum in tests/WindowsSearchConfigurator.UnitTests/Core/Models/LogSeverityTests.cs"
Task: "Add unit tests for SessionStatus enum in tests/WindowsSearchConfigurator.UnitTests/Core/Models/SessionStatusTests.cs"
```

## Parallel Example: User Story 1 - Phase 3

```bash
# Launch infrastructure implementations in parallel:
Task: "Implement LogFileNameGenerator class in src/WindowsSearchConfigurator/Utilities/LogFileNameGenerator.cs"
Task: "Implement FileLogger class with Initialize, WriteEntryAsync, WriteSessionHeaderAsync, WriteSessionFooterAsync methods in src/WindowsSearchConfigurator/Utilities/FileLogger.cs"

# Launch all unit tests in parallel after implementations complete:
Task: "Add unit tests for LogFileNameGenerator in tests/WindowsSearchConfigurator.UnitTests/Utilities/LogFileNameGeneratorTests.cs"
Task: "Add unit tests for FileLogger initialization and error handling in tests/WindowsSearchConfigurator.UnitTests/Utilities/FileLoggerTests.cs"
Task: "Add unit tests for FileLogger write operations in tests/WindowsSearchConfigurator.UnitTests/Utilities/FileLoggerWriteTests.cs"
Task: "Add unit tests for enhanced VerboseLogger dual-output in tests/WindowsSearchConfigurator.UnitTests/Utilities/VerboseLoggerTests.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup ✅ (review only)
2. Complete Phase 2: Foundational (CRITICAL - models and interfaces)
3. Complete Phase 3: User Story 1 - Infrastructure (file logging core)
4. Complete Phase 4: User Story 1 - Integration (wire to Program.cs)
5. Complete Phase 5: User Story 1 - Completion (comprehensive logging + tests)
6. **STOP and VALIDATE**: Test User Story 1 independently
7. Deploy/demo if ready - **MVP COMPLETE** 🎯

### Incremental Delivery

1. Complete Setup + Foundational (Phases 1-2) → Foundation ready
2. Add User Story 1 (Phases 3-5) → Test independently → Deploy/Demo (MVP!)
3. Add User Story 2 (Phase 6) → Test independently → Deploy/Demo (Log history review)
4. Add User Story 3 (Phase 7) → Test independently → Deploy/Demo (Logging control)
5. Polish (Phase 8) → Final release
6. Each story adds value without breaking previous stories

### Batch Strategy (Avoid Context Window Overflow)

**User Story 1 is split into 3 batches**:
- **Batch 1 (Phase 3)**: 8 files - Core infrastructure (models, interfaces, FileLogger, LogFileNameGenerator, tests)
- **Batch 2 (Phase 4)**: 6 files - Integration (Program.cs, 3 commands)
- **Batch 3 (Phase 5)**: 9 files - Completion (remaining commands, services, integration/contract tests)

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together (Phase 1-2)
2. Once Foundational is done:
   - Developer A: User Story 1 (Phases 3-5) - Core feature
   - Wait for Developer A to complete Phase 5
3. After US1 complete:
   - Developer B: User Story 2 (Phase 6) - Documentation/testing
   - Developer C: User Story 3 (Phase 7) - Performance/control
   - Developer A: Polish (Phase 8) - Edge cases
4. Stories complete and integrate independently

---

## Task Summary

- **Total Tasks**: 56
- **Setup Phase**: 3 tasks (review only)
- **Foundational Phase**: 10 tasks (models, interfaces, unit tests)
- **User Story 1**: 22 tasks (infrastructure, integration, comprehensive logging, tests)
- **User Story 2**: 5 tasks (multi-session support, persistence, documentation)
- **User Story 3**: 6 tasks (control, performance, documentation)
- **Polish Phase**: 10 tasks (error handling, final testing, documentation)

### Tasks per Priority

- **P1 (MVP)**: 22 tasks (User Story 1 - Phases 3-5)
- **P2**: 5 tasks (User Story 2 - Phase 6)
- **P3**: 6 tasks (User Story 3 - Phase 7)
- **Foundation**: 10 tasks (Phase 2 - BLOCKS all stories)
- **Polish**: 10 tasks (Phase 8 - Final improvements)

### Parallel Opportunities

- **36 tasks marked [P]** can run in parallel with other [P] tasks in same phase
- **20 sequential tasks** require completion of dependencies first
- Each phase has clear checkpoints for validation

### Suggested MVP Scope

**Complete Phases 1-5 only** (User Story 1):
- 35 tasks total (Setup + Foundational + User Story 1)
- Delivers core verbose logging functionality
- Independently testable and deployable
- Approximately 60% of total work
- All acceptance criteria for User Story 1 met

---

## Format Validation ✅

All tasks follow the required checklist format:
- ✅ Checkbox: `- [ ]` at start
- ✅ Task ID: Sequential (T001-T056)
- ✅ [P] marker: Only on parallelizable tasks
- ✅ [Story] label: Only on user story phases (US1, US2, US3)
- ✅ Description: Clear action with exact file path
- ✅ Setup/Foundational: No story labels
- ✅ User Story phases: All have story labels
- ✅ Polish phase: No story labels

---

## Notes

- [P] tasks = different files, no dependencies within the batch
- [Story] label maps task to specific user story for traceability (US1, US2, US3)
- Each user story is independently completable and testable
- Stop at any checkpoint to validate story independently
- User Story 1 is split into 3 batches (4-6 files each) to avoid AI context window overflow
- User Story 2 depends on User Story 1 (need logs to review)
- User Story 3 depends on User Story 1 (need logging to control)
- All file paths are absolute within the WindowsSearchConfigurator project structure
- Constitution principles validated: All gates pass ✅
