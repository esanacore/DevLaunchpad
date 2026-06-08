using DevLaunchpad.Tests.Helpers;
using Xunit;

namespace DevLaunchpad.Tests;

public sealed class GitHelperTests : IDisposable
{
    private readonly TempGitRepo _temp = new();

    public void Dispose() => _temp.Dispose();

    // ── GetCurrentBranch ────────────────────────────────────────────

    [Fact]
    public void GetCurrentBranch_NormalRef_ReturnsBranchName()
    {
        string repo = _temp.CreateSubRepo("repo1", branch: "main");

        string? branch = GitHelper.GetCurrentBranch(repo);

        Assert.Equal("main", branch);
    }

    [Fact]
    public void GetCurrentBranch_FeatureBranch_ReturnsBranchName()
    {
        string repo = _temp.CreateSubRepo("repo2", branch: "feature/my-feature");

        string? branch = GitHelper.GetCurrentBranch(repo);

        Assert.Equal("feature/my-feature", branch);
    }

    [Fact]
    public void GetCurrentBranch_DetachedHead_ReturnsShortHash()
    {
        string fullHash = "abc1234def5678901234567890abcdef12345678";
        string repo = _temp.CreateSubRepo("detached", detachedHash: fullHash);

        string? branch = GitHelper.GetCurrentBranch(repo);

        Assert.Equal("abc1234", branch);
    }

    [Fact]
    public void GetCurrentBranch_ShortDetachedHash_ReturnsAsIs()
    {
        // Edge case: hash shorter than 7 chars (unusual but possible in corrupted state)
        string repo = _temp.CreateSubRepo("short", detachedHash: "abc12");

        string? branch = GitHelper.GetCurrentBranch(repo);

        Assert.Equal("abc12", branch);
    }

    [Fact]
    public void GetCurrentBranch_MissingHeadFile_ReturnsNull()
    {
        // Create a repo without writing HEAD
        string repoPath = _temp.CreateDirectory("no-head");
        Directory.CreateDirectory(Path.Combine(repoPath, ".git"));

        string? branch = GitHelper.GetCurrentBranch(repoPath);

        Assert.Null(branch);
    }

    [Fact]
    public void GetCurrentBranch_MissingGitDir_ReturnsNull()
    {
        string plainDir = _temp.CreateDirectory("not-a-repo");

        string? branch = GitHelper.GetCurrentBranch(plainDir);

        Assert.Null(branch);
    }

    [Fact]
    public void GetCurrentBranch_NonExistentPath_ReturnsNull()
    {
        string? branch = GitHelper.GetCurrentBranch(Path.Combine(_temp.RootPath, "does-not-exist"));

        Assert.Null(branch);
    }

    // ── ParseOriginUrl ──────────────────────────────────────────────

    [Fact]
    public void ParseOriginUrl_HttpsUrl_ReturnsUrl()
    {
        string[] lines =
        [
            "[remote \"origin\"]",
            "\turl = https://github.com/owner/repo.git",
            "\tfetch = +refs/heads/*:refs/remotes/origin/*",
        ];

        string? url = GitHelper.ParseOriginUrl(lines);

        Assert.Equal("https://github.com/owner/repo.git", url);
    }

    [Fact]
    public void ParseOriginUrl_SshScpStyle_ReturnsUrl()
    {
        string[] lines =
        [
            "[remote \"origin\"]",
            "\turl = git@github.com:owner/repo.git",
        ];

        string? url = GitHelper.ParseOriginUrl(lines);

        Assert.Equal("git@github.com:owner/repo.git", url);
    }

    [Fact]
    public void ParseOriginUrl_NoOriginSection_ReturnsNull()
    {
        string[] lines =
        [
            "[remote \"upstream\"]",
            "\turl = https://github.com/upstream/repo.git",
        ];

        string? url = GitHelper.ParseOriginUrl(lines);

        Assert.Null(url);
    }

    [Fact]
    public void ParseOriginUrl_EmptyConfig_ReturnsNull()
    {
        string? url = GitHelper.ParseOriginUrl(Array.Empty<string>());

        Assert.Null(url);
    }

    [Fact]
    public void ParseOriginUrl_MultipleRemotes_ReturnsOriginOnly()
    {
        string[] lines =
        [
            "[remote \"upstream\"]",
            "\turl = https://github.com/upstream/repo.git",
            "[remote \"origin\"]",
            "\turl = https://github.com/owner/repo.git",
            "[remote \"fork\"]",
            "\turl = https://github.com/fork/repo.git",
        ];

        string? url = GitHelper.ParseOriginUrl(lines);

        Assert.Equal("https://github.com/owner/repo.git", url);
    }

    [Fact]
    public void ParseOriginUrl_OriginWithoutUrl_ReturnsNull()
    {
        string[] lines =
        [
            "[remote \"origin\"]",
            "\tfetch = +refs/heads/*:refs/remotes/origin/*",
            "[core]",
            "\trepositoryformatversion = 0",
        ];

        string? url = GitHelper.ParseOriginUrl(lines);

        Assert.Null(url);
    }

    // ── NormalizeRemoteUrl ───────────────────────────────────────────

    [Fact]
    public void NormalizeRemoteUrl_Https_Passthrough()
    {
        string? result = GitHelper.NormalizeRemoteUrl("https://github.com/owner/repo");

        Assert.Equal("https://github.com/owner/repo", result);
    }

    [Fact]
    public void NormalizeRemoteUrl_Http_Passthrough()
    {
        string? result = GitHelper.NormalizeRemoteUrl("http://gitlab.local/owner/repo");

        Assert.Equal("http://gitlab.local/owner/repo", result);
    }

    [Fact]
    public void NormalizeRemoteUrl_StripsDotGitSuffix()
    {
        string? result = GitHelper.NormalizeRemoteUrl("https://github.com/owner/repo.git");

        Assert.Equal("https://github.com/owner/repo", result);
    }

    [Fact]
    public void NormalizeRemoteUrl_GitAtScp_ConvertsToHttps()
    {
        string? result = GitHelper.NormalizeRemoteUrl("git@github.com:owner/repo.git");

        Assert.Equal("https://github.com/owner/repo", result);
    }

    [Fact]
    public void NormalizeRemoteUrl_GitAtScp_NoDotGit()
    {
        string? result = GitHelper.NormalizeRemoteUrl("git@gitlab.com:group/project");

        Assert.Equal("https://gitlab.com/group/project", result);
    }

    [Fact]
    public void NormalizeRemoteUrl_SshProtocol_ConvertsToHttps()
    {
        string? result = GitHelper.NormalizeRemoteUrl("ssh://git@github.com/owner/repo.git");

        Assert.Equal("https://github.com/owner/repo", result);
    }

    [Fact]
    public void NormalizeRemoteUrl_SshProtocol_NoGitUser()
    {
        string? result = GitHelper.NormalizeRemoteUrl("ssh://bitbucket.org/owner/repo");

        Assert.Equal("https://bitbucket.org/owner/repo", result);
    }

    [Fact]
    public void NormalizeRemoteUrl_NullOrEmpty_ReturnsNull()
    {
        Assert.Null(GitHelper.NormalizeRemoteUrl(null!));
        Assert.Null(GitHelper.NormalizeRemoteUrl(""));
        Assert.Null(GitHelper.NormalizeRemoteUrl("   "));
    }

    [Fact]
    public void NormalizeRemoteUrl_UnknownScheme_ReturnsNull()
    {
        string? result = GitHelper.NormalizeRemoteUrl("ftp://example.com/repo");

        Assert.Null(result);
    }

    // ── GetRemoteWebUrl (integration with file system) ──────────────

    [Fact]
    public void GetRemoteWebUrl_WithOrigin_ReturnsNormalizedUrl()
    {
        string repo = _temp.CreateSubRepo("with-remote", remoteUrl: "git@github.com:owner/repo.git");

        string? webUrl = GitHelper.GetRemoteWebUrl(repo);

        Assert.Equal("https://github.com/owner/repo", webUrl);
    }

    [Fact]
    public void GetRemoteWebUrl_NoConfig_ReturnsNull()
    {
        string repo = _temp.CreateSubRepo("no-remote", branch: "main");

        string? webUrl = GitHelper.GetRemoteWebUrl(repo);

        Assert.Null(webUrl);
    }
}
