using System;
using System.Diagnostics;
using System.IO;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace DevLaunchpad;

/// <summary>
/// Centralized, exception-safe process launching for the extension.
///
/// Every launch is wrapped so that a missing executable or invalid path surfaces a toast to the
/// user and is written to the debug log, rather than throwing out of an <c>Invoke</c> call and
/// risking the out-of-process COM server being torn down.
/// </summary>
internal static class ProcessLauncher
{
    public static CommandResult Start(ProcessStartInfo startInfo)
    {
        ArgumentNullException.ThrowIfNull(startInfo);

        try
        {
            Process.Start(startInfo);
            return CommandResult.Dismiss();
        }
        catch (Exception ex)
        {
            string target = string.IsNullOrWhiteSpace(startInfo.FileName) ? "(unknown)" : startInfo.FileName;
            DevLaunchpadConfig.WriteDebugLog($"Launch failed for '{target}' '{startInfo.Arguments}': {ex}");
            return CommandResult.ShowToast($"Could not launch \"{target}\": {ex.Message}");
        }
    }

    public static CommandResult OpenFolder(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
        {
            return CommandResult.ShowToast($"Folder not found: {path}");
        }

        return Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"\"{path}\"",
            UseShellExecute = true
        });
    }

    public static CommandResult OpenUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return CommandResult.ShowToast("No URL configured.");
        }

        return Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }

    public static CommandResult OpenInEditor(string path)
    {
        var config = DevLaunchpadConfig.Load();

        if (string.IsNullOrWhiteSpace(config.EditorCommand))
        {
            return CommandResult.ShowToast("No editor command configured.");
        }

        return Start(new ProcessStartInfo
        {
            FileName = config.EditorCommand,
            Arguments = $"\"{path}\"",
            UseShellExecute = true
        });
    }

    public static CommandResult OpenInTerminal(string path)
    {
        var config = DevLaunchpadConfig.Load();

        if (string.IsNullOrWhiteSpace(config.TerminalCommand))
        {
            return CommandResult.ShowToast("No terminal command configured.");
        }

        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
        {
            return CommandResult.ShowToast($"Folder not found: {path}");
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = config.TerminalCommand,
            UseShellExecute = true,
            WorkingDirectory = path
        };

        // Windows Terminal launches its profile in its own default directory and ignores the
        // parent process working directory, so it needs an explicit "-d". Classic shells honor
        // WorkingDirectory set above.
        if (IsWindowsTerminal(config.TerminalCommand))
        {
            startInfo.Arguments = $"-d \"{path}\"";
        }

        return Start(startInfo);
    }

    public static CommandResult RunCommand(string target, string arguments)
    {
        if (string.IsNullOrWhiteSpace(target))
        {
            return CommandResult.ShowToast("No command target configured.");
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = target,
            UseShellExecute = true
        };

        if (!string.IsNullOrWhiteSpace(arguments))
        {
            startInfo.Arguments = arguments;
        }

        return Start(startInfo);
    }

    private static bool IsWindowsTerminal(string terminalCommand)
    {
        string exe = Path.GetFileNameWithoutExtension(terminalCommand).ToLowerInvariant();
        return exe is "wt" or "wtd";
    }
}
