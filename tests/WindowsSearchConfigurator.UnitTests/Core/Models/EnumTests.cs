using WindowsSearchConfigurator.Core.Models;

namespace WindowsSearchConfigurator.UnitTests.Core.Models;

/// <summary>
/// Unit tests for all enum types in Core.Models.
/// Tests enum value definitions and conversions.
/// </summary>
[TestFixture]
public class EnumTests
{
    #region RuleType Tests

    [Test]
    public void RuleType_ShouldHaveExpectedValues()
    {
        // Assert
        Assert.That(Enum.IsDefined(typeof(RuleType), (int)RuleType.Include));
        Assert.That(Enum.IsDefined(typeof(RuleType), (int)RuleType.Exclude));
    }

    [Test]
    public void RuleType_IncludeValue_ShouldBeZero()
    {
        // Assert
        Assert.That((int)RuleType.Include, Is.EqualTo(0));
    }

    #endregion

    #region RuleSource Tests

    [Test]
    public void RuleSource_ShouldHaveExpectedValues()
    {
        // Assert
        Assert.That(Enum.IsDefined(typeof(RuleSource), (int)RuleSource.User));
        Assert.That(Enum.IsDefined(typeof(RuleSource), (int)RuleSource.System));
        Assert.That(Enum.IsDefined(typeof(RuleSource), (int)RuleSource.Imported));
    }

    #endregion

    #region IndexingDepth Tests

    [Test]
    public void IndexingDepth_ShouldHaveExpectedValues()
    {
        // Assert
        Assert.That(Enum.IsDefined(typeof(IndexingDepth), (int)IndexingDepth.PropertiesOnly));
        Assert.That(Enum.IsDefined(typeof(IndexingDepth), (int)IndexingDepth.PropertiesAndContents));
    }

    #endregion

    #region FilterType Tests

    [Test]
    public void FilterType_ShouldHaveExpectedValues()
    {
        // Assert
        Assert.That(Enum.IsDefined(typeof(FilterType), (int)FilterType.Include));
        Assert.That(Enum.IsDefined(typeof(FilterType), (int)FilterType.Exclude));
    }

    #endregion

    #region FilterTarget Tests

    [Test]
    public void FilterTarget_ShouldHaveExpectedValues()
    {
        // Assert
        Assert.That(Enum.IsDefined(typeof(FilterTarget), (int)FilterTarget.FileExtension));
        Assert.That(Enum.IsDefined(typeof(FilterTarget), (int)FilterTarget.FileName));
        Assert.That(Enum.IsDefined(typeof(FilterTarget), (int)FilterTarget.Subfolder));
    }

    #endregion

    #region PathType Tests

    [Test]
    public void PathType_ShouldHaveExpectedValues()
    {
        // Assert
        Assert.That(Enum.IsDefined(typeof(PathType), (int)PathType.Local));
        Assert.That(Enum.IsDefined(typeof(PathType), (int)PathType.UNC));
        Assert.That(Enum.IsDefined(typeof(PathType), (int)PathType.Relative));
    }

    #endregion

    #region RegistrationMode Tests

    [Test]
    public void RegistrationMode_ShouldHaveExpectedValues()
    {
        // Assert
        Assert.That(Enum.IsDefined(typeof(RegistrationMode), (int)RegistrationMode.Interactive));
        Assert.That(Enum.IsDefined(typeof(RegistrationMode), (int)RegistrationMode.Automatic));
        Assert.That(Enum.IsDefined(typeof(RegistrationMode), (int)RegistrationMode.Manual));
        Assert.That(Enum.IsDefined(typeof(RegistrationMode), (int)RegistrationMode.Declined));
    }

    #endregion

    #region RegistrationOutcome Tests

    [Test]
    public void RegistrationOutcome_ShouldHaveExpectedValues()
    {
        // Assert
        Assert.That(Enum.IsDefined(typeof(RegistrationOutcome), (int)RegistrationOutcome.Success));
        Assert.That(Enum.IsDefined(typeof(RegistrationOutcome), (int)RegistrationOutcome.Failed));
        Assert.That(Enum.IsDefined(typeof(RegistrationOutcome), (int)RegistrationOutcome.Timeout));
        Assert.That(Enum.IsDefined(typeof(RegistrationOutcome), (int)RegistrationOutcome.InsufficientPrivileges));
    }

    #endregion

    #region COMValidationState Tests

    [Test]
    public void COMValidationState_ShouldHaveExpectedValues()
    {
        // Assert
        Assert.That(Enum.IsDefined(typeof(COMValidationState), (int)COMValidationState.NotChecked));
        Assert.That(Enum.IsDefined(typeof(COMValidationState), (int)COMValidationState.Valid));
        Assert.That(Enum.IsDefined(typeof(COMValidationState), (int)COMValidationState.CLSIDNotFound));
        Assert.That(Enum.IsDefined(typeof(COMValidationState), (int)COMValidationState.InstantiationFailed));
    }

    #endregion

    #region ValidationSeverity Tests

    [Test]
    public void ValidationSeverity_ShouldHaveExpectedValues()
    {
        // Assert
        Assert.That(Enum.IsDefined(typeof(ValidationSeverity), (int)ValidationSeverity.None));
        Assert.That(Enum.IsDefined(typeof(ValidationSeverity), (int)ValidationSeverity.Warning));
        Assert.That(Enum.IsDefined(typeof(ValidationSeverity), (int)ValidationSeverity.Error));
    }

    [Test]
    public void ValidationSeverity_Values_ShouldBeInOrder()
    {
        // Assert
        Assert.That((int)ValidationSeverity.None, Is.EqualTo(0));
        Assert.That((int)ValidationSeverity.Warning, Is.EqualTo(1));
        Assert.That((int)ValidationSeverity.Error, Is.EqualTo(2));
    }

    #endregion

    #region Enum Conversion Tests

    [Test]
    public void Enums_CanConvertToString()
    {
        // Act & Assert
        Assert.That(RuleType.Include.ToString(), Is.EqualTo("Include"));
        Assert.That(RuleSource.User.ToString(), Is.EqualTo("User"));
        Assert.That(PathType.Local.ToString(), Is.EqualTo("Local"));
    }

    [Test]
    public void Enums_CanParseFromString()
    {
        // Act & Assert
        Assert.That(Enum.Parse<RuleType>("Include"), Is.EqualTo(RuleType.Include));
        Assert.That(Enum.Parse<RuleSource>("User"), Is.EqualTo(RuleSource.User));
        Assert.That(Enum.Parse<PathType>("UNC"), Is.EqualTo(PathType.UNC));
    }

    #endregion
}
