# TODO

This file is the living roadmap for Dev Launchpad.

## Features

- [ ] Git integration (status, branches, operations)
  - [x] Read-only branch/remote inspection (no process spawn) via `GitHelper`
  - [x] Repository tech-stack detection (`ProjectTypeDetector`) shown in the list and searchable
  - [x] Quick links (Issues, Pull/Merge Requests) and Copy Clone Command for GitHub/GitLab/Bitbucket
  - [x] Dirty/ahead-behind status indicators (`↕` out-of-sync with remote, `*` staged changes or mid-operation)
- [x] Search and filtering for large repository lists (also filters by tech stack)
- [x] Recent items tracking
- [ ] Theme support
- [ ] Keyboard shortcuts configuration
- [ ] Cloud sync for configuration (optional)

## Technical Debt

- [ ] Async/Await: Use async pattern for I/O operations.
- [ ] Background Services: Long-running tasks (git status, indexing).

## Refactoring

- [ ] Plugin System: Load additional features from external assemblies.

## Testing

- [x] Cover config defaults/paths/debug-log, `ProcessLauncher` guard paths, and the
      `DevLaunchpadSettings` config-seeding bridge.
- [ ] Increase test coverage for page implementations (`RepoPage`, `ConfigPage`, etc.). These are
      currently untested because they bind directly to the Command Palette UI toolkit; extracting
      their logic (as was done for `RepoScanner`) would make them testable.
- [ ] Cover the `DevLaunchpadSettings` write-back path (`OnSettingsChanged` persistence), which is
      event-driven and not exercised by the current seeding tests.

## Documentation

- [ ] Add screenshots to README.md.
- [ ] Icon customization per page/command (document how-to).

## Tooling

- [x] Adopt gstack (optional mode) for AI-assisted contributor workflows; recommended install documented in `CLAUDE.md`.
- [ ] Decide whether to enforce gstack via team "required" mode (adds a PreToolUse install-check hook that blocks work without gstack) or keep it optional.

## Nice-to-Have

- [ ] Export/import configuration profiles.
- [ ] Telemetry: Anonymous usage analytics (opt-in).