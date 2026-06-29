// Copyright (c) Eric Sanacore
// SPDX-License-Identifier: GPL-3.0-only

using System;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace DevLaunchpad;

/// <summary>
/// Bridges the Command Palette in-palette settings form to <see cref="DevLaunchpadConfig"/>.
///
/// The toolkit <see cref="Settings"/> object renders an editable form (toggles, text
/// fields, choice sets) directly inside Command Palette. Rather than introduce a second store,
/// the form is seeded from <c>config.json</c> and changes are written straight back to it, so
/// <c>config.json</c> remains the single source of truth read by every page.
/// </summary>
public sealed class DevLaunchpadSettings
{
    private const string RepoRootKey = "repoRoot";
    private const string EditorKey = "editorCommand";
    private const string TerminalKey = "terminalCommand";

    private readonly Settings _settings = new();

    public DevLaunchpadSettings()
    {
        var config = DevLaunchpadConfig.Load();

        _settings.Add(new TextSetting(RepoRootKey, config.RepoRoot)
        {
            Label = "Repository root folder",
            Description = "Folder scanned recursively for Git repositories (e.g. C:\\Projects).",
        });

        _settings.Add(new TextSetting(EditorKey, config.EditorCommand)
        {
            Label = "Editor command",
            Description = "Executable used for \"Open in Editor\" actions (e.g. code).",
        });

        _settings.Add(new TextSetting(TerminalKey, config.TerminalCommand)
        {
            Label = "Terminal command",
            Description = "Executable used for \"Open in Terminal\" actions (e.g. wt, pwsh, powershell, cmd).",
        });

        _settings.SettingsChanged += OnSettingsChanged;
    }

    /// <summary>
    /// The toolkit settings object. Assign to <c>CommandProvider.Settings</c> and expose
    /// <c>Settings.SettingsPage</c> as a command to surface the form in Command Palette.
    /// </summary>
    public Settings Settings => _settings;

    private void OnSettingsChanged(object sender, Settings args)
    {
        PersistCurrentSettings();
    }

    internal void PersistCurrentSettings()
    {
        try
        {
            var config = DevLaunchpadConfig.Load();

            var repoRoot = _settings.GetSetting<string>(RepoRootKey);
            var editor = _settings.GetSetting<string>(EditorKey);
            var terminal = _settings.GetSetting<string>(TerminalKey);

            if (!string.IsNullOrWhiteSpace(repoRoot))
            {
                config.RepoRoot = repoRoot;
            }

            if (!string.IsNullOrWhiteSpace(editor))
            {
                config.EditorCommand = editor;
            }

            if (!string.IsNullOrWhiteSpace(terminal))
            {
                config.TerminalCommand = terminal;
            }

            config.Save();
        }
        catch (Exception ex)
        {
            DevLaunchpadConfig.WriteDebugLog($"Persisting settings failed: {ex}");
        }
    }
}
