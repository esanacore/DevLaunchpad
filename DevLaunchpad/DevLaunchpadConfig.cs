using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
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

    public static DevLaunchpadConfig Load()
    {
        string configPath = GetConfigPath();

        try
        {
            EnsureConfigDirectoryExists();
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

    public static string GetConfigDirectory()
    {
        return Path.Combine(ApplicationData.Current.LocalCacheFolder.Path, "DevLaunchpad");
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
        return "Using packaged app storage (LocalCacheFolder)";
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

    public static void ResetToDefaults()
    {
        string configPath = GetConfigPath();
        WriteDefaultConfig(configPath);
        WriteDebugLog("Config reset to defaults.");
    }
    public static void ReloadConfig()
    {
        string configPath = GetConfigPath();
        EnsureDefaultConfigExists(configPath);
        _ = Load();
        WriteDebugLog("Config reloaded.");
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

    public static void OpenConfigFolder()
    {
        EnsureConfigDirectoryExists();

        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = GetConfigDirectory(),
            UseShellExecute = true
        });
    }

    public static void OpenConfigFile()
    {
        string configPath = GetConfigPath();
        EnsureDefaultConfigExists(configPath);

        Process.Start(new ProcessStartInfo
        {
            FileName = configPath,
            UseShellExecute = true
        });
    }

    public static void OpenConfigFileInEditor()
    {
        var config = Load();

        Process.Start(new ProcessStartInfo
        {
            FileName = config.EditorCommand,
            Arguments = $"\"{GetConfigPath()}\"",
            UseShellExecute = true
        });
    }

    public static void OpenDebugLog()
    {
        EnsureConfigDirectoryExists();

        if (!File.Exists(GetDebugLogPath()))
        {
            File.WriteAllText(GetDebugLogPath(), "Debug log created." + Environment.NewLine);
        }

        Process.Start(new ProcessStartInfo
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