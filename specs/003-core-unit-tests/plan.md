# Implementation Plan: Core Model Unit Tests

**Branch**: `003-core-unit-tests` | **Date**: 2025-10-23 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/003-core-unit-tests/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Create comprehensive unit tests for all Core model classes in the WindowsSearchConfigurator project. Tests will verify data validation, state transitions, factory methods, property initialization, collection behavior, and edge cases across all models including ValidationResult, OperationResult, IndexRule, ConfigurationFile, FileExtensionSetting, COMRegistrationAttempt, and associated enums. Target minimum 80% code coverage with independent, fast-running tests using NUnit framework.

## Technical Context

**Language/Version**: C# / .NET 8.0 (LTS)  
**Primary Dependencies**: NUnit 4.x (test framework), NUnit3TestAdapter (test runner), Microsoft.NET.Test.Sdk  
**Storage**: N/A (unit tests operate on in-memory model instances)  
**Testing**: NUnit (standardized across all C# projects per constitution)  
**Target Platform**: Windows 10/11 and Windows Server 2016+  
**Project Type**: Single project with separate test assemblies (WindowsSearchConfigurator.UnitTests)  
**Performance Goals**: Test suite executes in under 5 seconds on standard development machine  
**Constraints**: Tests must run independently without external dependencies (database, file system, registry, network); isolated to Core.Models namespace only  
**Scale/Scope**: Approximately 19 model classes/enums to test with minimum 80% code coverage target

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### ✅ I. Automated Testing (NON-NEGOTIABLE)
**Status**: PASS  
**Rationale**: This feature IS the implementation of automated unit tests for Core models. All test code will be accompanied by validation that tests actually execute and pass.

### ✅ II. Windows API Safety
**Status**: PASS (N/A)  
**Rationale**: Unit tests for Core models do not interact with Windows APIs. Models are pure data structures.

### ✅ III. User Configuration Control
**Status**: PASS (N/A)  
**Rationale**: Unit tests do not modify user configuration. They test model behavior in isolation.

### ✅ IV. Clear Interface Design
**Status**: PASS  
**Rationale**: Test names will follow clear naming conventions (e.g., `ValidationResult_Success_ShouldSetIsValidTrue`) making test intent obvious. Tests serve as documentation of model behavior.

### ✅ V. Documentation and Maintainability
**Status**: PASS  
**Rationale**: Tests themselves document expected model behavior. Test classes will include XML comments explaining what aspect of models is being tested.

### ✅ VI. Incremental Implementation (NON-NEGOTIABLE)
**Status**: PASS  
**Rationale**: Test implementation will be organized by model class with approximately 3-5 test files per implementation batch:
- Batch 1: ValidationResult, OperationResult tests (2 test files)
- Batch 2: IndexRule, ConfigurationFile tests (2 test files)
- Batch 3: FileExtensionSetting, COMRegistrationAttempt tests (2 test files)
- Batch 4: Enum and remaining model tests (2-3 test files)

### ✅ VII. Source Control Discipline (NON-NEGOTIABLE)
**Status**: PASS  
**Rationale**: Work will be committed on feature branch `003-core-unit-tests`. Conventional commits will be used (e.g., `test(core): add ValidationResult unit tests`). Each test batch will be committed as logical units. GitHub is the source control platform.

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
    ├── Core/
    │   ├── Models/                          # Classes being tested
    │   │   ├── ValidationResult.cs
    │   │   ├── OperationResult.cs
    │   │   ├── IndexRule.cs
    │   │   ├── ConfigurationFile.cs
    │   │   ├── FileExtensionSetting.cs
    │   │   ├── COMRegistrationAttempt.cs
    │   │   ├── FileTypeFilter.cs
    │   │   ├── IndexLocation.cs
    │   │   ├── COMRegistrationStatus.cs
    │   │   ├── RegistrationOptions.cs
    │   │   └── [Enums: RuleType, RuleSource, IndexingDepth, etc.]
    │   └── Interfaces/

tests/
├── WindowsSearchConfigurator.UnitTests/     # MAIN TEST LOCATION
│   ├── Core/
│   │   └── Models/                          # New test files go here
│   │       ├── ValidationResultTests.cs     # [TO CREATE]
│   │       ├── OperationResultTests.cs      # [TO CREATE]
│   │       ├── IndexRuleTests.cs            # [TO CREATE]
│   │       ├── ConfigurationFileTests.cs    # [TO CREATE]
│   │       ├── FileExtensionSettingTests.cs # [TO CREATE]
│   │       ├── COMRegistrationAttemptTests.cs # [TO CREATE]
│   │       ├── FileTypeFilterTests.cs       # [TO CREATE]
│   │       ├── EnumTests.cs                 # [TO CREATE]
│   │       └── [Additional model tests as needed]
│   └── WindowsSearchConfigurator.UnitTests.csproj
├── WindowsSearchConfigurator.IntegrationTests/
└── WindowsSearchConfigurator.ContractTests/
```

**Structure Decision**: Using existing single-project structure. All new unit tests will be added to `tests/WindowsSearchConfigurator.UnitTests/Core/Models/` directory to mirror the source structure. This maintains consistency with the existing test organization pattern where test directory structure mirrors source directory structure.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

No violations detected. All constitution principles are satisfied.

---

## Phase Completion Status

### ✅ Phase 0: Outline & Research (COMPLETE)
**Deliverable**: `research.md`

All technical unknowns have been resolved:
- ✅ NUnit best practices for data model testing
- ✅ Code coverage tooling for .NET 8 (Coverlet + ReportGenerator)
- ✅ Test naming conventions and organization patterns
- ✅ Testing patterns for factory methods and immutable objects
- ✅ .NET 8 testing framework updates and compatibility

**Outcome**: No "NEEDS CLARIFICATION" items remain. Ready for implementation.

---

### ✅ Phase 1: Design & Contracts (COMPLETE)
**Deliverables**: 
- ✅ `data-model.md` - Documents all 19 models being tested with properties, constructors, factory methods, and test scenarios
- ✅ `contracts/test-contract.md` - Defines testing contracts, assertion patterns, coverage requirements, and success criteria
- ✅ `quickstart.md` - Provides developer guide with commands, examples, debugging, and troubleshooting
- ✅ Agent context updated - GitHub Copilot context file updated with NUnit 4.x, .NET 8, and testing framework information

**Re-evaluation of Constitution Check**: All principles remain satisfied post-design.

---

### 🔜 Phase 2: Planning (NEXT)
**Command**: `/speckit.tasks` (NOT executed by `/speckit.plan`)

This will generate `tasks.md` with:
- Detailed implementation batches (4-6 files each)
- Task breakdown for each test file
- Acceptance criteria per task
- Commit strategy

---

## Implementation Readiness

✅ **Ready for Phase 2**: All planning prerequisites complete. Feature is well-defined with:
- Clear technical decisions (NUnit 4.x, Coverlet, standard patterns)
- Comprehensive model inventory (19 models documented)
- Explicit test contracts (factory methods, constructors, properties, edge cases)
- Developer-friendly quickstart guide
- 80% coverage target with measurement strategy
- Incremental implementation strategy (4 batches)

**Next Action**: Run `/speckit.tasks` to generate detailed implementation tasks.
