# TODO

This file is the living roadmap for Dev Launchpad.

## Features

- [ ] Git integration (status, branches, operations)
- [ ] Search and filtering for large repository lists
- [ ] Recent items tracking
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

## Nice-to-Have

- [ ] Export/import configuration profiles.
- [ ] Telemetry: Anonymous usage analytics (opt-in).