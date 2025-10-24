using NUnit.Framework;
using WindowsSearchConfigurator.Core.Models;
using WindowsSearchConfigurator.Utilities;

namespace WindowsSearchConfigurator.UnitTests.Utilities;

[TestFixture]
public class FileLoggerWriteTests
{
    private string _testLogDirectory = null!;
    private FileLogger _fileLogger = null!;
    private string _logFilePath = null!;

    [SetUp]
    public void SetUp()
    {
        _testLogDirectory = Path.Combine(Path.GetTempPath(), $"FileLoggerWriteTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testLogDirectory);
        _logFilePath = Path.Combine(_testLogDirectory, "test.log");
        _fileLogger = new FileLogger();
        _fileLogger.Initialize(_logFilePath);
    }

    [TearDown]
    public void TearDown()
    {
        _fileLogger?.Dispose();
        
        if (Directory.Exists(_testLogDirectory))
        {
            try
            {
                Directory.Delete(_testLogDirectory, recursive: true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    [Test]
    public async Task WriteEntryAsync_WithValidEntry_WritesToFile()
    {
        // Arrange
        var entry = new LogEntry(
            DateTime.UtcNow,
            LogSeverity.Info,
            "TestComponent",
            "Test message"
        );

        // Act
        await _fileLogger.WriteEntryAsync(entry);
        _fileLogger.Dispose(); // Flush and close

        // Assert
        var content = await File.ReadAllTextAsync(_logFilePath);
        Assert.That(content, Does.Contain("TestComponent: Test message"));
        Assert.That(content, Does.Contain("[INFO]"));
    }

    [Test]
    public async Task WriteEntryAsync_WithExceptionSeverity_WritesStackTrace()
    {
        // Arrange
        var entry = new LogEntry(
            DateTime.UtcNow,
            LogSeverity.Exception,
            "TestComponent",
            "Exception occurred",
            "   at TestMethod()\n   at TestClass.Run()"
        );

        // Act
        await _fileLogger.WriteEntryAsync(entry);
        _fileLogger.Dispose();

        // Assert
        var content = await File.ReadAllTextAsync(_logFilePath);
        Assert.That(content, Does.Contain("[EXCEPTION]"));
        Assert.That(content, Does.Contain("Exception occurred"));
        Assert.That(content, Does.Contain("Stack Trace:"));
        Assert.That(content, Does.Contain("at TestMethod()"));
    }

    [Test]
    public async Task WriteSessionHeaderAsync_WritesCorrectFormat()
    {
        // Arrange
        var session = new LogSession
        {
            SessionId = Guid.NewGuid(),
            StartTime = new DateTime(2025, 10, 23, 14, 30, 0, DateTimeKind.Utc),
            CommandLine = "test.exe --verbose",
            UserName = "testuser",
            WorkingDirectory = "C:\\test",
            WindowsVersion = "Windows 11",
            RuntimeVersion = "8.0.0",
            Status = SessionStatus.InProgress
        };

        // Act
        await _fileLogger.WriteSessionHeaderAsync(session);
        _fileLogger.Dispose();

        // Assert
        var content = await File.ReadAllTextAsync(_logFilePath);
        Assert.Multiple(() =>
        {
            Assert.That(content, Does.Contain("LOG SESSION START"));
            Assert.That(content, Does.Contain($"Session ID: {session.SessionId}"));
            Assert.That(content, Does.Contain("Start Time: 2025-10-23T14:30:00.000Z"));
            Assert.That(content, Does.Contain("Command: test.exe --verbose"));
            Assert.That(content, Does.Contain("User: testuser"));
            Assert.That(content, Does.Contain("Working Directory: C:\\test"));
            Assert.That(content, Does.Contain("Windows Version: Windows 11"));
            Assert.That(content, Does.Contain(".NET Runtime: 8.0.0"));
        });
    }

    [Test]
    public async Task WriteSessionFooterAsync_WritesCorrectFormat()
    {
        // Arrange
        var session = new LogSession
        {
            SessionId = Guid.NewGuid(),
            StartTime = new DateTime(2025, 10, 23, 14, 30, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2025, 10, 23, 14, 30, 10, 234, DateTimeKind.Utc),
            CommandLine = "test.exe",
            ExitCode = 0,
            Status = SessionStatus.Success
        };

        // Act
        await _fileLogger.WriteSessionFooterAsync(session);
        _fileLogger.Dispose();

        // Assert
        var content = await File.ReadAllTextAsync(_logFilePath);
        Assert.Multiple(() =>
        {
            Assert.That(content, Does.Contain("LOG SESSION END"));
            Assert.That(content, Does.Contain("End Time: 2025-10-23T14:30:10.234Z"));
            Assert.That(content, Does.Contain("Duration: 10.234 seconds"));
            Assert.That(content, Does.Contain("Exit Code: 0"));
            Assert.That(content, Does.Contain("Status: SUCCESS"));
        });
    }

    [Test]
    public async Task WriteEntryAsync_WhenNotInitialized_DoesNotThrow()
    {
        // Arrange
        var uninitializedLogger = new FileLogger();
        var entry = new LogEntry(DateTime.UtcNow, LogSeverity.Info, "Test", "Message");

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await uninitializedLogger.WriteEntryAsync(entry));
        uninitializedLogger.Dispose();
    }

    [Test]
    public async Task WriteSessionHeaderAsync_WhenNotInitialized_DoesNotThrow()
    {
        // Arrange
        var uninitializedLogger = new FileLogger();
        var session = new LogSession
        {
            SessionId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow,
            CommandLine = "test"
        };

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await uninitializedLogger.WriteSessionHeaderAsync(session));
        uninitializedLogger.Dispose();
    }

    [Test]
    public async Task WriteSessionFooterAsync_WhenNotInitialized_DoesNotThrow()
    {
        // Arrange
        var uninitializedLogger = new FileLogger();
        var session = new LogSession
        {
            SessionId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow,
            CommandLine = "test",
            ExitCode = 0,
            Status = SessionStatus.Success
        };

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await uninitializedLogger.WriteSessionFooterAsync(session));
        uninitializedLogger.Dispose();
    }

    [Test]
    public async Task WriteEntryAsync_MultipleEntries_WritesInOrder()
    {
        // Arrange
        var entry1 = new LogEntry(DateTime.UtcNow, LogSeverity.Info, "Component1", "Message 1");
        var entry2 = new LogEntry(DateTime.UtcNow, LogSeverity.Operation, "Component2", "Message 2");
        var entry3 = new LogEntry(DateTime.UtcNow, LogSeverity.Warning, "Component3", "Message 3");

        // Act
        await _fileLogger.WriteEntryAsync(entry1);
        await _fileLogger.WriteEntryAsync(entry2);
        await _fileLogger.WriteEntryAsync(entry3);
        _fileLogger.Dispose();

        // Assert
        var content = await File.ReadAllTextAsync(_logFilePath);
        var lines = content.Split('\n');
        
        var message1Index = Array.FindIndex(lines, l => l.Contains("Message 1"));
        var message2Index = Array.FindIndex(lines, l => l.Contains("Message 2"));
        var message3Index = Array.FindIndex(lines, l => l.Contains("Message 3"));

        Assert.That(message1Index, Is.LessThan(message2Index));
        Assert.That(message2Index, Is.LessThan(message3Index));
    }

    [Test]
    public async Task WriteEntryAsync_SeverityLevels_FormatCorrectly()
    {
        // Arrange & Act
        await _fileLogger.WriteEntryAsync(new LogEntry(DateTime.UtcNow, LogSeverity.Info, "C", "M"));
        await _fileLogger.WriteEntryAsync(new LogEntry(DateTime.UtcNow, LogSeverity.Operation, "C", "M"));
        await _fileLogger.WriteEntryAsync(new LogEntry(DateTime.UtcNow, LogSeverity.Warning, "C", "M"));
        await _fileLogger.WriteEntryAsync(new LogEntry(DateTime.UtcNow, LogSeverity.Error, "C", "M"));
        await _fileLogger.WriteEntryAsync(new LogEntry(DateTime.UtcNow, LogSeverity.Exception, "C", "M", "stack"));
        _fileLogger.Dispose();

        // Assert
        var content = await File.ReadAllTextAsync(_logFilePath);
        Assert.Multiple(() =>
        {
            Assert.That(content, Does.Contain("[INFO]"));
            Assert.That(content, Does.Contain("[OPERATION]"));
            Assert.That(content, Does.Contain("[WARNING]"));
            Assert.That(content, Does.Contain("[ERROR]"));
            Assert.That(content, Does.Contain("[EXCEPTION]"));
        });
    }
}
