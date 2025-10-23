# Test Contract: Core Model Unit Tests

**Feature**: 003-core-unit-tests  
**Date**: 2025-10-23  
**Version**: 1.0

## Overview

This document defines the testing contract for Core model unit tests. It specifies the expected behavior, test coverage requirements, and validation rules that all tests must satisfy.

---

## Test Framework Contract

### Required Dependencies

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
  <PackageReference Include="NUnit" Version="4.0.1" />
  <PackageReference Include="NUnit3TestAdapter" Version="4.5.0" />
  <PackageReference Include="coverlet.collector" Version="6.0.0" />
</ItemGroup>
```

### Test Discovery Contract
- **Test Class Marker**: `[TestFixture]` attribute (optional but recommended)
- **Test Method Marker**: `[Test]` attribute (required)
- **Test Naming**: `[MethodName]_[Scenario]_[ExpectedBehavior]` pattern
- **Namespace**: Mirror source namespace with `.UnitTests` inserted
  - Source: `WindowsSearchConfigurator.Core.Models`
  - Test: `WindowsSearchConfigurator.UnitTests.Core.Models`

---

## Test Execution Contract

### Performance Requirements (SC-005)
- **Maximum Execution Time**: 5 seconds for full test suite
- **Test Independence**: All tests must be independently executable
- **No External Dependencies**: No database, file system, registry, or network access
- **Parallel Safe**: Tests can run in parallel without conflicts

### Exit Codes
- **0**: All tests passed
- **Non-zero**: One or more tests failed

---

## Coverage Contract (FR-015, SC-002)

### Minimum Coverage Requirements

| Metric | Target | Requirement Level |
|--------|--------|------------------|
| Line Coverage | 80% | MUST |
| Branch Coverage | 70% | SHOULD |
| Method Coverage | 80% | MUST |

### Coverage Measurement
```bash
# Command to run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Expected output location
TestResults/{guid}/coverage.cobertura.xml
```

### Coverage Scope
- **In Scope**: All files in `src/WindowsSearchConfigurator/Core/Models/*.cs`
- **Out of Scope**: Interface definitions, external dependencies

---

## Model Testing Contracts

### ValidationResult Contract

**Factory Method: Success**
```csharp
Input:  normalizedValue (string)
Output: ValidationResult with:
  - IsValid = true
  - NormalizedValue = normalizedValue
  - ErrorMessage = null
  - Severity = ValidationSeverity.None
```

**Factory Method: Failure**
```csharp
Input:  error (string)
Output: ValidationResult with:
  - IsValid = false
  - NormalizedValue = null
  - ErrorMessage = error
  - Severity = ValidationSeverity.Error
```

**Factory Method: Warning**
```csharp
Input:  warning (string), normalizedValue (string)
Output: ValidationResult with:
  - IsValid = true
  - NormalizedValue = normalizedValue
  - ErrorMessage = warning
  - Severity = ValidationSeverity.Warning
```

**Test Scenarios Required** (FR-001, SC-003):
- ✅ Success with valid value sets all properties correctly
- ✅ Failure with error message sets all properties correctly
- ✅ Warning with both parameters sets all properties correctly
- ✅ Factory methods produce immutable instances (init-only properties)

---

### OperationResult Contract

**Factory Method: Ok**
```csharp
Input:  message (string, optional, default="Operation completed successfully")
Output: OperationResult with:
  - Success = true
  - Message = message
  - ErrorCode = null
  - Exception = null
```

**Factory Method: Fail**
```csharp
Input:  message (string), errorCode (int?, optional), exception (Exception?, optional)
Output: OperationResult with:
  - Success = false
  - Message = message
  - ErrorCode = errorCode
  - Exception = exception
```

**Test Scenarios Required** (FR-002, SC-003):
- ✅ Ok with default message
- ✅ Ok with custom message
- ✅ Fail with message only
- ✅ Fail with message and error code
- ✅ Fail with all parameters (message, error code, exception)

---

### OperationResult&lt;T&gt; Contract

**Factory Method: Ok<T>**
```csharp
Input:  value (T), message (string, optional)
Output: OperationResult<T> with:
  - Success = true
  - Value = value
  - Message = message
  - ErrorCode = null
  - Exception = null
```

**Factory Method: Fail<T>**
```csharp
Input:  message (string), errorCode (int?, optional), exception (Exception?, optional)
Output: OperationResult<T> with:
  - Success = false
  - Value = default(T)
  - Message = message
  - ErrorCode = errorCode
  - Exception = exception
```

**Test Scenarios Required** (FR-013, SC-003):
- ✅ Ok with value type (int, bool)
- ✅ Ok with reference type (string, IndexRule)
- ✅ Ok with nullable type
- ✅ Fail returns default value
- ✅ Value reference preservation vs copy

---

### IndexRule Contract

**Constructor: Default**
```csharp
Input:  (none)
Output: IndexRule with:
  - Id = newly generated Guid (not Guid.Empty)
  - CreatedDate = current UTC time (±1 second tolerance)
  - ModifiedDate = current UTC time (±1 second tolerance)
  - FileTypeFilters = empty list (not null)
  - ExcludedSubfolders = empty list (not null)
  - Recursive = true
  - IsUserDefined = true
```

**Constructor: Parameterized**
```csharp
Input:  path (string), ruleType (RuleType), source (RuleSource)
Output: IndexRule with:
  - Id = newly generated Guid (not Guid.Empty)
  - Path = path
  - RuleType = ruleType
  - Source = source
  - CreatedDate = current UTC time
  - ModifiedDate = current UTC time
  - FileTypeFilters = empty list (not null)
  - ExcludedSubfolders = empty list (not null)
  - Recursive = true
  - IsUserDefined = true
```

**Test Scenarios Required** (FR-003, FR-007, FR-010, FR-011, SC-004):
- ✅ Default constructor generates unique Guid
- ✅ Default constructor sets timestamps to UTC now
- ✅ Parameterized constructor sets all properties correctly
- ✅ FileTypeFilters initializes as empty list (not null)
- ✅ ExcludedSubfolders initializes as empty list (not null)
- ✅ Multiple instances have unique Guids
- ✅ CreatedDate ≤ ModifiedDate

---

### ConfigurationFile Contract

**Constructor: Default**
```csharp
Input:  (none)
Output: ConfigurationFile with:
  - ExportDate = current UTC time (±1 second tolerance)
  - Rules = empty list (not null)
  - ExtensionSettings = empty list (not null)
```

**Constructor: Parameterized**
```csharp
Input:  version (string), exportedBy (string), machineName (string)
Output: ConfigurationFile with:
  - Version = version
  - ExportedBy = exportedBy
  - MachineName = machineName
  - ExportDate = current UTC time
  - Rules = empty list (not null)
  - ExtensionSettings = empty list (not null)
  - Checksum = null
```

**Test Scenarios Required** (FR-004, FR-007, FR-008, SC-004):
- ✅ Default constructor sets ExportDate to UTC now
- ✅ Parameterized constructor sets all required properties
- ✅ Rules initializes as empty list (not null)
- ✅ ExtensionSettings initializes as empty list (not null)
- ✅ Required properties must be set (Version, ExportedBy, MachineName)
- ✅ Optional Checksum can be null

---

### FileExtensionSetting Contract

**Constructor: Default**
```csharp
Input:  (none)
Output: FileExtensionSetting with:
  - ModifiedDate = current UTC time (±1 second tolerance)
```

**Constructor: Parameterized**
```csharp
Input:  extension (string), indexingDepth (IndexingDepth), isDefaultSetting (bool, default=false)
Output: FileExtensionSetting with:
  - Extension = extension
  - IndexingDepth = indexingDepth
  - IsDefaultSetting = isDefaultSetting
  - ModifiedDate = current UTC time
```

**Test Scenarios Required** (FR-005, FR-010, SC-004):
- ✅ Default constructor sets ModifiedDate to UTC now
- ✅ Parameterized constructor sets all properties correctly
- ✅ IsDefaultSetting defaults to false when not specified
- ✅ Required properties must be set (Extension, IndexingDepth)

---

### COMRegistrationAttempt Contract

**Properties: All Settable/Gettable**
```csharp
Properties:
  - AttemptId: Guid
  - Timestamp: DateTime
  - Mode: RegistrationMode
  - User: string (default="")
  - IsAdministrator: bool
  - DLLPath: string (default="")
  - RegistrationMethod: string (default="regsvr32")
  - Outcome: RegistrationOutcome
  - ExitCode: int?
  - ErrorMessage: string?
  - DurationMs: long
  - PostValidation: COMValidationState
```

**Test Scenarios Required** (FR-006, FR-009, FR-012, SC-004):
- ✅ All properties are settable
- ✅ All properties are gettable
- ✅ AttemptId generates unique Guid
- ✅ Default values for string properties (empty string, not null)
- ✅ Default value for RegistrationMethod is "regsvr32"
- ✅ Nullable properties can be null (ExitCode, ErrorMessage)
- ✅ Enum properties store values correctly

---

### Enumeration Contract

**Required Tests for Each Enum** (FR-012):
- ✅ Can assign each defined enum value
- ✅ Integer values match expected values
- ✅ ToString() produces expected string representation
- ✅ Default value behavior (typically 0)

**Enums to Test**:
- RuleType (Include=0, Exclude=1)
- RuleSource (WindowsDefault, UserConfigured, ImportedConfiguration)
- IndexingDepth (PropertiesOnly, PropertiesAndContent)
- RegistrationMode (Automatic, Manual, Skipped)
- RegistrationOutcome (Success, Failure, Skipped)
- ValidationSeverity (None=0, Warning=1, Error=2)
- FilterType (Include, Exclude)
- FilterTarget (FileName, Extension, Directory)
- PathType (LocalFileSystem, NetworkShare, Unknown)
- COMValidationState (NotValidated, Valid, Invalid)

---

## Edge Case Testing Contract (SC-007)

Each test group MUST include scenarios for:

### 1. Normal Operation
- Happy path with valid inputs
- All properties set correctly
- Factory methods work as designed

### 2. Empty Values
- Empty strings where strings are used
- Empty collections where collections are used
- Null values for nullable properties

### 3. Boundary Conditions
- Guid.Empty vs generated Guid
- DateTime.MinValue, DateTime.MaxValue, DateTime.UtcNow
- Collection add/remove operations
- Required vs optional property enforcement

**Examples**:
```csharp
// Normal
var rule = new IndexRule("C:\\Test", RuleType.Include, RuleSource.UserConfigured);

// Empty collections
Assert.That(rule.FileTypeFilters, Is.Empty);
Assert.That(rule.FileTypeFilters, Is.Not.Null);

// Boundary - Guid uniqueness
var rule1 = new IndexRule();
var rule2 = new IndexRule();
Assert.That(rule1.Id, Is.Not.EqualTo(rule2.Id));

// Boundary - DateTime
var before = DateTime.UtcNow;
var rule3 = new IndexRule();
var after = DateTime.UtcNow;
Assert.That(rule3.CreatedDate, Is.InRange(before, after));
```

---

## Assertion Contract

### Assertion Patterns

**Equality**:
```csharp
Assert.That(actual, Is.EqualTo(expected));
Assert.That(actual, Is.Not.EqualTo(unexpected));
```

**Boolean**:
```csharp
Assert.That(result.Success, Is.True);
Assert.That(result.IsValid, Is.False);
```

**Null Checks**:
```csharp
Assert.That(value, Is.Null);
Assert.That(value, Is.Not.Null);
```

**Collection**:
```csharp
Assert.That(collection, Is.Empty);
Assert.That(collection, Is.Not.Null);
Assert.That(collection, Has.Count.EqualTo(5));
```

**Range**:
```csharp
Assert.That(timestamp, Is.GreaterThanOrEqualTo(before));
Assert.That(timestamp, Is.LessThanOrEqualTo(after));
Assert.That(value, Is.InRange(min, max));
```

**Type**:
```csharp
Assert.That(result.Value, Is.InstanceOf<IndexRule>());
Assert.That(result.Exception, Is.TypeOf<UnauthorizedAccessException>());
```

**Reference**:
```csharp
Assert.That(result.Value, Is.SameAs(originalObject));  // Same reference
Assert.That(result.Value, Is.Not.SameAs(originalObject));  // Different reference
```

---

## Test Organization Contract

### File Structure
```csharp
// File: ValidationResultTests.cs
namespace WindowsSearchConfigurator.UnitTests.Core.Models;

[TestFixture]
public class ValidationResultTests
{
    #region Success Factory Tests
    
    [Test]
    public void Success_WithValidValue_ShouldSetIsValidTrue()
    {
        // Arrange
        var normalizedValue = "C:\\Test";
        
        // Act
        var result = ValidationResult.Success(normalizedValue);
        
        // Assert
        Assert.That(result.IsValid, Is.True);
    }
    
    #endregion
    
    #region Failure Factory Tests
    
    // ...
    
    #endregion
}
```

### Test Method Structure (Arrange-Act-Assert)
1. **Arrange**: Set up test data and preconditions
2. **Act**: Execute the method/property being tested
3. **Assert**: Verify expected outcomes

### Region Organization (Optional but Recommended)
- Group related tests using `#region` directives
- Typical groupings: Constructor tests, Factory method tests, Property tests, Edge case tests

---

## Validation Success Criteria

### Test Completeness (SC-001, SC-006)
- ✅ All Core models have corresponding test files
- ✅ All constructors tested
- ✅ All factory methods tested (minimum 2 scenarios each per SC-003)
- ✅ All public properties tested
- ✅ All enums tested
- ✅ Zero test failures when suite runs (SC-006)

### Coverage Achievement (SC-002)
- ✅ Minimum 80% line coverage for Core.Models namespace
- ✅ Coverage report generated successfully
- ✅ No critical code paths untested

### Performance (SC-005)
- ✅ Full test suite completes in under 5 seconds
- ✅ No slow tests (>500ms per test)

### Independence (SC-008)
- ✅ Tests run without database access
- ✅ Tests run without file system access
- ✅ Tests run without registry access
- ✅ Tests run without network access
- ✅ Tests can run in parallel

### Quality (SC-009, SC-010)
- ✅ All collection properties verified non-null (SC-009)
- ✅ All auto-generated identifiers verified (SC-010)
- ✅ All auto-generated timestamps verified (SC-010)
- ✅ Test names follow naming convention
- ✅ Tests use Arrange-Act-Assert pattern

---

## Test Execution Commands

### Run All Tests
```bash
dotnet test
```

### Run With Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Run Specific Test File
```bash
dotnet test --filter "FullyQualifiedName~ValidationResultTests"
```

### Run Specific Test Method
```bash
dotnet test --filter "FullyQualifiedName~ValidationResultTests.Success_WithValidValue_ShouldSetIsValidTrue"
```

### Generate Coverage Report
```bash
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:Html
```

---

## Continuous Integration Contract

### GitHub Actions Requirements
- Tests MUST pass before merge to default branch
- Coverage report MUST be generated
- Test results MUST be published as artifacts
- Failed tests MUST block pull request merge

### Expected CI Workflow
```yaml
- name: Run Tests
  run: dotnet test --collect:"XPlat Code Coverage"
  
- name: Check Coverage
  run: |
    # Verify minimum 80% coverage
    # (implementation varies by CI tooling)
    
- name: Publish Test Results
  if: always()
  uses: actions/upload-artifact@v3
  with:
    name: test-results
    path: '**/TestResults/**'
```

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-10-23 | Initial contract definition |

---

## Contract Compliance

This contract MUST be satisfied for the feature to be considered complete. All tests MUST conform to these specifications, and any deviations MUST be documented with justification.
