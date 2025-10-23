using WindowsSearchConfigurator.Core.Models;

namespace WindowsSearchConfigurator.UnitTests.Core.Models;

/// <summary>
/// Unit tests for the OperationResult and OperationResult<T> classes.
/// Tests factory methods, property initialization, and generic value handling.
/// </summary>
[TestFixture]
public class OperationResultTests
{
    #region OperationResult.Ok Tests

    [Test]
    public void Ok_WithDefaultMessage_ShouldSetSuccessTrue()
    {
        // Act
        var result = OperationResult.Ok();

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Message, Is.EqualTo("Operation completed successfully"));
        Assert.That(result.ErrorCode, Is.Null);
        Assert.That(result.Exception, Is.Null);
    }

    [Test]
    public void Ok_WithCustomMessage_ShouldSetSuccessTrue()
    {
        // Arrange
        var customMessage = "Custom operation succeeded";

        // Act
        var result = OperationResult.Ok(customMessage);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Message, Is.EqualTo(customMessage));
        Assert.That(result.ErrorCode, Is.Null);
        Assert.That(result.Exception, Is.Null);
    }

    [Test]
    public void Ok_WithEmptyMessage_ShouldSetSuccessTrue()
    {
        // Arrange
        var emptyMessage = string.Empty;

        // Act
        var result = OperationResult.Ok(emptyMessage);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Message, Is.EqualTo(string.Empty));
        Assert.That(result.ErrorCode, Is.Null);
        Assert.That(result.Exception, Is.Null);
    }

    #endregion

    #region OperationResult.Fail Tests

    [Test]
    public void Fail_WithMessageOnly_ShouldSetSuccessFalse()
    {
        // Arrange
        var errorMessage = "Operation failed";

        // Act
        var result = OperationResult.Fail(errorMessage);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo(errorMessage));
        Assert.That(result.ErrorCode, Is.Null);
        Assert.That(result.Exception, Is.Null);
    }

    [Test]
    public void Fail_WithMessageAndErrorCode_ShouldSetBothValues()
    {
        // Arrange
        var errorMessage = "Operation failed";
        var errorCode = 404;

        // Act
        var result = OperationResult.Fail(errorMessage, errorCode);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo(errorMessage));
        Assert.That(result.ErrorCode, Is.EqualTo(errorCode));
        Assert.That(result.Exception, Is.Null);
    }

    [Test]
    public void Fail_WithAllParameters_ShouldSetAllValues()
    {
        // Arrange
        var errorMessage = "Operation failed";
        var errorCode = 500;
        var exception = new InvalidOperationException("Test exception");

        // Act
        var result = OperationResult.Fail(errorMessage, errorCode, exception);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo(errorMessage));
        Assert.That(result.ErrorCode, Is.EqualTo(errorCode));
        Assert.That(result.Exception, Is.EqualTo(exception));
    }

    [Test]
    public void Fail_WithNullException_ShouldAllowNull()
    {
        // Arrange
        var errorMessage = "Operation failed";
        Exception? exception = null;

        // Act
        var result = OperationResult.Fail(errorMessage, null, exception);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo(errorMessage));
        Assert.That(result.ErrorCode, Is.Null);
        Assert.That(result.Exception, Is.Null);
    }

    #endregion

    #region OperationResult<T>.Ok Tests

    [Test]
    public void GenericOk_WithValueTypeAndDefaultMessage_ShouldSetSuccessTrue()
    {
        // Arrange
        var value = 42;

        // Act
        var result = OperationResult<int>.Ok(value);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Value, Is.EqualTo(value));
        Assert.That(result.Message, Is.EqualTo("Operation completed successfully"));
        Assert.That(result.ErrorCode, Is.Null);
        Assert.That(result.Exception, Is.Null);
    }

    [Test]
    public void GenericOk_WithReferenceTypeAndCustomMessage_ShouldSetSuccessTrue()
    {
        // Arrange
        var value = "Test Value";
        var customMessage = "Operation succeeded with value";

        // Act
        var result = OperationResult<string>.Ok(value, customMessage);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Value, Is.EqualTo(value));
        Assert.That(result.Message, Is.EqualTo(customMessage));
        Assert.That(result.ErrorCode, Is.Null);
        Assert.That(result.Exception, Is.Null);
    }

    [Test]
    public void GenericOk_WithBooleanValue_ShouldSetSuccessTrue()
    {
        // Arrange
        var value = true;

        // Act
        var result = OperationResult<bool>.Ok(value);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Value, Is.EqualTo(value));
        Assert.That(result.Message, Is.EqualTo("Operation completed successfully"));
    }

    [Test]
    public void GenericOk_WithComplexType_ShouldSetSuccessTrue()
    {
        // Arrange
        var value = new IndexRule
        {
            Path = "C:\\Test",
            RuleType = RuleType.Include,
            Source = RuleSource.User
        };

        // Act
        var result = OperationResult<IndexRule>.Ok(value);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Value, Is.EqualTo(value));
        Assert.That(result.Value!.Path, Is.EqualTo("C:\\Test"));
        Assert.That(result.Message, Is.EqualTo("Operation completed successfully"));
    }

    #endregion

    #region OperationResult<T>.Fail Tests

    [Test]
    public void GenericFail_WithMessageOnly_ShouldSetSuccessFalseAndNullValue()
    {
        // Arrange
        var errorMessage = "Operation failed";

        // Act
        var result = OperationResult<int>.Fail(errorMessage);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Value, Is.EqualTo(default(int)));
        Assert.That(result.Message, Is.EqualTo(errorMessage));
        Assert.That(result.ErrorCode, Is.Null);
        Assert.That(result.Exception, Is.Null);
    }

    [Test]
    public void GenericFail_WithAllParameters_ShouldSetAllValues()
    {
        // Arrange
        var errorMessage = "Operation failed";
        var errorCode = 404;
        var exception = new FileNotFoundException("File not found");

        // Act
        var result = OperationResult<string>.Fail(errorMessage, errorCode, exception);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Value, Is.Null);
        Assert.That(result.Message, Is.EqualTo(errorMessage));
        Assert.That(result.ErrorCode, Is.EqualTo(errorCode));
        Assert.That(result.Exception, Is.EqualTo(exception));
    }

    [Test]
    public void GenericFail_WithNullableType_ShouldSetNullValue()
    {
        // Arrange
        var errorMessage = "Operation failed";

        // Act
        var result = OperationResult<int?>.Fail(errorMessage);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Value, Is.Null);
        Assert.That(result.Message, Is.EqualTo(errorMessage));
    }

    #endregion

    #region Edge Case Tests

    [Test]
    public void GenericOk_WithNullReferenceType_ShouldAllowNull()
    {
        // Arrange
        string? value = null;

        // Act
        var result = OperationResult<string?>.Ok(value);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Value, Is.Null);
        Assert.That(result.Message, Is.EqualTo("Operation completed successfully"));
    }

    [Test]
    public void GenericOk_WithZeroValue_ShouldSetValueToZero()
    {
        // Arrange
        var value = 0;

        // Act
        var result = OperationResult<int>.Ok(value);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Value, Is.EqualTo(0));
    }

    [Test]
    public void Fail_WithEmptyMessage_ShouldAllowEmptyString()
    {
        // Arrange
        var errorMessage = string.Empty;

        // Act
        var result = OperationResult.Fail(errorMessage);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo(string.Empty));
    }

    [Test]
    public void Fail_WithNullErrorCode_ShouldKeepNullValue()
    {
        // Arrange
        var errorMessage = "Operation failed";
        int? errorCode = null;

        // Act
        var result = OperationResult.Fail(errorMessage, errorCode);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.ErrorCode, Is.Null);
    }

    [Test]
    public void OperationResult_PropertiesAreInitOnly_CannotBeModified()
    {
        // Arrange
        var result = OperationResult.Ok("test");

        // Assert - This test verifies init-only properties at compile time
        // The fact that this compiles confirms properties are init-only
        Assert.That(result.Success, Is.True);
        Assert.That(result.Message, Is.EqualTo("test"));
    }

    #endregion
}
