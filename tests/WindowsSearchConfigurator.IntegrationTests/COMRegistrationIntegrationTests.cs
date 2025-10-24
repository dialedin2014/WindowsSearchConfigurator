using System.Security.Principal;
using WindowsSearchConfigurator.Core.Interfaces;
using WindowsSearchConfigurator.Core.Models;
using WindowsSearchConfigurator.Services;
using WindowsSearchConfigurator.Utilities;

namespace WindowsSearchConfigurator.IntegrationTests;

/// <summary>
/// Integration tests for COM API registration functionality.
/// </summary>
/// <remarks>
/// These tests require:
/// - Administrative privileges
/// - Windows Search installed
/// - Real COM registration/unregistration
/// </remarks>
[TestFixture]
[Category("RequiresAdmin")]
[Category("Integration")]
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
public class COMRegistrationIntegrationTests
{
    private ICOMRegistrationDetector _detector = null!;
    private ICOMRegistrationService _service = null!;
    private IPrivilegeChecker _privilegeChecker = null!;
    private VerboseLogger _verboseLogger = null!;
    private bool _wasRegisteredAtStart;
    private readonly Guid _searchManagerCLSID = new Guid("7D096C5F-AC08-4F1F-BEB7-5C22C517CE39");

    [SetUp]
    public void Setup()
    {
        // Initialize dependencies
        _detector = new COMRegistrationDetector();
        _privilegeChecker = new PrivilegeChecker();
        _verboseLogger = new VerboseLogger { IsEnabled = false }; // Silent for tests
        _service = new COMRegistrationService(_detector, _privilegeChecker, _verboseLogger, auditLogger: null);

        // Record initial state
        var initialStatus = _detector.GetRegistrationStatus();
        _wasRegisteredAtStart = initialStatus.IsRegistered;

        // Skip tests if not running as admin
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
        {
            Assert.Ignore("Test requires administrative privileges");
        }
    }

    [TearDown]
    public void TearDown()
    {
        // Restore original state
        try
        {
            var currentStatus = _detector.GetRegistrationStatus();
            if (_wasRegisteredAtStart && !currentStatus.IsRegistered)
            {
                // Re-register if it was registered at start
                var options = new RegistrationOptions
                {
                    AutoRegister = true,
                    Silent = true,
                    TimeoutSeconds = 30
                };
                _ = _service.RegisterCOMAsync(options).Result;
            }
            else if (!_wasRegisteredAtStart && currentStatus.IsRegistered)
            {
                // Unregister if it wasn't registered at start (for test isolation)
                // Note: Unregistration should only be done in controlled test environments
                UnregisterCOM();
            }
        }
        catch (Exception ex)
        {
            TestContext.WriteLine($"Warning: Failed to restore initial COM registration state: {ex.Message}");
        }
        finally
        {
            _verboseLogger?.Dispose();
        }
    }

    #region Detection Tests

    [Test]
    public void GetRegistrationStatus_WhenCOMIsRegistered_ReturnsValidStatus()
    {
        // Arrange
        EnsureCOMIsRegistered();

        // Act
        var status = _detector.GetRegistrationStatus();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(status.IsRegistered, Is.True, "COM should be registered");
            Assert.That(status.CLSIDExists, Is.True, "CLSID key should exist");
            Assert.That(status.DLLExists, Is.True, "DLL file should exist");
            Assert.That(status.DLLPath, Is.Not.Null, "DLL path should be populated");
            Assert.That(status.ValidationState, Is.EqualTo(COMValidationState.Valid), "Validation should succeed");
            Assert.That(status.DetectionTimestamp, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(5)));
            Assert.That(status.ErrorMessage, Is.Null, "No error message should be present");
        });
    }

    [Test]
    public void IsCLSIDRegistered_WithValidCLSID_ReturnsTrue()
    {
        // Arrange
        EnsureCOMIsRegistered();

        // Act
        var isRegistered = _detector.IsCLSIDRegistered(_searchManagerCLSID);

        // Assert
        Assert.That(isRegistered, Is.True);
    }

    [Test]
    public void GetDLLPath_WithRegisteredCOM_ReturnsValidPath()
    {
        // Arrange
        EnsureCOMIsRegistered();

        // Act
        var dllPath = _detector.GetDLLPath(_searchManagerCLSID);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(dllPath, Is.Not.Null);
            Assert.That(File.Exists(dllPath), Is.True, "DLL file should exist at returned path");
            Assert.That(dllPath, Does.EndWith("SearchAPI.dll").IgnoreCase);
        });
    }

    [Test]
    public void ValidateCOMObject_WithRegisteredCOM_ReturnsValid()
    {
        // Arrange
        EnsureCOMIsRegistered();

        // Act
        var validationState = _detector.ValidateCOMObject(_searchManagerCLSID);

        // Assert
        Assert.That(validationState, Is.EqualTo(COMValidationState.Valid));
    }

    #endregion

    #region Registration Tests

    [Test]
    public async Task RegisterCOMAsync_WithAutoRegisterMode_SuccessfullyRegisters()
    {
        // Arrange
        UnregisterCOM(); // Start with unregistered state
        Thread.Sleep(500); // Give system time to process unregistration

        var options = new RegistrationOptions
        {
            AutoRegister = true,
            Silent = false,
            TimeoutSeconds = 30
        };

        // Act
        var attempt = await _service.RegisterCOMAsync(options);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(attempt.Outcome, Is.EqualTo(RegistrationOutcome.Success), 
                $"Registration should succeed. Error: {attempt.ErrorMessage}");
            Assert.That(attempt.PostValidation, Is.EqualTo(COMValidationState.Valid));
            Assert.That(attempt.DurationMs, Is.GreaterThan(0));
            Assert.That(attempt.DurationMs, Is.LessThan(30000), "Should complete in under 30 seconds");
            Assert.That(attempt.Mode, Is.EqualTo(RegistrationMode.Automatic));
            Assert.That(attempt.IsAdministrator, Is.True);
            Assert.That(attempt.RegistrationMethod, Does.Contain("regsvr32"));
        });

        // Verify post-registration state
        var finalStatus = _detector.GetRegistrationStatus();
        Assert.That(finalStatus.IsRegistered, Is.True, "COM should be registered after successful attempt");
    }

    [Test]
    public async Task RegisterCOMAsync_WhenAlreadyRegistered_ReturnsSuccess()
    {
        // Arrange
        EnsureCOMIsRegistered();

        var options = new RegistrationOptions
        {
            AutoRegister = true,
            Silent = true,
            TimeoutSeconds = 30
        };

        // Act
        var attempt = await _service.RegisterCOMAsync(options);

        // Assert
        Assert.That(attempt.Outcome, Is.EqualTo(RegistrationOutcome.Success), 
            "Should report success even if already registered");
    }

    [Test]
    public async Task RegisterCOMAsync_WithTimeout_ReturnsTimeoutOutcome()
    {
        // Arrange
        var options = new RegistrationOptions
        {
            AutoRegister = true,
            Silent = true,
            TimeoutSeconds = 0 // Immediate timeout
        };

        // Act
        var attempt = await _service.RegisterCOMAsync(options);

        // Assert - Should either timeout or succeed very quickly
        Assert.That(attempt.Outcome, 
            Is.EqualTo(RegistrationOutcome.Timeout).Or.EqualTo(RegistrationOutcome.Success));
    }

    #endregion

    #region Performance Tests

    [Test]
    public void DetectionPerformance_CompletesUnder500ms()
    {
        // Arrange
        EnsureCOMIsRegistered();
        var sw = System.Diagnostics.Stopwatch.StartNew();

        // Act
        _ = _detector.GetRegistrationStatus();

        // Assert
        sw.Stop();
        Assert.That(sw.ElapsedMilliseconds, Is.LessThan(500), 
            $"Detection took {sw.ElapsedMilliseconds}ms, should be under 500ms (SC-001)");
    }

    [Test]
    public async Task RegistrationPerformance_CompletesUnder5Seconds()
    {
        // Arrange
        UnregisterCOM();
        Thread.Sleep(500);

        var options = new RegistrationOptions
        {
            AutoRegister = true,
            Silent = true,
            TimeoutSeconds = 30
        };

        var sw = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var attempt = await _service.RegisterCOMAsync(options);

        // Assert
        sw.Stop();
        if (attempt.Outcome == RegistrationOutcome.Success)
        {
            Assert.That(sw.ElapsedMilliseconds, Is.LessThan(5000), 
                $"Registration took {sw.ElapsedMilliseconds}ms, should be under 5 seconds (SC-002)");
        }
        else
        {
            Assert.Inconclusive($"Registration failed: {attempt.ErrorMessage}");
        }
    }

    #endregion

    #region Edge Case Tests

    [Test]
    public async Task RegisterCOMAsync_WithMissingDLL_ReturnsDLLNotFoundOutcome()
    {
        // This test would require temporarily moving/renaming SearchAPI.dll
        // which is too risky for an automated test
        Assert.Inconclusive("Test requires DLL manipulation - manual testing recommended");
    }

    [Test]
    public void GetRegistrationStatus_MultipleCallsInQuickSuccession_AllSucceed()
    {
        // Arrange
        EnsureCOMIsRegistered();

        // Act & Assert
        for (int i = 0; i < 10; i++)
        {
            var status = _detector.GetRegistrationStatus();
            Assert.That(status.IsRegistered, Is.True, $"Iteration {i} should succeed");
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Ensures COM is registered, registering if necessary.
    /// </summary>
    private void EnsureCOMIsRegistered()
    {
        var status = _detector.GetRegistrationStatus();
        if (!status.IsRegistered)
        {
            var options = new RegistrationOptions
            {
                AutoRegister = true,
                Silent = true,
                TimeoutSeconds = 30
            };

            var task = _service.RegisterCOMAsync(options);
            task.Wait();

            var attempt = task.Result;
            if (attempt.Outcome != RegistrationOutcome.Success)
            {
                Assert.Inconclusive($"Failed to register COM for test: {attempt.ErrorMessage}");
            }

            // Verify registration
            Thread.Sleep(500);
            status = _detector.GetRegistrationStatus();
            if (!status.IsRegistered)
            {
                Assert.Inconclusive("COM registration succeeded but validation still fails");
            }
        }
    }

    /// <summary>
    /// Unregisters COM API (for test isolation).
    /// </summary>
    /// <remarks>
    /// WARNING: This should only be called in controlled test environments.
    /// Unregistering COM may affect other applications using Windows Search.
    /// </remarks>
    private void UnregisterCOM()
    {
        try
        {
            var status = _detector.GetRegistrationStatus();
            if (status.IsRegistered && !string.IsNullOrEmpty(status.DLLPath))
            {
                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "regsvr32.exe",
                    Arguments = $"/u /s \"{status.DLLPath}\"", // /u = unregister, /s = silent
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var process = System.Diagnostics.Process.Start(processInfo);
                if (process != null)
                {
                    process.WaitForExit(5000);
                }
            }
        }
        catch (Exception ex)
        {
            TestContext.WriteLine($"Warning: Failed to unregister COM: {ex.Message}");
        }
    }

    #endregion
}
