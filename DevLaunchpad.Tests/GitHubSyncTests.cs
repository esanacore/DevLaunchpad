// Copyright (c) Eric Sanacore
// SPDX-License-Identifier: GPL-3.0-only

using Xunit;

namespace DevLaunchpad.Tests;

/// <summary>
/// Tests for <see cref="GitHubSync.BuildSyncScript"/>, the pure builder for the bulk clone/pull
/// PowerShell script.
/// </summary>
public sealed class GitHubSyncTests
{
    [Fact]
    public void BuildSyncScript_ContainsCoreCommands()
    {
        string script = GitHubSync.BuildSyncScript();

        Assert.Contains("gh repo list", script);
        Assert.Contains("--json nameWithOwner", script);
        Assert.Contains("gh repo clone", script);
        Assert.Contains("git -C", script);
        Assert.Contains("pull --ff-only", script);
        // Existing repos are detected by their .git folder, mirroring the shell one-liner.
        Assert.Contains(".git", script);
    }

    [Fact]
    public void BuildSyncScript_ChecksToolingAndAuthUpFront()
    {
        string script = GitHubSync.BuildSyncScript();

        Assert.Contains("gh auth status", script);
        Assert.Contains("gh", script);
        Assert.Contains("git", script);
    }

    [Fact]
    public void BuildSyncScript_DefaultLimit_IsOneThousand()
    {
        Assert.Contains("--limit 1000", GitHubSync.BuildSyncScript());
    }

    [Theory]
    [InlineData(5, "--limit 5")]
    [InlineData(42, "--limit 42")]
    [InlineData(2000, "--limit 2000")]
    public void BuildSyncScript_RespectsLimit(int limit, string expected)
    {
        string script = GitHubSync.BuildSyncScript(limit);

        Assert.Contains(expected, script);
        // The placeholder must be fully substituted.
        Assert.DoesNotContain("__REPO_LIMIT__", script);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void BuildSyncScript_NonPositiveLimit_FallsBackToDefault(int limit)
    {
        Assert.Contains("--limit 1000", GitHubSync.BuildSyncScript(limit));
    }

    [Fact]
    public void ScriptFileName_IsPowerShellScript()
    {
        Assert.EndsWith(".ps1", GitHubSync.ScriptFileName);
    }
}
