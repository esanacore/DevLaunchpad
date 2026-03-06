using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace DevLaunchpad.Pages;

public sealed partial class ConfigPage : ListPage
{
    public ConfigPage()
    {
        Title = "Configuration";
        Name = "config";
    }

    public override IListItem[] GetItems()
    {
        var config = DevLaunchpadConfig.Load();
        string configPath = DevLaunchpadConfig.GetConfigPath();
        string configDir = DevLaunchpadConfig.GetConfigDirectory();
        string debugLogPath = DevLaunchpadConfig.GetDebugLogPath();
        string storageDescription = DevLaunchpadConfig.GetStorageDescription();

        return
        [
            new ListItem(new NoOpCommand())
            {
                Title = "Storage Mode",
                Subtitle = storageDescription
            },

            new ListItem(new NoOpCommand())
            {
                Title = "Current Repo Root",
                Subtitle = config.RepoRoot
            },

            new ListItem(new NoOpCommand())
            {
                Title = "Editor Command",
                Subtitle = config.EditorCommand
            },

            new ListItem(new NoOpCommand())
            {
                Title = "Terminal Command",
                Subtitle = config.TerminalCommand
            },

            new ListItem(new NoOpCommand())
            {
                Title = "Config File",
                Subtitle = configPath
            },

            new ListItem(new NoOpCommand())
            {
                Title = "Debug Log",
                Subtitle = debugLogPath
            },

            new ListItem(new AnonymousCommand(() => DevLaunchpadConfig.OpenConfigFolder()))
            {
                Title = "Open Config Folder",
                Subtitle = configDir
            },

            new ListItem(new AnonymousCommand(() => DevLaunchpadConfig.OpenConfigFile()))
            {
                Title = "Open Config File",
                Subtitle = "Open config.json with the default app"
            },

            new ListItem(new AnonymousCommand(() => DevLaunchpadConfig.OpenConfigFileInEditor()))
            {
                Title = "Open Config File in Editor",
                Subtitle = "Edit config.json using your configured editor"
            },

            new ListItem(new AnonymousCommand(() => DevLaunchpadConfig.OpenDebugLog()))
            {
                Title = "Open Debug Log",
                Subtitle = "Inspect config creation and load issues"
            },

            new ListItem(new AnonymousCommand(() => DevLaunchpadConfig.ReloadConfig()))
            {
                Title = "Reload Config",
                Subtitle = "Reload config.json from disk"
            },

            new ListItem(new AnonymousCommand(() => DevLaunchpadConfig.ResetToDefaults()))
            {
                Title = "Reset Config to Defaults",
                Subtitle = "Overwrite config.json with default settings"
            }
        ];
    }

    private sealed partial class NoOpCommand : InvokableCommand
    {
        public override CommandResult Invoke() => CommandResult.KeepOpen();
    }
}