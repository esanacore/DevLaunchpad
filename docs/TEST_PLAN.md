# Test Plan

This document defines how Dev Launchpad is tested, what coverage it targets, and where coverage gaps currently exist.

It is a living document. Update it whenever the test strategy, targets, or known gaps change.

## Test Strategy

Dev Launchpad uses a unit-test-first strategy. The Command Palette UI toolkit does not support
headless testing, so coverage is focused on extracted, framework-independent logic.

- **Unit tests**: isolated behavior of each extractable module. Location: `DevLaunchpad.Tests/`.
  Command: `dotnet test DevLaunchpad.Tests/DevLaunchpad.Tests.csproj --nologo -v q`
- **Integration tests**: none currently. Page-level integration is validated manually in the
  Command Palette host (see gap log below).
- **End-to-end tests**: manual — install the extension, exercise each page, and verify behavior
  in the live Command Palette. Triggered before releases.

## How to Run Tests

- Full suite: `dotnet test DevLaunchpad.Tests/DevLaunchpad.Tests.csproj --nologo -v q`
- With coverage: `dotnet test DevLaunchpad.Tests/DevLaunchpad.Tests.csproj --collect:"XPlat Code Coverage"`
- Single test class: `dotnet test --filter "FullyQualifiedName~GitHelperTests" --nologo -v q`

## Test Files

| File | What it covers |
| --- | --- |
| `ConfigDefaultsTests.cs` | Default config generation and field validity |
| `ConfigLogicTests.cs` | Config load/save, recent-repo tracking, pinning, reset |
| `ConfigSerializationTests.cs` | JSON round-trip and AOT source-generated context |
| `DevLaunchpadSettingsTests.cs` | `DevLaunchpadSettings` config seeding and write-back bridge |
| `GitHelperTests.cs` | Branch parsing, remote URL detection, clone command, Issues/PR URL derivation |
| `ProcessLauncherConfigTests.cs` | Config-driven process launcher behavior |
| `ProcessLauncherTests.cs` | Input validation guard paths and `IsWindowsTerminal` detection |
| `ProjectTypeDetectorTests.cs` | Tech-stack inference from marker files, priority ordering |
| `RepoScannerTests.cs` | Git repository discovery, depth bounding, project-type population |
| `StoreReadinessCheckerTests.cs` | MSIX manifest, Store documentation, privacy policy, publish profiles, and `runFullTrust` readiness |

### Test Helpers

- `Helpers/TempConfigDir.cs` — isolated temp config directory fixture (auto-cleaned)
- `Helpers/TempGitRepo.cs` — temporary `.git` stub tree fixture for filesystem-based tests
- `Helpers/ConfigStateCollection.cs` — config state collection helper

## Coverage Targets

| Scope | Metric | Floor |
| --- | --- | --- |
| Extractable logic (non-UI) | Line | 80% |
| `GitHelper`, `RepoScanner`, `ProjectTypeDetector` | Branch | 85% |
| `ProcessLauncher` guard paths | Branch | 95% |

New or modified logic in extractable modules should meet the floor on its own.

## Continuous Coverage Evaluation

Coverage is not yet measured in CI. Run locally with `--collect:"XPlat Code Coverage"` and
record snapshots here.

| Date | Overall coverage | Notes |
| --- | --- | --- |
| 2026-06-27 | ~unknown~ | Baseline not yet measured; coverage tooling not wired into CI |
| 2026-06-27 | Not measured | Added focused `DevLaunchpadSettings` write-back regression coverage; no coverage collector run |
| 2026-06-30 | Not measured | Added config backup export and Store readiness checker coverage; focused and full test suites run without coverage collector |

## Coverage Gap Log

| Gap ID | Area / behavior | Risk | Status | TODO ref |
| --- | --- | --- | --- | --- |
| GAP-001 | `RepoPage` — live search, pinning, context menu actions | Med | Open | TODO: Increase test coverage for page implementations |
| GAP-002 | `ConfigPage` — open/reload/reset actions | Med | Open | TODO: Increase test coverage for page implementations |
| GAP-003 | `DevToolsPage`, `LocalServersPage`, `FavoriteWebsitesPage`, `CustomCommandsPage` | Low | Open | TODO: Increase test coverage for page implementations |
| GAP-004 | `DevLaunchpadSettings.OnSettingsChanged` write-back path | Med | Closed | Covered by `DevLaunchpadSettingsTests.PersistCurrentSettings_WritesFormValuesToConfig` and blank-value guard test |
| GAP-005 | CI coverage reporting — no automated gate exists | Low | Open | Wire `--collect:"XPlat Code Coverage"` into the CI workflow |
| GAP-006 | Store readiness checker is covered by unit tests but not exposed as a standalone contributor command | Low | Open | Add a contributor-facing command or CI job that runs `StoreReadinessChecker` outside the test suite |
