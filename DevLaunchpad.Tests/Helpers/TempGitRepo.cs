using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DevLaunchpad.Tests.Helpers;

/// <summary>
/// Creates a temporary directory tree with <c>.git</c> metadata for testing
/// <see cref="GitHelper"/> and <see cref="RepoScanner"/>.
///
/// Disposing the instance recursively deletes the temporary tree.
/// </summary>
internal sealed class TempGitRepo : IDisposable
{
    public string RootPath { get; }

    /// <summary>
    /// Creates a temporary root directory (no <c>.git</c> — it's just a container).
    /// Call <see cref="CreateSubRepo"/> to add actual repos inside it.
    /// </summary>
    public TempGitRepo()
    {
        RootPath = Path.Combine(Path.GetTempPath(), "DevLaunchpadTests_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(RootPath);
    }

    /// <summary>
    /// Creates a Git repository stub at <paramref name="relativePath"/> under the root.
    /// </summary>
    /// <param name="relativePath">Relative path from root (e.g. "myrepo" or "group/myrepo").</param>
    /// <param name="branch">Branch name to write to <c>.git/HEAD</c>. If <c>null</c>, HEAD is omitted.</param>
    /// <param name="remoteUrl">Optional origin remote URL to write to <c>.git/config</c>.</param>
    /// <param name="detachedHash">If set, writes this raw hash to HEAD instead of a branch ref.</param>
    /// <returns>Full path of the created repository directory.</returns>
    public string CreateSubRepo(
        string relativePath,
        string? branch = "main",
        string? remoteUrl = null,
        string? detachedHash = null)
    {
        string repoPath = Path.Combine(RootPath, relativePath);
        string gitDir = Path.Combine(repoPath, ".git");
        Directory.CreateDirectory(gitDir);

        if (detachedHash is not null)
        {
            File.WriteAllText(Path.Combine(gitDir, "HEAD"), detachedHash + "\n");
        }
        else if (branch is not null)
        {
            File.WriteAllText(Path.Combine(gitDir, "HEAD"), $"ref: refs/heads/{branch}\n");
        }

        if (remoteUrl is not null)
        {
            string config = $"""
                [remote "origin"]
                	url = {remoteUrl}
                	fetch = +refs/heads/*:refs/remotes/origin/*
                """;
            File.WriteAllText(Path.Combine(gitDir, "config"), config);
        }

        return repoPath;
    }

    /// <summary>
    /// Creates a plain (non-repo) subdirectory, optionally with a specific name
    /// for testing skip-directory logic.
    /// </summary>
    public string CreateDirectory(string relativePath)
    {
        string path = Path.Combine(RootPath, relativePath);
        Directory.CreateDirectory(path);
        return path;
    }

    /// <summary>
    /// Writes a loose ref file under <c>.git/<paramref name="refPath"/></c>,
    /// optionally setting its modification time.
    /// </summary>
    public void WriteRef(string repoPath, string refPath, string hash, DateTime? mtime = null)
    {
        string fullPath = Path.Combine(repoPath, ".git", refPath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, hash + "\n");
        if (mtime.HasValue)
        {
            File.SetLastWriteTimeUtc(fullPath, mtime.Value);
        }
    }

    /// <summary>
    /// Writes a <c>packed-refs</c> file with the given ref/hash pairs.
    /// </summary>
    public void WritePackedRefs(string repoPath, IEnumerable<(string refPath, string hash)> refs)
    {
        var sb = new StringBuilder("# pack-refs with: peeled fully-peeled sorted\n");
        foreach (var (refPath, hash) in refs)
        {
            sb.Append(hash).Append(' ').AppendLine(refPath);
        }
        File.WriteAllText(Path.Combine(repoPath, ".git", "packed-refs"), sb.ToString());
    }

    /// <summary>
    /// Writes the <c>.git/index</c> file, optionally setting its modification time.
    /// </summary>
    public void WriteIndex(string repoPath, DateTime? lastWriteUtc = null)
    {
        string indexPath = Path.Combine(repoPath, ".git", "index");
        File.WriteAllBytes(indexPath, Array.Empty<byte>());
        if (lastWriteUtc.HasValue)
        {
            File.SetLastWriteTimeUtc(indexPath, lastWriteUtc.Value);
        }
    }

    /// <summary>
    /// Writes a sentinel file that marks an in-progress git operation,
    /// e.g. <c>MERGE_HEAD</c>, <c>CHERRY_PICK_HEAD</c>, <c>REBASE_HEAD</c>.
    /// </summary>
    public void WriteSpecialStateMarker(string repoPath, string fileName)
    {
        File.WriteAllText(Path.Combine(repoPath, ".git", fileName), "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa\n");
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(RootPath))
            {
                Directory.Delete(RootPath, recursive: true);
            }
        }
        catch
        {
            // Best-effort cleanup; temp directories are cleaned by the OS eventually.
        }
    }
}
