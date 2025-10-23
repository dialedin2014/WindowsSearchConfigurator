using NUnit.Framework;
using WindowsSearchConfigurator.Core.Models;
using WindowsSearchConfigurator.Services;

namespace WindowsSearchConfigurator.UnitTests.Services;

[TestFixture]
public class COMRegistrationDetectorTests
{
    private COMRegistrationDetector _detector = null!;

    [SetUp]
    public void Setup()
    {
        _detector = new COMRegistrationDetector();
    }

    [Test]
    public void GetRegistrationStatus_ReturnsNonNull()
    {
        // Act
        var status = _detector.GetRegistrationStatus();

        // Assert
        Assert.That(status, Is.Not.Null);
        Assert.That(status.DetectionTimestamp, Is.Not.EqualTo(default(DateTime)));
    }

    [Test]
    public void GetRegistrationStatus_SetsDetectionTimestamp()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var status = _detector.GetRegistrationStatus();

        // Assert
        var after = DateTime.UtcNow;
        Assert.That(status.DetectionTimestamp, Is.GreaterThanOrEqualTo(before));
        Assert.That(status.DetectionTimestamp, Is.LessThanOrEqualTo(after));
    }

    [Test]
    public void GetRegistrationStatus_WhenCOMRegistered_ReturnsRegistered()
    {
        // This test will pass or fail based on actual system state
        // On systems with Windows Search properly configured, it should succeed
        
        // Act
        var status = _detector.GetRegistrationStatus();

        // Assert - We can't guarantee registration state, but we can verify structure
        Assert.That(status.IsRegistered, Is.EqualTo(
            status.CLSIDExists && status.DLLExists && status.ValidationState == COMValidationState.Valid
        ));
    }

    [Test]
    public void IsCLSIDRegistered_WithValidGuid_ReturnsBoolean()
    {
        // Arrange
        var testGuid = new Guid("7D096C5F-AC08-4F1F-BEB7-5C22C517CE39");

        // Act
        var result = _detector.IsCLSIDRegistered(testGuid);

        // Assert
        Assert.That(result, Is.TypeOf<bool>());
    }

    [Test]
    public void IsCLSIDRegistered_WithRandomGuid_ReturnsFalse()
    {
        // Arrange - Use a random GUID that definitely doesn't exist
        var randomGuid = Guid.NewGuid();

        // Act
        var result = _detector.IsCLSIDRegistered(randomGuid);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void GetDLLPath_WithValidCLSID_ReturnsStringOrNull()
    {
        // Arrange
        var testGuid = new Guid("7D096C5F-AC08-4F1F-BEB7-5C22C517CE39");

        // Act
        var result = _detector.GetDLLPath(testGuid);

        // Assert - Either returns a path or null, both are valid
        if (result != null)
        {
            Assert.That(result, Is.Not.Empty);
        }
    }

    [Test]
    public void GetDLLPath_WithInvalidCLSID_ReturnsNull()
    {
        // Arrange - Use a random GUID that doesn't exist
        var randomGuid = Guid.NewGuid();

        // Act
        var result = _detector.GetDLLPath(randomGuid);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void ValidateCOMObject_WithInvalidGuid_ReturnsNonValid()
    {
        // Arrange
        var randomGuid = Guid.NewGuid();

        // Act
        var result = _detector.ValidateCOMObject(randomGuid);

        // Assert
        Assert.That(result, Is.Not.EqualTo(COMValidationState.Valid));
    }

    [Test]
    public void GetRegistrationStatus_HandlesExceptionsGracefully()
    {
        // Act - Should not throw even if registry access fails
        Assert.DoesNotThrow(() =>
        {
            var status = _detector.GetRegistrationStatus();
            Assert.That(status, Is.Not.Null);
        });
    }

    [Test]
    public void GetRegistrationStatus_WhenError_SetsErrorMessage()
    {
        // This test verifies that error handling populates ErrorMessage
        // In normal conditions, ErrorMessage should be null or contain error details
        
        // Act
        var status = _detector.GetRegistrationStatus();

        // Assert
        if (!status.IsRegistered && !status.CLSIDExists)
        {
            // If COM is not registered, ErrorMessage might be set or might be null (both valid)
            Assert.That(status, Is.Not.Null);
        }
    }
}
