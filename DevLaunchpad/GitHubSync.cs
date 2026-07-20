// Copyright (c) Eric Sanacore
// SPDX-License-Identifier: GPL-3.0-only

using System.Globalization;

namespace DevLaunchpad;

/// <summary>
/// Builds the PowerShell script that bulk-syncs every GitHub repository the authenticated
/// user can access into the current directory.
///
/// <para>The script mirrors the familiar shell one-liner:</para>
/// <code>
/// gh repo list --limit 1000 --json nameWithOwner --jq '.[].nameWithOwner'
///   | while read -r repo; do
///       dir="${repo##*/}"
///       if [ -d "$dir/.git" ]; then git -C "$dir" pull --ff-only; else gh repo clone "$repo"; fi
///     done
/// </code>
///
/// <para>Script generation is a pure function so it can be unit-tested; running it (writing the
/// file and launching a terminal) lives in the page / <see cref="ProcessLauncher"/>.</para>
/// </summary>
internal static class GitHubSync
{
    /// <summary>Default cap on the number of repositories fetched from GitHub.</summary>
    public const int DefaultRepoLimit = 1000;

    /// <summary>File name used for the generated script under the config directory.</summary>
    public const string ScriptFileName = "sync-all-github-repos.ps1";

    /// <summary>
    /// Placeholder substituted with the repository limit. Kept out of the raw template so the
    /// script body can use PowerShell braces freely without C# interpolation escaping.
    /// </summary>
    private const string LimitToken = "__REPO_LIMIT__";

    /// <summary>
    /// Returns a self-contained PowerShell script that clones repositories not present locally and
    /// fast-forward-pulls those that are. It verifies <c>gh</c>, <c>git</c>, and GitHub
    /// authentication up front, and continues past individual repository failures, printing a
    /// summary at the end. The script operates on the process working directory, so callers launch
    /// it with the projects folder as the working directory.
    /// </summary>
    /// <param name="limit">Maximum repositories to fetch. Non-positive values fall back to
    /// <see cref="DefaultRepoLimit"/>.</param>
    public static string BuildSyncScript(int limit = DefaultRepoLimit)
    {
        if (limit < 1)
        {
            limit = DefaultRepoLimit;
        }

        return ScriptTemplate.Replace(
            LimitToken,
            limit.ToString(CultureInfo.InvariantCulture));
    }

    // Note: a plain (non-interpolated) raw string literal so PowerShell's own $ and { } are left
    // untouched. The repository limit is injected via LimitToken to avoid escaping every brace.
    private const string ScriptTemplate = """
# Dev Launchpad - Sync All GitHub Repositories
# Auto-generated. Clones repositories you do not have locally and fast-forward-pulls the rest.
#
# Requirements:
#   * GitHub CLI (gh)  - https://cli.github.com
#   * An authenticated session - run 'gh auth login' once.
#   * git available on PATH.
#
# Runs against the current directory (Dev Launchpad launches it in your projects folder).

$ErrorActionPreference = 'Continue'

function Test-ToolPresent([string] $name) {
    return [bool] (Get-Command $name -ErrorAction SilentlyContinue)
}

if (-not (Test-ToolPresent 'gh')) {
    Write-Host 'GitHub CLI (gh) is not installed or not on PATH. See https://cli.github.com.' -ForegroundColor Red
    exit 1
}
if (-not (Test-ToolPresent 'git')) {
    Write-Host 'git is not installed or not on PATH.' -ForegroundColor Red
    exit 1
}

Write-Host 'Checking GitHub CLI authentication...' -ForegroundColor Cyan
gh auth status 1>$null 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "Not signed in to GitHub. Run 'gh auth login' first." -ForegroundColor Red
    exit 1
}

Write-Host 'Fetching repository list...' -ForegroundColor Cyan
$repos = gh repo list --limit __REPO_LIMIT__ --json nameWithOwner --jq '.[].nameWithOwner'
if ($LASTEXITCODE -ne 0) {
    Write-Host 'Failed to list repositories.' -ForegroundColor Red
    exit 1
}

$cloned = 0
$updated = 0
$failed = 0

foreach ($repo in $repos) {
    if ([string]::IsNullOrWhiteSpace($repo)) { continue }

    # Local folder matches gh's default clone target: the repository name.
    $dir = $repo.Split('/')[-1]

    if (Test-Path -Path (Join-Path $dir '.git')) {
        Write-Host "updating $dir" -ForegroundColor Yellow
        git -C $dir pull --ff-only
        if ($LASTEXITCODE -eq 0) { $updated++ } else { $failed++ }
    }
    else {
        Write-Host "cloning $repo" -ForegroundColor Green
        gh repo clone $repo
        if ($LASTEXITCODE -eq 0) { $cloned++ } else { $failed++ }
    }
}

Write-Host ''
Write-Host "Done. Cloned $cloned, updated $updated, failed $failed." -ForegroundColor Cyan
""";
}
