using WindowsSearchConfigurator.Core.Models;

namespace WindowsSearchConfigurator.UnitTests.Core.Models;

/// <summary>
/// Unit tests for the FileTypeFilter class.
/// Tests constructor and property behavior.
/// </summary>
[TestFixture]
public class FileTypeFilterTests
{
    [Test]
    public void ParameterizedConstructor_ShouldSetAllProperties()
    {
        // Arrange
        var pattern = "*.txt";
        var filterType = FilterType.Include;
        var appliesTo = FilterTarget.FileExtension;

        // Act
        var filter = new FileTypeFilter(pattern, filterType, appliesTo);

        // Assert
        Assert.That(filter.Pattern, Is.EqualTo(pattern));
        Assert.That(filter.FilterType, Is.EqualTo(filterType));
        Assert.That(filter.AppliesTo, Is.EqualTo(appliesTo));
    }

    [Test]
    public void ObjectInitializer_ShouldSetAllProperties()
    {
        // Act
        var filter = new FileTypeFilter
        {
            Pattern = "*.log",
            FilterType = FilterType.Exclude,
            AppliesTo = FilterTarget.FileName
        };

        // Assert
        Assert.That(filter.Pattern, Is.EqualTo("*.log"));
        Assert.That(filter.FilterType, Is.EqualTo(FilterType.Exclude));
        Assert.That(filter.AppliesTo, Is.EqualTo(FilterTarget.FileName));
    }

    [Test]
    public void Properties_CanBeModified()
    {
        // Arrange
        var filter = new FileTypeFilter("*.txt", FilterType.Include, FilterTarget.FileExtension);

        // Act
        filter.Pattern = "*.md";
        filter.FilterType = FilterType.Exclude;
        filter.AppliesTo = FilterTarget.Subfolder;

        // Assert
        Assert.That(filter.Pattern, Is.EqualTo("*.md"));
        Assert.That(filter.FilterType, Is.EqualTo(FilterType.Exclude));
        Assert.That(filter.AppliesTo, Is.EqualTo(FilterTarget.Subfolder));
    }
}
