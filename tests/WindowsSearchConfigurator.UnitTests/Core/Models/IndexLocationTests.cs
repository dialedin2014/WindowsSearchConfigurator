using WindowsSearchConfigurator.Core.Models;

namespace WindowsSearchConfigurator.UnitTests.Core.Models;

/// <summary>
/// Unit tests for the IndexLocation class.
/// Tests constructors and property behavior.
/// </summary>
[TestFixture]
public class IndexLocationTests
{
    [Test]
    public void ParameterizedConstructor_ShouldSetRequiredProperties()
    {
        // Arrange
        var path = "C:\\Test\\Path";
        var pathType = PathType.Local;

        // Act
        var location = new IndexLocation(path, pathType);

        // Assert
        Assert.That(location.FullPath, Is.EqualTo(path));
        Assert.That(location.PathType, Is.EqualTo(pathType));
    }

    [Test]
    public void ObjectInitializer_ShouldSetAllProperties()
    {
        // Act
        var location = new IndexLocation
        {
            FullPath = "\\\\Server\\Share",
            PathType = PathType.UNC,
            Exists = true,
            IsAccessible = true,
            VolumeLabel = "NetworkShare"
        };

        // Assert
        Assert.That(location.FullPath, Is.EqualTo("\\\\Server\\Share"));
        Assert.That(location.PathType, Is.EqualTo(PathType.UNC));
        Assert.That(location.Exists, Is.True);
        Assert.That(location.IsAccessible, Is.True);
        Assert.That(location.VolumeLabel, Is.EqualTo("NetworkShare"));
    }

    [Test]
    public void VolumeLabel_CanBeNull()
    {
        // Act
        var location = new IndexLocation("C:\\Test", PathType.Local);

        // Assert
        Assert.That(location.VolumeLabel, Is.Null);
    }

    [Test]
    public void Properties_CanBeModified()
    {
        // Arrange
        var location = new IndexLocation("C:\\Test", PathType.Local);

        // Act
        location.Exists = true;
        location.IsAccessible = false;
        location.VolumeLabel = "TestDrive";

        // Assert
        Assert.That(location.Exists, Is.True);
        Assert.That(location.IsAccessible, Is.False);
        Assert.That(location.VolumeLabel, Is.EqualTo("TestDrive"));
    }
}
