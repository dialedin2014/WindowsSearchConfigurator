using WindowsSearchConfigurator.Core.Models;

namespace WindowsSearchConfigurator.UnitTests.Core.Models;

/// <summary>
/// Unit tests for the COMRegistrationStatus class.
/// Tests property behavior and relationships.
/// </summary>
[TestFixture]
public class COMRegistrationStatusTests
{
    [Test]
    public void DefaultConstructor_ShouldSetDefaultValues()
    {
        // Act
        var status = new COMRegistrationStatus();

        // Assert
        Assert.That(status.IsRegistered, Is.False);
        Assert.That(status.CLSIDExists, Is.False);
        Assert.That(status.DLLExists, Is.False);
        Assert.That(status.DLLPath, Is.Null);
        Assert.That(status.ErrorMessage, Is.Null);
    }

    [Test]
    public void Properties_CanBeSetAndRetrieved()
    {
        // Act
        var status = new COMRegistrationStatus
        {
            IsRegistered = true,
            CLSIDExists = true,
            DLLPath = "C:\\Windows\\System32\\searchapi.dll",
            DLLExists = true,
            ValidationState = COMValidationState.Valid,
            DetectionTimestamp = DateTime.UtcNow
        };

        // Assert
        Assert.That(status.IsRegistered, Is.True);
        Assert.That(status.CLSIDExists, Is.True);
        Assert.That(status.DLLPath, Is.EqualTo("C:\\Windows\\System32\\searchapi.dll"));
        Assert.That(status.DLLExists, Is.True);
        Assert.That(status.ValidationState, Is.EqualTo(COMValidationState.Valid));
    }

    [Test]
    public void ErrorMessage_CanBeNull()
    {
        // Act
        var status = new COMRegistrationStatus();

        // Assert
        Assert.That(status.ErrorMessage, Is.Null);
    }

    [Test]
    public void ErrorMessage_CanBeSet()
    {
        // Arrange
        var status = new COMRegistrationStatus();
        var error = "COM registration failed";

        // Act
        status.ErrorMessage = error;

        // Assert
        Assert.That(status.ErrorMessage, Is.EqualTo(error));
    }
}
