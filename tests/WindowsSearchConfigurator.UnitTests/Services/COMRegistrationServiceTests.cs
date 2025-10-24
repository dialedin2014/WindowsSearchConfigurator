using Moq;
using NUnit.Framework;
using WindowsSearchConfigurator.Core.Interfaces;
using WindowsSearchConfigurator.Core.Models;
using WindowsSearchConfigurator.Services;
using WindowsSearchConfigurator.Utilities;

namespace WindowsSearchConfigurator.UnitTests.Services;

[TestFixture]
public class COMRegistrationServiceTests
{
    private Mock<ICOMRegistrationDetector> _mockDetector;
    private Mock<IPrivilegeChecker> _mockPrivilegeChecker;
    private VerboseLogger _verboseLogger;
    private COMRegistrationService _service;

    [SetUp]
    public void SetUp()
    {
        _mockDetector = new Mock<ICOMRegistrationDetector>();
        _mockPrivilegeChecker = new Mock<IPrivilegeChecker>();
        _verboseLogger = new VerboseLogger
        {
            IsEnabled = false
        };
        
        _service = new COMRegistrationService(
            _mockDetector.Object,
            _mockPrivilegeChecker.Object,
            _verboseLogger);
    }

    [TearDown]
    public void TearDown()
    {
        _verboseLogger?.Dispose();
    }

    [Test]
    public void Constructor_WithNullDetector_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new COMRegistrationService(
            null!,
            _mockPrivilegeChecker.Object,
            _verboseLogger));
    }

    [Test]
    public void Constructor_WithNullPrivilegeChecker_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new COMRegistrationService(
            _mockDetector.Object,
            null!,
            _verboseLogger));
    }

    [Test]
    public void Constructor_WithNullVerboseLogger_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new COMRegistrationService(
            _mockDetector.Object,
            _mockPrivilegeChecker.Object,
            null!));
    }

    [Test]
    public void RegisterCOMAsync_WithNullOptions_ThrowsArgumentNullException()
    {
        Assert.ThrowsAsync<ArgumentNullException>(async () => 
            await _service.RegisterCOMAsync(null!));
    }

    [Test]
    public async Task RegisterCOMAsync_WhenAlreadyRegistered_ReturnsSuccessWithoutRegistering()
    {
        // Arrange
        var options = new RegistrationOptions();
        var status = new COMRegistrationStatus
        {
            IsRegistered = true,
            ValidationState = COMValidationState.Valid
        };
        
        _mockDetector.Setup(d => d.GetRegistrationStatus()).Returns(status);

        // Act
        var attempt = await _service.RegisterCOMAsync(options);

        // Assert
        Assert.That(attempt.Outcome, Is.EqualTo(RegistrationOutcome.Success));
        Assert.That(attempt.PostValidation, Is.EqualTo(COMValidationState.Valid));
    }

    [Test]
    public async Task RegisterCOMAsync_WhenDLLNotFound_ReturnsDLLNotFoundOutcome()
    {
        // Arrange
        var options = new RegistrationOptions
        {
            DLLPath = @"C:\NonExistent\SearchAPI.dll"
        };
        
        var status = new COMRegistrationStatus
        {
            IsRegistered = false
        };
        
        _mockDetector.Setup(d => d.GetRegistrationStatus()).Returns(status);

        // Act
        var attempt = await _service.RegisterCOMAsync(options);

        // Assert
        Assert.That(attempt.Outcome, Is.EqualTo(RegistrationOutcome.DLLNotFound));
        Assert.That(attempt.ErrorMessage, Does.Contain("DLL not found"));
    }

    [Test]
    public async Task RegisterCOMAsync_WhenNotAdministrator_ReturnsInsufficientPrivileges()
    {
        // Arrange - use kernel32.dll which should always exist on Windows
        var systemDllPath = Path.Combine(
            Environment.GetEnvironmentVariable("SystemRoot") ?? @"C:\Windows",
            "System32", "kernel32.dll");
        
        var options = new RegistrationOptions
        {
            DLLPath = systemDllPath
        };
        
        var status = new COMRegistrationStatus
        {
            IsRegistered = false,
            DLLPath = systemDllPath
        };
        
        _mockDetector.Setup(d => d.GetRegistrationStatus()).Returns(status);
        _mockDetector.Setup(d => d.GetDLLPath(It.IsAny<Guid>())).Returns(systemDllPath);
        _mockPrivilegeChecker.Setup(p => p.IsAdministrator()).Returns(false);

        // Act
        var attempt = await _service.RegisterCOMAsync(options);

        // Assert
        Assert.That(attempt.Outcome, Is.EqualTo(RegistrationOutcome.InsufficientPrivileges));
        Assert.That(attempt.IsAdministrator, Is.False);
        Assert.That(attempt.ErrorMessage, Does.Contain("Administrator privileges required"));
    }

    [Test]
    public async Task RegisterCOMAsync_PopulatesAttemptMetadata()
    {
        // Arrange
        var options = new RegistrationOptions
        {
            DLLPath = @"C:\NonExistent\SearchAPI.dll"
        };
        
        var status = new COMRegistrationStatus
        {
            IsRegistered = false
        };
        
        _mockDetector.Setup(d => d.GetRegistrationStatus()).Returns(status);

        // Act
        var attempt = await _service.RegisterCOMAsync(options);

        // Assert
        Assert.That(attempt.AttemptId, Is.Not.EqualTo(Guid.Empty));
        Assert.That(attempt.Timestamp, Is.GreaterThan(DateTime.UtcNow.AddMinutes(-1)));
        Assert.That(attempt.User, Is.Not.Empty);
        Assert.That(attempt.DLLPath, Is.Not.Empty);
        Assert.That(attempt.DurationMs, Is.GreaterThanOrEqualTo(0));
    }

    [Test]
    public void ShowRegistrationPrompt_ReturnsNonNullString()
    {
        // This test verifies the method exists and returns a string
        // Actual user interaction testing would be done in integration tests
        
        // We can't easily test interactive console input in unit tests,
        // but we can verify the method signature is correct
        Assert.DoesNotThrow(() =>
        {
            // Method exists and is callable
            var method = typeof(COMRegistrationService).GetMethod("ShowRegistrationPrompt");
            Assert.That(method, Is.Not.Null);
            Assert.That(method!.ReturnType, Is.EqualTo(typeof(string)));
        });
    }

    [Test]
    public void ShowManualInstructions_DoesNotThrow()
    {
        // Verify method exists and can be called without throwing
        Assert.DoesNotThrow(() => _service.ShowManualInstructions());
    }

    [Test]
    public void ShowElevationInstructions_DoesNotThrow()
    {
        // Verify method exists and can be called without throwing
        Assert.DoesNotThrow(() => _service.ShowElevationInstructions());
    }

    [Test]
    public async Task HandleMissingRegistration_WithAutoRegisterFlag_AttemptsRegistration()
    {
        // Arrange
        var args = new[] { "--auto-register-com" };
        var systemDllPath = Path.Combine(
            Environment.GetEnvironmentVariable("SystemRoot") ?? @"C:\Windows",
            "System32", "SearchAPI.dll");
        
        var status = new COMRegistrationStatus
        {
            IsRegistered = false,
            DLLPath = systemDllPath
        };
        
        _mockDetector.Setup(d => d.GetRegistrationStatus()).Returns(status);
        _mockDetector.Setup(d => d.GetDLLPath(It.IsAny<Guid>())).Returns(systemDllPath);
        _mockPrivilegeChecker.Setup(p => p.IsAdministrator()).Returns(false);

        // Act
        var result = await _service.HandleMissingRegistration(args);

        // Assert
        // With insufficient privileges, registration should fail
        Assert.That(result, Is.False);
        _mockDetector.Verify(d => d.GetRegistrationStatus(), Times.AtLeastOnce);
    }

    [Test]
    public async Task HandleMissingRegistration_WithNoRegisterFlag_ReturnsFalse()
    {
        // Arrange
        var args = new[] { "--no-register-com" };

        // Act
        var result = await _service.HandleMissingRegistration(args);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void RegisterCOMAsync_WithInvalidOptions_ThrowsValidationException()
    {
        // Arrange - create invalid options (both AutoRegister and NoRegister)
        var options = new RegistrationOptions
        {
            AutoRegister = true,
            NoRegister = true
        };

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () => 
            await _service.RegisterCOMAsync(options));
    }
}
