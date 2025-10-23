using WindowsSearchConfigurator.Core.Models;

namespace WindowsSearchConfigurator.UnitTests.Core.Models;

/// <summary>
/// Unit tests for the COMRegistrationAttempt class.
/// Tests property initialization and Guid uniqueness.
/// </summary>
[TestFixture]
public class COMRegistrationAttemptTests
{
    [Test]
    public void AttemptId_ShouldBeUniqueAcrossInstances()
    {
        // Act
        var attempt1 = new COMRegistrationAttempt { AttemptId = Guid.NewGuid() };
        var attempt2 = new COMRegistrationAttempt { AttemptId = Guid.NewGuid() };

        // Assert
        Assert.That(attempt1.AttemptId, Is.Not.EqualTo(attempt2.AttemptId));
    }

    [Test]
    public void DefaultConstructor_ShouldSetDefaultValues()
    {
        // Act
        var attempt = new COMRegistrationAttempt();

        // Assert
        Assert.That(attempt.User, Is.EqualTo(string.Empty));
        Assert.That(attempt.DLLPath, Is.EqualTo(string.Empty));
        Assert.That(attempt.RegistrationMethod, Is.EqualTo("regsvr32"));
    }

    [Test]
    public void Properties_CanBeSetAndRetrieved()
    {
        // Arrange
        var attempt = new COMRegistrationAttempt
        {
            AttemptId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Mode = RegistrationMode.Automatic,
            User = "TestUser",
            IsAdministrator = true,
            DLLPath = "C:\\Windows\\System32\\searchapi.dll",
            Outcome = RegistrationOutcome.Success
        };

        // Assert
        Assert.That(attempt.AttemptId, Is.Not.EqualTo(Guid.Empty));
        Assert.That(attempt.Mode, Is.EqualTo(RegistrationMode.Automatic));
        Assert.That(attempt.User, Is.EqualTo("TestUser"));
        Assert.That(attempt.IsAdministrator, Is.True);
        Assert.That(attempt.DLLPath, Is.EqualTo("C:\\Windows\\System32\\searchapi.dll"));
        Assert.That(attempt.Outcome, Is.EqualTo(RegistrationOutcome.Success));
    }

    [Test]
    public void AttemptId_GuidEmpty_ShouldBeAllowed()
    {
        // Act
        var attempt = new COMRegistrationAttempt { AttemptId = Guid.Empty };

        // Assert
        Assert.That(attempt.AttemptId, Is.EqualTo(Guid.Empty));
    }

    [Test]
    public void RegistrationMethod_CanBeModified()
    {
        // Arrange
        var attempt = new COMRegistrationAttempt();

        // Act
        attempt.RegistrationMethod = "manual";

        // Assert
        Assert.That(attempt.RegistrationMethod, Is.EqualTo("manual"));
    }
}
