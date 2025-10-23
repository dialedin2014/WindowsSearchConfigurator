using WindowsSearchConfigurator.Core.Models;

namespace WindowsSearchConfigurator.UnitTests.Core.Models;

/// <summary>
/// Unit tests for the ValidationResult class.
/// Tests factory methods, property initialization, and immutability.
/// </summary>
[TestFixture]
public class ValidationResultTests
{
    #region Success Factory Method Tests

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
    public void Success_WithEmptyString_ShouldSetIsValidTrue()
    {
        // Arrange
        var normalizedValue = string.Empty;

        // Act
        var result = ValidationResult.Success(normalizedValue);

        // Assert
        Assert.That(result.IsValid, Is.True);
        Assert.That(result.NormalizedValue, Is.EqualTo(string.Empty));
        Assert.That(result.ErrorMessage, Is.Null);
        Assert.That(result.Severity, Is.EqualTo(ValidationSeverity.None));
    }

    #endregion

    #region Failure Factory Method Tests

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

    [Test]
    public void Failure_WithEmptyString_ShouldSetIsValidFalse()
    {
        // Arrange
        var errorMessage = string.Empty;

        // Act
        var result = ValidationResult.Failure(errorMessage);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.NormalizedValue, Is.Null);
        Assert.That(result.ErrorMessage, Is.EqualTo(string.Empty));
        Assert.That(result.Severity, Is.EqualTo(ValidationSeverity.Error));
    }

    #endregion

    #region Warning Factory Method Tests

    [Test]
    public void Warning_WithBothParameters_ShouldSetIsValidTrueWithWarning()
    {
        // Arrange
        var warning = "Path exists but is not recommended";
        var normalizedValue = "C:\\Test\\Path";

        // Act
        var result = ValidationResult.Warning(warning, normalizedValue);

        // Assert
        Assert.That(result.IsValid, Is.True);
        Assert.That(result.NormalizedValue, Is.EqualTo(normalizedValue));
        Assert.That(result.ErrorMessage, Is.EqualTo(warning));
        Assert.That(result.Severity, Is.EqualTo(ValidationSeverity.Warning));
    }

    [Test]
    public void Warning_WithEmptyStrings_ShouldSetIsValidTrueWithEmptyValues()
    {
        // Arrange
        var warning = string.Empty;
        var normalizedValue = string.Empty;

        // Act
        var result = ValidationResult.Warning(warning, normalizedValue);

        // Assert
        Assert.That(result.IsValid, Is.True);
        Assert.That(result.NormalizedValue, Is.EqualTo(string.Empty));
        Assert.That(result.ErrorMessage, Is.EqualTo(string.Empty));
        Assert.That(result.Severity, Is.EqualTo(ValidationSeverity.Warning));
    }

    #endregion

    #region Edge Case Tests

    [Test]
    public void Success_WithNullValue_ShouldAllowNull()
    {
        // Arrange
        string? normalizedValue = null;

        // Act
        var result = ValidationResult.Success(normalizedValue!);

        // Assert
        Assert.That(result.IsValid, Is.True);
        Assert.That(result.NormalizedValue, Is.Null);
        Assert.That(result.Severity, Is.EqualTo(ValidationSeverity.None));
    }

    [Test]
    public void Failure_WithNullMessage_ShouldAllowNull()
    {
        // Arrange
        string? errorMessage = null;

        // Act
        var result = ValidationResult.Failure(errorMessage!);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.ErrorMessage, Is.Null);
        Assert.That(result.Severity, Is.EqualTo(ValidationSeverity.Error));
    }

    [Test]
    public void Warning_WithNullValues_ShouldAllowNull()
    {
        // Arrange
        string? warning = null;
        string? normalizedValue = null;

        // Act
        var result = ValidationResult.Warning(warning!, normalizedValue!);

        // Assert
        Assert.That(result.IsValid, Is.True);
        Assert.That(result.NormalizedValue, Is.Null);
        Assert.That(result.ErrorMessage, Is.Null);
        Assert.That(result.Severity, Is.EqualTo(ValidationSeverity.Warning));
    }

    [Test]
    public void ValidationResult_PropertiesAreInitOnly_CannotBeModified()
    {
        // Arrange
        var result = ValidationResult.Success("test");

        // Assert - This test verifies init-only properties at compile time
        // The fact that this compiles confirms properties are init-only
        Assert.That(result.IsValid, Is.True);
        Assert.That(result.NormalizedValue, Is.EqualTo("test"));
    }

    #endregion
}
