using DevLaunchpad.Tests.Helpers;
using Xunit;

namespace DevLaunchpad.Tests;

/// <summary>
/// Tests for the <see cref="ProcessLauncher"/> guard paths that depend on
/// <see cref="DevLaunchpadConfig"/>. Each test uses an isolated temp config directory so the
/// launcher reads a known editor/terminal command without touching the real user config.
///
/// These exercise only the pre-launch validation branches (empty command, missing folder),
/// which return a toast instead of spawning a process. The success branches are intentionally
/// not exercised because they would launch a real process on the host.
/// </summary>
[Collection(ConfigStateCollection.Name)]
public sealed class ProcessLauncherConfigTests : IDisposable
{
    private readonly TempConfigDir _configDir = new();

    public void Dispose() => _configDir.Dispose();

    private static void SaveConfig(string editor, string terminal)
    {
        var config = DevLaunchpadConfig.Load();
        config.EditorCommand = editor;
        config.TerminalCommand = terminal;
        config.Save();
    }

    // ── OpenInEditor ────────────────────────────────────────────────

    [Fact]
    public void OpenInEditor_NoEditorConfigured_ReturnsToast()
    {
        SaveConfig(editor: "", terminal: "wt");

        var result = ProcessLauncher.OpenInEditor(@"C:\some\path");

        // Empty editor command short-circuits to a toast rather than launching.
        Assert.NotNull(result);
    }

    [Fact]
    public void OpenInEditor_WhitespaceEditorConfigured_ReturnsToast()
    {
        SaveConfig(editor: "   ", terminal: "wt");

        var result = ProcessLauncher.OpenInEditor(@"C:\some\path");

        Assert.NotNull(result);
    }

    // ── OpenInTerminal ──────────────────────────────────────────────

    [Fact]
    public void OpenInTerminal_NoTerminalConfigured_ReturnsToast()
    {
        SaveConfig(editor: "code", terminal: "");

        var result = ProcessLauncher.OpenInTerminal(_configDir.DirectoryPath);

        Assert.NotNull(result);
    }

    [Fact]
    public void OpenInTerminal_NonexistentPath_ReturnsToast()
    {
        // Terminal command is configured, but the target folder does not exist, so the
        // folder-existence guard should short-circuit before any launch attempt.
        SaveConfig(editor: "code", terminal: "wt");

        var result = ProcessLauncher.OpenInTerminal(@"C:\This\Path\Does\Not\Exist\98765");

        Assert.NotNull(result);
    }

    [Fact]
    public void OpenInTerminal_EmptyPath_ReturnsToast()
    {
        SaveConfig(editor: "code", terminal: "wt");

        var result = ProcessLauncher.OpenInTerminal("");

        Assert.NotNull(result);
    }

    // ── Start ───────────────────────────────────────────────────────

    [Fact]
    public void Start_NullStartInfo_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => ProcessLauncher.Start(null!));
    }
}
