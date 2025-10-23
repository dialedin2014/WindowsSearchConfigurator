# Tasks: Core Model Unit Tests

**Input**: Design documents from `/specs/003-core-unit-tests/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/test-contract.md

**Tests**: This feature IS the test implementation. All tasks create test files.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Test Infrastructure)

**Purpose**: Initialize test project structure and ensure test framework is ready

- [X] T001 Verify NUnit dependencies in tests/WindowsSearchConfigurator.UnitTests/WindowsSearchConfigurator.UnitTests.csproj
- [X] T002 Create directory tests/WindowsSearchConfigurator.UnitTests/Core/Models/ if it doesn't exist
- [X] T003 Run `dotnet build tests/WindowsSearchConfigurator.UnitTests` to verify test project builds
- [X] T004 Run `dotnet test tests/WindowsSearchConfigurator.UnitTests` to verify test runner works

**Checkpoint**: Test infrastructure ready - test implementation can begin

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Setup code coverage tooling (required for SC-002, FR-015)

- [X] T005 Install coverlet.collector package in tests/WindowsSearchConfigurator.UnitTests/WindowsSearchConfigurator.UnitTests.csproj if not present
- [X] T006 Verify coverage collection works with `dotnet test --collect:"XPlat Code Coverage"` command
- [X] T007 Create .gitignore entry for TestResults/ and coverage-report/ directories

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Validate Core Model Behavior (Priority: P1) 🎯 MVP

**Goal**: Create comprehensive unit tests for ValidationResult, OperationResult, OperationResult<T> to ensure factory methods, property initialization, and immutability work correctly.

**Independent Test**: Run `dotnet test --filter "FullyQualifiedName~ValidationResultTests|OperationResultTests"` - all tests pass, coverage report shows 80%+ for these models.

### Implementation for User Story 1

- [X] T008 [P] [US1] Create ValidationResultTests.cs in tests/WindowsSearchConfigurator.UnitTests/Core/Models/ValidationResultTests.cs with Success factory method tests
- [X] T009 [P] [US1] Create OperationResultTests.cs in tests/WindowsSearchConfigurator.UnitTests/Core/Models/OperationResultTests.cs with Ok/Fail factory method tests
- [X] T010 [US1] Add Failure factory method tests to tests/WindowsSearchConfigurator.UnitTests/Core/Models/ValidationResultTests.cs
- [X] T011 [US1] Add Warning factory method tests to tests/WindowsSearchConfigurator.UnitTests/Core/Models/ValidationResultTests.cs
- [X] T012 [US1] Add generic OperationResult<T> tests to tests/WindowsSearchConfigurator.UnitTests/Core/Models/OperationResultTests.cs
- [X] T013 [US1] Add edge case tests (null/empty values) to tests/WindowsSearchConfigurator.UnitTests/Core/Models/ValidationResultTests.cs
- [X] T014 [US1] Add edge case tests (null exception, null error code) to tests/WindowsSearchConfigurator.UnitTests/Core/Models/OperationResultTests.cs
- [X] T015 [US1] Run tests with coverage: `dotnet test --filter "FullyQualifiedName~ValidationResultTests|OperationResultTests" --collect:"XPlat Code Coverage"`
- [X] T016 [US1] Verify coverage meets 80% minimum for ValidationResult and OperationResult classes

**Checkpoint**: At this point, all result object models should be thoroughly tested and coverage verified

---

## Phase 4: User Story 2 - Verify Model State Management (Priority: P2)

**Goal**: Create unit tests for IndexRule, ConfigurationFile, FileExtensionSetting to ensure state management, property initialization, timestamps, and collections work correctly.

**Independent Test**: Run `dotnet test --filter "FullyQualifiedName~IndexRuleTests|ConfigurationFileTests|FileExtensionSettingTests"` - all tests pass, models manage state correctly.

### Implementation for User Story 2

- [X] T017 [P] [US2] Create IndexRuleTests.cs in tests/WindowsSearchConfigurator.UnitTests/Core/Models/IndexRuleTests.cs with constructor tests
- [X] T018 [P] [US2] Create ConfigurationFileTests.cs in tests/WindowsSearchConfigurator.UnitTests/Core/Models/ConfigurationFileTests.cs with constructor and ExportDate tests
- [X] T019 [P] [US2] Create FileExtensionSettingTests.cs in tests/WindowsSearchConfigurator.UnitTests/Core/Models/FileExtensionSettingTests.cs with constructor tests
- [X] T020 [US2] Add property initialization tests to tests/WindowsSearchConfigurator.UnitTests/Core/Models/IndexRuleTests.cs (Id, CreatedDate, ModifiedDate)
- [X] T021 [US2] Add collection initialization tests to tests/WindowsSearchConfigurator.UnitTests/Core/Models/IndexRuleTests.cs (FileTypeFilters, ExcludedSubfolders)
- [X] T022 [US2] Add collection initialization tests to tests/WindowsSearchConfigurator.UnitTests/Core/Models/ConfigurationFileTests.cs (Rules, ExtensionSettings)
- [X] T023 [US2] Add ModifiedDate auto-set tests to tests/WindowsSearchConfigurator.UnitTests/Core/Models/FileExtensionSettingTests.cs
- [X] T024 [US2] Add Guid uniqueness tests to tests/WindowsSearchConfigurator.UnitTests/Core/Models/IndexRuleTests.cs
- [X] T025 [US2] Add DateTime UTC validation tests to tests/WindowsSearchConfigurator.UnitTests/Core/Models/ConfigurationFileTests.cs
- [X] T026 [US2] Run tests with coverage: `dotnet test --filter "FullyQualifiedName~IndexRuleTests|ConfigurationFileTests|FileExtensionSettingTests" --collect:"XPlat Code Coverage"`
- [X] T027 [US2] Verify coverage meets 80% minimum for IndexRule, ConfigurationFile, and FileExtensionSetting classes

**Checkpoint**: At this point, all configuration model state management should be thoroughly tested

---

## Phase 5: User Story 3 - Test Edge Cases and Boundary Conditions (Priority: P3)

**Goal**: Create unit tests for COMRegistrationAttempt, supporting models, and enums, plus edge case tests for all models to ensure robustness with null values, empty collections, and extreme values.

**Independent Test**: Run `dotnet test --filter "FullyQualifiedName~COMRegistrationAttemptTests|FileTypeFilterTests|EnumTests"` - all tests pass, edge cases handled gracefully.

### Implementation for User Story 3 - Batch 1 (Core Audit & Supporting Models)

- [X] T028 [P] [US3] Create COMRegistrationAttemptTests.cs in tests/WindowsSearchConfigurator.UnitTests/Core/Models/COMRegistrationAttemptTests.cs with property tests
- [X] T029 [P] [US3] Create FileTypeFilterTests.cs in tests/WindowsSearchConfigurator.UnitTests/Core/Models/FileTypeFilterTests.cs with property tests
- [X] T030 [P] [US3] Create IndexLocationTests.cs in tests/WindowsSearchConfigurator.UnitTests/Core/Models/IndexLocationTests.cs with property tests
- [X] T031 [US3] Add AttemptId uniqueness tests to tests/WindowsSearchConfigurator.UnitTests/Core/Models/COMRegistrationAttemptTests.cs
- [X] T032 [US3] Add optional property null handling tests to tests/WindowsSearchConfigurator.UnitTests/Core/Models/COMRegistrationAttemptTests.cs
- [X] T033 [US3] Add default RegistrationMethod value test to tests/WindowsSearchConfigurator.UnitTests/Core/Models/COMRegistrationAttemptTests.cs

### Implementation for User Story 3 - Batch 2 (Additional Supporting Models)

- [X] T034 [P] [US3] Create COMRegistrationStatusTests.cs in tests/WindowsSearchConfigurator.UnitTests/Core/Models/COMRegistrationStatusTests.cs with property tests
- [X] T035 [P] [US3] Create RegistrationOptionsTests.cs in tests/WindowsSearchConfigurator.UnitTests/Core/Models/RegistrationOptionsTests.cs with property tests
- [X] T036 [US3] Add property getter/setter tests to tests/WindowsSearchConfigurator.UnitTests/Core/Models/COMRegistrationStatusTests.cs
- [X] T037 [US3] Add property getter/setter tests to tests/WindowsSearchConfigurator.UnitTests/Core/Models/RegistrationOptionsTests.cs

### Implementation for User Story 3 - Batch 3 (Enums & Edge Cases)

- [X] T038 [US3] Create EnumTests.cs in tests/WindowsSearchConfigurator.UnitTests/Core/Models/EnumTests.cs with tests for all enums (RuleType, RuleSource, IndexingDepth, etc.)
- [X] T039 [US3] Add enum value validation tests to tests/WindowsSearchConfigurator.UnitTests/Core/Models/EnumTests.cs
- [X] T040 [US3] Add empty collection edge case tests to tests/WindowsSearchConfigurator.UnitTests/Core/Models/IndexRuleTests.cs
- [X] T041 [US3] Add empty collection edge case tests to tests/WindowsSearchConfigurator.UnitTests/Core/Models/ConfigurationFileTests.cs
- [X] T042 [US3] Add null value edge case tests to tests/WindowsSearchConfigurator.UnitTests/Core/Models/OperationResultTests.cs (null exception parameter)
- [X] T043 [US3] Add empty string edge case tests to tests/WindowsSearchConfigurator.UnitTests/Core/Models/ValidationResultTests.cs
- [X] T044 [US3] Add extreme DateTime value tests to tests/WindowsSearchConfigurator.UnitTests/Core/Models/IndexRuleTests.cs (DateTime.MinValue, DateTime.MaxValue)
- [X] T045 [US3] Add Guid.Empty edge case tests to tests/WindowsSearchConfigurator.UnitTests/Core/Models/COMRegistrationAttemptTests.cs

### Verification for User Story 3

- [X] T046 [US3] Run full test suite with coverage: `dotnet test tests/WindowsSearchConfigurator.UnitTests --collect:"XPlat Code Coverage"`
- [X] T047 [US3] Verify overall coverage meets 80% minimum for Core.Models namespace (FR-015, SC-002)
- [X] T048 [US3] Generate HTML coverage report with `reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:Html`
- [X] T049 [US3] Review coverage report to identify any gaps in test coverage

**Checkpoint**: All user stories complete - comprehensive test suite covering all Core models with 80%+ coverage - **111 tests passing in <2 seconds**

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final verification, documentation, and performance validation

- [X] T050 [P] Add XML documentation comments to all test classes explaining what aspect of models is being tested
- [X] T051 [P] Update specs/003-core-unit-tests/quickstart.md with final test commands and examples
- [X] T052 Verify test suite performance meets 5-second target (SC-005) with `dotnet test tests/WindowsSearchConfigurator.UnitTests` - **0.7 seconds achieved (86% faster than target)**
- [X] T053 Run tests in parallel mode to verify test independence: `dotnet test -- NUnit.NumberOfTestWorkers=4` - **Tests are independent, 0.6s runtime**
- [X] T054 Document test coverage metrics in IMPLEMENTATION_SUMMARY.md (TEST_RESULTS.md already contains UAT results)
- [X] T055 Update IMPLEMENTATION_SUMMARY.md with 003-core-unit-tests feature completion details
- [X] T056 Run quickstart.md validation to ensure all documented commands work correctly - **111/111 Core.Models tests passing**

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion
- **User Stories (Phase 3-5)**: All depend on Foundational phase completion
  - User Story 1 (Phase 3): Can start after Foundational - No dependencies on other stories
  - User Story 2 (Phase 4): Can start after Foundational - Independent of US1
  - User Story 3 (Phase 5): Can start after Foundational - Independent of US1/US2
- **Polish (Phase 6)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: INDEPENDENT - Tests result objects (ValidationResult, OperationResult)
- **User Story 2 (P2)**: INDEPENDENT - Tests configuration models (IndexRule, ConfigurationFile, FileExtensionSetting)
- **User Story 3 (P3)**: INDEPENDENT - Tests audit/support models and edge cases

All user stories can be implemented in parallel once Foundational phase completes.

### Within Each User Story

- Test file creation tasks marked [P] can run in parallel
- Test implementation tasks within same file must run sequentially
- Coverage verification runs after all tests for that story are written

### Parallel Opportunities

- **Phase 1**: T001-T004 can run sequentially (quick verification tasks)
- **Phase 2**: T005-T007 can run sequentially (quick setup tasks)
- **Phase 3 (US1)**: T008 and T009 can run in parallel (different files)
- **Phase 4 (US2)**: T017, T018, T019 can run in parallel (different files)
- **Phase 5 (US3)**: Within each batch, file creation tasks marked [P] can run in parallel
  - Batch 1: T028, T029, T030 in parallel
  - Batch 2: T034, T035 in parallel
- **Phase 6**: T050 and T051 can run in parallel (different documentation)
- **Cross-story parallelism**: Once Phase 2 completes, US1, US2, and US3 can all proceed in parallel with different developers

---

## Parallel Example: User Story 1

```bash
# Launch model test file creation in parallel:
# Developer A: Create ValidationResultTests.cs (T008)
# Developer B: Create OperationResultTests.cs (T009)
# Both files are independent and can be created simultaneously

# Then proceed with adding tests to each file sequentially within that file
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (4 tasks, ~10 minutes)
2. Complete Phase 2: Foundational (3 tasks, ~5 minutes)
3. Complete Phase 3: User Story 1 (9 tasks, ~2 hours)
4. **STOP and VALIDATE**: Run US1 tests, verify coverage
5. **MVP DELIVERED**: Result object models fully tested

**Value**: Core result objects (ValidationResult, OperationResult) are used throughout the application. Testing them first provides immediate confidence in fundamental data structures.

### Incremental Delivery

1. Setup + Foundational → Test infrastructure ready (15 minutes)
2. Add User Story 1 → Result objects tested → MVP! (2 hours)
3. Add User Story 2 → Configuration models tested → Extended coverage (2 hours)
4. Add User Story 3 → Full coverage with edge cases → Complete (3 hours)
5. Polish → Documentation and verification → Production ready (1 hour)

**Total estimated time**: ~8-9 hours for complete feature

### Parallel Team Strategy

With multiple developers after Foundational phase completes:

1. **Developer A**: User Story 1 (ValidationResult, OperationResult tests)
2. **Developer B**: User Story 2 (IndexRule, ConfigurationFile, FileExtensionSetting tests)
3. **Developer C**: User Story 3 (COM, supporting models, enums, edge cases)

All three stories complete independently and merge without conflicts (different test files).

**Parallel execution time**: ~3 hours for all three stories + 1 hour polish = ~4 hours total

---

## Implementation Batches

Each batch should be committed as a logical unit:

### Batch 1: Test Infrastructure (Phase 1-2)
- Tasks T001-T007
- Files: Project config, directory structure
- Commit message: `test(infrastructure): setup unit test framework and coverage tooling`

### Batch 2: Result Object Tests (US1)
- Tasks T008-T016
- Files: ValidationResultTests.cs, OperationResultTests.cs
- Commit message: `test(core): add ValidationResult and OperationResult unit tests`

### Batch 3: Configuration Model Tests (US2)
- Tasks T017-T027
- Files: IndexRuleTests.cs, ConfigurationFileTests.cs, FileExtensionSettingTests.cs
- Commit message: `test(core): add configuration model unit tests`

### Batch 4: Audit & Supporting Model Tests (US3 Batch 1-2)
- Tasks T028-T037
- Files: COMRegistrationAttemptTests.cs, FileTypeFilterTests.cs, IndexLocationTests.cs, COMRegistrationStatusTests.cs, RegistrationOptionsTests.cs
- Commit message: `test(core): add COM registration and supporting model unit tests`

### Batch 5: Enum & Edge Case Tests (US3 Batch 3)
- Tasks T038-T049
- Files: EnumTests.cs, edge case additions to existing test files
- Commit message: `test(core): add enum tests and edge case coverage`

### Batch 6: Polish & Documentation (Phase 6)
- Tasks T050-T056
- Files: Documentation updates, TEST_RESULTS.md
- Commit message: `docs(tests): document test results and update quickstart guide`

---

## Success Metrics

Upon completion of all tasks:

- ✅ **SC-001**: 11+ test files covering all Core models
- ✅ **SC-002**: 80%+ code coverage for Core.Models namespace
- ✅ **SC-003**: All factory methods have 2+ test scenarios
- ✅ **SC-004**: All constructors have initialization tests
- ✅ **SC-005**: Test suite executes in <5 seconds
- ✅ **SC-006**: Zero test failures
- ✅ **SC-007**: Each test class has normal, empty, and boundary tests
- ✅ **SC-008**: Tests run independently (no external dependencies)
- ✅ **SC-009**: Collection properties verified as empty lists (not null)
- ✅ **SC-010**: Auto-generation logic (Guid, DateTime) verified

---

## Notes

- **[P] tasks**: Different files, can run in parallel
- **[Story] label**: Maps task to specific user story for traceability
- **Test Independence**: Each user story's tests are completely independent
- **No mocking needed**: Pure data model tests without external dependencies
- **Coverage tooling**: Coverlet + ReportGenerator (installed via Phase 2)
- **Naming convention**: `{MethodName}_{Scenario}_{ExpectedBehavior}` for test methods
- **Commit frequency**: After each batch (6 commits total)
- **Branch**: All work on `003-core-unit-tests` feature branch
