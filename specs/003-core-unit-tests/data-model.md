# Data Model: Core Model Unit Tests

**Feature**: 003-core-unit-tests  
**Date**: 2025-10-23

## Overview

This document describes the test data structures and organization for Core model unit tests. Since this is a testing feature, the "data model" refers to how tests are organized, what test data patterns are used, and how test coverage maps to source models.

---

## Test Organization Model

### Test Project Structure

```
WindowsSearchConfigurator.UnitTests/
├── Core/
│   └── Models/
│       ├── ValidationResultTests.cs
│       ├── OperationResultTests.cs
│       ├── IndexRuleTests.cs
│       ├── ConfigurationFileTests.cs
│       ├── FileExtensionSettingTests.cs
│       ├── COMRegistrationAttemptTests.cs
│       ├── FileTypeFilterTests.cs
│       ├── IndexLocationTests.cs
│       ├── RegistrationOptionsTests.cs
│       ├── COMRegistrationStatusTests.cs
│       └── EnumTests.cs
```

### Test Class Naming Pattern

**Pattern**: `{ModelName}Tests`

- **Example**: `ValidationResult.cs` → `ValidationResultTests.cs`
- **Namespace**: `WindowsSearchConfigurator.UnitTests.Core.Models`
- **Attribute**: `[TestFixture]`

---

## Source Models Being Tested

### Category 1: Result Objects (Immutable)

#### ValidationResult
**Purpose**: Represents validation outcome with normalized value and error details

| Property | Type | Mutability | Test Focus |
|----------|------|------------|------------|
| IsValid | bool | init | Factory method sets correctly |
| NormalizedValue | string? | init | Success sets, Failure leaves null |
| ErrorMessage | string? | init | Failure sets, Success leaves null |
| Severity | ValidationSeverity | init | Correct severity for each factory |

**Factory Methods to Test**:
- `Success(string normalizedValue)` → IsValid=true, NormalizedValue set, Severity=None
- `Failure(string error)` → IsValid=false, ErrorMessage set, Severity=Error
- `Warning(string warning, string normalizedValue)` → IsValid=true, both messages set, Severity=Warning

**Test Scenarios**:
1. Each factory method sets all properties correctly
2. Null/empty string handling
3. Init-only property behavior

---

#### OperationResult
**Purpose**: Represents operation outcome with status and optional error details

| Property | Type | Mutability | Test Focus |
|----------|------|------------|------------|
| Success | bool | init | Factory methods set correctly |
| Message | string | init | Always set (default or custom) |
| ErrorCode | int? | init | Only set on failure |
| Exception | Exception? | init | Captured on failure |

**Factory Methods to Test**:
- `Ok(string message = "...")` → Success=true, Message set
- `Fail(string message, int? errorCode, Exception? exception)` → Success=false, all error fields

**Test Scenarios**:
1. Ok with default message
2. Ok with custom message
3. Fail with all parameters
4. Fail with partial parameters

---

#### OperationResult&lt;T&gt;
**Purpose**: Generic variant that includes typed result value

| Property | Type | Mutability | Test Focus |
|----------|------|------------|------------|
| Value | T? | init | Stores correct type |
| *(inherits OperationResult)* | - | - | Base functionality works |

**Factory Methods to Test**:
- `Ok(T value, string message = "...")` → Value set correctly
- `Fail(...)` → Value is null/default

**Test Scenarios**:
1. Generic with value types (int, bool)
2. Generic with reference types (string, IndexRule)
3. Generic with nullable types
4. Value preservation vs reference

---

### Category 2: Configuration Models (Mutable)

#### IndexRule
**Purpose**: Represents a search indexing rule with path and filters

| Property | Type | Mutability | Test Focus |
|----------|------|------------|------------|
| Id | Guid | set | Auto-generated on construction |
| Path | string | required, set | Constructor parameter |
| RuleType | RuleType | required, set | Constructor parameter |
| Source | RuleSource | required, set | Constructor parameter |
| Recursive | bool | set | Defaults to true |
| FileTypeFilters | List&lt;FileTypeFilter&gt; | set | Initialized as empty list |
| ExcludedSubfolders | List&lt;string&gt; | set | Initialized as empty list |
| IsUserDefined | bool | set | Defaults to true |
| CreatedDate | DateTime | set | Auto-set to UtcNow |
| ModifiedDate | DateTime | set | Auto-set to UtcNow |

**Constructors to Test**:
1. Default constructor: Generates Id, sets timestamps
2. Parameterized constructor: Sets required properties + auto-values

**Test Scenarios**:
1. Default constructor auto-generation (Id, timestamps)
2. Parameterized constructor property mapping
3. Collection properties initialize as empty lists (not null)
4. Required property enforcement
5. Guid uniqueness across instances
6. DateTime values are reasonable UTC times

---

#### ConfigurationFile
**Purpose**: Export/import container for rules and settings

| Property | Type | Mutability | Test Focus |
|----------|------|------------|------------|
| Version | string | required, set | Constructor parameter |
| ExportDate | DateTime | set | Auto-set to UtcNow |
| ExportedBy | string | required, set | Constructor parameter |
| MachineName | string | required, set | Constructor parameter |
| Rules | List&lt;IndexRule&gt; | set | Initialized as empty list |
| ExtensionSettings | List&lt;FileExtensionSetting&gt; | set | Initialized as empty list |
| Checksum | string? | set | Optional integrity hash |

**Constructors to Test**:
1. Default constructor: Sets ExportDate automatically
2. Parameterized constructor: Sets all required + ExportDate

**Test Scenarios**:
1. ExportDate auto-set on construction
2. Collections initialize as empty lists
3. Required properties enforced
4. Optional Checksum can be null

---

#### FileExtensionSetting
**Purpose**: Configures indexing depth for file extension

| Property | Type | Mutability | Test Focus |
|----------|------|------------|------------|
| Extension | string | required, set | Constructor parameter |
| IndexingDepth | IndexingDepth | required, set | Constructor parameter |
| IsDefaultSetting | bool | set | Constructor parameter (default false) |
| ModifiedDate | DateTime | set | Auto-set to UtcNow |

**Constructors to Test**:
1. Default constructor: Sets ModifiedDate
2. Parameterized constructor: Sets all properties + ModifiedDate

**Test Scenarios**:
1. ModifiedDate auto-set on construction
2. IsDefaultSetting defaults to false
3. Required properties enforced

---

#### COMRegistrationAttempt
**Purpose**: Audit record for COM registration attempts

| Property | Type | Mutability | Test Focus |
|----------|------|------------|------------|
| AttemptId | Guid | set | Should be unique |
| Timestamp | DateTime | set | UTC time of attempt |
| Mode | RegistrationMode | set | How registration initiated |
| User | string | set | Windows username |
| IsAdministrator | bool | set | Admin privilege flag |
| DLLPath | string | set | Path to DLL |
| RegistrationMethod | string | set | Defaults to "regsvr32" |
| Outcome | RegistrationOutcome | set | Success/failure status |
| ExitCode | int? | set | Process exit code |
| ErrorMessage | string? | set | Error details if failed |
| DurationMs | long | set | Time taken |
| PostValidation | COMValidationState | set | Validation result |

**Test Scenarios**:
1. All properties settable and gettable
2. Default RegistrationMethod value
3. Optional properties can be null
4. AttemptId uniqueness

---

### Category 3: Supporting Models

#### FileTypeFilter
**Purpose**: Defines file type inclusion/exclusion patterns

| Property | Type | Test Focus |
|----------|------|------------|
| Pattern | string | Settable |
| FilterType | FilterType | Enum value storage |
| Target | FilterTarget | Enum value storage |

#### IndexLocation
**Purpose**: Represents a physical location to index

*Properties vary - test all are settable/gettable*

#### COMRegistrationStatus
**Purpose**: Current state of COM registration

*Properties vary - test all are settable/gettable*

#### RegistrationOptions
**Purpose**: Options for COM registration behavior

*Properties vary - test all are settable/gettable*

---

### Category 4: Enumerations

#### RuleType
```csharp
public enum RuleType
{
    Include = 0,
    Exclude = 1
}
```

#### RuleSource
```csharp
public enum RuleSource
{
    WindowsDefault,
    UserConfigured,
    ImportedConfiguration
}
```

#### IndexingDepth
```csharp
public enum IndexingDepth
{
    PropertiesOnly,
    PropertiesAndContent
}
```

#### RegistrationMode
```csharp
public enum RegistrationMode
{
    Automatic,
    Manual,
    Skipped
}
```

#### RegistrationOutcome
```csharp
public enum RegistrationOutcome
{
    Success,
    Failure,
    Skipped
}
```

#### ValidationSeverity
```csharp
public enum ValidationSeverity
{
    None = 0,
    Warning = 1,
    Error = 2
}
```

#### FilterType
```csharp
public enum FilterType
{
    Include,
    Exclude
}
```

#### FilterTarget
```csharp
public enum FilterTarget
{
    FileName,
    Extension,
    Directory
}
```

#### PathType
```csharp
public enum PathType
{
    LocalFileSystem,
    NetworkShare,
    Unknown
}
```

#### COMValidationState
```csharp
public enum COMValidationState
{
    NotValidated,
    Valid,
    Invalid
}
```

**Enum Test Strategy**:
- Verify enum values can be assigned
- Verify integer values match expectations
- Verify ToString() produces expected names
- Verify parsing from string works (if applicable)

---

## Test Data Patterns

### Valid Test Data Examples

```csharp
// ValidationResult
var validResult = ValidationResult.Success("C:\\NormalizedPath");
var invalidResult = ValidationResult.Failure("Path is invalid");
var warningResult = ValidationResult.Warning("Path modified", "C:\\ModifiedPath");

// OperationResult
var successOp = OperationResult.Ok("Configuration applied");
var failedOp = OperationResult.Fail("Access denied", 5, new UnauthorizedAccessException());

// OperationResult<T>
var resultWithValue = OperationResult<int>.Ok(42);
var resultWithRule = OperationResult<IndexRule>.Ok(new IndexRule("C:\\Test", RuleType.Include, RuleSource.UserConfigured));

// IndexRule
var rule = new IndexRule("C:\\Users\\Public", RuleType.Include, RuleSource.UserConfigured)
{
    Recursive = true,
    FileTypeFilters = new List<FileTypeFilter> { /* filters */ }
};

// ConfigurationFile
var config = new ConfigurationFile("1.0", "DOMAIN\\User", "WORKSTATION")
{
    Rules = new List<IndexRule> { rule },
    Checksum = "abc123..."
};

// FileExtensionSetting
var extSetting = new FileExtensionSetting(".txt", IndexingDepth.PropertiesAndContent, false);
```

### Edge Case Test Data

```csharp
// Empty strings
var emptyPath = "";
var nullValue = (string?)null;

// Empty collections
var emptyRules = new List<IndexRule>();
var emptyFilters = new List<FileTypeFilter>();

// Boundary DateTime values
var minDate = DateTime.MinValue;
var maxDate = DateTime.MaxValue;
var utcNow = DateTime.UtcNow;

// Guid edge cases
var emptyGuid = Guid.Empty;
var newGuid = Guid.NewGuid();

// Null nullable properties
int? nullErrorCode = null;
Exception? nullException = null;
string? nullMessage = null;
```

---

## Validation Rules (from Spec)

### Property Initialization Rules
1. **Guid properties**: Must generate unique values (not Guid.Empty) when using default constructors
2. **DateTime properties**: Must be set to reasonable UTC values on construction
3. **Collection properties**: Must initialize as empty lists, never null
4. **Required properties**: Must be set via constructor or object initializer
5. **Optional properties**: Can be null (string?, int?, Exception?)

### Factory Method Rules
1. **Success factories**: Set Success/IsValid=true, clear error fields
2. **Failure factories**: Set Success/IsValid=false, populate error fields
3. **Warning factories**: Set IsValid=true, but include warning message

### State Transition Rules
1. **CreatedDate ≤ ModifiedDate**: ModifiedDate should not be before CreatedDate
2. **ExportDate**: Should be approximately current UTC time when created
3. **Immutable objects**: Cannot change properties after factory creation

---

## Coverage Targets

| Model Category | Target Coverage | Priority |
|----------------|----------------|----------|
| Result Objects | 100% | P1 |
| Configuration Models | 90%+ | P1 |
| Supporting Models | 80%+ | P2 |
| Enumerations | 80%+ | P3 |

**Overall Target**: Minimum 80% code coverage for Core.Models namespace (per FR-015)

---

## Test Execution Model

### Test Grouping
- **Fast Unit Tests**: All tests in this feature (no I/O, no external dependencies)
- **Execution Time**: < 5 seconds for full suite (per SC-005)
- **Parallel Execution**: Tests can run in parallel (no shared state)

### Test Independence
- Each test creates its own model instances
- No shared test data between tests
- No setup/teardown required for most tests
- No external dependencies (database, file system, registry, network)

---

## Summary

This feature tests **19 model classes/enums**:
- **3 Result objects**: ValidationResult, OperationResult, OperationResult<T>
- **4 Configuration models**: IndexRule, ConfigurationFile, FileExtensionSetting, COMRegistrationAttempt
- **4 Supporting models**: FileTypeFilter, IndexLocation, COMRegistrationStatus, RegistrationOptions
- **10 Enumerations**: RuleType, RuleSource, IndexingDepth, RegistrationMode, RegistrationOutcome, ValidationSeverity, FilterType, FilterTarget, PathType, COMValidationState

**Total estimated test files**: 11 files (some models grouped, enums in one file)

**Implementation batches** (per Constitution VI):
1. Batch 1: ValidationResult, OperationResult (2 files)
2. Batch 2: IndexRule, ConfigurationFile (2 files)
3. Batch 3: FileExtensionSetting, COMRegistrationAttempt (2 files)
4. Batch 4: Supporting models + Enums (3-4 files)
