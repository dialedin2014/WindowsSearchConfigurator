using NUnit.Framework;
using WindowsSearchConfigurator.Core.Models;

namespace WindowsSearchConfigurator.UnitTests.Core.Models;

[TestFixture]
public class LogSeverityTests
{
    [Test]
    public void LogSeverity_HasExpectedValues()
    {
        // Assert
        Assert.That((int)LogSeverity.Info, Is.EqualTo(0));
        Assert.That((int)LogSeverity.Operation, Is.EqualTo(1));
        Assert.That((int)LogSeverity.Warning, Is.EqualTo(2));
        Assert.That((int)LogSeverity.Error, Is.EqualTo(3));
        Assert.That((int)LogSeverity.Exception, Is.EqualTo(4));
    }

    [Test]
    public void LogSeverity_CanBeConvertedToString()
    {
        // Assert
        Assert.That(LogSeverity.Info.ToString(), Is.EqualTo("Info"));
        Assert.That(LogSeverity.Operation.ToString(), Is.EqualTo("Operation"));
        Assert.That(LogSeverity.Warning.ToString(), Is.EqualTo("Warning"));
        Assert.That(LogSeverity.Error.ToString(), Is.EqualTo("Error"));
        Assert.That(LogSeverity.Exception.ToString(), Is.EqualTo("Exception"));
    }

    [Test]
    public void LogSeverity_CanBeParsedFromString()
    {
        // Act & Assert
        Assert.That(Enum.Parse<LogSeverity>("Info"), Is.EqualTo(LogSeverity.Info));
        Assert.That(Enum.Parse<LogSeverity>("Operation"), Is.EqualTo(LogSeverity.Operation));
        Assert.That(Enum.Parse<LogSeverity>("Warning"), Is.EqualTo(LogSeverity.Warning));
        Assert.That(Enum.Parse<LogSeverity>("Error"), Is.EqualTo(LogSeverity.Error));
        Assert.That(Enum.Parse<LogSeverity>("Exception"), Is.EqualTo(LogSeverity.Exception));
    }

    [Test]
    public void LogSeverity_AllValuesAreDefined()
    {
        // Arrange
        var allValues = Enum.GetValues<LogSeverity>();

        // Assert
        Assert.That(allValues.Length, Is.EqualTo(5));
        Assert.That(allValues, Does.Contain(LogSeverity.Info));
        Assert.That(allValues, Does.Contain(LogSeverity.Operation));
        Assert.That(allValues, Does.Contain(LogSeverity.Warning));
        Assert.That(allValues, Does.Contain(LogSeverity.Error));
        Assert.That(allValues, Does.Contain(LogSeverity.Exception));
    }
}
