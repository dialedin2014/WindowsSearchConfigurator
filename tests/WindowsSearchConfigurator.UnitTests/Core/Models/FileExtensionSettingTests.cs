using WindowsSearchConfigurator.Core.Models;

namespace WindowsSearchConfigurator.UnitTests.Core.Models;

/// <summary>
/// Unit tests for the FileExtensionSetting class.
/// Tests constructors, ModifiedDate auto-set, and default values.
/// </summary>
[TestFixture]
public class FileExtensionSettingTests
{
    #region Constructor Tests

    [Test]
    public void DefaultConstructor_ShouldSetModifiedDate()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var setting = new FileExtensionSetting
        {
            Extension = ".txt",
            IndexingDepth = IndexingDepth.PropertiesAndContents
        };

        // Arrange
        var after = DateTime.UtcNow;

        // Assert
        Assert.That(setting.ModifiedDate, Is.GreaterThanOrEqualTo(before));
        Assert.That(setting.ModifiedDate, Is.LessThanOrEqualTo(after));
    }

    [Test]
    public void ParameterizedConstructor_ShouldSetAllProperties()
    {
        // Arrange
        var extension = ".log";
        var depth = IndexingDepth.PropertiesOnly;
        var isDefault = true;

        // Act
        var setting = new FileExtensionSetting(extension, depth, isDefault);

        // Assert
        Assert.That(setting.Extension, Is.EqualTo(extension));
        Assert.That(setting.IndexingDepth, Is.EqualTo(depth));
        Assert.That(setting.IsDefaultSetting, Is.EqualTo(isDefault));
        Assert.That(setting.ModifiedDate, Is.Not.EqualTo(default(DateTime)));
    }

    [Test]
    public void ParameterizedConstructor_ShouldDefaultIsDefaultSettingToFalse()
    {
        // Arrange
        var extension = ".txt";
        var depth = IndexingDepth.PropertiesAndContents;

        // Act
        var setting = new FileExtensionSetting(extension, depth);

        // Assert
        Assert.That(setting.IsDefaultSetting, Is.False);
    }

    #endregion

    #region Default Values Tests

    [Test]
    public void IsDefaultSetting_ShouldDefaultToFalse()
    {
        // Act
        var setting = new FileExtensionSetting
        {
            Extension = ".txt",
            IndexingDepth = IndexingDepth.PropertiesAndContents
        };

        // Assert
        Assert.That(setting.IsDefaultSetting, Is.False);
    }

    #endregion

    #region ModifiedDate Auto-Set Tests

    [Test]
    public void ModifiedDate_ShouldBeSetOnConstruction()
    {
        // Act
        var setting = new FileExtensionSetting(".txt", IndexingDepth.PropertiesAndContents);

        // Assert
        Assert.That(setting.ModifiedDate, Is.Not.EqualTo(default(DateTime)));
        Assert.That(setting.ModifiedDate, Is.LessThanOrEqualTo(DateTime.UtcNow));
    }

    [Test]
    public void ModifiedDate_ShouldBeUtc()
    {
        // Act
        var setting = new FileExtensionSetting(".txt", IndexingDepth.PropertiesAndContents);

        // Assert
        Assert.That(setting.ModifiedDate.Kind, Is.EqualTo(DateTimeKind.Utc));
    }

    [Test]
    public void ModifiedDate_ShouldBeReasonable()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var setting = new FileExtensionSetting(".txt", IndexingDepth.PropertiesAndContents);

        // Arrange
        var after = DateTime.UtcNow;

        // Assert
        Assert.That(setting.ModifiedDate, Is.GreaterThanOrEqualTo(before));
        Assert.That(setting.ModifiedDate, Is.LessThanOrEqualTo(after));
    }

    #endregion

    #region Property Getter/Setter Tests

    [Test]
    public void Extension_CanBeSetAndRetrieved()
    {
        // Arrange
        var setting = new FileExtensionSetting(".txt", IndexingDepth.PropertiesAndContents);

        // Act
        setting.Extension = ".log";

        // Assert
        Assert.That(setting.Extension, Is.EqualTo(".log"));
    }

    [Test]
    public void IndexingDepth_CanBeSetAndRetrieved()
    {
        // Arrange
        var setting = new FileExtensionSetting(".txt", IndexingDepth.PropertiesAndContents);

        // Act
        setting.IndexingDepth = IndexingDepth.PropertiesOnly;

        // Assert
        Assert.That(setting.IndexingDepth, Is.EqualTo(IndexingDepth.PropertiesOnly));
    }

    [Test]
    public void IsDefaultSetting_CanBeSetAndRetrieved()
    {
        // Arrange
        var setting = new FileExtensionSetting(".txt", IndexingDepth.PropertiesAndContents);

        // Act
        setting.IsDefaultSetting = true;

        // Assert
        Assert.That(setting.IsDefaultSetting, Is.True);
    }

    [Test]
    public void ModifiedDate_CanBeUpdated()
    {
        // Arrange
        var setting = new FileExtensionSetting(".txt", IndexingDepth.PropertiesAndContents);
        var newDate = DateTime.UtcNow.AddDays(1);

        // Act
        setting.ModifiedDate = newDate;

        // Assert
        Assert.That(setting.ModifiedDate, Is.EqualTo(newDate));
    }

    #endregion

    #region Edge Case Tests

    [Test]
    public void ModifiedDate_ShouldNotBeMinValue()
    {
        // Act
        var setting = new FileExtensionSetting(".txt", IndexingDepth.PropertiesAndContents);

        // Assert
        Assert.That(setting.ModifiedDate, Is.Not.EqualTo(DateTime.MinValue));
    }

    [Test]
    public void ModifiedDate_ShouldNotBeMaxValue()
    {
        // Act
        var setting = new FileExtensionSetting(".txt", IndexingDepth.PropertiesAndContents);

        // Assert
        Assert.That(setting.ModifiedDate, Is.Not.EqualTo(DateTime.MaxValue));
    }

    [Test]
    public void Extension_CanBeEmptyString()
    {
        // Act
        var setting = new FileExtensionSetting(string.Empty, IndexingDepth.PropertiesAndContents);

        // Assert
        Assert.That(setting.Extension, Is.EqualTo(string.Empty));
    }

    #endregion
}
