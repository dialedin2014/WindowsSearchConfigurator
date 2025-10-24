using Moq;
using NUnit.Framework;
using WindowsSearchConfigurator.Core.Interfaces;
using WindowsSearchConfigurator.Core.Models;
using WindowsSearchConfigurator.Utilities;

namespace WindowsSearchConfigurator.UnitTests.Utilities;

[TestFixture]
public class VerboseLoggerTests
{
    private VerboseLogger _verboseLogger = null!;
    private Mock<IFileLogger> _mockFileLogger = null!;

    [SetUp]
    public void SetUp()
    {
        _verboseLogger = new VerboseLogger();
        _mockFileLogger = new Mock<IFileLogger>();
    }

    [TearDown]
    public void TearDown()
    {
        _verboseLogger?.Dispose();
    }

    [Test]
    public void IsEnabled_DefaultValue_IsFalse()
    {
        // Assert
        Assert.That(_verboseLogger.IsEnabled, Is.False);
    }

    [Test]
    public void IsEnabled_CanBeSet()
    {
        // Act
        _verboseLogger.IsEnabled = true;

        // Assert
        Assert.That(_verboseLogger.IsEnabled, Is.True);
    }

    [Test]
    public void InitializeFileLogging_WithValidParameters_ReturnsTrue()
    {
        // Arrange
        var logFilePath = "C:\\temp\\test.log";
        var commandLine = "test.exe --verbose";
        _mockFileLogger.Setup(f => f.Initialize(logFilePath)).Returns(true);
        _mockFileLogger.Setup(f => f.WriteSessionHeaderAsync(It.IsAny<LogSession>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = _verboseLogger.InitializeFileLogging(_mockFileLogger.Object, logFilePath, commandLine);

        // Assert
        Assert.That(result, Is.True);
        _mockFileLogger.Verify(f => f.Initialize(logFilePath), Times.Once);
        _mockFileLogger.Verify(f => f.WriteSessionHeaderAsync(It.IsAny<LogSession>()), Times.Once);
    }

    [Test]
    public void InitializeFileLogging_WhenInitializeFails_ReturnsFalse()
    {
        // Arrange
        var logFilePath = "C:\\invalid\\test.log";
        var commandLine = "test.exe";
        _mockFileLogger.Setup(f => f.Initialize(logFilePath)).Returns(false);

        // Act
        var result = _verboseLogger.InitializeFileLogging(_mockFileLogger.Object, logFilePath, commandLine);

        // Assert
        Assert.That(result, Is.False);
        _mockFileLogger.Verify(f => f.WriteSessionHeaderAsync(It.IsAny<LogSession>()), Times.Never);
    }

    [Test]
    public void InitializeFileLogging_CreatesSessionWithCorrectMetadata()
    {
        // Arrange
        var logFilePath = "C:\\temp\\test.log";
        var commandLine = "test.exe --verbose --option";
        LogSession? capturedSession = null;

        _mockFileLogger.Setup(f => f.Initialize(logFilePath)).Returns(true);
        _mockFileLogger.Setup(f => f.WriteSessionHeaderAsync(It.IsAny<LogSession>()))
            .Callback<LogSession>(s => capturedSession = s)
            .Returns(Task.CompletedTask);

        // Act
        _verboseLogger.InitializeFileLogging(_mockFileLogger.Object, logFilePath, commandLine);

        // Assert
        Assert.That(capturedSession, Is.Not.Null);
        Assert.That(capturedSession!.CommandLine, Is.EqualTo(commandLine));
        Assert.That(capturedSession.Status, Is.EqualTo(SessionStatus.InProgress));
        Assert.That(capturedSession.SessionId, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public void CompleteSession_WithSuccessExitCode_SetsStatusToSuccess()
    {
        // Arrange
        LogSession? capturedFooterSession = null;
        _mockFileLogger.Setup(f => f.Initialize(It.IsAny<string>())).Returns(true);
        _mockFileLogger.Setup(f => f.WriteSessionHeaderAsync(It.IsAny<LogSession>()))
            .Returns(Task.CompletedTask);
        _mockFileLogger.Setup(f => f.WriteSessionFooterAsync(It.IsAny<LogSession>()))
            .Callback<LogSession>(s => capturedFooterSession = s)
            .Returns(Task.CompletedTask);

        _verboseLogger.InitializeFileLogging(_mockFileLogger.Object, "test.log", "test.exe");

        // Act
        _verboseLogger.CompleteSession(0);

        // Assert
        Assert.That(capturedFooterSession, Is.Not.Null);
        Assert.That(capturedFooterSession!.ExitCode, Is.EqualTo(0));
        Assert.That(capturedFooterSession.Status, Is.EqualTo(SessionStatus.Success));
        Assert.That(capturedFooterSession.EndTime, Is.Not.Null);
    }

    [Test]
    public void CompleteSession_WithNonZeroExitCode_SetsStatusToFailed()
    {
        // Arrange
        LogSession? capturedFooterSession = null;
        _mockFileLogger.Setup(f => f.Initialize(It.IsAny<string>())).Returns(true);
        _mockFileLogger.Setup(f => f.WriteSessionHeaderAsync(It.IsAny<LogSession>()))
            .Returns(Task.CompletedTask);
        _mockFileLogger.Setup(f => f.WriteSessionFooterAsync(It.IsAny<LogSession>()))
            .Callback<LogSession>(s => capturedFooterSession = s)
            .Returns(Task.CompletedTask);

        _verboseLogger.InitializeFileLogging(_mockFileLogger.Object, "test.log", "test.exe");

        // Act
        _verboseLogger.CompleteSession(1);

        // Assert
        Assert.That(capturedFooterSession, Is.Not.Null);
        Assert.That(capturedFooterSession!.ExitCode, Is.EqualTo(1));
        Assert.That(capturedFooterSession.Status, Is.EqualTo(SessionStatus.Failed));
    }

    [Test]
    public void WriteLine_WhenNotEnabled_DoesNotWriteToFile()
    {
        // Arrange
        _verboseLogger.IsEnabled = false;
        _mockFileLogger.Setup(f => f.IsEnabled).Returns(true);

        // Act
        _verboseLogger.WriteLine("Test message");

        // Assert
        _mockFileLogger.Verify(f => f.WriteEntryAsync(It.IsAny<LogEntry>()), Times.Never);
    }

    [Test]
    public void WriteLine_WhenEnabledWithFileLogger_WritesToFile()
    {
        // Arrange
        _verboseLogger.IsEnabled = true;
        _mockFileLogger.Setup(f => f.IsEnabled).Returns(true);
        _mockFileLogger.Setup(f => f.Initialize(It.IsAny<string>())).Returns(true);
        _mockFileLogger.Setup(f => f.WriteSessionHeaderAsync(It.IsAny<LogSession>()))
            .Returns(Task.CompletedTask);
        _mockFileLogger.Setup(f => f.WriteEntryAsync(It.IsAny<LogEntry>()))
            .Returns(Task.CompletedTask);

        _verboseLogger.InitializeFileLogging(_mockFileLogger.Object, "test.log", "test.exe");

        // Act
        _verboseLogger.WriteLine("Test message");

        // Assert
        _mockFileLogger.Verify(f => f.WriteEntryAsync(It.Is<LogEntry>(e => 
            e.Message == "Test message" && 
            e.Severity == LogSeverity.Info &&
            e.Component == "General"
        )), Times.Once);
    }

    [Test]
    public void WriteError_WhenEnabledWithFileLogger_WritesToFile()
    {
        // Arrange
        _verboseLogger.IsEnabled = true;
        _mockFileLogger.Setup(f => f.IsEnabled).Returns(true);
        _mockFileLogger.Setup(f => f.Initialize(It.IsAny<string>())).Returns(true);
        _mockFileLogger.Setup(f => f.WriteSessionHeaderAsync(It.IsAny<LogSession>()))
            .Returns(Task.CompletedTask);
        _mockFileLogger.Setup(f => f.WriteEntryAsync(It.IsAny<LogEntry>()))
            .Returns(Task.CompletedTask);

        _verboseLogger.InitializeFileLogging(_mockFileLogger.Object, "test.log", "test.exe");

        // Act
        _verboseLogger.WriteError("Error message");

        // Assert
        _mockFileLogger.Verify(f => f.WriteEntryAsync(It.Is<LogEntry>(e => 
            e.Message == "Error message" && 
            e.Severity == LogSeverity.Error
        )), Times.Once);
    }

    [Test]
    public void WriteException_WhenEnabledWithFileLogger_WritesToFile()
    {
        // Arrange
        _verboseLogger.IsEnabled = true;
        _mockFileLogger.Setup(f => f.IsEnabled).Returns(true);
        _mockFileLogger.Setup(f => f.Initialize(It.IsAny<string>())).Returns(true);
        _mockFileLogger.Setup(f => f.WriteSessionHeaderAsync(It.IsAny<LogSession>()))
            .Returns(Task.CompletedTask);
        _mockFileLogger.Setup(f => f.WriteEntryAsync(It.IsAny<LogEntry>()))
            .Returns(Task.CompletedTask);

        _verboseLogger.InitializeFileLogging(_mockFileLogger.Object, "test.log", "test.exe");
        var exception = new InvalidOperationException("Test exception");

        // Act
        _verboseLogger.WriteException(exception);

        // Assert
        _mockFileLogger.Verify(f => f.WriteEntryAsync(It.Is<LogEntry>(e => 
            e.Severity == LogSeverity.Exception &&
            e.Message.Contains("InvalidOperationException") &&
            e.Message.Contains("Test exception") &&
            e.StackTrace != null
        )), Times.Once);
    }

    [Test]
    public void WriteOperation_WhenEnabledWithFileLogger_WritesToFile()
    {
        // Arrange
        _verboseLogger.IsEnabled = true;
        _mockFileLogger.Setup(f => f.IsEnabled).Returns(true);
        _mockFileLogger.Setup(f => f.Initialize(It.IsAny<string>())).Returns(true);
        _mockFileLogger.Setup(f => f.WriteSessionHeaderAsync(It.IsAny<LogSession>()))
            .Returns(Task.CompletedTask);
        _mockFileLogger.Setup(f => f.WriteEntryAsync(It.IsAny<LogEntry>()))
            .Returns(Task.CompletedTask);

        _verboseLogger.InitializeFileLogging(_mockFileLogger.Object, "test.log", "test.exe");

        // Act
        _verboseLogger.WriteOperation("RegistryRead", "HKLM\\Software\\Test");

        // Assert
        _mockFileLogger.Verify(f => f.WriteEntryAsync(It.Is<LogEntry>(e => 
            e.Severity == LogSeverity.Operation &&
            e.Component == "RegistryRead" &&
            e.Message == "HKLM\\Software\\Test"
        )), Times.Once);
    }

    [Test]
    public void Dispose_DisposesFileLogger()
    {
        // Arrange
        _mockFileLogger.Setup(f => f.Initialize(It.IsAny<string>())).Returns(true);
        _mockFileLogger.Setup(f => f.WriteSessionHeaderAsync(It.IsAny<LogSession>()))
            .Returns(Task.CompletedTask);
        _verboseLogger.InitializeFileLogging(_mockFileLogger.Object, "test.log", "test.exe");

        // Act
        _verboseLogger.Dispose();

        // Assert
        _mockFileLogger.Verify(f => f.Dispose(), Times.Once);
    }

    [Test]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Act & Assert
        Assert.DoesNotThrow(() =>
        {
            _verboseLogger.Dispose();
            _verboseLogger.Dispose();
            _verboseLogger.Dispose();
        });
    }
}
