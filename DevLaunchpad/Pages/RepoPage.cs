using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DevLaunchpad.Pages;

public sealed partial class RepoPage : DynamicListPage
{
    // Bound the recursive scan so a deep or pathological tree can't hang the extension.
    private const int MaxScanDepth = 6;

    // Fluent UI glyphs (Segoe MDL2 Assets).
    private const string PinnedGlyph = "\uE841";  // Pinned
    private const string RepoGlyph = "\uE8B7";    // Folder

    // Directories that never contain a repo root worth surfacing and are expensive to walk.
    private static readonly HashSet<string> SkipDirectories = new(StringComparer.OrdinalIgnoreCase)
    {
        "node_modules", "bin", "obj", ".vs", ".idea", ".vscode",
        "packages", "dist", "build", "out", "target", "vendor", "__pycache__",
    };

    private readonly object _cacheLock = new();
    private List<RepoEntry>? _cache;
    private string _cachedRoot = string.Empty;

    public RepoPage()
    {
        Title = "Repositories";
        Name = "repos";
        PlaceholderText = "Search repositories...";
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        // Items are filtered in-memory from the cached scan, so just re-raise.
        RaiseItemsChanged(0);
    }

    public override IListItem[] GetItems()
    {
        var config = DevLaunchpadConfig.Load();
        string repoRoot = config.RepoRoot;

        if (string.IsNullOrWhiteSpace(repoRoot) || !Directory.Exists(repoRoot))
        {
            return
            [
                new ListItem(new NoOpCommand())
                {
                    Title = "Projects folder not found",
                    Subtitle = string.IsNullOrWhiteSpace(repoRoot) ? "(repo root not configured)" : repoRoot,
                },
            ];
        }

        var repos = GetRepos(repoRoot);

        string query = SearchText;
        IEnumerable<RepoEntry> matches = repos;
        if (!string.IsNullOrWhiteSpace(query))
        {
            matches = repos.Where(r =>
                r.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                r.FullPath.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        var ordered = matches
            .OrderBy(r => RankIn(config.PinnedRepos, r.FullPath) == int.MaxValue ? 1 : 0)
            .ThenBy(r => RankIn(config.PinnedRepos, r.FullPath))
            .ThenBy(r => RankIn(config.RecentRepos, r.FullPath))
            .ThenBy(r => r.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var items = ordered
            .Select(r => (IListItem)BuildRepoItem(r, IsPinned(config.PinnedRepos, r.FullPath)))
            .ToList();

        if (items.Count == 0)
        {
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = string.IsNullOrWhiteSpace(query) ? "No git repositories found" : "No matching repositories",
                Subtitle = repoRoot,
            });
        }

        return items.ToArray();
    }

    private ListItem BuildRepoItem(RepoEntry repo, bool pinned)
    {
        string path = repo.FullPath;
        string? webUrl = repo.WebUrl;

        string subtitle = repo.Branch is null ? path : $"[{repo.Branch}]  {path}";

        var moreCommands = new List<IContextItem>
        {
            new CommandContextItem(LaunchCommand("Open Folder", path, () => ProcessLauncher.OpenFolder(path))),
            new CommandContextItem(LaunchCommand("Open in Terminal", path, () => ProcessLauncher.OpenInTerminal(path))),
            new Separator(),
            new CommandContextItem(new SafeInvokableCommand(
                "Copy Path",
                () => ClipboardHelper.CopyText(path, "Copied repository path."))),
        };

        if (webUrl is not null)
        {
            string label = webUrl.Contains("github.com", StringComparison.OrdinalIgnoreCase)
                ? "Open on GitHub"
                : "Open Remote in Browser";
            moreCommands.Add(new CommandContextItem(LaunchCommand(
                label, path, () => ProcessLauncher.OpenUrl(webUrl))));
        }

        moreCommands.Add(new Separator());
        moreCommands.Add(new CommandContextItem(new SafeInvokableCommand(
            pinned ? "Unpin" : "Pin",
            () =>
            {
                var result = DevLaunchpadConfig.TogglePin(path);
                RaiseItemsChanged(0);
                return result;
            })));

        return new ListItem(LaunchCommand("Open in Editor", path, () => ProcessLauncher.OpenInEditor(path)))
        {
            Title = repo.DisplayName,
            Subtitle = subtitle,
            Icon = new IconInfo(pinned ? PinnedGlyph : RepoGlyph),
            MoreCommands = moreCommands.ToArray(),
        };
    }

    // Wraps a launch so the repository is recorded as recently used before it opens.
    private static SafeInvokableCommand LaunchCommand(string name, string repoPath, Func<CommandResult> launch)
    {
        return new SafeInvokableCommand(name, () =>
        {
            DevLaunchpadConfig.RecordRecentRepo(repoPath);
            return launch();
        });
    }

    private static int RankIn(List<string> list, string path)
    {
        int index = list.FindIndex(p => string.Equals(p, path, StringComparison.OrdinalIgnoreCase));
        return index < 0 ? int.MaxValue : index;
    }

    private static bool IsPinned(List<string> pinned, string path)
        => pinned.Any(p => string.Equals(p, path, StringComparison.OrdinalIgnoreCase));

    private List<RepoEntry> GetRepos(string repoRoot)
    {
        lock (_cacheLock)
        {
            if (_cache != null && string.Equals(_cachedRoot, repoRoot, StringComparison.OrdinalIgnoreCase))
            {
                return _cache;
            }

            var discovered = new List<RepoEntry>();
            try
            {
                ScanDirectory(repoRoot, discovered, repoRoot, 0);
            }
            catch
            {
                // Keep the extension resilient if anything unexpected happens.
            }

            _cache = discovered;
            _cachedRoot = repoRoot;
            return discovered;
        }
    }

    private static void ScanDirectory(string currentPath, List<RepoEntry> results, string repoRoot, int depth)
    {
        try
        {
            if (Directory.Exists(Path.Combine(currentPath, ".git")))
            {
                results.Add(new RepoEntry(
                    Path.GetRelativePath(repoRoot, currentPath),
                    currentPath,
                    GitHelper.GetCurrentBranch(currentPath),
                    GitHelper.GetRemoteWebUrl(currentPath)));
                return;
            }

            if (depth >= MaxScanDepth)
            {
                return;
            }

            foreach (var subdirectory in Directory.GetDirectories(currentPath))
            {
                string name = Path.GetFileName(subdirectory);
                if (SkipDirectories.Contains(name))
                {
                    continue;
                }

                ScanDirectory(subdirectory, results, repoRoot, depth + 1);
            }
        }
        catch
        {
            // Ignore folders we cannot access.
        }
    }

    private readonly record struct RepoEntry(string DisplayName, string FullPath, string? Branch, string? WebUrl);

    private sealed partial class NoOpCommand : InvokableCommand
    {
        public override CommandResult Invoke() => CommandResult.KeepOpen();
    }
}
