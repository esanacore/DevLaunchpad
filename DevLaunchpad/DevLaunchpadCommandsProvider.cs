using DevLaunchpad.Pages;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace DevLaunchpad;

/// <summary>
/// Provides command definitions for the Dev Launchpad extension.
/// 
/// This class defines all top-level commands available through the Command Palette,
/// including quick access to repositories, developer tools, local servers, favorite websites,
/// custom commands, and configuration management.
/// </summary>
public sealed partial class DevLaunchpadCommandsProvider : CommandProvider
{
    /// <summary>
    /// Collection of command items available in the Command Palette.
    /// </summary>
    private readonly CommandItem[] _commands;

    /// <summary>
    /// Backing settings model. Seeded from and saved back to config.json.
    /// </summary>
    private readonly DevLaunchpadSettings _settings = new();

    /// <summary>
    /// Initializes a new instance of the DevLaunchpadCommandsProvider.
    /// 
    /// Sets up the extension display name, icon, and defines all available commands and pages.
    /// </summary>
    public DevLaunchpadCommandsProvider()
    {
        DisplayName = "Dev Launchpad";
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");

        // Advertise the in-palette settings form to the Command Palette host.
        Settings = _settings.Settings;

        _commands =
        [
            // Main navigation pages
            new CommandItem(
                title: "Repositories",
                subtitle: "Browse and open Git repositories"
            )
            {
                Command = new RepoPage()
            },

            new CommandItem(
                title: "Developer Tools",
                subtitle: "Quick access to dev applications"
            )
            {
                Command = new DevToolsPage()
            },

            new CommandItem(
                title: "Local Servers",
                subtitle: "Open localhost development URLs"
            )
            {
                Command = new LocalServersPage()
            },

            new CommandItem(
                title: "Favorite Websites",
                subtitle: "Quick access to bookmarked sites"
            )
            {
                Command = new FavoriteWebsitesPage()
            },

            new CommandItem(
                title: "Clone Repository",
                subtitle: "Clone a Git repository by URL into your projects folder"
            )
            {
                Command = new CloneRepoPage()
            },

            new CommandItem(
                title: "Custom Commands",
                subtitle: "Run your configured custom commands"
            )
            {
                Command = new CustomCommandsPage()
            },

            new CommandItem(
                title: "Configuration",
                subtitle: "Manage Dev Launchpad settings"
            )
            {
                Command = new ConfigPage()
            },

            new CommandItem(
                title: "Settings",
                subtitle: "Edit repo root, editor, and terminal in Command Palette"
            )
            {
                Command = _settings.Settings.SettingsPage,
                Icon = new IconInfo("\uE713")
            }
        ];
    }

    /// <summary>
    /// Returns the collection of top-level commands available in the Command Palette.
    /// </summary>
    /// <returns>Array of command items.</returns>
    public override CommandItem[] TopLevelCommands() => _commands;
}