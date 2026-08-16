# Home

Welcome to the **Dev Launchpad** wiki. Wiki pages are authored under `wiki/`
in this repository and reviewed through normal pull requests.

## What this project does

A Microsoft PowerToys Command Palette extension for managing developer
workflows: browse and open local Git repositories with live search and pins,
bulk clone/pull all your GitHub repos via the `gh` CLI, launch developer
tools, manage local server URLs, bookmark websites, and run custom commands —
all from the Command Palette.

## Getting started

Build the `DevLaunchpad/` C# project and register the extension with
PowerToys Command Palette. See `docs/SETUP.md` for prerequisites and steps.

## How it works

`DevLaunchpad.cs` is the `IExtension` entry point, registered as a COM server
by `Program.cs`. `DevLaunchpadCommandsProvider.cs` defines the commands, and
each feature is a page under `Pages/` (repos, sync, dev tools, local servers,
favorites, custom commands). `RepoScanner.cs` discovers Git repositories with
a bounded recursive scan, and `GitHelper.cs` inspects branches and remotes
without spawning processes. See `docs/ARCHITECTURE.md`.

## Where things live

- `DevLaunchpad/` — the extension project (entry point, config, pages)
- `docs/` — setup, architecture, and governance docs
- `constitution/` — Eric's Engineering Constitution submodule (read-only)

## See also

- `docs/HELP.md` — common questions and troubleshooting
- `TODO.md` — the living roadmap
