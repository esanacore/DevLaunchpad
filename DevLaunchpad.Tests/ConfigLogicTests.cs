using System.IO;
using System.Text.Json;
using DevLaunchpad.Tests.Helpers;
using Xunit;

namespace DevLaunchpad.Tests;

/// <summary>
/// Tests for <see cref="DevLaunchpadConfig"/> business logic: Load/Save, recent repos,
/// pinning, and reset. Each test uses an isolated temp directory via <see cref="TempConfigDir"/>.
/// </summary>
[Collection(ConfigStateCollection.Name)]
public sealed class ConfigLogicTests : IDisposable
{
    private readonly TempConfigDir _configDir = new();

    public void Dispose() => _configDir.Dispose();

    // ── Load / Save ─────────────────────────────────────────────────

    [Fact]
    public void Load_CreatesDefaultFile_WhenMissing()
    {
        // The config directory exists (created by TempConfigDir) but no config.json yet.
        Assert.False(File.Exists(_configDir.ConfigFilePath));

        var config = DevLaunchpadConfig.Load();

        Assert.True(File.Exists(_configDir.ConfigFilePath));
        Assert.Equal(@"C:\Projects", config.RepoRoot);
    }

    [Fact]
    public void Load_ReturnsDefaults_OnCorruptJson()
    {
        File.WriteAllText(_configDir.ConfigFilePath, "{ this is not valid json !!!");

        var config = DevLaunchpadConfig.Load();

        // Should fall back to defaults, not throw.
        Assert.NotNull(config);
        Assert.Equal(@"C:\Projects", config.RepoRoot);
    }

    [Fact]
    public void Load_ReturnsDefaults_OnEmptyFile()
    {
        File.WriteAllText(_configDir.ConfigFilePath, "");

        var config = DevLaunchpadConfig.Load();

        Assert.NotNull(config);
        Assert.Equal(@"C:\Projects", config.RepoRoot);
    }

    [Fact]
    public void Save_ThenLoad_RoundTrips()
    {
        var original = DevLaunchpadConfig.Load();
        original.RepoRoot = @"X:\Custom\Path";
        original.EditorCommand = "vim";
        original.TerminalCommand = "bash";
        original.Save();

        var loaded = DevLaunchpadConfig.Load();

        Assert.Equal(@"X:\Custom\Path", loaded.RepoRoot);
        Assert.Equal("vim", loaded.EditorCommand);
        Assert.Equal("bash", loaded.TerminalCommand);
    }

    [Fact]
    public void ResetToDefaults_OverwritesExistingConfig()
    {
        var config = DevLaunchpadConfig.Load();
        config.RepoRoot = @"Z:\Custom";
        config.Save();

        DevLaunchpadConfig.ResetToDefaults();

        var reloaded = DevLaunchpadConfig.Load();
        Assert.Equal(@"C:\Projects", reloaded.RepoRoot);
    }

    [Fact]
    public void ReloadConfig_DoesNotThrow()
    {
        // Ensure a config exists first.
        DevLaunchpadConfig.Load();

        // ReloadConfig should not throw even when called redundantly.
        var result = DevLaunchpadConfig.ReloadConfig();
        Assert.NotNull(result);
    }

    [Fact]
    public void ExportConfigBackup_CopiesCurrentConfigToBackupsFolder()
    {
        var config = DevLaunchpadConfig.Load();
        config.RepoRoot = @"D:\Work";
        config.Save();

        DevLaunchpadConfig.ExportConfigBackup();

        string backupRoot = Path.Combine(_configDir.DirectoryPath, "backups");
        string backupPath = Assert.Single(Directory.GetFiles(backupRoot, "config-*.json"));
        string backupJson = File.ReadAllText(backupPath);

        Assert.Contains(@"""RepoRoot"": ""D:\\Work""", backupJson);
    }

    [Fact]
    public void ExportConfigBackup_CanRunTwiceWithoutOverwriting()
    {
        DevLaunchpadConfig.Load();

        DevLaunchpadConfig.ExportConfigBackup();
        DevLaunchpadConfig.ExportConfigBackup();

        string backupRoot = Path.Combine(_configDir.DirectoryPath, "backups");
        Assert.Equal(2, Directory.GetFiles(backupRoot, "config-*.json").Length);
    }

    // ── RecordRecentRepo ────────────────────────────────────────────

    [Fact]
    public void RecordRecentRepo_AddsToFront()
    {
        // Seed a config file.
        DevLaunchpadConfig.Load();

        DevLaunchpadConfig.RecordRecentRepo(@"C:\Repos\Alpha");
        DevLaunchpadConfig.RecordRecentRepo(@"C:\Repos\Beta");

        var config = DevLaunchpadConfig.Load();
        Assert.True(config.RecentRepos.Count >= 2);
        Assert.Equal(@"C:\Repos\Beta", config.RecentRepos[0]);
        Assert.Equal(@"C:\Repos\Alpha", config.RecentRepos[1]);
    }

    [Fact]
    public void RecordRecentRepo_Deduplicates()
    {
        DevLaunchpadConfig.Load();

        DevLaunchpadConfig.RecordRecentRepo(@"C:\Repos\Alpha");
        DevLaunchpadConfig.RecordRecentRepo(@"C:\Repos\Beta");
        DevLaunchpadConfig.RecordRecentRepo(@"C:\Repos\Alpha"); // re-add

        var config = DevLaunchpadConfig.Load();
        // Alpha should appear only once, at the front.
        Assert.Equal(@"C:\Repos\Alpha", config.RecentRepos[0]);
        int alphaCount = config.RecentRepos.FindAll(p => p == @"C:\Repos\Alpha").Count;
        Assert.Equal(1, alphaCount);
    }

    [Fact]
    public void RecordRecentRepo_CaseInsensitive()
    {
        DevLaunchpadConfig.Load();

        DevLaunchpadConfig.RecordRecentRepo(@"C:\Repos\Alpha");
        DevLaunchpadConfig.RecordRecentRepo(@"c:\repos\alpha"); // different case

        var config = DevLaunchpadConfig.Load();
        // The old entry should have been removed (case-insensitive match),
        // and only the new-cased version should remain.
        int alphaCount = config.RecentRepos.FindAll(
            p => string.Equals(p, @"C:\Repos\Alpha", StringComparison.OrdinalIgnoreCase)).Count;
        Assert.Equal(1, alphaCount);
    }

    [Fact]
    public void RecordRecentRepo_CapsAtMaximum()
    {
        DevLaunchpadConfig.Load();

        // Add 10 repos (max is 8).
        for (int i = 0; i < 10; i++)
        {
            DevLaunchpadConfig.RecordRecentRepo($@"C:\Repos\Repo{i}");
        }

        var config = DevLaunchpadConfig.Load();
        Assert.True(config.RecentRepos.Count <= 8,
            $"Expected at most 8 recent repos, got {config.RecentRepos.Count}");
        // Most recent should be last added.
        Assert.Equal(@"C:\Repos\Repo9", config.RecentRepos[0]);
    }

    [Fact]
    public void RecordRecentRepo_EmptyPath_NoOp()
    {
        DevLaunchpadConfig.Load();

        // These should be silently ignored.
        DevLaunchpadConfig.RecordRecentRepo("");
        DevLaunchpadConfig.RecordRecentRepo(null!);
        DevLaunchpadConfig.RecordRecentRepo("   ");

        var config = DevLaunchpadConfig.Load();
        Assert.Empty(config.RecentRepos);
    }

    // ── TogglePin ───────────────────────────────────────────────────

    [Fact]
    public void TogglePin_PinsNewRepo()
    {
        DevLaunchpadConfig.Load();

        DevLaunchpadConfig.TogglePin(@"C:\Repos\MyRepo");

        var config = DevLaunchpadConfig.Load();
        Assert.Contains(@"C:\Repos\MyRepo", config.PinnedRepos);
    }

    [Fact]
    public void TogglePin_UnpinsExistingRepo()
    {
        DevLaunchpadConfig.Load();

        DevLaunchpadConfig.TogglePin(@"C:\Repos\MyRepo"); // pin
        DevLaunchpadConfig.TogglePin(@"C:\Repos\MyRepo"); // unpin

        var config = DevLaunchpadConfig.Load();
        Assert.DoesNotContain(@"C:\Repos\MyRepo", config.PinnedRepos);
    }

    [Fact]
    public void TogglePin_CaseInsensitive()
    {
        DevLaunchpadConfig.Load();

        DevLaunchpadConfig.TogglePin(@"C:\Repos\MyRepo");
        DevLaunchpadConfig.TogglePin(@"c:\repos\myrepo"); // different case → should unpin

        var config = DevLaunchpadConfig.Load();
        Assert.Empty(config.PinnedRepos);
    }
}
