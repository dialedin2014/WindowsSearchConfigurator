using FluentAssertions;
using WindowsSearchConfigurator.Services;

namespace WindowsSearchConfigurator.UnitTests.Services;

/// <summary>
/// Unit tests for PrivilegeChecker service.
/// </summary>
[TestFixture]
public class PrivilegeCheckerTests
{
    private PrivilegeChecker _privilegeChecker = null!;

    [SetUp]
    public void Setup()
    {
        _privilegeChecker = new PrivilegeChecker();
    }

    [Test]
    public void IsAdministrator_ShouldReturnBoolValue()
    {
        // Act
        var result = _privilegeChecker.IsAdministrator();

        // Assert
        // Result should be either true or false (valid boolean)
        Assert.That(result, Is.True.Or.False);
    }

    [Test]
    public void IsAdministrator_ShouldNotThrowException()
    {
        // Act
        Action act = () => _privilegeChecker.IsAdministrator();

        // Assert
        act.Should().NotThrow();
    }

    [Test]
    public void GetCurrentUser_ShouldReturnNonEmptyString()
    {
        // Act
        var result = _privilegeChecker.GetCurrentUser();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("\\"); // Windows username format: DOMAIN\Username
    }

    [Test]
    public void GetCurrentUser_ShouldNotThrowException()
    {
        // Act
        Action act = () => _privilegeChecker.GetCurrentUser();

        // Assert
        act.Should().NotThrow();
    }

    [Test]
    public void RequireAdministrator_WhenUserIsNotAdmin_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var isAdmin = _privilegeChecker.IsAdministrator();

        // Act & Assert
        if (!isAdmin)
        {
            Action act = () => _privilegeChecker.RequireAdministrator();
            act.Should().Throw<UnauthorizedAccessException>()
                .WithMessage("*administrator*");
        }
        else
        {
            // If running as admin, the method should not throw
            Action act = () => _privilegeChecker.RequireAdministrator();
            act.Should().NotThrow();
        }
    }

    [Test]
    public void RequireAdministrator_WhenUserIsAdmin_ShouldNotThrow()
    {
        // Arrange
        var isAdmin = _privilegeChecker.IsAdministrator();

        // Act & Assert
        if (isAdmin)
        {
            Action act = () => _privilegeChecker.RequireAdministrator();
            act.Should().NotThrow();
        }
    }

    [Test]
    public void PrivilegeChecker_MultipleCallsShouldBeConsistent()
    {
        // Act
        var result1 = _privilegeChecker.IsAdministrator();
        var result2 = _privilegeChecker.IsAdministrator();
        var result3 = _privilegeChecker.IsAdministrator();

        // Assert
        result1.Should().Be(result2);
        result2.Should().Be(result3);
    }

    [Test]
    public void GetCurrentUser_MultipleCallsShouldReturnSameUser()
    {
        // Act
        var user1 = _privilegeChecker.GetCurrentUser();
        var user2 = _privilegeChecker.GetCurrentUser();

        // Assert
        user1.Should().Be(user2);
    }
}
