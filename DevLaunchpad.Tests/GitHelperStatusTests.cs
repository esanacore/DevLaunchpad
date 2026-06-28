// Copyright (c) Eric Sanacore
// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.IO;
using DevLaunchpad.Tests.Helpers;
using Xunit;

namespace DevLaunchpad.Tests;

public sealed class GitHelperStatusTests : IDisposable
{
    private readonly TempGitRepo _temp = new();

    private const string Hash1 = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private const string Hash2 = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";

    public void Dispose() => _temp.Dispose();

    // ── ReadRefHash ──────────────────────────────────────────────────

    [Fact]
    public void ReadRefHash_LooseRef_ReturnsHash()
    {
        string repo = _temp.CreateSubRepo("repo", branch: "main");
        _temp.WriteRef(repo, "refs/heads/main", Hash1);

        Assert.Equal(Hash1, GitHelper.ReadRefHash(repo, "refs/heads/main"));
    }

    [Fact]
    public void ReadRefHash_PackedRef_ReturnsHash()
    {
        string repo = _temp.CreateSubRepo("repo", branch: "main");
        _temp.WritePackedRefs(repo, [("refs/heads/main", Hash1)]);

        Assert.Equal(Hash1, GitHelper.ReadRefHash(repo, "refs/heads/main"));
    }

    [Fact]
    public void ReadRefHash_LooseTakesPrecedenceOverPacked()
    {
        string repo = _temp.CreateSubRepo("repo", branch: "main");
        _temp.WriteRef(repo, "refs/heads/main", Hash1);
        _temp.WritePackedRefs(repo, [("refs/heads/main", Hash2)]);

        Assert.Equal(Hash1, GitHelper.ReadRefHash(repo, "refs/heads/main"));
    }

    [Fact]
    public void ReadRefHash_NoRef_ReturnsNull()
    {
        string repo = _temp.CreateSubRepo("repo", branch: "main");

        Assert.Null(GitHelper.ReadRefHash(repo, "refs/heads/main"));
    }

    [Fact]
    public void ReadRefHash_WrongRefInPacked_ReturnsNull()
    {
        string repo = _temp.CreateSubRepo("repo", branch: "main");
        _temp.WritePackedRefs(repo, [("refs/heads/other", Hash1)]);

        Assert.Null(GitHelper.ReadRefHash(repo, "refs/heads/main"));
    }

    [Fact]
    public void ReadRefHash_PackedCommentAndPeeledLinesIgnored()
    {
        string repo = _temp.CreateSubRepo("repo", branch: "main");
        _temp.WritePackedRefs(repo,
        [
            ("refs/heads/main", Hash1),
            ("refs/tags/v1.0", Hash2),
        ]);

        Assert.Equal(Hash1, GitHelper.ReadRefHash(repo, "refs/heads/main"));
        Assert.Equal(Hash2, GitHelper.ReadRefHash(repo, "refs/tags/v1.0"));
    }

    // ── GetLocalBranchHash / GetRemoteTrackingHash ───────────────────

    [Fact]
    public void GetLocalBranchHash_LooseRef_ReturnsHash()
    {
        string repo = _temp.CreateSubRepo("repo", branch: "main");
        _temp.WriteRef(repo, "refs/heads/main", Hash1);

        Assert.Equal(Hash1, GitHelper.GetLocalBranchHash(repo, "main"));
    }

    [Fact]
    public void GetLocalBranchHash_SlashBranch_ReturnsHash()
    {
        string repo = _temp.CreateSubRepo("repo", branch: "feature/my-feature");
        _temp.WriteRef(repo, "refs/heads/feature/my-feature", Hash1);

        Assert.Equal(Hash1, GitHelper.GetLocalBranchHash(repo, "feature/my-feature"));
    }

    [Fact]
    public void GetRemoteTrackingHash_LooseRef_ReturnsHash()
    {
        string repo = _temp.CreateSubRepo("repo", branch: "main");
        _temp.WriteRef(repo, "refs/remotes/origin/main", Hash2);

        Assert.Equal(Hash2, GitHelper.GetRemoteTrackingHash(repo, "main"));
    }

    [Fact]
    public void GetRemoteTrackingHash_NoRef_ReturnsNull()
    {
        string repo = _temp.CreateSubRepo("repo", branch: "main");

        Assert.Null(GitHelper.GetRemoteTrackingHash(repo, "main"));
    }

    // ── GetSpecialState ──────────────────────────────────────────────

    [Fact]
    public void GetSpecialState_Clean_ReturnsNull()
    {
        string repo = _temp.CreateSubRepo("repo");

        Assert.Null(GitHelper.GetSpecialState(repo));
    }

    [Fact]
    public void GetSpecialState_MergeHead_ReturnsMerge()
    {
        string repo = _temp.CreateSubRepo("repo");
        _temp.WriteSpecialStateMarker(repo, "MERGE_HEAD");

        Assert.Equal("merge", GitHelper.GetSpecialState(repo));
    }

    [Fact]
    public void GetSpecialState_CherryPickHead_ReturnsCherryPick()
    {
        string repo = _temp.CreateSubRepo("repo");
        _temp.WriteSpecialStateMarker(repo, "CHERRY_PICK_HEAD");

        Assert.Equal("cherry-pick", GitHelper.GetSpecialState(repo));
    }

    [Fact]
    public void GetSpecialState_RebaseHead_ReturnsRebase()
    {
        string repo = _temp.CreateSubRepo("repo");
        _temp.WriteSpecialStateMarker(repo, "REBASE_HEAD");

        Assert.Equal("rebase", GitHelper.GetSpecialState(repo));
    }

    [Fact]
    public void GetSpecialState_RebaseMergeDir_ReturnsRebase()
    {
        string repo = _temp.CreateSubRepo("repo");
        Directory.CreateDirectory(Path.Combine(repo, ".git", "rebase-merge"));

        Assert.Equal("rebase", GitHelper.GetSpecialState(repo));
    }

    [Fact]
    public void GetSpecialState_RebaseApplyDir_ReturnsRebase()
    {
        string repo = _temp.CreateSubRepo("repo");
        Directory.CreateDirectory(Path.Combine(repo, ".git", "rebase-apply"));

        Assert.Equal("rebase", GitHelper.GetSpecialState(repo));
    }

    // ── IsDirty ─────────────────────────────────────────────────────

    [Fact]
    public void IsDirty_SpecialState_ReturnsTrue()
    {
        string repo = _temp.CreateSubRepo("repo", branch: "main");
        _temp.WriteSpecialStateMarker(repo, "MERGE_HEAD");

        Assert.True(GitHelper.IsDirty(repo, "main"));
    }

    [Fact]
    public void IsDirty_IndexNewerThanRef_ReturnsTrue()
    {
        string repo = _temp.CreateSubRepo("repo", branch: "main");
        var refTime = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        _temp.WriteRef(repo, "refs/heads/main", Hash1, mtime: refTime);
        _temp.WriteIndex(repo, lastWriteUtc: refTime.AddMinutes(5));

        Assert.True(GitHelper.IsDirty(repo, "main"));
    }

    [Fact]
    public void IsDirty_IndexOlderThanRef_ReturnsFalse()
    {
        string repo = _temp.CreateSubRepo("repo", branch: "main");
        var refTime = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        _temp.WriteRef(repo, "refs/heads/main", Hash1, mtime: refTime);
        _temp.WriteIndex(repo, lastWriteUtc: refTime.AddMinutes(-5));

        Assert.False(GitHelper.IsDirty(repo, "main"));
    }

    [Fact]
    public void IsDirty_IndexSameMtimeAsRef_ReturnsFalse()
    {
        // Exactly equal means the index was last written at commit time — not dirty.
        string repo = _temp.CreateSubRepo("repo", branch: "main");
        var t = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        _temp.WriteRef(repo, "refs/heads/main", Hash1, mtime: t);
        _temp.WriteIndex(repo, lastWriteUtc: t);

        Assert.False(GitHelper.IsDirty(repo, "main"));
    }

    [Fact]
    public void IsDirty_NoIndex_ReturnsFalse()
    {
        string repo = _temp.CreateSubRepo("repo", branch: "main");
        _temp.WriteRef(repo, "refs/heads/main", Hash1);
        // No index written.

        Assert.False(GitHelper.IsDirty(repo, "main"));
    }

    [Fact]
    public void IsDirty_BranchInPackedRefs_ReturnsFalse()
    {
        // When the branch ref is packed, mtime comparison is unavailable.
        string repo = _temp.CreateSubRepo("repo", branch: "main");
        _temp.WritePackedRefs(repo, [("refs/heads/main", Hash1)]);
        _temp.WriteIndex(repo);

        Assert.False(GitHelper.IsDirty(repo, "main"));
    }

    [Fact]
    public void IsDirty_NullBranch_ReturnsFalse()
    {
        string repo = _temp.CreateSubRepo("repo", branch: null);

        Assert.False(GitHelper.IsDirty(repo, null));
    }

    [Fact]
    public void IsDirty_SlashBranch_UsesCorrectRefPath()
    {
        string repo = _temp.CreateSubRepo("repo", branch: "feature/my-feature");
        var refTime = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        _temp.WriteRef(repo, "refs/heads/feature/my-feature", Hash1, mtime: refTime);
        _temp.WriteIndex(repo, lastWriteUtc: refTime.AddSeconds(1));

        Assert.True(GitHelper.IsDirty(repo, "feature/my-feature"));
    }
}
