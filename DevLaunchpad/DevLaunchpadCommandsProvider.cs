using DevLaunchpad.Pages;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System.Diagnostics;


namespace DevLaunchpad;

/// <summary>
/// Provides command definitions for the Dev Launchpad extension.
/// 
/// This class defines all top-level commands available through the Command Palette,
/// including quick access to developer tools like GitHub, PowerShell, and Visual Studio Code.
/// </summary>
public sealed partial class DevLaunchpadCommandsProvider : CommandProvider
{
    /// <summary>
    /// Collection of command items available in the Command Palette.
    /// </summary>
    private readonly CommandItem[] _commands;

    /// <summary>
    /// Initializes a new instance of the DevLaunchpadCommandsProvider.
    /// 
    /// Sets up the extension display name, icon, and defines all available commands.
    /// Each command is configured with a title, subtitle, and an associated action.
    /// </summary>
    public DevLaunchpadCommandsProvider()
    {
        DisplayName = "Dev Launchpad";
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");

        _commands =
        [
            /// <summary>Quick link to GitHub repository.</summary>
            new CommandItem(
                title: "Open GitHub",
                subtitle: "Launch github.com"
            )
            {
                Command = new OpenUrlCommand("https://github.com")
            },
            /// <summary>Launches PowerShell for command-line operations.</summary>
            new CommandItem(
                title: "Open PowerShell",
                subtitle: "Launch PowerShell"
            )
            {
                Command = new AnonymousCommand(() =>
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "powershell.exe",
                            UseShellExecute = true
                        });
                    })
            },
            /// <summary>Launches Visual Studio Code editor.</summary>
            new CommandItem(
                title: "Open VS Code",
                subtitle: "Launch Visual Studio Code"
            )
            {
                Command = new AnonymousCommand(() =>
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "code",
                            UseShellExecute = true
                        });
                    })
            }
        ];
    }

    /// <summary>
    /// Returns the collection of top-level commands available in the Command Palette.
    /// </summary>
    /// <returns>Array of command items.</returns>
    public override CommandItem[] TopLevelCommands() => _commands;
}