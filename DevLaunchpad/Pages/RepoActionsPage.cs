using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System.Diagnostics;

namespace DevLaunchpad.Pages;

public sealed partial class RepoActionsPage : ListPage
{
    private readonly string _repoName;
    private readonly string _repoPath;

    public RepoActionsPage(string repoName, string repoPath)
    {
        _repoName = repoName;
        _repoPath = repoPath;

        Title = repoName;
        Name = $"repo-{repoName}";
    }

    public override IListItem[] GetItems()
    {
        return
        [
            new ListItem(new AnonymousCommand(() => OpenFolder(_repoPath)))
            {
                Title = "Open Folder",
                Subtitle = _repoPath
            },

            new ListItem(new AnonymousCommand(() => OpenInEditor(_repoPath)))
            {
                Title = "Open in Editor",
                Subtitle = _repoPath
            },

            new ListItem(new AnonymousCommand(() => OpenInTerminal(_repoPath)))
            {
                Title = "Open in Terminal",
                Subtitle = _repoPath
            }
        ];
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

    private static void OpenInEditor(string path)
    {
        var config = DevLaunchpadConfig.Load();

        Process.Start(new ProcessStartInfo
        {
            FileName = config.EditorCommand,
            Arguments = $"\"{path}\"",
            UseShellExecute = true
        });
    }

    private static void OpenInTerminal(string path)
    {
        var config = DevLaunchpadConfig.Load();

        Process.Start(new ProcessStartInfo
        {
            FileName = config.TerminalCommand,
            Arguments = $"-d \"{path}\"",
            UseShellExecute = true
        });
    }
}