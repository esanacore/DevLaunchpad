using System.IO;
using DevLaunchpad.Tests.Helpers;
using Xunit;

namespace DevLaunchpad.Tests;

/// <summary>
/// Tests for <see cref="ProjectTypeDetector"/> — repository tech-stack inference from root
/// marker files. Uses <see cref="TempGitRepo"/> as a disposable temp directory container.
/// </summary>
public sealed class ProjectTypeDetectorTests : IDisposable
{
    private readonly TempGitRepo _temp = new();

    public void Dispose() => _temp.Dispose();

    private string MakeRepoWith(string name, params string[] markerFiles)
    {
        string repo = _temp.CreateDirectory(name);
        foreach (string marker in markerFiles)
        {
            File.WriteAllText(Path.Combine(repo, marker), "// marker");
        }

        return repo;
    }

    [Theory]
    [InlineData("package.json", "Node")]
    [InlineData("Cargo.toml", "Rust")]
    [InlineData("go.mod", "Go")]
    [InlineData("pyproject.toml", "Python")]
    [InlineData("requirements.txt", "Python")]
    [InlineData("setup.py", "Python")]
    [InlineData("Pipfile", "Python")]
    [InlineData("pom.xml", "Java")]
    [InlineData("build.gradle", "Java")]
    [InlineData("build.gradle.kts", "Java")]
    [InlineData("pubspec.yaml", "Dart")]
    [InlineData("Gemfile", "Ruby")]
    [InlineData("composer.json", "PHP")]
    [InlineData("Dockerfile", "Docker")]
    [InlineData("docker-compose.yml", "Docker")]
    public void Detect_SingleMarker_ReturnsExpectedLabel(string marker, string expected)
    {
        string repo = MakeRepoWith($"repo-{expected}-{marker}", marker);

        Assert.Equal(expected, ProjectTypeDetector.Detect(repo));
    }

    [Theory]
    [InlineData("MySolution.sln")]
    [InlineData("MyApp.csproj")]
    [InlineData("Library.fsproj")]
    [InlineData("Legacy.vbproj")]
    public void Detect_DotNetGlobMarkers_ReturnsDotNet(string marker)
    {
        string repo = MakeRepoWith($"repo-dotnet-{marker}", marker);

        Assert.Equal(".NET", ProjectTypeDetector.Detect(repo));
    }

    [Fact]
    public void Detect_DotNetTakesPriorityOverFileMarkers()
    {
        // A repo that is both .NET and Node (e.g. a web app with a JS front-end).
        string repo = MakeRepoWith("dotnet-and-node", "App.csproj", "package.json");

        Assert.Equal(".NET", ProjectTypeDetector.Detect(repo));
    }

    [Fact]
    public void Detect_LanguageMarkerTakesPriorityOverDocker()
    {
        string repo = MakeRepoWith("node-and-docker", "package.json", "Dockerfile");

        Assert.Equal("Node", ProjectTypeDetector.Detect(repo));
    }

    [Fact]
    public void Detect_NoMarkers_ReturnsNull()
    {
        string repo = MakeRepoWith("empty-repo");

        Assert.Null(ProjectTypeDetector.Detect(repo));
    }

    [Fact]
    public void Detect_NonexistentPath_ReturnsNull()
    {
        Assert.Null(ProjectTypeDetector.Detect(Path.Combine(_temp.RootPath, "does-not-exist")));
    }

    [Fact]
    public void Detect_NullOrWhitespace_ReturnsNull()
    {
        Assert.Null(ProjectTypeDetector.Detect(null!));
        Assert.Null(ProjectTypeDetector.Detect(""));
        Assert.Null(ProjectTypeDetector.Detect("   "));
    }
}
