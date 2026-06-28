// Copyright (c) Eric Sanacore
// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.IO;
using System.Linq;

namespace DevLaunchpad;

/// <summary>
/// Lightweight, read-only Git inspection that never spawns a process.
///
/// Branch and remote information is read straight from the repository's <c>.git</c> directory,
/// so it is cheap enough to call for every repository while building the list. Anything that
/// would require running <c>git</c> (e.g. dirty-status, fetch, pull) is intentionally left out
/// of the hot path.
/// </summary>
internal static class GitHelper
{
    /// <summary>
    /// Returns the current branch name, or a short commit hash when in detached HEAD state.
    /// Returns <c>null</c> if the branch cannot be determined.
    /// </summary>
    public static string? GetCurrentBranch(string repoPath)
    {
        try
        {
            string headPath = Path.Combine(repoPath, ".git", "HEAD");
            if (!File.Exists(headPath))
            {
                return null;
            }

            string head = File.ReadAllText(headPath).Trim();

            const string refPrefix = "ref:";
            if (head.StartsWith(refPrefix, StringComparison.Ordinal))
            {
                string reference = head[refPrefix.Length..].Trim();
                const string headsPrefix = "refs/heads/";
                return reference.StartsWith(headsPrefix, StringComparison.Ordinal)
                    ? reference[headsPrefix.Length..]
                    : reference;
            }

            // Detached HEAD: the file holds a raw commit hash.
            return head.Length >= 7 ? head[..7] : head;
        }
        catch (Exception ex)
        {
            DevLaunchpadConfig.WriteDebugLog($"GetCurrentBranch failed for '{repoPath}': {ex}");
            return null;
        }
    }

    /// <summary>
    /// Resolves the origin remote to a browsable HTTPS URL (e.g. https://github.com/owner/repo),
    /// translating SSH-style remotes. Returns <c>null</c> when no usable remote is found.
    /// </summary>
    public static string? GetRemoteWebUrl(string repoPath)
    {
        try
        {
            string configPath = Path.Combine(repoPath, ".git", "config");
            if (!File.Exists(configPath))
            {
                return null;
            }

            string? url = ParseOriginUrl(File.ReadAllLines(configPath));
            return url is null ? null : NormalizeRemoteUrl(url);
        }
        catch (Exception ex)
        {
            DevLaunchpadConfig.WriteDebugLog($"GetRemoteWebUrl failed for '{repoPath}': {ex}");
            return null;
        }
    }

    internal static string? ParseOriginUrl(string[] lines)
    {
        bool inOrigin = false;
        foreach (string raw in lines)
        {
            string line = raw.Trim();

            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                inOrigin = line.Replace(" ", string.Empty)
                    .Equals("[remote\"origin\"]", StringComparison.OrdinalIgnoreCase);
                continue;
            }

            if (inOrigin && line.StartsWith("url", StringComparison.OrdinalIgnoreCase))
            {
                int eq = line.IndexOf('=');
                if (eq >= 0)
                {
                    return line[(eq + 1)..].Trim();
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Builds an HTTPS <c>git clone</c> command line for a normalized web URL
    /// (the kind returned by <see cref="GetRemoteWebUrl"/>). Returns <c>null</c> for empty input.
    /// </summary>
    public static string? BuildCloneCommand(string webUrl)
    {
        if (string.IsNullOrWhiteSpace(webUrl))
        {
            return null;
        }

        string cloneUrl = webUrl.EndsWith(".git", StringComparison.OrdinalIgnoreCase)
            ? webUrl
            : webUrl + ".git";

        return $"git clone {cloneUrl}";
    }

    /// <summary>
    /// Derives the issues page URL for a normalized web URL on a recognized host
    /// (GitHub, GitLab, Bitbucket). Returns <c>null</c> for unrecognized hosts or empty input.
    /// </summary>
    public static string? GetIssuesUrl(string webUrl)
    {
        string? baseUrl = HostKind(webUrl, out string kind);
        if (baseUrl is null)
        {
            return null;
        }

        return kind switch
        {
            "github" => $"{baseUrl}/issues",
            "gitlab" => $"{baseUrl}/-/issues",
            "bitbucket" => $"{baseUrl}/issues",
            _ => null,
        };
    }

    /// <summary>
    /// Derives the pull/merge requests page URL for a normalized web URL on a recognized host.
    /// Returns <c>null</c> for unrecognized hosts or empty input.
    /// </summary>
    public static string? GetPullRequestsUrl(string webUrl)
    {
        string? baseUrl = HostKind(webUrl, out string kind);
        if (baseUrl is null)
        {
            return null;
        }

        return kind switch
        {
            "github" => $"{baseUrl}/pulls",
            "gitlab" => $"{baseUrl}/-/merge_requests",
            "bitbucket" => $"{baseUrl}/pull-requests",
            _ => null,
        };
    }

    /// <summary>
    /// The label a host uses for change proposals: "Merge Requests" on GitLab,
    /// "Pull Requests" elsewhere. Returns <c>null</c> when the host is unrecognized.
    /// </summary>
    public static string? GetPullRequestsLabel(string webUrl)
    {
        if (HostKind(webUrl, out string kind) is null)
        {
            return null;
        }

        return kind == "gitlab" ? "Merge Requests" : "Pull Requests";
    }

    /// <summary>
    /// Classifies the host of <paramref name="webUrl"/> as "github", "gitlab", or "bitbucket".
    /// Returns the trimmed base URL (out via the return value) and sets <paramref name="kind"/>,
    /// or <c>null</c> when the URL is empty/unparseable or the host is unrecognized.
    /// </summary>
    private static string? HostKind(string webUrl, out string kind)
    {
        kind = string.Empty;

        if (string.IsNullOrWhiteSpace(webUrl) ||
            !Uri.TryCreate(webUrl, UriKind.Absolute, out Uri? uri))
        {
            return null;
        }

        string host = uri.Host.ToLowerInvariant();
        string baseUrl = webUrl.TrimEnd('/');

        if (host.Contains("github", StringComparison.Ordinal))
        {
            kind = "github";
        }
        else if (host.Contains("gitlab", StringComparison.Ordinal))
        {
            kind = "gitlab";
        }
        else if (host.Contains("bitbucket", StringComparison.Ordinal))
        {
            kind = "bitbucket";
        }
        else
        {
            return null;
        }

        return baseUrl;
    }

    /// <summary>
    /// Reads the commit hash for <paramref name="refPath"/> (relative to <c>.git</c>,
    /// e.g. <c>refs/heads/main</c>) from the loose ref file, falling back to
    /// <c>packed-refs</c>. Returns <c>null</c> when the ref cannot be resolved.
    /// </summary>
    internal static string? ReadRefHash(string repoPath, string refPath)
    {
        try
        {
            // 1. Loose ref file.
            string loosePath = Path.Combine(repoPath, ".git", refPath.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(loosePath))
            {
                string content = File.ReadAllText(loosePath).Trim();
                return content.Length == 40 ? content : null;
            }

            // 2. packed-refs: plain text, one "<40-char-hash> <ref>" line per entry.
            string packedRefsPath = Path.Combine(repoPath, ".git", "packed-refs");
            if (!File.Exists(packedRefsPath))
            {
                return null;
            }

            foreach (string line in File.ReadAllLines(packedRefsPath))
            {
                if (line.Length == 0 || line[0] == '#' || line[0] == '^')
                {
                    continue;
                }

                int space = line.IndexOf(' ');
                if (space == 40 && line.Length > 41 &&
                    line[(space + 1)..].TrimEnd().Equals(refPath, StringComparison.Ordinal))
                {
                    return line[..40];
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            DevLaunchpadConfig.WriteDebugLog($"ReadRefHash failed for '{repoPath}' ref '{refPath}': {ex}");
            return null;
        }
    }

    /// <summary>
    /// Returns the commit hash the local branch points to, or <c>null</c> if unavailable.
    /// </summary>
    public static string? GetLocalBranchHash(string repoPath, string branch)
        => ReadRefHash(repoPath, $"refs/heads/{branch}");

    /// <summary>
    /// Returns the commit hash for <c>origin/<paramref name="branch"/></c>, or <c>null</c>
    /// when no remote-tracking ref exists.
    /// </summary>
    public static string? GetRemoteTrackingHash(string repoPath, string branch)
        => ReadRefHash(repoPath, $"refs/remotes/origin/{branch}");

    /// <summary>
    /// Returns the name of any in-progress git operation ("merge", "cherry-pick", "rebase"),
    /// or <c>null</c> when the working tree is in a normal state.
    /// </summary>
    internal static string? GetSpecialState(string repoPath)
    {
        try
        {
            string gitDir = Path.Combine(repoPath, ".git");

            if (File.Exists(Path.Combine(gitDir, "MERGE_HEAD")))
            {
                return "merge";
            }
            if (File.Exists(Path.Combine(gitDir, "CHERRY_PICK_HEAD")))
            {
                return "cherry-pick";
            }
            if (File.Exists(Path.Combine(gitDir, "REBASE_HEAD")) ||
                Directory.Exists(Path.Combine(gitDir, "rebase-merge")) ||
                Directory.Exists(Path.Combine(gitDir, "rebase-apply")))
            {
                return "rebase";
            }

            return null;
        }
        catch (Exception ex)
        {
            DevLaunchpadConfig.WriteDebugLog($"GetSpecialState failed for '{repoPath}': {ex}");
            return null;
        }
    }

    /// <summary>
    /// Returns <c>true</c> when the repository appears dirty.
    ///
    /// <para><b>Limitations (filesystem-only, no process spawn):</b></para>
    /// <list type="bullet">
    ///   <item>In-progress operations (merge, rebase, cherry-pick) are detected accurately.</item>
    ///   <item>Staged changes are inferred by comparing the modification time of
    ///         <c>.git/index</c> against the loose branch ref: a newer index means something
    ///         was staged since the last commit. Only works when the branch ref is a loose
    ///         file; packed-refs lack the necessary mtime.</item>
    ///   <item>Unstaged changes to tracked files are <em>not</em> detected.</item>
    /// </list>
    /// </summary>
    public static bool IsDirty(string repoPath, string? branch)
    {
        try
        {
            if (GetSpecialState(repoPath) is not null)
            {
                return true;
            }

            if (branch is null)
            {
                return false;
            }

            string indexPath = Path.Combine(repoPath, ".git", "index");
            if (!File.Exists(indexPath))
            {
                return false;
            }

            // Build the path to the loose branch ref, handling slash-separated branch names
            // like "feature/my-branch" on all platforms.
            string[] branchParts = branch.Split('/');
            string refFilePath = Path.Combine([repoPath, ".git", "refs", "heads", .. branchParts]);

            if (!File.Exists(refFilePath))
            {
                // Branch is in packed-refs; mtime comparison unavailable.
                return false;
            }

            return File.GetLastWriteTimeUtc(indexPath) > File.GetLastWriteTimeUtc(refFilePath);
        }
        catch (Exception ex)
        {
            DevLaunchpadConfig.WriteDebugLog($"IsDirty failed for '{repoPath}': {ex}");
            return false;
        }
    }

    internal static string? NormalizeRemoteUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        // Strip a trailing ".git" suffix.
        if (url.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
        {
            url = url[..^4];
        }

        if (url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
            url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            return url;
        }

        // scp-like syntax: git@host:owner/repo  ->  https://host/owner/repo
        if (url.StartsWith("git@", StringComparison.OrdinalIgnoreCase))
        {
            string rest = url["git@".Length..];
            int colon = rest.IndexOf(':');
            if (colon > 0)
            {
                string host = rest[..colon];
                string path = rest[(colon + 1)..].TrimStart('/');
                return $"https://{host}/{path}";
            }
        }

        // ssh://git@host/owner/repo  ->  https://host/owner/repo
        if (url.StartsWith("ssh://", StringComparison.OrdinalIgnoreCase))
        {
            string rest = url["ssh://".Length..];
            if (rest.StartsWith("git@", StringComparison.OrdinalIgnoreCase))
            {
                rest = rest["git@".Length..];
            }

            return $"https://{rest}";
        }

        return null;
    }
}
