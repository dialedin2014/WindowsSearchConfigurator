using WindowsSearchConfigurator.Core.Models;

namespace WindowsSearchConfigurator.UnitTests.Core.Models;

/// <summary>
/// Unit tests for the IndexRule class.
/// Tests constructors, property initialization, auto-generated values, and collection behavior.
/// </summary>
[TestFixture]
public class IndexRuleTests
{
    #region Constructor Tests

    [Test]
    public void DefaultConstructor_ShouldGenerateIdAndSetTimestamps()
    {
        // Act
        var rule = new IndexRule
        {
            Path = "C:\\Test",
            RuleType = RuleType.Include,
            Source = RuleSource.User
        };

        // Assert
        Assert.That(rule.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(rule.CreatedDate, Is.LessThanOrEqualTo(DateTime.UtcNow));
        Assert.That(rule.CreatedDate, Is.GreaterThan(DateTime.UtcNow.AddSeconds(-5)));
        Assert.That(rule.ModifiedDate, Is.LessThanOrEqualTo(DateTime.UtcNow));
        Assert.That(rule.ModifiedDate, Is.GreaterThan(DateTime.UtcNow.AddSeconds(-5)));
    }

    [Test]
    public void ObjectInitializer_ShouldSetRequiredProperties()
    {
        // Arrange
        var path = "C:\\Test\\Path";
        var ruleType = RuleType.Include;
        var source = RuleSource.User;

        // Act
        var rule = new IndexRule
        {
            Path = path,
            RuleType = ruleType,
            Source = source
        };

        // Assert
        Assert.That(rule.Path, Is.EqualTo(path));
        Assert.That(rule.RuleType, Is.EqualTo(ruleType));
        Assert.That(rule.Source, Is.EqualTo(source));
        Assert.That(rule.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(rule.CreatedDate, Is.Not.EqualTo(default(DateTime)));
        Assert.That(rule.ModifiedDate, Is.Not.EqualTo(default(DateTime)));
    }

    [Test]
    public void DefaultConstructor_ShouldSetDefaultValues()
    {
        // Act
        var rule = new IndexRule
        {
            Path = "C:\\Test",
            RuleType = RuleType.Include,
            Source = RuleSource.User
        };

        // Assert
        Assert.That(rule.Recursive, Is.True);
        Assert.That(rule.IsUserDefined, Is.True);
    }

    #endregion

    #region Property Initialization Tests

    [Test]
    public void FileTypeFilters_ShouldInitializeAsEmptyList()
    {
        // Act
        var rule = new IndexRule
        {
            Path = "C:\\Test",
            RuleType = RuleType.Include,
            Source = RuleSource.User
        };

        // Assert
        Assert.That(rule.FileTypeFilters, Is.Not.Null);
        Assert.That(rule.FileTypeFilters, Is.Empty);
    }

    [Test]
    public void ExcludedSubfolders_ShouldInitializeAsEmptyList()
    {
        // Act
        var rule = new IndexRule
        {
            Path = "C:\\Test",
            RuleType = RuleType.Include,
            Source = RuleSource.User
        };

        // Assert
        Assert.That(rule.ExcludedSubfolders, Is.Not.Null);
        Assert.That(rule.ExcludedSubfolders, Is.Empty);
    }

    [Test]
    public void RecursiveProperty_ShouldDefaultToTrue()
    {
        // Act
        var rule = new IndexRule
        {
            Path = "C:\\Test",
            RuleType = RuleType.Include,
            Source = RuleSource.User
        };

        // Assert
        Assert.That(rule.Recursive, Is.True);
    }

    [Test]
    public void IsUserDefinedProperty_ShouldDefaultToTrue()
    {
        // Act
        var rule = new IndexRule
        {
            Path = "C:\\Test",
            RuleType = RuleType.Include,
            Source = RuleSource.User
        };

        // Assert
        Assert.That(rule.IsUserDefined, Is.True);
    }

    #endregion

    #region Guid Uniqueness Tests

    [Test]
    public void MultipleInstances_ShouldHaveUniqueIds()
    {
        // Act
        var rule1 = new IndexRule
        {
            Path = "C:\\Test1",
            RuleType = RuleType.Include,
            Source = RuleSource.User
        };
        var rule2 = new IndexRule
        {
            Path = "C:\\Test2",
            RuleType = RuleType.Include,
            Source = RuleSource.User
        };

        // Assert
        Assert.That(rule1.Id, Is.Not.EqualTo(rule2.Id));
    }

    [Test]
    public void MultipleInstances_WithInitializers_ShouldGenerateUniqueIds()
    {
        // Act
        var rule1 = new IndexRule { Path = "C:\\Test1", RuleType = RuleType.Include, Source = RuleSource.User };
        var rule2 = new IndexRule { Path = "C:\\Test2", RuleType = RuleType.Include, Source = RuleSource.User };
        var rule3 = new IndexRule { Path = "C:\\Test3", RuleType = RuleType.Include, Source = RuleSource.User };

        // Assert
        Assert.That(rule1.Id, Is.Not.EqualTo(rule2.Id));
        Assert.That(rule2.Id, Is.Not.EqualTo(rule3.Id));
        Assert.That(rule1.Id, Is.Not.EqualTo(rule3.Id));
    }

    #endregion

    #region DateTime UTC Validation Tests

    [Test]
    public void CreatedDate_ShouldBeUtc()
    {
        // Act
        var rule = new IndexRule { Path = "C:\\Test", RuleType = RuleType.Include, Source = RuleSource.User };

        // Assert
        Assert.That(rule.CreatedDate.Kind, Is.EqualTo(DateTimeKind.Utc));
    }

    [Test]
    public void ModifiedDate_ShouldBeUtc()
    {
        // Act
        var rule = new IndexRule { Path = "C:\\Test", RuleType = RuleType.Include, Source = RuleSource.User };

        // Assert
        Assert.That(rule.ModifiedDate.Kind, Is.EqualTo(DateTimeKind.Utc));
    }

    [Test]
    public void Timestamps_ShouldBeReasonable()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var rule = new IndexRule { Path = "C:\\Test", RuleType = RuleType.Include, Source = RuleSource.User };

        // Arrange
        var after = DateTime.UtcNow;

        // Assert
        Assert.That(rule.CreatedDate, Is.GreaterThanOrEqualTo(before));
        Assert.That(rule.CreatedDate, Is.LessThanOrEqualTo(after));
        Assert.That(rule.ModifiedDate, Is.GreaterThanOrEqualTo(before));
        Assert.That(rule.ModifiedDate, Is.LessThanOrEqualTo(after));
    }

    #endregion

    #region Collection Behavior Tests

    [Test]
    public void FileTypeFilters_CanAddItems()
    {
        // Arrange
        var rule = new IndexRule { Path = "C:\\Test", RuleType = RuleType.Include, Source = RuleSource.User };
        var filter = new FileTypeFilter
        {
            Pattern = "*.txt",
            FilterType = FilterType.Include,
            AppliesTo = FilterTarget.FileExtension
        };

        // Act
        rule.FileTypeFilters.Add(filter);

        // Assert
        Assert.That(rule.FileTypeFilters, Has.Count.EqualTo(1));
        Assert.That(rule.FileTypeFilters[0].Pattern, Is.EqualTo("*.txt"));
    }

    [Test]
    public void ExcludedSubfolders_CanAddItems()
    {
        // Arrange
        var rule = new IndexRule { Path = "C:\\Test", RuleType = RuleType.Include, Source = RuleSource.User };

        // Act
        rule.ExcludedSubfolders.Add("temp");
        rule.ExcludedSubfolders.Add("cache");

        // Assert
        Assert.That(rule.ExcludedSubfolders, Has.Count.EqualTo(2));
        Assert.That(rule.ExcludedSubfolders, Contains.Item("temp"));
        Assert.That(rule.ExcludedSubfolders, Contains.Item("cache"));
    }

    #endregion

    #region Edge Case Tests

    [Test]
    public void EmptyCollections_ShouldRemainEmpty()
    {
        // Act
        var rule = new IndexRule { Path = "C:\\Test", RuleType = RuleType.Include, Source = RuleSource.User };

        // Assert
        Assert.That(rule.FileTypeFilters, Is.Empty);
        Assert.That(rule.ExcludedSubfolders, Is.Empty);
    }

    [Test]
    public void Properties_CanBeModified()
    {
        // Arrange
        var rule = new IndexRule { Path = "C:\\Test", RuleType = RuleType.Include, Source = RuleSource.User };

        // Act
        rule.Recursive = false;
        rule.IsUserDefined = false;

        // Assert
        Assert.That(rule.Recursive, Is.False);
        Assert.That(rule.IsUserDefined, Is.False);
    }

    [Test]
    public void DateTimeMinValue_ShouldNotBeSet()
    {
        // Act
        var rule = new IndexRule { Path = "C:\\Test", RuleType = RuleType.Include, Source = RuleSource.User };

        // Assert
        Assert.That(rule.CreatedDate, Is.Not.EqualTo(DateTime.MinValue));
        Assert.That(rule.ModifiedDate, Is.Not.EqualTo(DateTime.MinValue));
    }

    [Test]
    public void DateTimeMaxValue_ShouldNotBeSet()
    {
        // Act
        var rule = new IndexRule { Path = "C:\\Test", RuleType = RuleType.Include, Source = RuleSource.User };

        // Assert
        Assert.That(rule.CreatedDate, Is.Not.EqualTo(DateTime.MaxValue));
        Assert.That(rule.ModifiedDate, Is.Not.EqualTo(DateTime.MaxValue));
    }

    #endregion
}
