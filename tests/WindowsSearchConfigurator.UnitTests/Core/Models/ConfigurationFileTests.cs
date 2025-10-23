using WindowsSearchConfigurator.Core.Models;

namespace WindowsSearchConfigurator.UnitTests.Core.Models;

/// <summary>
/// Unit tests for the ConfigurationFile class.
/// Tests constructors, ExportDate auto-set, and collection initialization.
/// </summary>
[TestFixture]
public class ConfigurationFileTests
{
    #region Constructor Tests

    [Test]
    public void DefaultConstructor_ShouldSetExportDate()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var config = new ConfigurationFile
        {
            Version = "1.0",
            ExportedBy = "TestUser",
            MachineName = "TestMachine"
        };

        // Arrange
        var after = DateTime.UtcNow;

        // Assert
        Assert.That(config.ExportDate, Is.GreaterThanOrEqualTo(before));
        Assert.That(config.ExportDate, Is.LessThanOrEqualTo(after));
    }

    [Test]
    public void ParameterizedConstructor_ShouldSetAllRequiredProperties()
    {
        // Arrange
        var version = "1.0";
        var exportedBy = "DOMAIN\\User";
        var machineName = "DESKTOP-TEST";

        // Act
        var config = new ConfigurationFile(version, exportedBy, machineName);

        // Assert
        Assert.That(config.Version, Is.EqualTo(version));
        Assert.That(config.ExportedBy, Is.EqualTo(exportedBy));
        Assert.That(config.MachineName, Is.EqualTo(machineName));
        Assert.That(config.ExportDate, Is.Not.EqualTo(default(DateTime)));
    }

    #endregion

    #region Collection Initialization Tests

    [Test]
    public void Rules_ShouldInitializeAsEmptyList()
    {
        // Act
        var config = new ConfigurationFile
        {
            Version = "1.0",
            ExportedBy = "TestUser",
            MachineName = "TestMachine"
        };

        // Assert
        Assert.That(config.Rules, Is.Not.Null);
        Assert.That(config.Rules, Is.Empty);
    }

    [Test]
    public void ExtensionSettings_ShouldInitializeAsEmptyList()
    {
        // Act
        var config = new ConfigurationFile
        {
            Version = "1.0",
            ExportedBy = "TestUser",
            MachineName = "TestMachine"
        };

        // Assert
        Assert.That(config.ExtensionSettings, Is.Not.Null);
        Assert.That(config.ExtensionSettings, Is.Empty);
    }

    [Test]
    public void Rules_CanAddItems()
    {
        // Arrange
        var config = new ConfigurationFile("1.0", "User", "Machine");
        var rule = new IndexRule
        {
            Path = "C:\\Test",
            RuleType = RuleType.Include,
            Source = RuleSource.User
        };

        // Act
        config.Rules.Add(rule);

        // Assert
        Assert.That(config.Rules, Has.Count.EqualTo(1));
        Assert.That(config.Rules[0].Path, Is.EqualTo("C:\\Test"));
    }

    [Test]
    public void ExtensionSettings_CanAddItems()
    {
        // Arrange
        var config = new ConfigurationFile("1.0", "User", "Machine");
        var setting = new FileExtensionSetting
        {
            Extension = ".txt",
            IndexingDepth = IndexingDepth.PropertiesAndContents
        };

        // Act
        config.ExtensionSettings.Add(setting);

        // Assert
        Assert.That(config.ExtensionSettings, Has.Count.EqualTo(1));
        Assert.That(config.ExtensionSettings[0].Extension, Is.EqualTo(".txt"));
    }

    #endregion

    #region DateTime UTC Validation Tests

    [Test]
    public void ExportDate_ShouldBeUtc()
    {
        // Act
        var config = new ConfigurationFile("1.0", "User", "Machine");

        // Assert
        Assert.That(config.ExportDate.Kind, Is.EqualTo(DateTimeKind.Utc));
    }

    [Test]
    public void ExportDate_ShouldBeReasonable()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var config = new ConfigurationFile("1.0", "User", "Machine");

        // Arrange
        var after = DateTime.UtcNow;

        // Assert
        Assert.That(config.ExportDate, Is.GreaterThanOrEqualTo(before));
        Assert.That(config.ExportDate, Is.LessThanOrEqualTo(after));
    }

    #endregion

    #region Optional Properties Tests

    [Test]
    public void Checksum_ShouldBeNullByDefault()
    {
        // Act
        var config = new ConfigurationFile("1.0", "User", "Machine");

        // Assert
        Assert.That(config.Checksum, Is.Null);
    }

    [Test]
    public void Checksum_CanBeSet()
    {
        // Arrange
        var config = new ConfigurationFile("1.0", "User", "Machine");
        var checksum = "SHA256HASH";

        // Act
        config.Checksum = checksum;

        // Assert
        Assert.That(config.Checksum, Is.EqualTo(checksum));
    }

    #endregion

    #region Edge Case Tests

    [Test]
    public void EmptyCollections_ShouldRemainEmpty()
    {
        // Act
        var config = new ConfigurationFile("1.0", "User", "Machine");

        // Assert
        Assert.That(config.Rules, Is.Empty);
        Assert.That(config.ExtensionSettings, Is.Empty);
    }

    [Test]
    public void ExportDate_ShouldNotBeMinValue()
    {
        // Act
        var config = new ConfigurationFile("1.0", "User", "Machine");

        // Assert
        Assert.That(config.ExportDate, Is.Not.EqualTo(DateTime.MinValue));
    }

    [Test]
    public void ExportDate_ShouldNotBeMaxValue()
    {
        // Act
        var config = new ConfigurationFile("1.0", "User", "Machine");

        // Assert
        Assert.That(config.ExportDate, Is.Not.EqualTo(DateTime.MaxValue));
    }

    #endregion
}
