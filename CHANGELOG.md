# Changelog

All notable changes to the Dev Launchpad project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- **Stash indicator** in the Repositories list: `~N` appended to the branch tag (e.g.
  `[main ~2]`) when the repository has stash entries. Count is read from
  `.git/logs/refs/stash` with no process spawn.
- **Status-based search filtering**: typing `dirty`, `unsynced`, `ahead`, `behind`, `stashed`,
  or `stash` in the Repositories search box filters to matching repos based on their git status.
- **Branch switching** in the Repositories context menu: all local branches (up to 15) appear
  as "Switch to `<branch>`" items. Switching runs `git checkout` and refreshes the list.
- **Clone Repository** top-level command: a search-as-you-type page that accepts any Git URL
  (https, git@, ssh://) and clones into the configured `RepoRoot` as a background process.
- **Terminal in Projects Root** in Developer Tools: opens the configured terminal directly at
  the root projects folder.
- **WSL command type** (`wsl-command`) for Custom Commands: runs a command inside WSL via
  `wsl.exe -- <command>`.
- **SSH command type** (`ssh`) for Custom Commands: opens an SSH session in the configured
  terminal (`wt -- ssh host` for Windows Terminal, raw `ssh host` otherwise).
- **MaxVersionTested bumped** from `10.0.22621.0` to `10.0.26100.0` in `Package.appxmanifest`
  to declare Windows 11 24H2 support.
- **Dirty/ahead-behind status indicators** in the Repositories list: `↕` appears in the branch
  tag when the local commit hash differs from the remote-tracking hash (needs push or pull);
  `*` appears when staged changes or an in-progress operation (merge, rebase, cherry-pick) is
  detected. Both compose: `[main ↕ *]`. Detection is filesystem-only with no process spawn;
  unstaged-only changes are not detected (see `docs/TEST_PLAN.md` gap log).
- **Repository tech-stack detection**: each repository is tagged with its primary stack
  (.NET, Node, Rust, Go, Python, Java, Dart, Ruby, PHP, Docker) inferred from root marker files
  (`*.sln`/`*.csproj`, `package.json`, `Cargo.toml`, `go.mod`, etc.) by a new read-only
  `ProjectTypeDetector`. The tag is shown in the Repositories list (e.g. `[main]  (Rust)`) and is
  matched by live search, so typing a stack name filters the list to that ecosystem. Like
  `GitHelper`, detection reads the filesystem and never spawns a process.
- **Repository quick links and clone command**: the Repositories context menu gains **Copy Clone
  Command** (`git clone <url>.git`) for any repo with a remote, plus host-aware **Open Issues** and
  **Open Pull Requests** / **Open Merge Requests** actions for GitHub, GitLab, and Bitbucket remotes.
- Unit tests for the new logic: `ProjectTypeDetectorTests` (all markers, priority ordering, and
  guard paths), `GitHelper` clone-command and issue/PR URL derivation, and `RepoScanner`
  project-type population.
- Initial release of Dev Launchpad Command Palette extension
- Repository browser with Git detection
- Developer tools quick launch
- Local server URL management
- Favorite websites bookmarking
- Custom command system (url, folder, command, terminal-in-folder)
- Configuration management system
- JSON-based configuration with auto-creation
- Debug logging system
- Centralized, exception-safe process launching (`ProcessLauncher`) that surfaces failures as
  toasts instead of crashing the extension
- Automatic migration of existing configuration from the legacy `LocalCacheFolder` location
- In-palette **Settings** form (toolkit `Settings`) to edit the repo root, editor command, and
  terminal command without hand-editing JSON; advertised to the Command Palette host via
  `CommandProvider.Settings` and exposed as a top-level "Settings" command. Edits are written
  straight back to `config.json` (single source of truth).
- Live search on the **Repositories** page via `DynamicListPage` — repos filter as you type by
  name or path
- Repository intelligence: each repo now shows its **current Git branch** (read directly from
  `.git/HEAD`, no process spawned) and a per-item **context menu** (open folder/terminal, copy
  path, open the origin remote in a browser, pin/unpin)
- **Pinned** and **recently opened** repositories are remembered (in `config.json`) and floated to
  the top of the list
- Expanded automated test coverage: config default generation and validity (all custom-command
  types present), path helpers, debug-log append, `ProcessLauncher` editor/terminal guard paths,
  and the `DevLaunchpadSettings` config-seeding bridge
- Microsoft Store packaging: a `MSIX` CI workflow that builds unsigned **x64** and **arm64**
  `.msix` packages and uploads them as artifacts, the `win-x64`/`win-arm64` publish profiles the
  packaging targets require, plus `docs/STORE.md` (submission checklist) and `docs/PRIVACY.md`
  (privacy policy required by the Store)
- README Update Check CI workflow (`readme-update-check.yml`) that fails a pull request into `main`
  when qualifying code is changed (`.cs`/`.csproj`/`.sln`/`.props`/`.targets`/`.appxmanifest` or a
  workflow file) without a matching `README.md` update; `docs/` changes and the workflow file
  itself are exempt, and draft PRs are skipped
- **Export Config Backup** action on the Configuration page. It copies the current `config.json`
  into a timestamped `backups` folder so users can preserve settings before upgrades,
  sideload testing, or Store package validation.
- **Store readiness checker** coverage that verifies the repo has the MSIX manifest, Store guide,
  privacy policy, x64/arm64 publish profiles, manifest identity fields, and the required
  `runFullTrust` capability before submission.

### Changed
- Package manifest cleaned up for Store submission: real publisher identity placeholders
  (`Eric Sanacore` instead of `Microsoft Corporation` / `A Lone Developer`), a descriptive
  tile description, version bumped to `1.0.0.0`, and `MaxVersionTested` raised to `10.0.22621.0`
- Dropped the unused `internetClient` capability (the extension makes no network calls of its own)
- Configuration and debug log now stored in the persistent `LocalFolder` instead of the
  purgeable `LocalCacheFolder`
- Repository scan is now bounded (max depth) and skips heavy directories (`node_modules`, `bin`,
  `obj`, etc.) and caches results per page open, so opening Repositories no longer triggers an
  unbounded recursive walk on every keystroke
- "Developer Apps" now launches the configured editor/terminal commands
- Terminal launches now set the working directory correctly for non–Windows-Terminal shells
- Reconciled licensing on GNU GPL v3.0 (source headers, README, LICENSE)

### Fixed
- Fixed NETSDK1097 build error by adding RuntimeIdentifier
- Fixed missing functionality exposure in CommandsProvider
- Added regression coverage for the in-palette Settings write-back path, including blank-value
  handling so empty form values do not overwrite existing config values.
- Added focused tests for configuration backup export and Store readiness checks.
- Guarded all `Process.Start` calls so a missing editor/terminal/target no longer throws out of
  the COM server
- Removed dead template page (`DevLaunchpadPage`) that shipped a "TODO" placeholder item
- Repaired CI: removed the placeholder `.NET Core Desktop` workflow and fixed the build workflow
  (correct platform, removed nonexistent test step)
- Repaired the test project build: pinned its `WindowsSdkPackageVersion` to match the main project
  (so it can reference `DevLaunchpad.dll`, fixing `NETSDK1148`) and enabled `ImplicitUsings` (fixing
  `CS0246` on `System` types). `dotnet test` now compiles under both the .NET 9 and .NET 10 SDKs.
- Made config-touching tests run sequentially via a shared xUnit collection, eliminating races on
  the process-wide `ConfigDirectoryOverride` static

### Security
- Configuration stored in Windows packaged app storage (LocalFolder)
- No credentials stored in configuration files

## [0.0.1.0] - 2025-01-XX

### Added
- Initial project structure
- MSIX packaging for Windows Store distribution
- PowerToys Command Palette integration
- COM server registration

[Unreleased]: https://github.com/esanacore/DevLaunchpad/compare/v0.0.1.0...HEAD
[0.0.1.0]: https://github.com/esanacore/DevLaunchpad/releases/tag/v0.0.1.0
