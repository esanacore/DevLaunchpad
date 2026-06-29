using DevLaunchpad.Tests.Helpers;
using Xunit;

namespace DevLaunchpad.Tests;

/// <summary>
/// Tests for <see cref="DevLaunchpadSettings"/>, the bridge that syncs the in-palette settings
/// form with <c>config.json</c>. Exercises the read and write paths using an isolated temp
/// config directory.
/// </summary>
[Collection(ConfigStateCollection.Name)]
public sealed class DevLaunchpadSettingsTests : IDisposable
{
    // Keys mirror the private constants in DevLaunchpadSettings.
    private const string RepoRootKey = "repoRoot";
    private const string EditorKey = "editorCommand";
    private const string TerminalKey = "terminalCommand";

    private readonly TempConfigDir _configDir = new();

    public void Dispose() => _configDir.Dispose();

    [Fact]
    public void Constructor_ExposesSettingsObject()
    {
        var settings = new DevLaunchpadSettings();

        Assert.NotNull(settings.Settings);
    }

    [Fact]
    public void Constructor_SeedsFormFromPersistedConfig()
    {
        var config = DevLaunchpadConfig.Load();
        config.RepoRoot = @"Q:\Work\Repos";
        config.EditorCommand = "subl";
        config.TerminalCommand = "pwsh";
        config.Save();

        var settings = new DevLaunchpadSettings();

        Assert.Equal(@"Q:\Work\Repos", settings.Settings.GetSetting<string>(RepoRootKey));
        Assert.Equal("subl", settings.Settings.GetSetting<string>(EditorKey));
        Assert.Equal("pwsh", settings.Settings.GetSetting<string>(TerminalKey));
    }

    [Fact]
    public void Constructor_UsesConfigDefaults_WhenNoConfigSaved()
    {
        // No config saved yet; Load() will materialize defaults, which the form should reflect.
        var settings = new DevLaunchpadSettings();

        Assert.Equal(@"C:\Projects", settings.Settings.GetSetting<string>(RepoRootKey));
        Assert.Equal("code", settings.Settings.GetSetting<string>(EditorKey));
        Assert.Equal("wt", settings.Settings.GetSetting<string>(TerminalKey));
    }

    [Fact]
    public void PersistCurrentSettings_WritesFormValuesToConfig()
    {
        var settings = new DevLaunchpadSettings();
        settings.Settings.Update(
            """
            {
              "repoRoot": "D:\\Repos",
              "editorCommand": "vim",
              "terminalCommand": "pwsh"
            }
            """);

        settings.PersistCurrentSettings();

        var config = DevLaunchpadConfig.Load();
        Assert.Equal(@"D:\Repos", config.RepoRoot);
        Assert.Equal("vim", config.EditorCommand);
        Assert.Equal("pwsh", config.TerminalCommand);
    }

    [Fact]
    public void PersistCurrentSettings_IgnoresBlankFormValues()
    {
        var config = DevLaunchpadConfig.Load();
        config.RepoRoot = @"E:\Existing";
        config.EditorCommand = "code-insiders";
        config.TerminalCommand = "wt";
        config.Save();

        var settings = new DevLaunchpadSettings();
        settings.Settings.Update(
            """
            {
              "repoRoot": "   ",
              "editorCommand": "",
              "terminalCommand": "pwsh"
            }
            """);

        settings.PersistCurrentSettings();

        var reloaded = DevLaunchpadConfig.Load();
        Assert.Equal(@"E:\Existing", reloaded.RepoRoot);
        Assert.Equal("code-insiders", reloaded.EditorCommand);
        Assert.Equal("pwsh", reloaded.TerminalCommand);
    }
}
