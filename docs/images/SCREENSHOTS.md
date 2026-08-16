# Screenshot capture spec

This is the shot list for the README gallery and the Microsoft Store listing. Capture
each image on Windows with PowerToys Command Palette open, then save it into this
`docs/images/` folder using the **exact filename** below so it wires into
[`README.md`](../../README.md) automatically.

## Format

- **Format:** PNG.
- **Dimensions:** 16:9, **1920×1080** recommended (Microsoft Store accepts desktop
  screenshots from **1366×768** up to **3840×2160**).
- **Theme:** capture in the theme you want to feature; a clean desktop background reads best.
- **Count:** the Store listing needs at least **one** screenshot and allows up to **ten**.

## Shots

| Filename | What it should show |
|---|---|
| `main-menu.png` | The Command Palette open on the Dev Launchpad top-level commands (Repositories, Developer Tools, Local Servers, Favorite Websites, Clone Repository, Sync All GitHub Repos, Custom Commands, Configuration). |
| `repositories.png` | The **Repositories** page listing a few repos with their branch tag and tech-stack tag (e.g. `[main]  (Rust)`), ideally with a live-search filter typed in. |
| `sync-repos.png` | The **Sync All GitHub Repos** page showing the *Sync All GitHub Repositories*, *Copy Sync Script*, and *Open Projects Folder* actions. |
| `configuration.png` | The **Configuration** page showing settings, *Export Config Backup*, and *Reload Config* actions. |

## Optional extras

Add any of these if you want a richer listing (reference them in the README/Store as needed):

- `custom-commands.png` — the Custom Commands page executing a configured command.
- `repo-context-menu.png` — a repository's context menu (Open in Editor/Terminal, Copy Clone Command, Open Issues/PRs, Pin).

Once the PNGs are in place, verify the README gallery renders them and that no
broken-image icons remain.
