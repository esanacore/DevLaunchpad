# TODO

This file is the living roadmap for Dev Launchpad.

## Features

- [x] Terminal in Projects Root (Developer Tools page)
- [x] WSL command type for Custom Commands (`wsl-command`)
- [x] SSH command type for Custom Commands (`ssh`)
- [ ] Git integration (status, branches, operations)
  - [x] Read-only branch/remote inspection (no process spawn) via `GitHelper`
  - [x] Repository tech-stack detection (`ProjectTypeDetector`) shown in the list and searchable
  - [x] Quick links (Issues, Pull/Merge Requests) and Copy Clone Command for GitHub/GitLab/Bitbucket
  - [x] Dirty/ahead-behind status indicators (`↕` out-of-sync with remote, `*` staged changes or mid-operation)
  - [x] Stash count indicator (`~N`) in branch tag; reads `.git/logs/refs/stash`
  - [x] Branch switching from context menu (up to 15 local branches; runs `git checkout`)
  - [x] Clone Repository top-level command (paste URL → clone into RepoRoot in background)
  - [x] Sync All GitHub Repos command (bulk clone new + `pull --ff-only` existing via `gh`, run in a
        visible terminal). Follow-up: optional owner/org scoping and a dry-run/preview mode.
- [x] Search and filtering for large repository lists (also filters by tech stack, dirty/unsynced/stashed)
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
- [x] Cover the `DevLaunchpadSettings` write-back path (`OnSettingsChanged` persistence), including
      blank-value handling for the settings form bridge.

## Documentation

- [ ] Add screenshots to README.md. *(README gallery + `docs/images/SCREENSHOTS.md` capture spec
      are wired; awaiting the actual PNGs captured on Windows.)*
- [ ] Icon customization per page/command (document how-to).

## Microsoft Store Release

In-repo prep is done (version 1.2.0, finalized changelog, accurate privacy policy, filled
governance docs, `StoreReadinessChecker` version gate). Remaining steps are external and must be
done from a Windows machine / Partner Center — see [`docs/STORE.md`](docs/STORE.md):

- [ ] Reserve the app name in Partner Center and obtain the identity values.
- [ ] Stamp the real `Identity/Name` + `Publisher` into `Package.appxmanifest` (do **not** commit
      real values to the public repo — use VS "Associate App with the Store").
- [ ] Capture the screenshots in `docs/images/SCREENSHOTS.md` and drop the PNGs into `docs/images/`.
- [ ] Host `docs/PRIVACY.md` (e.g. GitHub Pages) and enter the privacy-policy URL in the listing.
- [ ] Fill the Store listing: description, category (Developer tools), support contact.
- [ ] Build x64 + arm64 MSIX (CI artifacts or local), run WACK, then submit for certification.

## Tooling

- [x] Adopt gstack (optional mode) for AI-assisted contributor workflows; recommended install documented in `CLAUDE.md`.
- [ ] Decide whether to enforce gstack via team "required" mode (adds a PreToolUse install-check hook that blocks work without gstack) or keep it optional.

## Nice-to-Have

- [x] Export configuration backup snapshots from the Configuration page.
- [ ] Import configuration backup snapshots from the Configuration page.
- [ ] Add a contributor-facing command or CI job that runs `StoreReadinessChecker` outside the test suite.
- [ ] Telemetry: Anonymous usage analytics (opt-in).
