using WindowsSearchConfigurator.Core.Models;

namespace WindowsSearchConfigurator.UnitTests.Core.Models;

/// <summary>
/// Unit tests for the RegistrationOptions class.
/// Tests property behavior and validation logic.
/// </summary>
[TestFixture]
public class RegistrationOptionsTests
{
    [Test]
    public void DefaultConstructor_ShouldSetDefaultValues()
    {
        // Act
        var options = new RegistrationOptions();

        // Assert
        Assert.That(options.AutoRegister, Is.False);
        Assert.That(options.NoRegister, Is.False);
        Assert.That(options.Silent, Is.False);
        Assert.That(options.TimeoutSeconds, Is.EqualTo(5));
        Assert.That(options.DLLPath, Is.Null);
    }

    [Test]
    public void Properties_CanBeSetAndRetrieved()
    {
        // Act
        var options = new RegistrationOptions
        {
            AutoRegister = true,
            Silent = true,
            TimeoutSeconds = 10,
            DLLPath = "C:\\Custom\\searchapi.dll"
        };

        // Assert
        Assert.That(options.AutoRegister, Is.True);
        Assert.That(options.Silent, Is.True);
        Assert.That(options.TimeoutSeconds, Is.EqualTo(10));
        Assert.That(options.DLLPath, Is.EqualTo("C:\\Custom\\searchapi.dll"));
    }

    [Test]
    public void Validate_WithValidOptions_ShouldNotThrow()
    {
        // Arrange
        var options = new RegistrationOptions { AutoRegister = true, TimeoutSeconds = 10 };

        // Act & Assert
        Assert.DoesNotThrow(() => options.Validate());
    }

    [Test]
    public void Validate_WithBothAutoAndNoRegister_ShouldThrow()
    {
        // Arrange
        var options = new RegistrationOptions { AutoRegister = true, NoRegister = true };

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.That(ex!.Message, Does.Contain("mutually exclusive"));
    }

    [Test]
    public void Validate_WithZeroTimeout_ShouldThrow()
    {
        // Arrange
        var options = new RegistrationOptions { TimeoutSeconds = 0 };

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.That(ex!.Message, Does.Contain("between 1 and 60"));
    }

    [Test]
    public void Validate_WithNegativeTimeout_ShouldThrow()
    {
        // Arrange
        var options = new RegistrationOptions { TimeoutSeconds = -5 };

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.That(ex!.Message, Does.Contain("between 1 and 60"));
    }

    [Test]
    public void Validate_WithTimeoutAbove60_ShouldThrow()
    {
        // Arrange
        var options = new RegistrationOptions { TimeoutSeconds = 61 };

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.That(ex!.Message, Does.Contain("between 1 and 60"));
    }
}
