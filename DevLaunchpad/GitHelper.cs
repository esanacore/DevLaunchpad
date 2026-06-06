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

    private static string? ParseOriginUrl(string[] lines)
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

    private static string? NormalizeRemoteUrl(string url)
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
