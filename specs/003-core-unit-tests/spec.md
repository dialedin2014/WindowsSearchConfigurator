# Feature Specification: Core Model Unit Tests

**Feature Branch**: `003-core-unit-tests`  
**Created**: 2025-10-23  
**Status**: Draft  
**Input**: User description: "Write unit tests for the Core functionality."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Validate Core Model Behavior (Priority: P1)

As a developer maintaining WindowsSearchConfigurator, I need comprehensive unit tests for Core models to ensure that data validation, state transitions, and factory methods work correctly and prevent regressions.

**Why this priority**: Core models are fundamental data structures used throughout the application. Bugs in these models cascade to all dependent features. Testing them thoroughly ensures data integrity and system reliability.

**Independent Test**: Can be fully tested by creating instances of each Core model class, exercising their properties, methods, and validation logic, and verifying expected behavior without external dependencies. This delivers immediate value by catching data structure bugs early in development.

**Acceptance Scenarios**:

1. **Given** a ValidationResult factory method is called with success parameters, **When** the result is examined, **Then** IsValid is true, NormalizedValue is set, and ErrorMessage is null
2. **Given** a ValidationResult factory method is called with failure parameters, **When** the result is examined, **Then** IsValid is false, ErrorMessage is set, and NormalizedValue is null
3. **Given** an IndexRule is created with valid parameters, **When** its properties are accessed, **Then** all properties match the constructor arguments and Id is a valid Guid
4. **Given** a ConfigurationFile is instantiated, **When** ExportDate is checked, **Then** it is automatically set to the current UTC time
5. **Given** an OperationResult is created using the Ok factory method, **When** examined, **Then** Success is true and Message contains success text
6. **Given** an OperationResult is created using the Fail factory method, **When** examined, **Then** Success is false, ErrorCode is set, and Exception is captured

---

### User Story 2 - Verify Model State Management (Priority: P2)

As a developer, I need to ensure that Core models correctly manage their internal state, including timestamps, identifiers, and relationships, so that data remains consistent throughout the application lifecycle.

**Why this priority**: State management issues can lead to data corruption, audit trail problems, and difficult-to-debug runtime errors. Testing state transitions ensures models behave predictably.

**Independent Test**: Can be tested by creating model instances, modifying their state through setters or methods, and asserting that state changes follow expected rules. For example, verifying that ModifiedDate updates correctly, or that collections are initialized properly.

**Acceptance Scenarios**:

1. **Given** an IndexRule with FileTypeFilters, **When** filters are added or removed, **Then** the collection reflects the changes correctly
2. **Given** a COMRegistrationAttempt is created, **When** AttemptId is checked, **Then** it is a valid unique Guid
3. **Given** a FileExtensionSetting is created with a constructor, **When** ModifiedDate is checked, **Then** it is set to approximately the current UTC time
4. **Given** a ConfigurationFile with multiple Rules, **When** Rules collection is accessed, **Then** all rules are preserved in order
5. **Given** an IndexRule is modified, **When** ModifiedDate is updated, **Then** it reflects a time after CreatedDate

---

### User Story 3 - Test Edge Cases and Boundary Conditions (Priority: P3)

As a developer, I need unit tests that verify Core models handle edge cases gracefully, including null values, empty collections, extreme values, and invalid inputs, to prevent runtime errors and ensure robustness.

**Why this priority**: Edge cases are common sources of production bugs. While less critical than happy-path tests, they significantly improve code reliability and prevent unexpected failures.

**Independent Test**: Can be tested by providing boundary inputs (null, empty, extreme values) to model constructors, setters, and methods, then verifying appropriate behavior (defaults, exceptions, or graceful handling).

**Acceptance Scenarios**:

1. **Given** an IndexRule is created with an empty Path, **When** validation is attempted, **Then** the system handles it gracefully according to validation rules
2. **Given** a ConfigurationFile is created with empty Rules and ExtensionSettings collections, **When** accessed, **Then** collections return empty lists without throwing exceptions
3. **Given** an OperationResult<T> is created with a null Value, **When** Value is accessed, **Then** it returns null without throwing
4. **Given** a COMRegistrationAttempt with an empty User string, **When** User property is accessed, **Then** it returns an empty string not null
5. **Given** a ValidationResult with maximum length ErrorMessage, **When** displayed, **Then** it handles the full message without truncation or errors

---

### Edge Cases

- What happens when model properties are set to null where nullable?
- How do factory methods handle null or empty string arguments?
- What happens when collections (Rules, FileTypeFilters, ExtensionSettings) are accessed before initialization?
- How do models behave when DateTime values represent extreme dates (DateTime.MinValue, DateTime.MaxValue)?
- What happens when Guid properties are set to Guid.Empty?
- How do required properties enforce their requirements (compile-time vs runtime)?
- What happens when enums receive invalid integer values outside their defined range?
- How do generic OperationResult<T> instances behave with value types vs reference types?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Test suite MUST verify all ValidationResult factory methods (Success, Failure, Warning) produce correct IsValid, NormalizedValue, ErrorMessage, and Severity values
- **FR-002**: Test suite MUST verify OperationResult and OperationResult<T> factory methods (Ok, Fail) set Success, Message, ErrorCode, Exception, and Value properties correctly
- **FR-003**: Test suite MUST verify IndexRule constructors initialize Id, CreatedDate, ModifiedDate, Path, RuleType, and Source properties correctly
- **FR-004**: Test suite MUST verify ConfigurationFile automatically sets ExportDate to UTC time when instantiated
- **FR-005**: Test suite MUST verify FileExtensionSetting constructors set Extension, IndexingDepth, IsDefaultSetting, and ModifiedDate correctly
- **FR-006**: Test suite MUST verify COMRegistrationAttempt properties (AttemptId, Timestamp, Mode, User, IsAdministrator, DLLPath, etc.) are settable and gettable
- **FR-007**: Test suite MUST verify collection properties (Rules, FileTypeFilters, ExtensionSettings, ExcludedSubfolders) initialize as empty lists, not null
- **FR-008**: Test suite MUST verify required properties enforce their requirement (ConfigurationFile.Version, IndexRule.Path, etc.)
- **FR-009**: Test suite MUST verify models handle null values appropriately for nullable properties
- **FR-010**: Test suite MUST verify DateTime properties (CreatedDate, ModifiedDate, ExportDate, Timestamp) are set to reasonable UTC values
- **FR-011**: Test suite MUST verify Guid properties (Id, AttemptId) generate unique values when default constructors are used
- **FR-012**: Test suite MUST verify enum properties accept valid enum values and store them correctly
- **FR-013**: Test suite MUST verify OperationResult<T>.Value correctly stores and retrieves values of the generic type
- **FR-014**: Test suite MUST verify all Core model classes in the Models namespace have corresponding test coverage
- **FR-015**: Test suite MUST achieve at least 80% code coverage for Core.Models namespace

### Key Entities *(include if feature involves data)*

- **ValidationResult**: Immutable result object with IsValid flag, NormalizedValue, ErrorMessage, and Severity; includes static factory methods for Success/Failure/Warning
- **OperationResult**: Immutable result object with Success flag, Message, ErrorCode, and Exception; represents operation outcomes
- **OperationResult<T>**: Generic variant that includes a typed Value property for operations returning data
- **IndexRule**: Mutable rule configuration with Id, Path, RuleType, Source, timestamps, recursion flag, and filter collections
- **ConfigurationFile**: Export/import container with Version, ExportDate, ExportedBy, MachineName, Rules list, ExtensionSettings list, and optional Checksum
- **FileExtensionSetting**: Configuration for extension indexing depth with Extension, IndexingDepth, IsDefaultSetting flag, and ModifiedDate
- **COMRegistrationAttempt**: Audit record with AttemptId, Timestamp, Mode, User, IsAdministrator, DLLPath, RegistrationMethod, Outcome, and related metadata
- **Enums**: RuleType, RuleSource, IndexingDepth, RegistrationMode, RegistrationOutcome, ValidationSeverity, PathType, FilterType, FilterTarget, COMValidationState

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All Core data models have corresponding automated test coverage with consistent naming
- **SC-002**: Test suite achieves minimum 80% coverage for Core models functionality
- **SC-003**: All factory methods have at least 2 test scenarios verifying correct behavior
- **SC-004**: All model creation paths have at least 1 test verifying correct property initialization
- **SC-005**: Test suite executes in under 5 seconds on a standard development machine
- **SC-006**: Zero test failures when suite runs against current Core models
- **SC-007**: Each test group contains at least 3 scenarios covering normal operation, empty values, and boundary conditions
- **SC-008**: Test suite runs independently without external dependencies (database, file system, registry, network)
- **SC-009**: All collection properties have tests verifying they initialize as empty collections (not null)
- **SC-010**: All automatic identifier and timestamp generation logic has corresponding verification tests

## Assumptions

- Test framework is consistent with existing test project structure
- Tests follow standard testing patterns with clear arrange-act-assert structure
- Models are designed to be testable without complex dependencies
- Models are primarily data containers with simple validation and factory patterns
- Coverage tooling is available in the development environment
- Tests verify concrete implementations without mocking internal model logic

## Dependencies

- WindowsSearchConfigurator.Core project (models being tested)
- Existing test framework infrastructure
- Test runner capability in development environment

## Out of Scope

- Testing interface definitions (interfaces will be tested through their implementations)
- Integration testing of models with external systems (registry, file system, APIs)
- Performance testing or stress testing of model instantiation
- Testing serialization/deserialization behavior (covered in integration or contract tests)
- Testing how other components use these models (covered in their respective tests)
- User interface testing or end-to-end testing
- Testing framework library code
- Mutation testing or advanced coverage analysis
- Testing model persistence (no persistence in Core models)
