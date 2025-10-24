using NUnit.Framework;
using WindowsSearchConfigurator.Core.Models;

namespace WindowsSearchConfigurator.UnitTests.Core.Models;

[TestFixture]
public class SessionStatusTests
{
    [Test]
    public void SessionStatus_HasExpectedValues()
    {
        // Assert
        Assert.That((int)SessionStatus.InProgress, Is.EqualTo(0));
        Assert.That((int)SessionStatus.Success, Is.EqualTo(1));
        Assert.That((int)SessionStatus.Failed, Is.EqualTo(2));
        Assert.That((int)SessionStatus.Aborted, Is.EqualTo(3));
    }

    [Test]
    public void SessionStatus_CanBeConvertedToString()
    {
        // Assert
        Assert.That(SessionStatus.InProgress.ToString(), Is.EqualTo("InProgress"));
        Assert.That(SessionStatus.Success.ToString(), Is.EqualTo("Success"));
        Assert.That(SessionStatus.Failed.ToString(), Is.EqualTo("Failed"));
        Assert.That(SessionStatus.Aborted.ToString(), Is.EqualTo("Aborted"));
    }

    [Test]
    public void SessionStatus_CanBeParsedFromString()
    {
        // Act & Assert
        Assert.That(Enum.Parse<SessionStatus>("InProgress"), Is.EqualTo(SessionStatus.InProgress));
        Assert.That(Enum.Parse<SessionStatus>("Success"), Is.EqualTo(SessionStatus.Success));
        Assert.That(Enum.Parse<SessionStatus>("Failed"), Is.EqualTo(SessionStatus.Failed));
        Assert.That(Enum.Parse<SessionStatus>("Aborted"), Is.EqualTo(SessionStatus.Aborted));
    }

    [Test]
    public void SessionStatus_AllValuesAreDefined()
    {
        // Arrange
        var allValues = Enum.GetValues<SessionStatus>();

        // Assert
        Assert.That(allValues.Length, Is.EqualTo(4));
        Assert.That(allValues, Does.Contain(SessionStatus.InProgress));
        Assert.That(allValues, Does.Contain(SessionStatus.Success));
        Assert.That(allValues, Does.Contain(SessionStatus.Failed));
        Assert.That(allValues, Does.Contain(SessionStatus.Aborted));
    }
}
