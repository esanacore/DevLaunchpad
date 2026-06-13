using System.IO;
using System.Linq;
using DevLaunchpad.Tests.Helpers;
using Xunit;

namespace DevLaunchpad.Tests;

/// <summary>
/// Tests for <see cref="DevLaunchpadConfig"/> default-config generation, path helpers, and the
/// debug log. The hand-written default JSON literal is validated by round-tripping it back
/// through the deserializer, which catches a malformed default before it ships.
/// </summary>
[Collection(ConfigStateCollection.Name)]
public sealed class ConfigDefaultsTests : IDisposable
{
    private readonly TempConfigDir _configDir = new();

    public void Dispose() => _configDir.Dispose();

    // ── WriteDefaultConfig / default JSON ───────────────────────────

    [Fact]
    public void WriteDefaultConfig_ProducesParseableConfigWithExpectedDefaults()
    {
        DevLaunchpadConfig.WriteDefaultConfig(_configDir.ConfigFilePath);

        Assert.True(File.Exists(_configDir.ConfigFilePath));

        var config = DevLaunchpadConfig.Load();
        Assert.Equal(@"C:\Projects", config.RepoRoot);
        Assert.Equal("code", config.EditorCommand);
        Assert.Equal("wt", config.TerminalCommand);
    }

    [Fact]
    public void DefaultConfig_IncludesAllCustomCommandTypes()
    {
        DevLaunchpadConfig.WriteDefaultConfig(_configDir.ConfigFilePath);

        var config = DevLaunchpadConfig.Load();

        // The default config ships one example of each supported command type so users have a
        // working template for every type. Guard against silently dropping one.
        var types = config.CustomCommands.Select(c => c.Type).ToHashSet();
        Assert.Contains("command", types);
        Assert.Contains("url", types);
        Assert.Contains("folder", types);
        Assert.Contains("terminal-in-folder", types);
    }

    [Fact]
    public void DefaultConfig_HasNonEmptyUrlLists()
    {
        DevLaunchpadConfig.WriteDefaultConfig(_configDir.ConfigFilePath);

        var config = DevLaunchpadConfig.Load();

        Assert.NotEmpty(config.LocalUrls);
        Assert.NotEmpty(config.FavoriteWebsites);
        Assert.All(config.LocalUrls, u => Assert.False(string.IsNullOrWhiteSpace(u.Url)));
        Assert.All(config.FavoriteWebsites, u => Assert.False(string.IsNullOrWhiteSpace(u.Url)));
    }

    // ── EnsureDefaultConfigExists ───────────────────────────────────

    [Fact]
    public void EnsureDefaultConfigExists_CreatesFileWhenMissing()
    {
        Assert.False(File.Exists(_configDir.ConfigFilePath));

        DevLaunchpadConfig.EnsureDefaultConfigExists(_configDir.ConfigFilePath);

        Assert.True(File.Exists(_configDir.ConfigFilePath));
        Assert.True(new FileInfo(_configDir.ConfigFilePath).Length > 0);
    }

    [Fact]
    public void EnsureDefaultConfigExists_RecreatesWhenEmpty()
    {
        // An empty file (e.g. a truncated write) should be treated as missing and rewritten.
        File.WriteAllText(_configDir.ConfigFilePath, "");

        DevLaunchpadConfig.EnsureDefaultConfigExists(_configDir.ConfigFilePath);

        Assert.True(new FileInfo(_configDir.ConfigFilePath).Length > 0);
    }

    [Fact]
    public void EnsureDefaultConfigExists_PreservesExistingNonEmptyConfig()
    {
        var config = DevLaunchpadConfig.Load();
        config.RepoRoot = @"K:\Kept";
        config.Save();

        DevLaunchpadConfig.EnsureDefaultConfigExists(_configDir.ConfigFilePath);

        // A populated config must not be clobbered.
        Assert.Equal(@"K:\Kept", DevLaunchpadConfig.Load().RepoRoot);
    }

    // ── Path helpers ────────────────────────────────────────────────

    [Fact]
    public void GetConfigDirectory_HonorsOverride()
    {
        Assert.Equal(_configDir.DirectoryPath, DevLaunchpadConfig.GetConfigDirectory());
    }

    [Fact]
    public void GetConfigPath_IsConfigJsonWithinDirectory()
    {
        Assert.Equal(
            Path.Combine(_configDir.DirectoryPath, "config.json"),
            DevLaunchpadConfig.GetConfigPath());
    }

    [Fact]
    public void GetDebugLogPath_IsDebugLogWithinDirectory()
    {
        Assert.Equal(
            Path.Combine(_configDir.DirectoryPath, "debug.log"),
            DevLaunchpadConfig.GetDebugLogPath());
    }

    [Fact]
    public void GetStorageDescription_ReturnsNonEmptyDescription()
    {
        Assert.False(string.IsNullOrWhiteSpace(DevLaunchpadConfig.GetStorageDescription()));
    }

    // ── WriteDebugLog ───────────────────────────────────────────────

    [Fact]
    public void WriteDebugLog_AppendsMessageToLogFile()
    {
        DevLaunchpadConfig.WriteDebugLog("first entry");
        DevLaunchpadConfig.WriteDebugLog("second entry");

        Assert.True(File.Exists(DevLaunchpadConfig.GetDebugLogPath()));
        string contents = File.ReadAllText(DevLaunchpadConfig.GetDebugLogPath());
        Assert.Contains("first entry", contents);
        Assert.Contains("second entry", contents);
    }
}
