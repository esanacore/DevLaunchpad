using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System.Diagnostics;

namespace DevLaunchpad.Pages;

public sealed partial class DevToolsPage : ListPage
{
    public DevToolsPage()
    {
        Title = "Developer Apps";
        Name = "dev-tools";
    }

    public override IListItem[] GetItems()
    {
        var config = DevLaunchpadConfig.Load();

        return
        [
            new ListItem(new SafeInvokableCommand(
                "Open Editor",
                () => ProcessLauncher.Start(new ProcessStartInfo { FileName = config.EditorCommand, UseShellExecute = true })))
            {
                Title = "Open Editor",
                Subtitle = $"Launch {config.EditorCommand}"
            },

            new ListItem(new SafeInvokableCommand(
                "Open PowerShell",
                () => ProcessLauncher.Start(new ProcessStartInfo { FileName = "powershell.exe", UseShellExecute = true })))
            {
                Title = "Open PowerShell",
                Subtitle = "Launch Windows PowerShell"
            },

            new ListItem(new SafeInvokableCommand(
                "Open Terminal",
                () => ProcessLauncher.Start(new ProcessStartInfo { FileName = config.TerminalCommand, UseShellExecute = true })))
            {
                Title = "Open Terminal",
                Subtitle = $"Launch {config.TerminalCommand}"
            }
        ];
    }
}
