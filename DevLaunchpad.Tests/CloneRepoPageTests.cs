// Copyright (c) Eric Sanacore
// SPDX-License-Identifier: GPL-3.0-only

using DevLaunchpad.Pages;
using Xunit;

namespace DevLaunchpad.Tests;

/// <summary>
/// Tests for <see cref="CloneRepoPage.ExtractRepoName"/>.
/// </summary>
public sealed class CloneRepoPageTests
{
    [Theory]
    [InlineData("https://github.com/owner/my-repo.git", "my-repo")]
    [InlineData("https://github.com/owner/my-repo", "my-repo")]
    [InlineData("git@github.com:owner/my-repo.git", "my-repo")]
    [InlineData("git@github.com:owner/my-repo", "my-repo")]
    [InlineData("ssh://git@github.com/owner/my-repo.git", "my-repo")]
    [InlineData("https://gitlab.com/group/sub/project.git", "project")]
    public void ExtractRepoName_ValidUrls_ReturnsName(string url, string expected)
    {
        Assert.Equal(expected, CloneRepoPage.ExtractRepoName(url));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-a-url")]
    public void ExtractRepoName_InvalidInputs_ReturnsNull(string input)
    {
        Assert.Null(CloneRepoPage.ExtractRepoName(input));
    }

    [Fact]
    public void ExtractRepoName_TrailingSlash_StillWorks()
    {
        Assert.Equal("my-repo", CloneRepoPage.ExtractRepoName("https://github.com/owner/my-repo/"));
    }
}
