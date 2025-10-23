# Implementation Tasks: COM API Registration Support

**Feature**: COM API Registration Support  
**Branch**: `002-com-api-registration`  
**Date**: 2025-10-22  
**Plan**: [plan.md](./plan.md) | **Spec**: [spec.md](./spec.md)

## Overview

This document provides a complete task list for implementing COM API registration support in WindowsSearchConfigurator. Tasks are organized by user story priority to enable independent implementation and testing. Each phase represents a deliverable increment that can be tested independently.

## Task Organization

- **Phase 1**: Setup - Project preparation
- **Phase 2**: Foundational - Core infrastructure needed by all stories
- **Phase 3**: User Story 1 (P1) - Detection and Notification
- **Phase 4**: User Story 2 & 3 (P2) - Interactive Registration with Privilege Awareness
- **Phase 5**: User Story 4 (P3) - Non-Interactive Mode Support
- **Phase 6**: Polish & Integration Testing

Each task follows the format: `- [ ] [TaskID] [P] [Story] Description with file path`
- `[P]` = Parallelizable (can be done simultaneously with other [P] tasks)
- `[Story]` = User story label (US1, US2, US3, US4)

## Implementation Strategy

**MVP Scope**: User Story 1 (P1) - Detection and Notification only
- Provides immediate value by detecting and reporting COM API issues
- Can be released independently for diagnostic purposes
- Foundation for all subsequent stories

**Incremental Delivery**:
1. Phase 3 (US1): Detection - Delivers diagnostic capability
2. Phase 4 (US2-3): Interactive registration - Enables manual remediation
3. Phase 5 (US4): Automation flags - Enables CI/CD scenarios

## Dependencies Graph

```
Phase 1 (Setup)
    ↓
Phase 2 (Foundational)
    ↓
Phase 3 (US1: Detection) ← MVP/First Deliverable
    ↓
Phase 4 (US2-3: Interactive Registration) ← Can run in parallel with Phase 5
    ↓
Phase 5 (US4: Non-Interactive Flags)
    ↓
Phase 6 (Polish & Integration Testing)
```

**User Story Dependencies**:
- US1 (Detection) → No dependencies (can implement first)
- US2 (Interactive Offer) → Requires US1 (detection must work first)
- US3 (Privilege Checking) → Requires US2 (part of registration flow)
- US4 (Non-Interactive) → Requires US2+US3 (uses same registration logic)

## Phase 1: Setup

**Goal**: Prepare project infrastructure and validate environment

**Estimated Effort**: 30 minutes

### Tasks

- [x] T001 Verify project builds successfully with `dotnet build`
- [x] T002 Verify all existing tests pass with `dotnet test`
- [x] T003 Review existing PrivilegeChecker.cs in src/WindowsSearchConfigurator/Services/
- [x] T004 Review existing AuditLogger.cs in src/WindowsSearchConfigurator/Services/
- [x] T005 Review existing ConsoleFormatter.cs in src/WindowsSearchConfigurator/Utilities/
- [x] T006 Review existing WindowsSearchInterop.cs in src/WindowsSearchConfigurator/Infrastructure/
- [x] T007 Create feature branch checkpoint: `git commit -m "chore(com): Prepare for COM API registration implementation"`

## Phase 2: Foundational (Blocking Prerequisites)

**Goal**: Create core domain models and enums needed by all user stories

**Estimated Effort**: 1 hour

**Independent Test**: Domain models compile and can be instantiated in unit tests

### Tasks

- [x] T008 [P] Create COMValidationState enum in src/WindowsSearchConfigurator/Core/Models/COMValidationState.cs
- [x] T009 [P] Create RegistrationMode enum in src/WindowsSearchConfigurator/Core/Models/RegistrationMode.cs
- [x] T010 [P] Create RegistrationOutcome enum in src/WindowsSearchConfigurator/Core/Models/RegistrationOutcome.cs
- [x] T011 Create COMRegistrationStatus model in src/WindowsSearchConfigurator/Core/Models/COMRegistrationStatus.cs
- [x] T012 Create COMRegistrationAttempt model in src/WindowsSearchConfigurator/Core/Models/COMRegistrationAttempt.cs
- [x] T013 Create RegistrationOptions model in src/WindowsSearchConfigurator/Core/Models/RegistrationOptions.cs
- [x] T014 Create ICOMRegistrationDetector interface in src/WindowsSearchConfigurator/Core/Interfaces/ICOMRegistrationDetector.cs
- [x] T015 Create ICOMRegistrationService interface in src/WindowsSearchConfigurator/Core/Interfaces/ICOMRegistrationService.cs
- [x] T016 Commit foundational models: `git commit -m "feat(com): Add COM registration domain models and interfaces"`

## Phase 3: User Story 1 - Detection and Notification (P1)

**User Story**: When a system administrator runs WindowsSearchConfigurator on a machine where the COM API is not registered, they receive a clear notification explaining the issue.

**Goal**: Implement COM detection and clear error messaging

**Estimated Effort**: 3-4 hours

**Independent Test**: Run tool on system with COM API unregistered → displays clear, non-technical error message (FR-001, FR-002, SC-001, SC-004)

**Files in This Phase**: 4 files (within batch limit)

### Tasks

- [x] T017 [US1] Create COMRegistrationDetector service in src/WindowsSearchConfigurator/Services/COMRegistrationDetector.cs with CLSID detection logic
- [x] T018 [US1] Create unit tests for COMRegistrationDetector in tests/WindowsSearchConfigurator.UnitTests/Services/COMRegistrationDetectorTests.cs
- [x] T019 [US1] Add COM validation method to WindowsSearchInterop in src/WindowsSearchConfigurator/Infrastructure/WindowsSearchInterop.cs
- [x] T020 [US1] Add COM validation tests to tests/WindowsSearchConfigurator.UnitTests/Infrastructure/WindowsSearchInteropTests.cs
- [x] T021 [US1] Add detection notification message to ConsoleFormatter in src/WindowsSearchConfigurator/Utilities/ConsoleFormatter.cs
- [x] T022 [US1] Integrate COM detection into Program.cs startup in src/WindowsSearchConfigurator/Program.cs (detection only, no registration yet)
- [x] T023 [US1] Run unit tests for detection: `dotnet test --filter "FullyQualifiedName~COMRegistrationDetector"`
- [x] T024 [US1] Manual test: Run tool on system with COM unregistered, verify clear error message
- [x] T025 [US1] Manual test: Run tool on system with COM registered, verify no error message
- [x] T026 [US1] Commit US1 implementation: `git commit -m "feat(com): Implement COM detection and notification (US1)"`

**Acceptance Criteria**:
- ✅ Tool detects COM API registration status before any command execution
- ✅ Clear, non-technical error message displayed when COM not registered
- ✅ No error message when COM is registered
- ✅ Detection completes in <500ms
- ✅ All unit tests pass

## Phase 4: User Story 2 & 3 - Interactive Registration (P2)

**User Story 2**: Administrator is offered an option to attempt automatic registration with clear guidance.

**User Story 3**: System checks for administrative privileges and provides clear guidance.

**Goal**: Enable interactive registration workflow with privilege checking

**Estimated Effort**: 4-5 hours

**Independent Test**: 
- US2: Run tool without admin → offered registration → decline → see manual instructions
- US3: Run tool without admin → accept offer → see elevation guidance

**Files in This Phase**: 6 files (split into two sub-phases to stay within batch limit)

### Phase 4a: Registration Service Core (4 files)

- [x] T027 [US2] Create COMRegistrationService in src/WindowsSearchConfigurator/Services/COMRegistrationService.cs with registration logic
- [x] T028 [US2] Create unit tests for COMRegistrationService in tests/WindowsSearchConfigurator.UnitTests/Services/COMRegistrationServiceTests.cs
- [x] T029 [US2] Add interactive prompt to ConsoleFormatter in src/WindowsSearchConfigurator/Utilities/ConsoleFormatter.cs
- [x] T030 [US2] Add manual instructions display to ConsoleFormatter in src/WindowsSearchConfigurator/Utilities/ConsoleFormatter.cs
- [x] T031 [US2] Run unit tests for registration service: `dotnet test --filter "FullyQualifiedName~COMRegistrationService"`
- [x] T032 [US2] Commit US2 core: `git commit -m "feat(com): Add registration service and interactive prompts (US2)"`

### Phase 4b: Privilege Checking Integration (2 files)

- [ ] T033 [US3] Enhance PrivilegeChecker with COM-specific context in src/WindowsSearchConfigurator/Services/PrivilegeChecker.cs
- [ ] T034 [US3] Add privilege check tests in tests/WindowsSearchConfigurator.UnitTests/Services/PrivilegeCheckerTests.cs
- [ ] T035 [US3] Integrate registration workflow into Program.cs in src/WindowsSearchConfigurator/Program.cs
- [ ] T036 [US3] Add elevation instructions to ConsoleFormatter in src/WindowsSearchConfigurator/Utilities/ConsoleFormatter.cs
- [ ] T037 [US3] Run all unit tests: `dotnet test --filter "FullyQualifiedName~UnitTests"`
- [ ] T038 [US2][US3] Manual test (as admin): Accept registration offer → verify registration succeeds
- [ ] T039 [US2][US3] Manual test (as admin): Decline registration offer → verify manual instructions shown
- [ ] T040 [US3] Manual test (as standard user): Accept offer → verify elevation instructions shown
- [ ] T041 [US2][US3] Commit US2+US3 implementation: `git commit -m "feat(com): Complete interactive registration with privilege checking (US2, US3)"`

**Acceptance Criteria US2**:
- ✅ User is offered registration when COM not registered
- ✅ User can accept, decline, or quit
- ✅ Declining shows manual instructions
- ✅ Registration attempt uses regsvr32.exe
- ✅ Registration outcome is validated and reported

**Acceptance Criteria US3**:
- ✅ Privilege check performed before registration
- ✅ Standard users see elevation instructions
- ✅ Admin users can complete registration
- ✅ Registration failures show troubleshooting steps
- ✅ Registration completes in <5 seconds when successful

## Phase 5: User Story 4 - Non-Interactive Mode Support (P3)

**User Story**: Tool can handle COM registration without user interaction via command-line flags for CI/CD scenarios.

**Goal**: Add --auto-register-com and --no-register-com flags

**Estimated Effort**: 2-3 hours

**Independent Test**: 
- Run with `--auto-register-com` → automatic registration without prompt
- Run with `--no-register-com` → immediate exit if COM not registered

**Files in This Phase**: 3 files (within batch limit)

### Tasks

- [ ] T042 [US4] Add global options to Program.cs in src/WindowsSearchConfigurator/Program.cs (--auto-register-com, --no-register-com)
- [ ] T043 [US4] Update COMRegistrationService to handle flags in src/WindowsSearchConfigurator/Services/COMRegistrationService.cs
- [ ] T044 [US4] Create contract tests for CLI flags in tests/WindowsSearchConfigurator.ContractTests/COMRegistrationContractTests.cs
- [ ] T045 [US4] Run contract tests: `dotnet test --filter "FullyQualifiedName~ContractTests"`
- [ ] T046 [US4] Manual test: Run with `--auto-register-com` (as admin) → verify automatic registration
- [ ] T047 [US4] Manual test: Run with `--no-register-com` (COM not registered) → verify immediate exit with error
- [ ] T048 [US4] Manual test: Run with both flags → verify conflict error (exit code 3)
- [ ] T049 [US4] Manual test: Run with `--auto-register-com` (as standard user) → verify elevation error (exit code 2)
- [ ] T050 [US4] Commit US4 implementation: `git commit -m "feat(com): Add non-interactive mode flags (US4)"`

**Acceptance Criteria**:
- ✅ --auto-register-com flag recognized and processed
- ✅ --no-register-com flag recognized and processed
- ✅ Flags are mutually exclusive (error if both specified)
- ✅ Automatic mode works without prompts
- ✅ No-register mode exits immediately
- ✅ Correct exit codes returned (0, 1, 2, 3)

## Phase 6: Polish & Integration Testing

**Goal**: Integration tests, documentation, and final validation

**Estimated Effort**: 2-3 hours

**Independent Test**: Full end-to-end tests with all scenarios

**Files in This Phase**: 4 files (within batch limit)

### Tasks

- [ ] T051 Create integration tests in tests/WindowsSearchConfigurator.IntegrationTests/COMRegistrationIntegrationTests.cs
- [ ] T052 Add COM troubleshooting section to README.md
- [ ] T053 Update CHANGELOG.md with COM registration feature
- [ ] T054 Add AuditLogger integration for all COM events in src/WindowsSearchConfigurator/Services/COMRegistrationService.cs
- [ ] T055 Run full test suite: `dotnet test`
- [ ] T056 Run integration tests (as admin): `dotnet test --filter "FullyQualifiedName~IntegrationTests"`
- [ ] T057 Verify all FR requirements implemented (FR-001 through FR-012)
- [ ] T058 Verify all SC criteria met (SC-001 through SC-006)
- [ ] T059 Manual test: Complete user story acceptance scenarios (all 4 stories)
- [ ] T060 Manual test: Edge cases (DLL missing, partial registration, timeout)
- [ ] T061 Code review: Verify constitution compliance (automated testing, Windows API safety, user control)
- [ ] T062 Final commit: `git commit -m "feat(com): Complete COM API registration support with tests and documentation"`
- [ ] T063 Create pull request: `gh pr create --title "feat(com): Add COM API registration support" --body "Implements COM API detection and registration (US1-US4)"`

**Acceptance Criteria**:
- ✅ All unit tests pass (100+ tests)
- ✅ All integration tests pass (requires admin)
- ✅ All contract tests pass
- ✅ Documentation updated
- ✅ All functional requirements implemented
- ✅ All success criteria met
- ✅ Constitution gates remain green

## Parallel Execution Opportunities

Tasks marked with `[P]` can be executed in parallel within their phase:

**Phase 2 (Foundational)**:
- T008, T009, T010 (enums) - All independent, can run simultaneously

**Phase 3 (US1)**:
- No parallel opportunities (tasks are sequential due to dependencies)

**Phase 4a (US2)**:
- No parallel opportunities (service and tests are interdependent)

**Phase 4b (US3)**:
- No parallel opportunities (privilege checking integrates with existing service)

**Phase 5 (US4)**:
- No parallel opportunities (flags integrate with existing service and Program.cs)

**Phase 6 (Polish)**:
- T051, T052, T053 (tests and docs) - Can run simultaneously

## Testing Strategy

### Unit Tests (No Admin Required)
- **Location**: `tests/WindowsSearchConfigurator.UnitTests/`
- **Scope**: 
  - COMRegistrationDetector: Registry detection logic
  - COMRegistrationService: Registration orchestration
  - PrivilegeChecker: Admin detection
- **Approach**: Mock registry access, process execution, and Windows APIs
- **Run**: `dotnet test --filter "FullyQualifiedName~UnitTests"`

### Integration Tests (Admin Required)
- **Location**: `tests/WindowsSearchConfigurator.IntegrationTests/`
- **Scope**: 
  - End-to-end registration flow
  - Actual regsvr32.exe execution
  - Real registry access and COM instantiation
- **Approach**: Unregister COM in setup, register in teardown
- **Run**: `dotnet test --filter "FullyQualifiedName~IntegrationTests"` (as Administrator)

### Contract Tests (No Admin Required)
- **Location**: `tests/WindowsSearchConfigurator.ContractTests/`
- **Scope**:
  - CLI flag parsing (--auto-register-com, --no-register-com)
  - Exit codes (0, 1, 2, 3)
  - Help text content
- **Approach**: Parse command-line args, verify behavior
- **Run**: `dotnet test --filter "FullyQualifiedName~ContractTests"`

### Manual Testing Checklist

**Interactive Mode**:
- [ ] Run as admin when COM not registered → Should offer registration (Y/N/Q)
- [ ] Select "Yes" → Should register successfully and continue
- [ ] Select "No" → Should show manual instructions and exit
- [ ] Select "Quit" → Should exit cleanly
- [ ] Run as standard user when COM not registered → Should show elevation instructions

**Automatic Mode (--auto-register-com)**:
- [ ] Run as admin when COM not registered → Should auto-register without prompt
- [ ] Run as standard user when COM not registered → Should show elevation error (exit code 2)
- [ ] Run when COM already registered → Should proceed silently

**No Register Mode (--no-register-com)**:
- [ ] Run when COM not registered → Should exit immediately with error (exit code 1)
- [ ] Run when COM registered → Should proceed normally

**Edge Cases**:
- [ ] SearchAPI.dll missing → Should show "DLL not found" error
- [ ] CLSID registered but DLL corrupt → Should detect and report
- [ ] Registration succeeds but validation fails → Should show warning
- [ ] regsvr32.exe times out → Should show timeout error
- [ ] Both flags specified → Should show conflict error (exit code 3)

## Task Summary

**Total Tasks**: 63

**By Phase**:
- Phase 1 (Setup): 7 tasks
- Phase 2 (Foundational): 9 tasks  
- Phase 3 (US1): 10 tasks
- Phase 4 (US2+US3): 15 tasks
- Phase 5 (US4): 9 tasks
- Phase 6 (Polish): 13 tasks

**By Type**:
- Implementation: 28 tasks
- Testing (unit/integration/contract): 18 tasks
- Manual testing: 12 tasks
- Commits/Reviews: 5 tasks

**Parallelizable Tasks**: 3 (T008, T009, T010 in Phase 2)

## Success Metrics

This feature is complete when:

1. **All Functional Requirements Met**:
   - FR-001: COM detection ✓
   - FR-002: Clear error messages ✓
   - FR-003: Registration offer ✓
   - FR-004: Privilege checking ✓
   - FR-005: Elevation instructions ✓
   - FR-006: Registration execution ✓
   - FR-007: Outcome reporting ✓
   - FR-008: Fallback guidance ✓
   - FR-009: Auto-register flag ✓
   - FR-010: No-register flag ✓
   - FR-011: Post-registration validation ✓
   - FR-012: Audit logging ✓

2. **All Success Criteria Met**:
   - SC-001: 100% clear notifications ✓
   - SC-002: <1 minute registration process ✓
   - SC-003: 90%+ success rate ✓
   - SC-004: Zero cryptic errors ✓
   - SC-005: Zero manual intervention in CI/CD ✓
   - SC-006: 100% logged attempts ✓

3. **All Tests Pass**:
   - Unit tests: 100% pass rate
   - Integration tests: 100% pass rate (requires admin)
   - Contract tests: 100% pass rate
   - Manual test checklist: All items verified

4. **Constitution Compliance**:
   - Automated testing: ✓ (unit, integration, contract)
   - Windows API safety: ✓ (error handling, validation)
   - User control: ✓ (explicit approval required)
   - Clear interface: ✓ (non-technical messages)
   - Documentation: ✓ (quickstart, README, CHANGELOG)
   - Incremental implementation: ✓ (4-6 file batches)
   - Source control: ✓ (frequent commits, conventional format)

## Next Steps

1. **Begin Implementation**:
   ```powershell
   # Start with Phase 1 (Setup)
   # Follow tasks in order T001 → T063
   ```

2. **Use /speckit.implement**:
   - Executes tasks automatically
   - Respects batch limits (4-6 files)
   - Runs tests after each batch
   - Creates commits with conventional format

3. **MVP Delivery** (Optional):
   - Complete through Phase 3 (US1)
   - Deploy detection-only version
   - Gather user feedback
   - Continue with Phase 4+ based on feedback

4. **Full Feature Delivery**:
   - Complete all phases (1-6)
   - Pass all tests and validations
   - Create pull request
   - Request code review

## Notes

- **Batch Size Limit**: Each implementation phase respects the 4-6 file maximum to prevent AI context overflow
- **Test-First Approach**: Unit tests created alongside or before implementation for critical paths
- **Incremental Commits**: Commit after each phase completion with conventional commit messages
- **Independent Testing**: Each user story can be tested independently of others
- **MVP Option**: Phase 3 (US1) delivers standalone diagnostic value and can be released independently
