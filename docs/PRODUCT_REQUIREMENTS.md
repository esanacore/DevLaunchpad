# Product Requirements

This document is the product and implementation contract for Dev Launchpad.
Requirement IDs are stable and are referenced by tests and traceability records.

## Functional Requirements

| ID | Level | Requirement | Acceptance criteria |
| --- | --- | --- | --- |
| FR-001 | MUST | Dev Launchpad must expose Command Palette pages for repositories, developer tools, local servers, favorite websites, custom commands, and configuration. | FR-001-AC-1: The extension provider exposes the top-level commands documented in `README.md`. |
| FR-002 | MUST | The Repositories page must discover local Git repositories below the configured repo root without spawning `git` for read-only metadata. | FR-002-AC-1: Repository scanner tests verify bounded discovery, branch data, tech stack tags, and status indicators. |
| FR-003 | MUST | Users must be able to manage and preserve local configuration stored in packaged app storage. | FR-003-AC-1: Configuration tests verify load, save, reset, reload, recent repos, pinned repos, and backup export behavior. |
| FR-004 | SHOULD | Dev Launchpad should provide repo-aware quick actions for opening folders, terminals, editors, remotes, Issues, Pull Requests, and clone commands. | FR-004-AC-1: `GitHelperTests`, `RepoScannerTests`, and process launcher tests verify URL/action derivation and guard paths. |
| FR-005 | MUST | The repository must maintain a repeatable Microsoft Store readiness path for MSIX submission. | FR-005-AC-1: Store readiness tests verify manifest, privacy, Store guide, publish profiles, identity fields, and required capabilities. |

## Non-Functional Requirements

| ID | Level | Requirement | Acceptance criteria |
| --- | --- | --- | --- |
| NFR-001 | MUST | Configuration and debug logs must stay in Windows packaged app storage and must not contain credentials by default. | NFR-001-AC-1: Config path tests and privacy/security docs describe the storage boundary and no-credential default config. |
| NFR-002 | MUST | Store packaging metadata must avoid unnecessary app capabilities. | NFR-002-AC-1: Store readiness checks require `runFullTrust` and no code path adds network capabilities for the extension itself. |
| NFR-003 | SHOULD | Extractable logic should remain testable without launching the Command Palette host. | NFR-003-AC-1: Unit tests cover config, git helper, repo scanning, project detection, process guard paths, settings write-back, clone parsing, and Store readiness logic. |

## Non-Goals

- Dev Launchpad does not store source-control credentials or personal access tokens.
- Dev Launchpad does not perform Git network operations on behalf of the user except user-triggered clone commands.
- Dev Launchpad does not currently sync configuration across devices.
