# Copilot Instructions for Dev Launchpad

## Project Overview

Dev Launchpad is a Microsoft PowerToys Command Palette extension built with .NET 9, C# 12, and MSIX packaging. It provides developer workflow shortcuts (repos, tools, servers, bookmarks, custom commands).

## Architecture

- **Out-of-process COM server** communicating with PowerToys via WinRT/COM interfaces
- **Single-project MSIX** packaging for Windows Store distribution
- **Source-generated JSON** (`System.Text.Json`) for AOT compatibility — no runtime reflection
- Pages inherit from `ListPage` (or `DynamicListPage` for search); commands use `SafeInvokableCommand`

## Build & Run

```powershell
# Restore + build (x64, Debug)
dotnet build DevLaunchpad/DevLaunchpad.csproj -c Debug -p:Platform=x64 -r win-x64 --self-contained

# Restore + build (Release with trimming)
dotnet build DevLaunchpad/DevLaunchpad.csproj -c Release -p:Platform=x64 -r win-x64 --self-contained
```

There is no test project yet. The extension is validated by deploying and testing manually in Command Palette.

## Coding Conventions

- **Nullable reference types** are enabled (`<Nullable>enable</Nullable>`).
- **.NET analyzers** run in `Recommended` mode — do not suppress analyzer warnings without justification.
- **AOT compatibility** is required: use source-generated serialization, avoid reflection-based APIs.
- Add new serializable types to `DevLaunchpadJsonContext.cs`.
- One page per feature, placed in the `Pages/` folder.
- Configuration changes require updating `DevLaunchpadConfig`, the JSON context, and the default config factory.
- Use `ProcessLauncher` for launching external processes (never call `Process.Start` directly).
- Wrap command callbacks in `SafeInvokableCommand` for consistent error handling.

## File Layout

| Path | Purpose |
|------|---------|
| `DevLaunchpad/DevLaunchpad.cs` | Extension entry point (`IExtension`) |
| `DevLaunchpad/Program.cs` | COM server bootstrap |
| `DevLaunchpad/DevLaunchpadCommandsProvider.cs` | Top-level command registration |
| `DevLaunchpad/DevLaunchpadConfig.cs` | Configuration load/save/defaults |
| `DevLaunchpad/DevLaunchpadJsonContext.cs` | Source-gen JSON context |
| `DevLaunchpad/Pages/` | Feature pages (Repo, DevTools, LocalServers, etc.) |
| `DevLaunchpad/ProcessLauncher.cs` | Safe process execution helper |
| `DevLaunchpad/SafeInvokableCommand.cs` | Error-handling command wrapper |

## Security

- Never store credentials or secrets in `config.json`.
- Validate file paths and URLs before use.
- Use `UseShellExecute = true` for process launching (OS-level security).
- The extension has no network access by design.

## PR & Commit Guidelines

- Keep PRs focused on a single feature or fix.
- Update `CHANGELOG.md` for user-facing changes.
- Ensure the `.NET` and `MSIX` CI workflows pass before merging.
