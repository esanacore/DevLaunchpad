# Changelog

All notable changes to the Dev Launchpad project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
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
