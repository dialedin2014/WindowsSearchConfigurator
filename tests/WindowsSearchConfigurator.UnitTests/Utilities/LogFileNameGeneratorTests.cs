using NUnit.Framework;
using WindowsSearchConfigurator.Utilities;

namespace WindowsSearchConfigurator.UnitTests.Utilities;

[TestFixture]
public class LogFileNameGeneratorTests
{
    private LogFileNameGenerator _generator = null!;

    [SetUp]
    public void SetUp()
    {
        _generator = new LogFileNameGenerator();
    }

    [Test]
    public void GenerateFileName_ReturnsValidFormat()
    {
        // Act
        var fileName = _generator.GenerateFileName();

        // Assert
        Assert.That(fileName, Does.StartWith("WindowsSearchConfigurator_"));
        Assert.That(fileName, Does.EndWith(".log"));
    }

    [Test]
    public void GenerateFileName_IncludesTimestamp()
    {
        // Act
        var fileName = _generator.GenerateFileName();

        // Assert - should have pattern: WindowsSearchConfigurator_yyyyMMdd_HHmmss_guid.log
        var parts = fileName.Split('_');
        Assert.That(parts.Length, Is.EqualTo(4)); // name_date_time_guid.log
        
        // Check date part (8 digits)
        var datePart = parts[1];
        Assert.That(datePart.Length, Is.EqualTo(8));
        Assert.That(datePart, Does.Match(@"^\d{8}$"));
        
        // Check time part (6 digits)
        var timePart = parts[2];
        Assert.That(timePart.Length, Is.EqualTo(6));
        Assert.That(timePart, Does.Match(@"^\d{6}$"));
    }

    [Test]
    public void GenerateFileName_IncludesGuid()
    {
        // Act
        var fileName = _generator.GenerateFileName();

        // Assert - GUID part should be 6 lowercase hex characters before .log
        var guidPart = fileName.Split('_')[3].Replace(".log", "");
        Assert.That(guidPart.Length, Is.EqualTo(6));
        Assert.That(guidPart, Does.Match(@"^[a-f0-9]{6}$"));
    }

    [Test]
    public void GenerateFileName_MatchesContractRegex()
    {
        // Act
        var fileName = _generator.GenerateFileName();

        // Assert - matches contract specification
        Assert.That(fileName, Does.Match(@"^WindowsSearchConfigurator_\d{8}_\d{6}_[a-f0-9]{6}\.log$"));
    }

    [Test]
    public void GenerateFileName_GeneratesUniqueNames()
    {
        // Act - generate multiple file names
        var name1 = _generator.GenerateFileName();
        Thread.Sleep(10); // Small delay to ensure different timestamp or GUID
        var name2 = _generator.GenerateFileName();

        // Assert - names should be different (due to timestamp or GUID)
        Assert.That(name1, Is.Not.EqualTo(name2));
    }

    [Test]
    public void GenerateFileName_ValidTimestampFormat()
    {
        // Act
        var fileName = _generator.GenerateFileName();
        var parts = fileName.Split('_');
        var datePart = parts[1];
        var timePart = parts[2];

        // Assert - parse date and time to verify valid format
        var year = int.Parse(datePart.Substring(0, 4));
        var month = int.Parse(datePart.Substring(4, 2));
        var day = int.Parse(datePart.Substring(6, 2));
        
        var hour = int.Parse(timePart.Substring(0, 2));
        var minute = int.Parse(timePart.Substring(2, 2));
        var second = int.Parse(timePart.Substring(4, 2));

        Assert.Multiple(() =>
        {
            Assert.That(year, Is.GreaterThanOrEqualTo(2025));
            Assert.That(month, Is.InRange(1, 12));
            Assert.That(day, Is.InRange(1, 31));
            Assert.That(hour, Is.InRange(0, 23));
            Assert.That(minute, Is.InRange(0, 59));
            Assert.That(second, Is.InRange(0, 59));
        });
    }

    [Test]
    public void GenerateFileName_DoesNotContainSpaces()
    {
        // Act
        var fileName = _generator.GenerateFileName();

        // Assert
        Assert.That(fileName, Does.Not.Contain(" "));
    }

    [Test]
    public void GenerateFileName_DoesNotContainInvalidPathCharacters()
    {
        // Act
        var fileName = _generator.GenerateFileName();

        // Assert - check for common invalid path characters
        var invalidChars = new[] { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };
        foreach (var ch in invalidChars)
        {
            Assert.That(fileName, Does.Not.Contain(ch.ToString()));
        }
    }
}
