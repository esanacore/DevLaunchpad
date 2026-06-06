using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

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
            new ListItem(new SafeInvokableCommand("Open Folder", () => ProcessLauncher.OpenFolder(_repoPath)))
            {
                Title = "Open Folder",
                Subtitle = _repoPath
            },

            new ListItem(new SafeInvokableCommand("Open in Editor", () => ProcessLauncher.OpenInEditor(_repoPath)))
            {
                Title = "Open in Editor",
                Subtitle = _repoPath
            },

            new ListItem(new SafeInvokableCommand("Open in Terminal", () => ProcessLauncher.OpenInTerminal(_repoPath)))
            {
                Title = "Open in Terminal",
                Subtitle = _repoPath
            }
        ];
    }
}
