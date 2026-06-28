using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DevLaunchpad.Pages;

public sealed partial class RepoPage : DynamicListPage
{
    // Fluent UI glyphs (Segoe MDL2 Assets).
    private const string PinnedGlyph = "\uE841";  // Pinned
    private const string RepoGlyph = "\uE8B7";    // Folder

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
                r.FullPath.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                (r.ProjectType is not null &&
                 r.ProjectType.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                StatusTokens(r.GitStatus).Contains(query, StringComparison.OrdinalIgnoreCase));
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

        string subtitle = BuildSubtitle(repo);

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

            string? issuesUrl = GitHelper.GetIssuesUrl(webUrl);
            if (issuesUrl is not null)
            {
                moreCommands.Add(new CommandContextItem(
                    new SafeInvokableCommand("Open Issues", () => ProcessLauncher.OpenUrl(issuesUrl))));
            }

            string? pullRequestsUrl = GitHelper.GetPullRequestsUrl(webUrl);
            if (pullRequestsUrl is not null)
            {
                string prLabel = $"Open {GitHelper.GetPullRequestsLabel(webUrl)}";
                moreCommands.Add(new CommandContextItem(
                    new SafeInvokableCommand(prLabel, () => ProcessLauncher.OpenUrl(pullRequestsUrl))));
            }

            string? cloneCommand = GitHelper.BuildCloneCommand(webUrl);
            if (cloneCommand is not null)
            {
                moreCommands.Add(new CommandContextItem(new SafeInvokableCommand(
                    "Copy Clone Command",
                    () => ClipboardHelper.CopyText(cloneCommand, "Copied clone command."))));
            }
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

        // Branch switching: show other local branches as "Switch to …" context items.
        var otherBranches = GitHelper.GetLocalBranches(path)
            .Where(b => !string.Equals(b, repo.Branch, StringComparison.Ordinal))
            .Take(15)
            .ToList();

        if (otherBranches.Count > 0)
        {
            moreCommands.Add(new Separator());
            foreach (string branchName in otherBranches)
            {
                string captured = branchName;
                moreCommands.Add(new CommandContextItem(new SafeInvokableCommand(
                    $"Switch to {captured}",
                    () =>
                    {
                        ProcessLauncher.RunGit(path, $"checkout {captured}");
                        InvalidateCache();
                        RaiseItemsChanged(0);
                        return CommandResult.ShowToast($"Switched to {captured}.");
                    })));
            }
        }

        return new ListItem(LaunchCommand("Open in Editor", path, () => ProcessLauncher.OpenInEditor(path)))
        {
            Title = repo.DisplayName,
            Subtitle = subtitle,
            Icon = new IconInfo(pinned ? PinnedGlyph : RepoGlyph),
            MoreCommands = moreCommands.ToArray(),
        };
    }

    // Builds the list subtitle as a metadata prefix (branch + status, project type) followed
    // by the path, e.g. "[main ↕ * ~2]  (Rust)  C:\src\my-repo".
    //   ↕   = local and remote tracking hashes differ (needs push or pull)
    //   *   = staged changes or mid-merge/rebase/cherry-pick
    //   ~N  = N stash entries
    private static string BuildSubtitle(RepoEntry repo)
    {
        var tags = new List<string>();

        if (repo.Branch is not null)
        {
            string branchLabel = repo.Branch;
            if (repo.GitStatus.HasRemote && !repo.GitStatus.IsInSync)
                branchLabel += " ↕";
            if (repo.GitStatus.IsDirty)
                branchLabel += " *";
            if (repo.GitStatus.StashCount > 0)
                branchLabel += $" ~{repo.GitStatus.StashCount}";
            tags.Add($"[{branchLabel}]");
        }

        if (repo.ProjectType is not null)
        {
            tags.Add($"({repo.ProjectType})");
        }

        return tags.Count == 0 ? repo.FullPath : $"{string.Join("  ", tags)}  {repo.FullPath}";
    }

    // Returns searchable status keywords for a git status snapshot.
    // Allows typing "dirty", "unsynced", or "stashed" to filter the list.
    private static string StatusTokens(RepoGitStatus status)
    {
        var sb = new StringBuilder();
        if (status.IsDirty)
            sb.Append("dirty ");
        if (status.HasRemote && !status.IsInSync)
            sb.Append("unsynced ahead behind ");
        if (status.StashCount > 0)
            sb.Append("stashed stash ");
        return sb.ToString();
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

    private void InvalidateCache()
    {
        lock (_cacheLock)
        {
            _cache = null;
            _cachedRoot = string.Empty;
        }
    }

    private List<RepoEntry> GetRepos(string repoRoot)
    {
        lock (_cacheLock)
        {
            if (_cache != null && string.Equals(_cachedRoot, repoRoot, StringComparison.OrdinalIgnoreCase))
            {
                return _cache;
            }

            _cache = RepoScanner.Scan(repoRoot);
            _cachedRoot = repoRoot;
            return _cache;
        }
    }

    private sealed partial class NoOpCommand : InvokableCommand
    {
        public override CommandResult Invoke() => CommandResult.KeepOpen();
    }
}

