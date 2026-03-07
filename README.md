# Dev Launchpad

Dev Launchpad is a Microsoft PowerToys Command Palette extension for quickly opening developer tools, repositories, local servers, favorite websites, and custom commands.

## Features

- Launch common developer apps
- Discover and open Git repositories from a configured root folder
- Open repositories in File Explorer, your editor, or a terminal
- Launch local development URLs
- Open favorite websites from config
- Run configurable custom commands
- Manage settings through a generated `config.json`

## Current Functionality

### Developer Apps
- Open VS Code / configured editor
- Open PowerShell
- Open Windows Terminal

### Repositories
- Recursively scans a configured repo root
- Detects repositories by `.git` folders
- Opens repos in:
  - File Explorer
  - configured editor
  - configured terminal

### Local Servers
- Opens configurable localhost URLs

### Favorite Websites
- Opens configurable bookmarks

### Custom Commands
Supports:
- `url`
- `folder`
- `command`
- `terminal-in-folder`

### Configuration
- Open config folder
- Open config file
- Open config file in editor
- Open debug log
- Reload config
- Reset config to defaults

## Configuration

Dev Launchpad automatically creates a `config.json` file in packaged app storage.

The Configuration page inside the extension shows the exact file location being used.

Example config:

```json
{
  "RepoRoot": "C:\\Projects",
  "EditorCommand": "code",
  "TerminalCommand": "wt",
  "LocalUrls": [
    {
      "Title": "Frontend",
      "Url": "http://localhost:5173"
    }
  ],
  "FavoriteWebsites": [
    {
      "Title": "GitHub",
      "Url": "https://github.com"
    }
  ],
  "CustomCommands": [
    {
      "Title": "Open PowerShell",
      "Type": "command",
      "Target": "powershell.exe",
      "Arguments": ""
    },
    {
      "Title": "Open Projects Folder",
      "Type": "folder",
      "Target": "C:\\Projects",
      "Arguments": ""
    }
  ]
}