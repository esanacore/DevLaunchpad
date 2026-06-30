using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.Storage;

namespace DevLaunchpad;

public sealed class DevLaunchpadConfig
{
    public string RepoRoot { get; set; } = @"C:\Projects";
    public string EditorCommand { get; set; } = "code";
    public string TerminalCommand { get; set; } = "wt";

    public List<NamedUrl> LocalUrls { get; set; } =
    [
        new NamedUrl { Title = "Frontend", Url = "http://localhost:5173" },
        new NamedUrl { Title = "Backend", Url = "http://localhost:8000" },
        new NamedUrl { Title = "React App", Url = "http://localhost:3000" }
    ];

    public List<NamedUrl> FavoriteWebsites { get; set; } =
    [
        new NamedUrl { Title = "GitHub", Url = "https://github.com" },
        new NamedUrl { Title = "ChatGPT", Url = "https://chatgpt.com" }
    ];

    public List<CustomCommandConfig> CustomCommands { get; set; } =
    [
        new CustomCommandConfig
        {
            Title = "Open PowerShell",
            Type = "command",
            Target = "powershell.exe"
        },
        new CustomCommandConfig
        {
            Title = "Open GitHub",
            Type = "url",
            Target = "https://github.com"
        }
    ];

    /// <summary>Full paths of repositories the user pinned to the top of the list.</summary>
    public List<string> PinnedRepos { get; set; } = [];

    /// <summary>Full paths of recently opened repositories, most-recent first.</summary>
    public List<string> RecentRepos { get; set; } = [];

    /// <summary>Maximum number of recent repositories to remember.</summary>
    private const int MaxRecentRepos = 8;

    public static DevLaunchpadConfig Load()
    {
        string configPath = GetConfigPath();

        try
        {
            EnsureConfigDirectoryExists();
            MigrateLegacyConfigIfNeeded();
            EnsureDefaultConfigExists(configPath);

            string json = File.ReadAllText(configPath, Encoding.UTF8);

            if (string.IsNullOrWhiteSpace(json))
            {
                WriteDefaultConfig(configPath);
                json = File.ReadAllText(configPath, Encoding.UTF8);
            }

            var config = JsonSerializer.Deserialize(
                json,
                DevLaunchpadJsonContext.Default.DevLaunchpadConfig);

            if (config == null)
            {
                WriteDefaultConfig(configPath);
                json = File.ReadAllText(configPath, Encoding.UTF8);

                config = JsonSerializer.Deserialize(
                    json,
                    DevLaunchpadJsonContext.Default.DevLaunchpadConfig);
            }

            return config ?? new DevLaunchpadConfig();
        }
        catch (Exception ex)
        {
            WriteDebugLog($"Load failed: {ex}");
            return new DevLaunchpadConfig();
        }
    }

    /// <summary>
    /// Persists this configuration instance to config.json. Used by the in-palette
    /// Settings UI so edits flow back into the single source of truth on disk.
    /// </summary>
    public void Save()
    {
        try
        {
            EnsureConfigDirectoryExists();

            string json = JsonSerializer.Serialize(
                this,
                DevLaunchpadJsonContext.Default.DevLaunchpadConfig);

            File.WriteAllText(
                GetConfigPath(),
                json,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            WriteDebugLog("Config saved.");
        }
        catch (Exception ex)
        {
            WriteDebugLog($"Save failed: {ex}");
            throw;
        }
    }

    /// <summary>
    /// Records <paramref name="repoPath"/> as the most recently opened repository.
    /// Best-effort: failures are logged and swallowed so launching never breaks.
    /// </summary>
    public static void RecordRecentRepo(string repoPath)
    {
        if (string.IsNullOrWhiteSpace(repoPath))
        {
            return;
        }

        try
        {
            var config = Load();
            config.RecentRepos.RemoveAll(p => string.Equals(p, repoPath, StringComparison.OrdinalIgnoreCase));
            config.RecentRepos.Insert(0, repoPath);

            if (config.RecentRepos.Count > MaxRecentRepos)
            {
                config.RecentRepos.RemoveRange(MaxRecentRepos, config.RecentRepos.Count - MaxRecentRepos);
            }

            config.Save();
        }
        catch (Exception ex)
        {
            WriteDebugLog($"RecordRecentRepo failed: {ex}");
        }
    }

    /// <summary>
    /// Toggles whether <paramref name="repoPath"/> is pinned to the top of the list.
    /// </summary>
    public static CommandResult TogglePin(string repoPath)
    {
        if (string.IsNullOrWhiteSpace(repoPath))
        {
            return CommandResult.ShowToast("No repository path provided.");
        }

        var config = Load();
        int removed = config.PinnedRepos.RemoveAll(p => string.Equals(p, repoPath, StringComparison.OrdinalIgnoreCase));

        bool pinnedNow = removed == 0;
        if (pinnedNow)
        {
            config.PinnedRepos.Add(repoPath);
        }

        config.Save();
        return CommandResult.ShowToast(pinnedNow ? "Pinned repository." : "Unpinned repository.");
    }

    /// <summary>
    /// When set, overrides the config directory path returned by <see cref="GetConfigDirectory"/>.
    /// Used by tests to redirect config I/O to a temp directory without requiring a packaged app context.
    /// </summary>
    internal static string? ConfigDirectoryOverride { get; set; }

    public static string GetConfigDirectory()
    {
        if (!string.IsNullOrEmpty(ConfigDirectoryOverride))
        {
            return ConfigDirectoryOverride;
        }

        // LocalFolder persists for the lifetime of the install. LocalCacheFolder (used by earlier
        // versions) can be purged by Windows/Storage Sense, which would silently lose user settings.
        return Path.Combine(ApplicationData.Current.LocalFolder.Path, "DevLaunchpad");
    }

    private static string GetLegacyConfigDirectory()
    {
        return Path.Combine(ApplicationData.Current.LocalCacheFolder.Path, "DevLaunchpad");
    }

    /// <summary>
    /// One-time migration of an existing config from the old LocalCacheFolder location to the
    /// persistent LocalFolder location. No-op once a config exists in the new location.
    /// </summary>
    private static void MigrateLegacyConfigIfNeeded()
    {
        try
        {
            string newConfigPath = GetConfigPath();
            if (File.Exists(newConfigPath))
            {
                return;
            }

            string legacyConfigPath = Path.Combine(GetLegacyConfigDirectory(), "config.json");
            if (!File.Exists(legacyConfigPath))
            {
                return;
            }

            EnsureConfigDirectoryExists();
            File.Copy(legacyConfigPath, newConfigPath, overwrite: false);

            string legacyLog = Path.Combine(GetLegacyConfigDirectory(), "debug.log");
            if (File.Exists(legacyLog) && !File.Exists(GetDebugLogPath()))
            {
                File.Copy(legacyLog, GetDebugLogPath(), overwrite: false);
            }

            WriteDebugLog($"Migrated config from legacy location: {legacyConfigPath}");
        }
        catch (Exception ex)
        {
            WriteDebugLog($"Legacy config migration skipped: {ex.Message}");
        }
    }

    public static string GetConfigPath()
    {
        return Path.Combine(GetConfigDirectory(), "config.json");
    }

    public static string GetDebugLogPath()
    {
        return Path.Combine(GetConfigDirectory(), "debug.log");
    }

    public static string GetStorageDescription()
    {
        return "Using packaged app storage (LocalFolder)";
    }

    public static void EnsureConfigDirectoryExists()
    {
        Directory.CreateDirectory(GetConfigDirectory());
    }

    public static void EnsureDefaultConfigExists(string configPath)
    {
        try
        {
            if (!File.Exists(configPath) || new FileInfo(configPath).Length == 0)
            {
                WriteDefaultConfig(configPath);
            }
        }
        catch (Exception ex)
        {
            WriteDebugLog($"EnsureDefaultConfigExists failed: {ex}");
            throw;
        }
    }

    public static void WriteDefaultConfig(string configPath)
    {
        try
        {
            EnsureConfigDirectoryExists();

            File.WriteAllText(
                configPath,
                GetDefaultConfigJson(),
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            if (!File.Exists(configPath))
            {
                throw new IOException("Config file was not created.");
            }

            if (new FileInfo(configPath).Length == 0)
            {
                throw new IOException("Config file was created but is empty.");
            }

            WriteDebugLog($"Default config written successfully to: {configPath}");
        }
        catch (Exception ex)
        {
            WriteDebugLog($"WriteDefaultConfig failed: {ex}");
            throw;
        }
    }

    public static CommandResult ResetToDefaults()
    {
        string configPath = GetConfigPath();
        WriteDefaultConfig(configPath);
        WriteDebugLog("Config reset to defaults.");
        return CommandResult.ShowToast("Configuration reset to defaults.");
    }

    public static CommandResult ReloadConfig()
    {
        string configPath = GetConfigPath();
        EnsureDefaultConfigExists(configPath);
        _ = Load();
        WriteDebugLog("Config reloaded.");
        return CommandResult.ShowToast("Configuration reloaded from disk.");
    }

    public static CommandResult ExportConfigBackup()
    {
        string configPath = GetConfigPath();
        EnsureDefaultConfigExists(configPath);

        string backupDirectory = Path.Combine(GetConfigDirectory(), "backups");
        Directory.CreateDirectory(backupDirectory);

        string backupStem = $"config-{DateTime.UtcNow:yyyyMMdd-HHmmss}";
        string backupPath = Path.Combine(backupDirectory, $"{backupStem}.json");
        for (int suffix = 1; File.Exists(backupPath); suffix++)
        {
            backupPath = Path.Combine(backupDirectory, $"{backupStem}-{suffix}.json");
        }

        File.Copy(configPath, backupPath, overwrite: false);
        WriteDebugLog($"Config backup exported to: {backupPath}");
        return CommandResult.ShowToast("Configuration backup exported.");
    }

    private static string GetDefaultConfigJson()
    {
        return """
{
  "RepoRoot": "C:\\Projects",
  "EditorCommand": "code",
  "TerminalCommand": "wt",
  "LocalUrls": [
    {
      "Title": "Frontend",
      "Url": "http://localhost:5173"
    },
    {
      "Title": "Backend",
      "Url": "http://localhost:8000"
    },
    {
      "Title": "React App",
      "Url": "http://localhost:3000"
    }
  ],
  "FavoriteWebsites": [
    {
      "Title": "GitHub",
      "Url": "https://github.com"
    },
    {
      "Title": "ChatGPT",
      "Url": "https://chatgpt.com"
    }
  ],
  "CustomCommands": [
    {
      "Title": "Open PowerShell",
      "Type": "command",
      "Target": "powershell.exe",
      "Arguments": ""
    },
    {
      "Title": "Open GitHub",
      "Type": "url",
      "Target": "https://github.com",
      "Arguments": ""
    },
    {
      "Title": "Open Projects Folder",
      "Type": "folder",
      "Target": "C:\\Projects",
      "Arguments": ""
    },
    {
      "Title": "Terminal in Projects",
      "Type": "terminal-in-folder",
      "Target": "C:\\Projects",
      "Arguments": ""
    }
  ]
}
""";
    }

    public static CommandResult OpenConfigFolder()
    {
        EnsureConfigDirectoryExists();
        return ProcessLauncher.OpenFolder(GetConfigDirectory());
    }

    public static CommandResult OpenConfigFile()
    {
        string configPath = GetConfigPath();
        EnsureDefaultConfigExists(configPath);

        return ProcessLauncher.Start(new ProcessStartInfo
        {
            FileName = configPath,
            UseShellExecute = true
        });
    }

    public static CommandResult OpenConfigFileInEditor()
    {
        EnsureDefaultConfigExists(GetConfigPath());
        return ProcessLauncher.OpenInEditor(GetConfigPath());
    }

    public static CommandResult OpenDebugLog()
    {
        EnsureConfigDirectoryExists();

        if (!File.Exists(GetDebugLogPath()))
        {
            File.WriteAllText(GetDebugLogPath(), "Debug log created." + Environment.NewLine);
        }

        return ProcessLauncher.Start(new ProcessStartInfo
        {
            FileName = GetDebugLogPath(),
            UseShellExecute = true
        });
    }

    public static void WriteDebugLog(string message)
    {
        try
        {
            EnsureConfigDirectoryExists();
            File.AppendAllText(
                GetDebugLogPath(),
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
        }
        catch
        {
        }
    }
}

public sealed class NamedUrl
{
    public string Title { get; set; } = "";
    public string Url { get; set; } = "";
}

public sealed class CustomCommandConfig
{
    public string Title { get; set; } = "";
    public string Type { get; set; } = "";
    public string Target { get; set; } = "";
    public string Arguments { get; set; } = "";
}
