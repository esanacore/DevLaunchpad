using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevLaunchpad.Pages;

public sealed partial class CustomCommandsPage : ListPage
{
    public CustomCommandsPage()
    {
        Title = "Custom Commands";
        Name = "custom-commands";
    }

    public override IListItem[] GetItems()
    {
        var items = new List<IListItem>();
        var config = DevLaunchpadConfig.Load();

        if (config.CustomCommands == null || config.CustomCommands.Count == 0)
        {
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "No custom commands configured",
                Subtitle = "Add entries to config.json"
            });

            return items.ToArray();
        }

        foreach (var entry in config.CustomCommands)
        {
            if (string.IsNullOrWhiteSpace(entry.Title) ||
                string.IsNullOrWhiteSpace(entry.Type) ||
                string.IsNullOrWhiteSpace(entry.Target))
            {
                continue;
            }

            string type = entry.Type.Trim().ToLowerInvariant();

            switch (type)
            {
                case "url":
                    items.Add(new ListItem(new OpenUrlCommand(entry.Target))
                    {
                        Title = entry.Title,
                        Subtitle = entry.Target
                    });
                    break;

                case "folder":
                    items.Add(new ListItem(new AnonymousCommand(() => OpenFolder(entry.Target)))
                    {
                        Title = entry.Title,
                        Subtitle = entry.Target
                    });
                    break;

                case "command":
                    items.Add(new ListItem(new AnonymousCommand(() => RunCommand(entry.Target, entry.Arguments)))
                    {
                        Title = entry.Title,
                        Subtitle = BuildSubtitle(entry.Target, entry.Arguments)
                    });
                    break;

                case "terminal-in-folder":
                    items.Add(new ListItem(new AnonymousCommand(() => OpenTerminalInFolder(entry.Target)))
                    {
                        Title = entry.Title,
                        Subtitle = entry.Target
                    });
                    break;
            }
        }

        if (items.Count == 0)
        {
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "No valid custom commands configured",
                Subtitle = "Check config.json"
            });
        }

        return items.ToArray();
    }

    private static string BuildSubtitle(string target, string arguments)
    {
        return string.IsNullOrWhiteSpace(arguments)
            ? target
            : $"{target} {arguments}";
    }

    private static void OpenFolder(string path)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = path,
            UseShellExecute = true
        });
    }

    private static void RunCommand(string target, string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = target,
            UseShellExecute = true
        };

        if (!string.IsNullOrWhiteSpace(arguments))
        {
            startInfo.Arguments = arguments;
        }

        Process.Start(startInfo);
    }

    private static void OpenTerminalInFolder(string path)
    {
        var config = DevLaunchpadConfig.Load();

        Process.Start(new ProcessStartInfo
        {
            FileName = config.TerminalCommand,
            Arguments = $"-d \"{path}\"",
            UseShellExecute = true
        });
    }

    private sealed partial class NoOpCommand : InvokableCommand
    {
        public override CommandResult Invoke() => CommandResult.KeepOpen();
    }
}