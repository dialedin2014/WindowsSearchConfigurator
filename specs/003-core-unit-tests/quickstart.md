# Quickstart: Core Model Unit Tests

**Feature**: 003-core-unit-tests  
**Date**: 2025-10-23  
**Status**: ✅ **IMPLEMENTATION COMPLETE**  
**Audience**: Developers working on WindowsSearchConfigurator

---

## ✅ Implementation Complete

**Test Suite**: 111 Core Model tests + 39 existing tests = **150 tests total**  
**Pass Rate**: 100% (150/150 passing)  
**Execution Time**: 0.7 seconds (86% faster than target)  
**Code Coverage**: 80%+ on Core.Models namespace  

### Test Files Created
- `Core/Models/ValidationResultTests.cs` (12 tests)
- `Core/Models/OperationResultTests.cs` (17 tests)
- `Core/Models/IndexRuleTests.cs` (18 tests)
- `Core/Models/ConfigurationFileTests.cs` (15 tests)
- `Core/Models/FileExtensionSettingTests.cs` (12 tests)
- `Core/Models/COMRegistrationAttemptTests.cs` (5 tests)
- `Core/Models/FileTypeFilterTests.cs` (3 tests)
- `Core/Models/IndexLocationTests.cs` (4 tests)
- `Core/Models/COMRegistrationStatusTests.cs` (4 tests)
- `Core/Models/RegistrationOptionsTests.cs` (7 tests)
- `Core/Models/EnumTests.cs` (14 tests covering 9 enums)

### Run All Core Model Tests
```bash
dotnet test tests/WindowsSearchConfigurator.UnitTests --filter FullyQualifiedName~Core.Models
```

Expected output:
```
Test Run Successful.
Total tests: 111
     Passed: 111
 Total time: ~0.7 seconds
```

---

## Overview

This quickstart guide helps you run, write, and debug unit tests for Core models in the WindowsSearchConfigurator project.

---

## Prerequisites

- .NET 8.0 SDK installed
- Visual Studio 2022, VS Code, or Rider IDE
- WindowsSearchConfigurator solution cloned locally

---

## Quick Commands

### Run All Tests
```bash
dotnet test
```

### Run Only Core Model Tests
```bash
dotnet test --filter "FullyQualifiedName~WindowsSearchConfigurator.UnitTests.Core.Models"
```

### Run With Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Run Specific Test Class
```bash
dotnet test --filter "FullyQualifiedName~ValidationResultTests"
```

### Run In Watch Mode (Auto-rerun on changes)
```bash
dotnet watch test
```

---

## Getting Started

### 1. Navigate to Test Project
```bash
cd tests/WindowsSearchConfigurator.UnitTests
```

### 2. Verify Test Project Builds
```bash
dotnet build
```

Expected output:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### 3. Run Tests
```bash
dotnet test
```

Expected output:
```
Starting test execution, please wait...
A total of X tests were run.
Passed: X, Failed: 0, Skipped: 0
Test Run Successful.
```

---

## Writing Your First Test

### Example: Testing ValidationResult

Create file: `tests/WindowsSearchConfigurator.UnitTests/Core/Models/ValidationResultTests.cs`

```csharp
namespace WindowsSearchConfigurator.UnitTests.Core.Models;

[TestFixture]
public class ValidationResultTests
{
    [Test]
    public void Success_WithValidValue_ShouldSetIsValidTrue()
    {
        // Arrange
        var normalizedValue = "C:\\Test\\Path";
        
        // Act
        var result = ValidationResult.Success(normalizedValue);
        
        // Assert
        Assert.That(result.IsValid, Is.True);
        Assert.That(result.NormalizedValue, Is.EqualTo(normalizedValue));
        Assert.That(result.ErrorMessage, Is.Null);
        Assert.That(result.Severity, Is.EqualTo(ValidationSeverity.None));
    }
    
    [Test]
    public void Failure_WithErrorMessage_ShouldSetIsValidFalse()
    {
        // Arrange
        var errorMessage = "Path is invalid";
        
        // Act
        var result = ValidationResult.Failure(errorMessage);
        
        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.NormalizedValue, Is.Null);
        Assert.That(result.ErrorMessage, Is.EqualTo(errorMessage));
        Assert.That(result.Severity, Is.EqualTo(ValidationSeverity.Error));
    }
}
```

### Run Your New Test
```bash
dotnet test --filter "FullyQualifiedName~ValidationResultTests"
```

---

## Common Testing Patterns

### Pattern 1: Factory Method Testing
```csharp
[Test]
public void Ok_WithDefaultMessage_ShouldSetSuccessTrue()
{
    // Act
    var result = OperationResult.Ok();
    
    // Assert
    Assert.That(result.Success, Is.True);
    Assert.That(result.Message, Is.EqualTo("Operation completed successfully"));
}
```

### Pattern 2: Constructor Testing
```csharp
[Test]
public void Constructor_ShouldGenerateUniqueGuid()
{
    // Act
    var rule1 = new IndexRule();
    var rule2 = new IndexRule();
    
    // Assert
    Assert.That(rule1.Id, Is.Not.EqualTo(Guid.Empty));
    Assert.That(rule2.Id, Is.Not.EqualTo(Guid.Empty));
    Assert.That(rule1.Id, Is.Not.EqualTo(rule2.Id));
}
```

### Pattern 3: Collection Initialization Testing
```csharp
[Test]
public void FileTypeFilters_ShouldInitializeAsEmptyList()
{
    // Act
    var rule = new IndexRule();
    
    // Assert
    Assert.That(rule.FileTypeFilters, Is.Not.Null);
    Assert.That(rule.FileTypeFilters, Is.Empty);
}
```

### Pattern 4: Timestamp Testing
```csharp
[Test]
public void Constructor_ShouldSetCreatedDateToUtcNow()
{
    // Arrange
    var before = DateTime.UtcNow;
    
    // Act
    var rule = new IndexRule();
    var after = DateTime.UtcNow;
    
    // Assert
    Assert.That(rule.CreatedDate, Is.GreaterThanOrEqualTo(before));
    Assert.That(rule.CreatedDate, Is.LessThanOrEqualTo(after));
}
```

### Pattern 5: Parameterized Tests
```csharp
[TestCase(RuleType.Include)]
[TestCase(RuleType.Exclude)]
public void Constructor_WithRuleType_ShouldSetCorrectly(RuleType ruleType)
{
    // Act
    var rule = new IndexRule("C:\\Test", ruleType, RuleSource.UserConfigured);
    
    // Assert
    Assert.That(rule.RuleType, Is.EqualTo(ruleType));
}
```

---

## Viewing Code Coverage

### Step 1: Install ReportGenerator Tool
```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
```

### Step 2: Run Tests With Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Step 3: Generate HTML Report
```bash
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:Html
```

### Step 4: Open Report
```bash
# Windows
start coverage-report/index.html

# Or navigate manually to:
# tests/WindowsSearchConfigurator.UnitTests/coverage-report/index.html
```

### Expected Coverage View
The HTML report shows:
- Overall coverage percentage
- Coverage by namespace
- Coverage by class
- Line-by-line coverage highlighting

**Target**: Minimum 80% coverage for `WindowsSearchConfigurator.Core.Models`

---

## Debugging Tests

### Visual Studio
1. Open test file
2. Click in left margin to set breakpoint
3. Right-click test method → "Debug Test(s)"
4. Debugger will pause at breakpoint

### VS Code
1. Install "C# Dev Kit" extension
2. Open test file
3. Click "Debug" link above test method
4. Or use "Test Explorer" panel

### Command Line
```bash
# Set environment variable for verbose output
$env:VSTEST_HOST_DEBUG=1
dotnet test --filter "FullyQualifiedName~YourTestName"
```

---

## Troubleshooting

### Problem: Tests Not Discovered

**Solution**: Ensure test class has `[TestFixture]` and methods have `[Test]`
```csharp
[TestFixture]  // Add this
public class ValidationResultTests
{
    [Test]  // Add this
    public void Success_ShouldWork() { }
}
```

### Problem: "Cannot Find Type" Errors

**Solution**: Add project reference in `.csproj`
```xml
<ItemGroup>
  <ProjectReference Include="..\..\src\WindowsSearchConfigurator\WindowsSearchConfigurator.csproj" />
</ItemGroup>
```

### Problem: Tests Run Slowly

**Solution**: Check for external dependencies
- No file I/O in tests
- No database connections
- No registry access
- Use in-memory objects only

### Problem: Coverage Report Not Generated

**Solution**: Install coverlet collector
```bash
dotnet add package coverlet.collector
```

### Problem: Tests Fail Intermittently

**Solution**: Check for timing issues in timestamp tests
```csharp
// Bad: Too precise
Assert.That(rule.CreatedDate, Is.EqualTo(DateTime.UtcNow));

// Good: Use range
var before = DateTime.UtcNow;
var rule = new IndexRule();
var after = DateTime.UtcNow;
Assert.That(rule.CreatedDate, Is.InRange(before, after));
```

---

## IDE-Specific Features

### Visual Studio 2022

**Test Explorer**:
- View → Test Explorer
- Shows all tests in tree view
- Run, debug, group by namespace/class

**Live Unit Testing** (Enterprise only):
- Test → Live Unit Testing → Start
- Automatically runs tests as you code
- Shows coverage in editor margins

**Code Coverage** (Enterprise only):
- Test → Analyze Code Coverage for All Tests
- Visual coverage highlighting

### VS Code

**Test Explorer**:
- Install "C# Dev Kit" extension
- Test panel in sidebar
- Run/debug tests from editor

**Coverage Gutters** (with extension):
- Install "Coverage Gutters" extension
- Shows coverage in editor after running tests with coverage

### JetBrains Rider

**Unit Test Explorer**:
- Built-in test runner
- Coverage visualization
- Continuous testing mode

---

## Continuous Integration

### GitHub Actions Workflow

Tests run automatically on:
- Push to feature branches
- Pull requests to main/master
- Manual workflow dispatch

### Viewing CI Results
1. Go to GitHub repository
2. Click "Actions" tab
3. Select workflow run
4. View test results and coverage

### Local CI Simulation
```bash
# Run the same commands as CI
dotnet restore
dotnet build --no-restore
dotnet test --no-build --collect:"XPlat Code Coverage"
```

---

## Best Practices

### ✅ DO
- Use descriptive test names: `Method_Scenario_ExpectedResult`
- Follow Arrange-Act-Assert pattern
- Test one concept per test method
- Use meaningful variable names
- Group related tests with regions
- Test edge cases (null, empty, boundary values)
- Keep tests fast (no I/O, no sleep)
- Make tests independent (no shared state)

### ❌ DON'T
- Don't test private methods directly
- Don't use Thread.Sleep() in tests
- Don't rely on external resources
- Don't write tests that depend on other tests
- Don't use magic numbers without explanation
- Don't skip arranging test data
- Don't mix multiple test concepts in one test

---

## Quick Reference

### Common NUnit Assertions
```csharp
// Equality
Assert.That(actual, Is.EqualTo(expected));
Assert.That(actual, Is.Not.EqualTo(unexpected));

// Boolean
Assert.That(value, Is.True);
Assert.That(value, Is.False);

// Null
Assert.That(value, Is.Null);
Assert.That(value, Is.Not.Null);

// Collections
Assert.That(collection, Is.Empty);
Assert.That(collection, Has.Count.EqualTo(5));
Assert.That(collection, Contains.Item("value"));

// Ranges
Assert.That(value, Is.GreaterThan(5));
Assert.That(value, Is.LessThanOrEqualTo(10));
Assert.That(value, Is.InRange(5, 10));

// Types
Assert.That(obj, Is.InstanceOf<IndexRule>());
Assert.That(obj, Is.TypeOf<ValidationResult>());

// References
Assert.That(obj1, Is.SameAs(obj2));  // Same reference
Assert.That(obj1, Is.Not.SameAs(obj2));  // Different reference

// Exceptions
Assert.Throws<ArgumentNullException>(() => new IndexRule(null, RuleType.Include, RuleSource.UserConfigured));
```

### Test Naming Examples
```csharp
// Factory methods
Success_WithValidValue_ShouldSetIsValidTrue()
Failure_WithErrorMessage_ShouldSetErrorFields()

// Constructors
Constructor_ShouldGenerateUniqueGuid()
Constructor_WithParameters_ShouldSetAllProperties()

// Properties
FileTypeFilters_ShouldInitializeAsEmptyList()
ExportDate_ShouldBeSetToUtcNow()

// Edge cases
Constructor_WithEmptyPath_ShouldThrowException()
Collection_WhenEmpty_ShouldNotBeNull()
```

---

## Additional Resources

- **NUnit Documentation**: https://docs.nunit.org/
- **.NET Testing Guide**: https://learn.microsoft.com/en-us/dotnet/core/testing/
- **Code Coverage**: https://github.com/coverlet-coverage/coverlet
- **Feature Spec**: [spec.md](./spec.md)
- **Data Model**: [data-model.md](./data-model.md)
- **Test Contract**: [contracts/test-contract.md](./contracts/test-contract.md)

---

## Next Steps

1. **Read the feature spec**: Understand requirements in [spec.md](./spec.md)
2. **Review data models**: See [data-model.md](./data-model.md) for models being tested
3. **Check contracts**: Review [contracts/test-contract.md](./contracts/test-contract.md)
4. **Start coding**: Follow implementation plan in [tasks.md](./tasks.md) (created by `/speckit.tasks`)
5. **Run tests frequently**: Use `dotnet watch test` for continuous feedback
6. **Check coverage**: Aim for 80%+ coverage
7. **Commit often**: Follow conventional commit format

---

## Support

For questions or issues:
1. Check this quickstart guide
2. Review spec and contracts
3. Check existing test files for examples
4. Consult NUnit documentation
5. Ask team members

Happy testing! 🧪
