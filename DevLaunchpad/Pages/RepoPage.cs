using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DevLaunchpad.Pages;

public sealed partial class RepoPage : ListPage
{
    public RepoPage()
    {
        Title = "Repositories";
        Name = "repos";
    }

    public override IListItem[] GetItems()
    {
        var items = new List<IListItem>();

        var config = DevLaunchpadConfig.Load();
        string repoRoot = config.RepoRoot;

        if (string.IsNullOrWhiteSpace(repoRoot) || !Directory.Exists(repoRoot))
        {
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "Projects folder not found",
                Subtitle = repoRoot
            });

            return items.ToArray();
        }

        var repoDirectories = FindGitRepos(repoRoot);

        foreach (var directory in repoDirectories.OrderBy(path => path))
        {
            string displayName = Path.GetRelativePath(repoRoot, directory);

            items.Add(new ListItem(new RepoActionsPage(displayName, directory))
            {
                Title = displayName,
                Subtitle = directory
            });
        }

        if (items.Count == 0)
        {
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "No git repositories found",
                Subtitle = repoRoot
            });
        }

        return items.ToArray();
    }

    private static List<string> FindGitRepos(string rootPath)
    {
        var results = new List<string>();

        try
        {
            ScanDirectory(rootPath, results);
        }
        catch
        {
            // Keep the extension resilient if anything unexpected happens.
        }

        return results;
    }

    private static void ScanDirectory(string currentPath, List<string> results)
    {
        try
        {
            if (Directory.Exists(Path.Combine(currentPath, ".git")))
            {
                results.Add(currentPath);
                return;
            }

            foreach (var subdirectory in Directory.GetDirectories(currentPath))
            {
                ScanDirectory(subdirectory, results);
            }
        }
        catch
        {
            // Ignore folders we cannot access.
        }
    }

    private sealed partial class NoOpCommand : InvokableCommand
    {
        public override CommandResult Invoke() => CommandResult.KeepOpen();
    }
}