// Copyright (c) Eric Sanacore
// SPDX-License-Identifier: GPL-3.0-only

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System.IO;
using System.Text;

namespace DevLaunchpad.Pages;

/// <summary>
/// Bulk-syncs every GitHub repository the authenticated user can access into the configured
/// <c>RepoRoot</c>: clones the ones not present locally and fast-forward-pulls the rest.
///
/// <para>The work is packaged as a generated PowerShell script (<see cref="GitHubSync"/>) that runs
/// in a visible terminal window, so progress over many repositories stays watchable and the palette
/// is not blocked. The script requires the GitHub CLI (<c>gh</c>) and an authenticated session.</para>
/// </summary>
public sealed partial class SyncReposPage : ListPage
{
    // Fluent UI glyphs (Segoe MDL2 Assets).
    private const string SyncGlyph = "\uE895";    // Sync
    private const string ScriptGlyph = "\uE756";  // Command prompt
    private const string FolderGlyph = "\uE8B7";  // Folder

    public SyncReposPage()
    {
        Title = "Sync All GitHub Repos";
        Name = "sync-repos";
    }

    public override IListItem[] GetItems()
    {
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

        if (!Directory.Exists(repoRoot))
        {
            return
            [
                new ListItem(new NoOpCommand())
                {
                    Title = "Projects folder not found",
                    Subtitle = repoRoot,
                },
            ];
        }

        return
        [
            new ListItem(new SafeInvokableCommand(
                "Sync All GitHub Repositories",
                () => RunSync(repoRoot)))
            {
                Title = "Sync All GitHub Repositories",
                Subtitle = $"Clone new + fast-forward pull existing repos into {repoRoot}",
                Icon = new IconInfo(SyncGlyph),
            },

            new ListItem(new SafeInvokableCommand(
                "Copy Sync Script",
                () => ClipboardHelper.CopyText(
                    GitHubSync.BuildSyncScript(),
                    "Copied sync script (PowerShell).")))
            {
                Title = "Copy Sync Script",
                Subtitle = "Copy the PowerShell script to the clipboard to review or run manually",
                Icon = new IconInfo(ScriptGlyph),
            },

            new ListItem(new SafeInvokableCommand(
                "Open Projects Folder",
                () => ProcessLauncher.OpenFolder(repoRoot)))
            {
                Title = "Open Projects Folder",
                Subtitle = repoRoot,
                Icon = new IconInfo(FolderGlyph),
            },
        ];
    }

    /// <summary>
    /// Writes the generated sync script into the config directory and launches it in a visible
    /// PowerShell window rooted at <paramref name="repoRoot"/>. Any I/O failure surfaces as a toast
    /// via the enclosing <see cref="SafeInvokableCommand"/>.
    /// </summary>
    private static CommandResult RunSync(string repoRoot)
    {
        DevLaunchpadConfig.EnsureConfigDirectoryExists();

        string scriptPath = Path.Combine(
            DevLaunchpadConfig.GetConfigDirectory(),
            GitHubSync.ScriptFileName);

        File.WriteAllText(
            scriptPath,
            GitHubSync.BuildSyncScript(),
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        return ProcessLauncher.RunPowerShellScriptVisible(scriptPath, repoRoot);
    }

    private sealed partial class NoOpCommand : InvokableCommand
    {
        public override CommandResult Invoke() => CommandResult.KeepOpen();
    }
}
