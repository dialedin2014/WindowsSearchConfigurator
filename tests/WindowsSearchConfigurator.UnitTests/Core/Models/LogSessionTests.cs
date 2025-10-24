using NUnit.Framework;
using WindowsSearchConfigurator.Core.Models;

namespace WindowsSearchConfigurator.UnitTests.Core.Models;

[TestFixture]
public class LogSessionTests
{
    [Test]
    public void LogSession_WithValidData_CreatesSuccessfully()
    {
        // Arrange & Act
        var session = new LogSession
        {
            SessionId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow,
            CommandLine = "test command",
            UserName = "testuser",
            WorkingDirectory = "C:\\test",
            WindowsVersion = "Windows 11",
            RuntimeVersion = "8.0.0",
            Status = SessionStatus.InProgress
        };

        // Assert
        Assert.That(session.SessionId, Is.Not.EqualTo(Guid.Empty));
        Assert.That(session.CommandLine, Is.EqualTo("test command"));
        Assert.That(session.Status, Is.EqualTo(SessionStatus.InProgress));
        Assert.That(session.EndTime, Is.Null);
        Assert.That(session.ExitCode, Is.Null);
        Assert.That(session.Duration, Is.Null);
    }

    [Test]
    public void Duration_WhenEndTimeSet_CalculatesCorrectly()
    {
        // Arrange
        var startTime = new DateTime(2025, 10, 23, 14, 30, 0, DateTimeKind.Utc);
        var endTime = new DateTime(2025, 10, 23, 14, 30, 10, DateTimeKind.Utc);
        var session = new LogSession
        {
            SessionId = Guid.NewGuid(),
            StartTime = startTime,
            EndTime = endTime,
            CommandLine = "test",
            Status = SessionStatus.Success
        };

        // Act
        var duration = session.Duration;

        // Assert
        Assert.That(duration, Is.Not.Null);
        Assert.That(duration.Value.TotalSeconds, Is.EqualTo(10).Within(0.001));
    }

    [Test]
    public void Duration_WhenEndTimeNotSet_ReturnsNull()
    {
        // Arrange
        var session = new LogSession
        {
            SessionId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow,
            CommandLine = "test",
            Status = SessionStatus.InProgress
        };

        // Act
        var duration = session.Duration;

        // Assert
        Assert.That(duration, Is.Null);
    }

    [Test]
    public void Validate_WithValidSession_DoesNotThrow()
    {
        // Arrange
        var session = new LogSession
        {
            SessionId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow,
            CommandLine = "test",
            Status = SessionStatus.InProgress
        };

        // Act & Assert
        Assert.DoesNotThrow(() => session.Validate());
    }

    [Test]
    public void Validate_WithEmptySessionId_ThrowsArgumentException()
    {
        // Arrange
        var session = new LogSession
        {
            SessionId = Guid.Empty,
            StartTime = DateTime.UtcNow,
            CommandLine = "test"
        };

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => session.Validate());
        Assert.That(ex?.ParamName, Is.EqualTo("SessionId"));
    }

    [Test]
    public void Validate_WithDefaultStartTime_ThrowsArgumentException()
    {
        // Arrange
        var session = new LogSession
        {
            SessionId = Guid.NewGuid(),
            StartTime = default,
            CommandLine = "test"
        };

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => session.Validate());
        Assert.That(ex?.ParamName, Is.EqualTo("StartTime"));
    }

    [Test]
    public void Validate_WithEndTimeBeforeStartTime_ThrowsArgumentException()
    {
        // Arrange
        var session = new LogSession
        {
            SessionId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddMinutes(-1),
            CommandLine = "test"
        };

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => session.Validate());
        Assert.That(ex?.ParamName, Is.EqualTo("EndTime"));
    }

    [Test]
    public void Validate_WithNullCommandLine_ThrowsArgumentException()
    {
        // Arrange
        var session = new LogSession
        {
            SessionId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow,
            CommandLine = null!
        };

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => session.Validate());
        Assert.That(ex?.ParamName, Is.EqualTo("CommandLine"));
    }

    [Test]
    public void Validate_WithInvalidExitCode_ThrowsArgumentException()
    {
        // Arrange
        var session = new LogSession
        {
            SessionId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow,
            CommandLine = "test",
            ExitCode = 256
        };

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => session.Validate());
        Assert.That(ex?.ParamName, Is.EqualTo("ExitCode"));
    }

    [Test]
    public void Validate_WithValidExitCode_DoesNotThrow()
    {
        // Arrange
        var session = new LogSession
        {
            SessionId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow,
            CommandLine = "test",
            ExitCode = 0
        };

        // Act & Assert
        Assert.DoesNotThrow(() => session.Validate());
    }
}
