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

### Changed
- N/A

### Fixed
- Fixed NETSDK1097 build error by adding RuntimeIdentifier
- Fixed missing functionality exposure in CommandsProvider

### Security
- Configuration stored in Windows packaged app storage (LocalCacheFolder)
- No credentials stored in configuration files

## [0.0.1.0] - 2025-01-XX

### Added
- Initial project structure
- MSIX packaging for Windows Store distribution
- PowerToys Command Palette integration
- COM server registration

[Unreleased]: https://github.com/esanacore/DevLaunchpad/compare/v0.0.1.0...HEAD
[0.0.1.0]: https://github.com/esanacore/DevLaunchpad/releases/tag/v0.0.1.0
