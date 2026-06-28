// Copyright (c) Eric Sanacore
// SPDX-License-Identifier: GPL-3.0-only

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.IO;

namespace DevLaunchpad.Pages;

/// <summary>
/// A search-as-you-type page that accepts a Git URL and clones the repository into
/// <c>RepoRoot</c>. The clone runs as a background process (fire-and-forget), so the
/// palette remains responsive for large repositories.
/// </summary>
public sealed partial class CloneRepoPage : DynamicListPage
{
    public CloneRepoPage()
    {
        Title = "Clone Repository";
        Name = "clone-repo";
        PlaceholderText = "Paste a Git URL to clone…";
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
        => RaiseItemsChanged(0);

    public override IListItem[] GetItems()
    {
        string url = SearchText?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(url))
        {
            return
            [
                new ListItem(new NoOpCommand())
                {
                    Title = "Paste a Git URL above to clone",
                    Subtitle = "Supports https://, git@, and ssh:// URLs",
                },
            ];
        }

        string? repoName = ExtractRepoName(url);

        if (repoName is null)
        {
            return
            [
                new ListItem(new NoOpCommand())
                {
                    Title = "Not a recognizable Git URL",
                    Subtitle = url,
                },
            ];
        }

        var config = DevLaunchpadConfig.Load();
        string repoRoot = config.RepoRoot;

        if (string.IsNullOrWhiteSpace(repoRoot))
        {
            return
            [
                new ListItem(new NoOpCommand())
                {
                    Title = "Repo root not configured",
                    Subtitle = "Set RepoRoot in Settings first",
                },
            ];
        }

        string dest = Path.Combine(repoRoot, repoName);
        string capturedUrl = url;

        return
        [
            new ListItem(new SafeInvokableCommand(
                $"Clone {repoName}",
                () => ProcessLauncher.RunGitBackground(repoRoot, $"clone \"{capturedUrl}\" \"{dest}\"")))
            {
                Title = $"Clone {repoName}",
                Subtitle = $"→ {dest}",
            },
        ];
    }

    /// <summary>
    /// Extracts the repository name from a Git URL by taking the last path segment
    /// and stripping any trailing <c>.git</c> suffix. Returns <c>null</c> for unrecognized inputs.
    /// </summary>
    internal static string? ExtractRepoName(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        string trimmed = url.TrimEnd('/', '\\');

        if (trimmed.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed[..^4];
        }

        // For scp-style URLs like git@github.com:owner/repo — work on the path after ':'
        if (trimmed.StartsWith("git@", StringComparison.OrdinalIgnoreCase))
        {
            int colon = trimmed.IndexOf(':');
            if (colon >= 0)
            {
                trimmed = trimmed[(colon + 1)..];
            }
        }

        int lastSlash = trimmed.LastIndexOfAny(['/', '\\', ':']);
        if (lastSlash < 0)
        {
            return null;
        }

        string name = trimmed[(lastSlash + 1)..];
        return string.IsNullOrWhiteSpace(name) ? null : name;
    }

    private sealed partial class NoOpCommand : InvokableCommand
    {
        public override CommandResult Invoke() => CommandResult.KeepOpen();
    }
}
