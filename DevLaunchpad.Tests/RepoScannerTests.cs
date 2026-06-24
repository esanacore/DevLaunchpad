using System.IO;
using System.Linq;
using DevLaunchpad.Tests.Helpers;
using Xunit;

namespace DevLaunchpad.Tests;

/// <summary>
/// Tests for <see cref="RepoScanner"/> — the extracted repository discovery logic.
/// Uses <see cref="TempGitRepo"/> to build temporary directory trees with <c>.git</c> stubs.
/// </summary>
public sealed class RepoScannerTests : IDisposable
{
    private readonly TempGitRepo _temp = new();

    public void Dispose() => _temp.Dispose();

    [Fact]
    public void Scan_FindsGitRepo()
    {
        _temp.CreateSubRepo("my-project", branch: "main");

        var results = RepoScanner.Scan(_temp.RootPath);

        Assert.Single(results);
        Assert.Equal("my-project", results[0].DisplayName);
    }

    [Fact]
    public void Scan_FindsNestedRepos()
    {
        _temp.CreateSubRepo("group/repo-a", branch: "main");
        _temp.CreateSubRepo("group/repo-b", branch: "develop");

        var results = RepoScanner.Scan(_temp.RootPath);

        Assert.Equal(2, results.Count);
        var names = results.Select(r => r.DisplayName).OrderBy(n => n).ToList();
        Assert.Contains(Path.Combine("group", "repo-a"), names);
        Assert.Contains(Path.Combine("group", "repo-b"), names);
    }

    [Fact]
    public void Scan_StopsAtGitDir_DoesNotDescendIntoRepo()
    {
        // Create a repo with a nested "subproject" that also has a .git dir.
        // The scanner should find the outer repo but NOT descend into it to find sub.
        string outerRepo = _temp.CreateSubRepo("outer", branch: "main");
        // Create a nested .git inside the outer repo's subdirectory
        string innerDir = Path.Combine(outerRepo, "packages", "inner");
        Directory.CreateDirectory(Path.Combine(innerDir, ".git"));
        File.WriteAllText(Path.Combine(innerDir, ".git", "HEAD"), "ref: refs/heads/main\n");

        var results = RepoScanner.Scan(_temp.RootPath);

        // Should only find "outer", not "outer/packages/inner"
        Assert.Single(results);
        Assert.Equal("outer", results[0].DisplayName);
    }

    [Fact]
    public void Scan_RespectsMaxDepth()
    {
        // Create a repo deeper than MaxScanDepth (6).
        // Root / a / b / c / d / e / f / deep-repo — that's depth 7.
        string deepPath = Path.Combine("a", "b", "c", "d", "e", "f", "deep-repo");
        _temp.CreateSubRepo(deepPath, branch: "main");

        var results = RepoScanner.Scan(_temp.RootPath);

        Assert.Empty(results);
    }

    [Fact]
    public void Scan_SkipsExcludedDirectories()
    {
        // Place a repo inside a node_modules directory — it should be skipped.
        _temp.CreateSubRepo("project/node_modules/hidden-repo", branch: "main");
        // Place a normal repo alongside it.
        _temp.CreateSubRepo("project/real-repo", branch: "main");

        var results = RepoScanner.Scan(_temp.RootPath);

        Assert.Single(results);
        Assert.Contains("real-repo", results[0].DisplayName);
    }

    [Fact]
    public void Scan_EmptyRoot_ReturnsEmptyList()
    {
        // The root exists but contains no repos.
        var results = RepoScanner.Scan(_temp.RootPath);

        Assert.Empty(results);
    }

    [Fact]
    public void Scan_NonexistentRoot_ReturnsEmptyList()
    {
        string fakePath = Path.Combine(_temp.RootPath, "does-not-exist");

        var results = RepoScanner.Scan(fakePath);

        Assert.Empty(results);
    }

    [Fact]
    public void Scan_IncludesGitBranchAndRemote()
    {
        _temp.CreateSubRepo("with-meta", branch: "feature/test", remoteUrl: "https://github.com/owner/repo.git");

        var results = RepoScanner.Scan(_temp.RootPath);

        Assert.Single(results);
        Assert.Equal("feature/test", results[0].Branch);
        Assert.Equal("https://github.com/owner/repo", results[0].WebUrl);
    }

    [Fact]
    public void Scan_PopulatesProjectType()
    {
        string repo = _temp.CreateSubRepo("rust-repo", branch: "main");
        File.WriteAllText(Path.Combine(repo, "Cargo.toml"), "[package]");

        var results = RepoScanner.Scan(_temp.RootPath);

        Assert.Single(results);
        Assert.Equal("Rust", results[0].ProjectType);
    }

    [Fact]
    public void Scan_ProjectTypeIsNull_WhenNoMarkers()
    {
        _temp.CreateSubRepo("plain-repo", branch: "main");

        var results = RepoScanner.Scan(_temp.RootPath);

        Assert.Single(results);
        Assert.Null(results[0].ProjectType);
    }

    [Fact]
    public void Scan_SkipsMultipleExcludedDirTypes()
    {
        // Test several excluded directory names.
        foreach (var dir in new[] { "bin", "obj", ".vs", "dist", "vendor" })
        {
            _temp.CreateSubRepo($"parent/{dir}/hidden", branch: "main");
        }

        _temp.CreateSubRepo("parent/actual-repo", branch: "main");

        var results = RepoScanner.Scan(_temp.RootPath);

        Assert.Single(results);
        Assert.Contains("actual-repo", results[0].DisplayName);
    }
}
