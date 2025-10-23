using NUnit.Framework;
using WindowsSearchConfigurator.Core.Models;
using WindowsSearchConfigurator.Infrastructure;

namespace WindowsSearchConfigurator.UnitTests.Infrastructure;

[TestFixture]
public class WindowsSearchInteropTests
{
    private WindowsSearchInterop _interop = null!;

    [SetUp]
    public void Setup()
    {
        _interop = new WindowsSearchInterop();
    }

    [Test]
    public void IsAvailable_ReturnsBoolean()
    {
        // Act
        var result = _interop.IsAvailable();

        // Assert
        Assert.That(result, Is.TypeOf<bool>());
    }

    [Test]
    public void ValidateCOMRegistration_ReturnsValidationState()
    {
        // Act
        var result = _interop.ValidateCOMRegistration();

        // Assert
        Assert.That(result, Is.TypeOf<COMValidationState>());
        Assert.That(Enum.IsDefined(typeof(COMValidationState), result), Is.True);
    }

    [Test]
    [Platform("Win")]
    public void ValidateCOMRegistration_OnWindows_ReturnsNonUnknownOrValid()
    {
        // On Windows, we should get a definitive result (not UnknownError)
        // It will be either Valid (if COM is registered) or a specific error state
        
        // Act
        var result = _interop.ValidateCOMRegistration();

        // Assert - On Windows, should be a meaningful result
        Assert.That(result, Is.Not.EqualTo(COMValidationState.NotChecked));
    }

    [Test]
    public void IsAvailable_DoesNotThrowException()
    {
        // Act & Assert
        Assert.DoesNotThrow(() =>
        {
            var result = _interop.IsAvailable();
        });
    }

    [Test]
    public void ValidateCOMRegistration_DoesNotThrowException()
    {
        // Act & Assert
        Assert.DoesNotThrow(() =>
        {
            var result = _interop.ValidateCOMRegistration();
        });
    }

    [Test]
    [Platform("Win")]
    public void IsAvailable_OnWindows_MatchesValidationResult()
    {
        // If IsAvailable returns true, ValidateCOMRegistration should return Valid
        // This tests consistency between the two methods
        
        // Act
        var isAvailable = _interop.IsAvailable();
        var validation = _interop.ValidateCOMRegistration();

        // Assert
        if (isAvailable)
        {
            Assert.That(validation, Is.EqualTo(COMValidationState.Valid));
        }
        else
        {
            Assert.That(validation, Is.Not.EqualTo(COMValidationState.Valid));
        }
    }
}
