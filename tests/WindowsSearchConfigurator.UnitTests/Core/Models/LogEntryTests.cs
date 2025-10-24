using NUnit.Framework;
using WindowsSearchConfigurator.Core.Models;

namespace WindowsSearchConfigurator.UnitTests.Core.Models;

[TestFixture]
public class LogEntryTests
{
    [Test]
    public void LogEntry_WithValidData_CreatesSuccessfully()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;
        var severity = LogSeverity.Info;
        var component = "TestComponent";
        var message = "Test message";

        // Act
        var entry = new LogEntry(timestamp, severity, component, message);

        // Assert
        Assert.That(entry.Timestamp, Is.EqualTo(timestamp));
        Assert.That(entry.Severity, Is.EqualTo(severity));
        Assert.That(entry.Component, Is.EqualTo(component));
        Assert.That(entry.Message, Is.EqualTo(message));
        Assert.That(entry.StackTrace, Is.Null);
    }

    [Test]
    public void LogEntry_WithStackTrace_CreatesSuccessfully()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;
        var severity = LogSeverity.Exception;
        var component = "TestComponent";
        var message = "Exception occurred";
        var stackTrace = "   at TestMethod()";

        // Act
        var entry = new LogEntry(timestamp, severity, component, message, stackTrace);

        // Assert
        Assert.That(entry.StackTrace, Is.EqualTo(stackTrace));
    }

    [Test]
    public void Validate_WithValidEntry_DoesNotThrow()
    {
        // Arrange
        var entry = new LogEntry(DateTime.UtcNow, LogSeverity.Info, "Component", "Message");

        // Act & Assert
        Assert.DoesNotThrow(() => entry.Validate());
    }

    [Test]
    public void Validate_WithDefaultTimestamp_ThrowsArgumentException()
    {
        // Arrange
        var entry = new LogEntry(default, LogSeverity.Info, "Component", "Message");

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => entry.Validate());
        Assert.That(ex?.ParamName, Is.EqualTo("Timestamp"));
    }

    [Test]
    public void Validate_WithEmptyComponent_ThrowsArgumentException()
    {
        // Arrange
        var entry = new LogEntry(DateTime.UtcNow, LogSeverity.Info, "", "Message");

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => entry.Validate());
        Assert.That(ex?.ParamName, Is.EqualTo("Component"));
    }

    [Test]
    public void Validate_WithNullMessage_ThrowsArgumentException()
    {
        // Arrange
        var entry = new LogEntry(DateTime.UtcNow, LogSeverity.Info, "Component", null!);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => entry.Validate());
        Assert.That(ex?.ParamName, Is.EqualTo("Message"));
    }

    [Test]
    public void Validate_ExceptionSeverityWithoutStackTrace_ThrowsArgumentException()
    {
        // Arrange
        var entry = new LogEntry(DateTime.UtcNow, LogSeverity.Exception, "Component", "Error", null);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => entry.Validate());
        Assert.That(ex?.ParamName, Is.EqualTo("StackTrace"));
    }

    [Test]
    public void Validate_ExceptionSeverityWithStackTrace_DoesNotThrow()
    {
        // Arrange
        var entry = new LogEntry(DateTime.UtcNow, LogSeverity.Exception, "Component", "Error", "   at Method()");

        // Act & Assert
        Assert.DoesNotThrow(() => entry.Validate());
    }

    [Test]
    public void LogEntry_EmptyMessageIsValid()
    {
        // Arrange
        var entry = new LogEntry(DateTime.UtcNow, LogSeverity.Info, "Component", string.Empty);

        // Act & Assert
        Assert.DoesNotThrow(() => entry.Validate());
    }
}
