# Research: Core Model Unit Tests

**Feature**: 003-core-unit-tests  
**Date**: 2025-10-23  
**Status**: Complete

## Overview

This document consolidates research findings for implementing comprehensive unit tests for Core models in the WindowsSearchConfigurator project using NUnit and .NET 8.

---

## Research Task 1: NUnit Best Practices for Data Model Testing

### Decision
Use NUnit 4.x with standard test attributes and assertion patterns optimized for data model validation.

### Rationale
- **NUnit 4.x** is the current stable version with full .NET 8 support
- **TestFixture attribute**: Marks test classes (optional in NUnit 4 but recommended for clarity)
- **Test attribute**: Marks individual test methods
- **Assert.That()**: Fluent assertion syntax is more readable than Assert.AreEqual()
- **SetUp/TearDown**: Use when tests need common initialization (minimal for model tests)

### Best Practices for Model Testing
1. **Naming Convention**: `[MethodName]_[Scenario]_[ExpectedBehavior]`
   - Example: `Success_WithValidValue_ShouldSetIsValidTrue`
   
2. **Arrange-Act-Assert Pattern**: Clear separation of test phases
   ```csharp
   [Test]
   public void Constructor_WithValidArguments_ShouldInitializeProperties()
   {
       // Arrange
       var path = "C:\\Test";
       var ruleType = RuleType.Include;
       
       // Act
       var rule = new IndexRule(path, ruleType, RuleSource.UserConfigured);
       
       // Assert
       Assert.That(rule.Path, Is.EqualTo(path));
       Assert.That(rule.RuleType, Is.EqualTo(ruleType));
   }
   ```

3. **One Assertion Per Concept**: Group related assertions, but test one logical concept per test
4. **Test Data Builders**: For complex objects, consider builder pattern
5. **Parameterized Tests**: Use `[TestCase]` for multiple similar scenarios

### Alternatives Considered
- **xUnit**: More modern, but NUnit is already standardized in constitution
- **MSTest**: Microsoft's framework, but less expressive assertion syntax

### References
- NUnit Documentation: https://docs.nunit.org/
- .NET Testing Best Practices: Microsoft Learn

---

## Research Task 2: Code Coverage Tooling for .NET 8

### Decision
Use built-in `dotnet test --collect:"XPlat Code Coverage"` with Coverlet for coverage collection, and ReportGenerator for visualization.

### Rationale
- **Coverlet**: Cross-platform code coverage tool integrated with dotnet CLI
- **XPlat Code Coverage**: Works consistently across Windows, Linux, macOS
- **ReportGenerator**: Converts coverage data to readable HTML reports
- **IDE Integration**: Visual Studio and VS Code support coverage visualization

### Implementation Steps
```powershell
# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Generate HTML report
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:Html
```

### Coverage Targets
- **Line Coverage**: Minimum 80% (per spec requirement FR-015)
- **Branch Coverage**: Aim for 70%+ for model validation logic
- **Focus Areas**: Factory methods, constructors, property initialization

### Alternatives Considered
- **Visual Studio Enterprise Coverage**: Requires Enterprise license, not portable
- **dotCover**: JetBrains tool, excellent but commercial
- **Coverlet is sufficient**: Free, open-source, good IDE integration

### References
- Coverlet Documentation: https://github.com/coverlet-coverage/coverlet
- Microsoft Code Coverage: https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-code-coverage

---

## Research Task 3: Test Naming Conventions and Organization Patterns

### Decision
Use `[ClassName]Tests.cs` file naming with grouped tests by method/property using nested test classes or comments.

### Rationale
- **File Naming**: `ValidationResultTests.cs` clearly maps to `ValidationResult.cs`
- **Class Naming**: `public class ValidationResultTests` matches file name
- **Namespace Alignment**: Mirror source namespace structure
  - Source: `WindowsSearchConfigurator.Core.Models`
  - Tests: `WindowsSearchConfigurator.UnitTests.Core.Models`

### Test Organization Patterns

**Option A: Single Test Class (Recommended for Simple Models)**
```csharp
namespace WindowsSearchConfigurator.UnitTests.Core.Models;

[TestFixture]
public class ValidationResultTests
{
    #region Success Factory Tests
    
    [Test]
    public void Success_WithValidValue_ShouldSetIsValidTrue() { }
    
    [Test]
    public void Success_WithValidValue_ShouldSetNormalizedValue() { }
    
    #endregion
    
    #region Failure Factory Tests
    
    [Test]
    public void Failure_WithErrorMessage_ShouldSetIsValidFalse() { }
    
    #endregion
}
```

**Option B: Nested Test Classes (For Complex Models)**
```csharp
[TestFixture]
public class IndexRuleTests
{
    [TestFixture]
    public class ConstructorTests
    {
        [Test]
        public void DefaultConstructor_ShouldGenerateNewGuid() { }
    }
    
    [TestFixture]
    public class PropertyTests
    {
        [Test]
        public void FileTypeFilters_ShouldInitializeAsEmptyList() { }
    }
}
```

### Method Naming Convention
Format: `[MethodUnderTest]_[Scenario]_[ExpectedResult]`

Examples:
- `Success_WithValidValue_ShouldSetIsValidTrue`
- `Constructor_WithNullPath_ShouldThrowArgumentNullException`
- `FileTypeFilters_WhenAccessed_ShouldNotBeNull`

### Alternatives Considered
- **Should/When/Then**: More BDD-style, less standard in C# community
- **Plain English**: Less structured, harder to parse
- **Underscore convention is clearest**: Widely adopted in .NET testing

### References
- Roy Osherove's "The Art of Unit Testing" naming conventions
- Microsoft Testing Best Practices

---

## Research Task 4: Testing Patterns for Factory Methods and Immutable Objects

### Decision
Use object initializer syntax verification for immutable objects and dedicated factory method tests that validate all properties.

### Rationale
- **Factory Methods**: Static methods that return pre-configured instances (e.g., `ValidationResult.Success()`)
- **Immutable Objects**: Properties with `init` keyword (e.g., `ValidationResult`)
- **Testing Strategy**: Verify factory methods set all expected properties correctly

### Testing Patterns

**Pattern 1: Factory Method Testing**
```csharp
[Test]
public void Success_WithNormalizedValue_ShouldSetAllPropertiesCorrectly()
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
```

**Pattern 2: Init-Only Property Testing**
```csharp
[Test]
public void Properties_WithInitSyntax_ShouldBeSettable()
{
    // Arrange & Act
    var result = new ValidationResult
    {
        IsValid = true,
        NormalizedValue = "test",
        Severity = ValidationSeverity.None
    };
    
    // Assert
    Assert.That(result.IsValid, Is.True);
    Assert.That(result.NormalizedValue, Is.EqualTo("test"));
}
```

**Pattern 3: Generic Type Testing (OperationResult<T>)**
```csharp
[Test]
public void Ok_WithStringValue_ShouldStoreValueCorrectly()
{
    // Arrange
    var expectedValue = "test value";
    
    // Act
    var result = OperationResult<string>.Ok(expectedValue);
    
    // Assert
    Assert.That(result.Success, Is.True);
    Assert.That(result.Value, Is.EqualTo(expectedValue));
}

[Test]
public void Ok_WithReferenceType_ShouldStoreReference()
{
    // Arrange
    var rule = new IndexRule("C:\\Test", RuleType.Include, RuleSource.UserConfigured);
    
    // Act
    var result = OperationResult<IndexRule>.Ok(rule);
    
    // Assert
    Assert.That(result.Value, Is.SameAs(rule));
}
```

**Pattern 4: Constructor Auto-Initialization Testing**
```csharp
[Test]
public void Constructor_ShouldAutoGenerateGuid()
{
    // Act
    var rule1 = new IndexRule();
    var rule2 = new IndexRule();
    
    // Assert
    Assert.That(rule1.Id, Is.Not.EqualTo(Guid.Empty));
    Assert.That(rule2.Id, Is.Not.EqualTo(Guid.Empty));
    Assert.That(rule1.Id, Is.Not.EqualTo(rule2.Id));
}

[Test]
public void Constructor_ShouldSetCreatedDateToUtcNow()
{
    // Arrange
    var beforeCreation = DateTime.UtcNow;
    
    // Act
    var rule = new IndexRule();
    var afterCreation = DateTime.UtcNow;
    
    // Assert
    Assert.That(rule.CreatedDate, Is.GreaterThanOrEqualTo(beforeCreation));
    Assert.That(rule.CreatedDate, Is.LessThanOrEqualTo(afterCreation));
}
```

### Edge Cases to Test
1. **Null Values**: For nullable properties, verify they accept null
2. **Empty Collections**: Verify collections initialize as empty lists (not null)
3. **Default Values**: Test default constructor behavior
4. **Generic Types**: Test both value types and reference types
5. **DateTime Precision**: Use range assertions for timestamps

### Alternatives Considered
- **Record Types**: C# records provide immutability, but existing code uses classes with init
- **FluentAssertions**: More expressive assertions, but adds dependency (NUnit's Is.* is sufficient)
- **AutoFixture**: Auto-generates test data, but overkill for simple models

### References
- C# Init-Only Setters: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/init
- Testing Immutable Objects: Martin Fowler's patterns
- Generic Testing Patterns: xUnit Patterns book

---

## Research Task 5: .NET 8 Testing Framework Updates

### Decision
Use NUnit 4.x features compatible with .NET 8, including improved async support and modern C# features.

### Rationale
- **NUnit 4.x**: Fully compatible with .NET 8, C# 12, and init properties
- **Async Tests**: Full support for async/await (not needed for model tests)
- **Nullable Reference Types**: NUnit respects nullable annotations
- **Required Properties**: C# 11+ required keyword is supported

### .NET 8 Specific Considerations
1. **Required Properties**: Models use `required` keyword, ensure tests set required properties
2. **Nullable Annotations**: Tests should respect nullable reference type contracts
3. **File-Scoped Namespaces**: Use in test files for cleaner syntax
4. **Global Usings**: Can centralize common using statements

### Example Test File Structure
```csharp
// Global usings in separate file (optional)
// GlobalUsings.cs:
global using NUnit.Framework;
global using WindowsSearchConfigurator.Core.Models;

// Test file with file-scoped namespace:
namespace WindowsSearchConfigurator.UnitTests.Core.Models;

[TestFixture]
public class ValidationResultTests
{
    // Tests here
}
```

### Package References Required
```xml
<ItemGroup>
  <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
  <PackageReference Include="NUnit" Version="4.0.1" />
  <PackageReference Include="NUnit3TestAdapter" Version="4.5.0" />
  <PackageReference Include="coverlet.collector" Version="6.0.0" />
</ItemGroup>
```

### Alternatives Considered
- **NUnit 3.x**: Older, but NUnit 4 is stable and recommended
- **xUnit with .NET 8**: Would require constitution change

---

## Summary of Key Decisions

| Area | Decision | Rationale |
|------|----------|-----------|
| Test Framework | NUnit 4.x | Constitution mandated, .NET 8 compatible |
| Coverage Tool | Coverlet + ReportGenerator | Cross-platform, free, integrated |
| File Naming | `[ClassName]Tests.cs` | Clear mapping to source |
| Method Naming | `Method_Scenario_Expected` | Standard C# convention |
| Organization | Mirror source structure | Easy navigation, clear correlation |
| Factory Testing | Verify all properties set | Ensures correctness of convenience methods |
| Immutability | Use object initializers in tests | Respects init-only properties |
| Coverage Target | 80% minimum | Per FR-015 requirement |
| Batch Size | 3-5 test files per batch | Respects constitution principle VI |

---

## Implementation Readiness

All research questions have been resolved. No "NEEDS CLARIFICATION" items remain. Ready to proceed to Phase 1: Design & Contracts.

**Next Steps**:
1. Create data-model.md documenting model structures being tested
2. Create contracts/ directory with test schemas if applicable
3. Create quickstart.md with examples of running tests
4. Update agent context with NUnit 4.x information
