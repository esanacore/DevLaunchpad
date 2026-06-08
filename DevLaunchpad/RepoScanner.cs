// Copyright (c) Eric Sanacore
// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.Generic;
using System.IO;

namespace DevLaunchpad;

/// <summary>
/// Discovers Git repositories under a root directory.
///
/// Extracted from <see cref="Pages.RepoPage"/> so the scanning logic can be tested
/// independently of the Command Palette UI layer.
/// </summary>
internal static class RepoScanner
{
    /// <summary>Maximum recursion depth for the directory scan.</summary>
    internal const int MaxScanDepth = 6;

    /// <summary>
    /// Directory names that never contain a repo root worth surfacing and are expensive to walk.
    /// </summary>
    internal static readonly HashSet<string> SkipDirectories = new(StringComparer.OrdinalIgnoreCase)
    {
        "node_modules", "bin", "obj", ".vs", ".idea", ".vscode",
        "packages", "dist", "build", "out", "target", "vendor", "__pycache__",
    };

    /// <summary>
    /// Recursively scans <paramref name="repoRoot"/> for Git repositories.
    /// </summary>
    /// <returns>A list of discovered repositories with metadata.</returns>
    public static List<RepoEntry> Scan(string repoRoot)
    {
        var results = new List<RepoEntry>();

        try
        {
            ScanDirectory(repoRoot, results, repoRoot, 0);
        }
        catch
        {
            // Keep the extension resilient if anything unexpected happens.
        }

        return results;
    }

    /// <summary>
    /// Recursive directory walker. Stops descending when a <c>.git</c> directory is found
    /// (that directory is a repo root) or when <paramref name="depth"/> exceeds
    /// <see cref="MaxScanDepth"/>.
    /// </summary>
    internal static void ScanDirectory(string currentPath, List<RepoEntry> results, string repoRoot, int depth)
    {
        try
        {
            if (Directory.Exists(Path.Combine(currentPath, ".git")))
            {
                results.Add(new RepoEntry(
                    Path.GetRelativePath(repoRoot, currentPath),
                    currentPath,
                    GitHelper.GetCurrentBranch(currentPath),
                    GitHelper.GetRemoteWebUrl(currentPath)));
                return;
            }

            if (depth >= MaxScanDepth)
            {
                return;
            }

            foreach (var subdirectory in Directory.GetDirectories(currentPath))
            {
                string name = Path.GetFileName(subdirectory);
                if (SkipDirectories.Contains(name))
                {
                    continue;
                }

                ScanDirectory(subdirectory, results, repoRoot, depth + 1);
            }
        }
        catch
        {
            // Ignore folders we cannot access.
        }
    }
}

/// <summary>
/// Represents a discovered Git repository with display metadata.
/// </summary>
internal readonly record struct RepoEntry(string DisplayName, string FullPath, string? Branch, string? WebUrl);
