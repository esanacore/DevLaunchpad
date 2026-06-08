using System;
using System.IO;

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
