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

    /// <summary>
    /// Runs a git command in <paramref name="workingDirectory"/>, waits up to 30 seconds for it
    /// to finish, and surfaces errors as a toast. Suitable for quick operations like
    /// <c>git checkout</c>.
    /// </summary>
    public static CommandResult RunGit(string workingDirectory, string arguments)
    {
        if (string.IsNullOrWhiteSpace(workingDirectory))
        {
            return CommandResult.ShowToast("No repository path available.");
        }

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
            };

            using var process = Process.Start(psi);
            if (process is null)
            {
                return CommandResult.ShowToast("Could not start git.");
            }

            string stderr = process.StandardError.ReadToEnd();
            process.WaitForExit(30_000);

            if (process.ExitCode != 0 && !string.IsNullOrWhiteSpace(stderr))
            {
                DevLaunchpadConfig.WriteDebugLog(
                    $"git {arguments} in '{workingDirectory}' exited {process.ExitCode}: {stderr.Trim()}");
                return CommandResult.ShowToast($"git error: {stderr.Trim()}");
            }

            return CommandResult.Dismiss();
        }
        catch (Exception ex)
        {
            DevLaunchpadConfig.WriteDebugLog($"RunGit failed '{arguments}' in '{workingDirectory}': {ex}");
            return CommandResult.ShowToast($"Could not run git: {ex.Message}");
        }
    }

    /// <summary>
    /// Starts a git command in <paramref name="workingDirectory"/> without waiting for it to
    /// finish. Suitable for long-running operations like <c>git clone</c>.
    /// </summary>
    public static CommandResult RunGitBackground(string workingDirectory, string arguments)
    {
        if (string.IsNullOrWhiteSpace(workingDirectory))
        {
            return CommandResult.ShowToast("No working directory configured.");
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "git",
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
            });

            return CommandResult.ShowToast("Git operation started in the background.");
        }
        catch (Exception ex)
        {
            DevLaunchpadConfig.WriteDebugLog($"RunGitBackground failed '{arguments}' in '{workingDirectory}': {ex}");
            return CommandResult.ShowToast($"Could not start git: {ex.Message}");
        }
    }

    /// <summary>
    /// Runs a PowerShell script in a new, visible console window so the user can watch progress of
    /// a long-running operation (e.g. bulk cloning). The window stays open after the script exits
    /// (<c>-NoExit</c>) so results remain readable. The script runs with
    /// <paramref name="workingDirectory"/> as its working directory.
    /// </summary>
    public static CommandResult RunPowerShellScriptVisible(string scriptPath, string workingDirectory)
    {
        if (string.IsNullOrWhiteSpace(scriptPath) || !File.Exists(scriptPath))
        {
            return CommandResult.ShowToast("Sync script not found.");
        }

        if (string.IsNullOrWhiteSpace(workingDirectory) || !Directory.Exists(workingDirectory))
        {
            return CommandResult.ShowToast($"Folder not found: {workingDirectory}");
        }

        return Start(new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoExit -ExecutionPolicy Bypass -File \"{scriptPath}\"",
            WorkingDirectory = workingDirectory,
            UseShellExecute = true,
        });
    }

    /// <summary>
    /// Launches a WSL command in a new terminal window via <c>wsl.exe</c>.
    /// </summary>
    public static CommandResult RunWslCommand(string wslCommand, string arguments)
    {
        if (string.IsNullOrWhiteSpace(wslCommand))
        {
            return CommandResult.ShowToast("No WSL command configured.");
        }

        string wslArgs = string.IsNullOrWhiteSpace(arguments)
            ? $"-- {wslCommand}"
            : $"-- {wslCommand} {arguments}";

        return Start(new ProcessStartInfo
        {
            FileName = "wsl.exe",
            Arguments = wslArgs,
            UseShellExecute = true,
        });
    }

    /// <summary>
    /// Opens an SSH session to <paramref name="host"/> in the configured terminal.
    /// For Windows Terminal, the session runs inside a new tab via <c>wt -- ssh host</c>;
    /// for other terminals, <c>ssh</c> is launched directly.
    /// </summary>
    public static CommandResult OpenSshSession(string host)
    {
        if (string.IsNullOrWhiteSpace(host))
        {
            return CommandResult.ShowToast("No SSH host configured.");
        }

        var config = DevLaunchpadConfig.Load();

        if (string.IsNullOrWhiteSpace(config.TerminalCommand))
        {
            return CommandResult.ShowToast("No terminal command configured.");
        }

        if (IsWindowsTerminal(config.TerminalCommand))
        {
            return Start(new ProcessStartInfo
            {
                FileName = config.TerminalCommand,
                Arguments = $"-- ssh {host}",
                UseShellExecute = true,
            });
        }

        return Start(new ProcessStartInfo
        {
            FileName = "ssh",
            Arguments = host,
            UseShellExecute = true,
        });
    }

    internal static bool IsWindowsTerminal(string terminalCommand)
    {
        string exe = Path.GetFileNameWithoutExtension(terminalCommand).ToLowerInvariant();
        return exe is "wt" or "wtd";
    }
}
