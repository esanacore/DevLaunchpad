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
        return
        [
            new ListItem(new AnonymousCommand(() =>
            {
                Process.Start("code");
            }))
            {
                Title = "Open VS Code",
                Subtitle = "Launch Visual Studio Code"
            },

            new ListItem(new AnonymousCommand(() =>
            {
                Process.Start("powershell.exe");
            }))
            {
                Title = "Open PowerShell",
                Subtitle = "Launch Windows PowerShell"
            },

            new ListItem(new AnonymousCommand(() =>
            {
                Process.Start("wt");
            }))
            {
                Title = "Open Windows Terminal",
                Subtitle = "Launch Windows Terminal"
            }
        ];
    }
}