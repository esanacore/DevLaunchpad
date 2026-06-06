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

### Changed
- Configuration and debug log now stored in the persistent `LocalFolder` instead of the
  purgeable `LocalCacheFolder`
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
