# Dev Launchpad

A Microsoft PowerToys Command Palette extension for managing developer workflows.

<!-- CONSTITUTION_START -->
## Engineering Constitution

This project follows the Engineering Constitution. The constitution is included as a Git submodule in constitution/.

AI agents and contributors should read AGENTS.md before beginning work.
<!-- CONSTITUTION_END -->

## Project Structure

```
DevLaunchpad/
├── DevLaunchpad/                    # Main extension project
│   ├── DevLaunchpad.cs             # Extension entry point (IExtension)
│   ├── Program.cs                  # COM server registration
│   ├── DevLaunchpadCommandsProvider.cs  # Command definitions
│   ├── DevLaunchpadConfig.cs       # Configuration management
│   ├── DevLaunchpadJsonContext.cs  # JSON serialization (AOT-compatible)
│   ├── Pages/                      # Feature implementations
│   │   ├── RepoPage.cs            # Git repository browser (live search, branch, pins, context menu)
│   │   ├── DevToolsPage.cs        # Developer tools launcher
│   │   ├── LocalServersPage.cs    # Local URL management
│   │   ├── FavoriteWebsitesPage.cs # Website bookmarks
│   │   ├── CustomCommandsPage.cs   # Custom command executor
│   │   └── ConfigPage.cs          # Configuration management UI
│   ├── Assets/                     # Icons and visual assets
│   ├── Package.appxmanifest       # MSIX package manifest
│   └── DevLaunchpad.csproj        # Project file
├── DevLaunchpad.Tests/             # Automated test project (xUnit)
│   ├── ConfigLogicTests.cs        # Config load/save, recent repos, pinning, reset
│   ├── ConfigSerializationTests.cs # JSON round-trip and AOT context
│   ├── GitHelperTests.cs          # Branch parsing, remote URL normalization
│   ├── ProcessLauncherTests.cs    # Input validation and IsWindowsTerminal
│   ├── RepoScannerTests.cs        # Repository discovery logic
│   ├── Helpers/
│   │   ├── TempConfigDir.cs       # Isolated temp config directory fixture
│   │   └── TempGitRepo.cs         # Temporary .git stub tree fixture
│   └── DevLaunchpad.Tests.csproj  # Test project file
├── docs/                           # Documentation and assets
│   └── images/                    # Screenshots and diagrams
├── CHANGELOG.md                    # Version history
└── README.md                       # This file
```

## Features

- **Repository Management**: Automatically discover and access Git repositories
- **Developer Tools**: Quick launch for VS Code, PowerShell, Windows Terminal
- **Local Servers**: One-click access to localhost development URLs
- **Favorite Websites**: Bookmark and quickly open frequently used sites
- **Custom Commands**: Execute configurable commands (URLs, folders, executables, terminal sessions)
- **Configuration Management**: JSON-based config with auto-reload and debug logging

## Quick Start

### Prerequisites

1. **PowerToys** (version with Command Palette support)
   - Download from: https://github.com/microsoft/PowerToys/releases
   - Ensure Command Palette is enabled in PowerToys Settings

2. **Windows 10/11** (version 19041 or higher)

3. **Visual Studio 2022 or later** (for building from source)

### Installation

#### Option 1: From Source

1. Clone the repository:
   ```powershell
   git clone https://github.com/esanacore/DevLaunchpad
   cd DevLaunchpad
   ```

2. Open `DevLaunchpad.sln` in Visual Studio

3. Build and deploy:
   - Set configuration to `Debug` or `Release`
   - Press `F5` to build and deploy
   - Or use: Build → Deploy Solution

4. Reload extensions in Command Palette:
   - Press `Win+Alt+Space`
   - Type `reload`
   - Select "Reload extensions"

#### Option 2: Prebuilt MSIX

Every push and PR builds unsigned **x64** and **arm64** `.msix` packages via the
[`MSIX`](.github/workflows/msix.yml) workflow; download them from the run's **Artifacts**.
To sideload, sign the `.msix` with a certificate you trust, then `Add-AppxPackage` it.

#### Option 3: From Microsoft Store (Coming Soon)

Once published, install directly from the Microsoft Store. See
[`docs/STORE.md`](docs/STORE.md) for the submission process and
[`docs/PRIVACY.md`](docs/PRIVACY.md) for the privacy policy.

### First Launch

1. Open Command Palette: Press `Win+Alt+Space`
2. Type "Dev Launchpad" or "dev"
3. You'll see 6 main commands:
   - Repositories
   - Developer Tools
   - Local Servers
   - Favorite Websites
   - Custom Commands
   - Configuration

4. Navigate to **Configuration** to:
   - View config file location
   - Edit settings
   - Set your repo root folder
   - Configure editor and terminal preferences

## Current Functionality

### 📁 Repositories (`RepoPage`)
- Recursively scans configured repo root for Git repositories
- Detects repositories by `.git` folders
- For each repository, provides actions:
  - **Open Folder**: Opens in Windows Explorer
  - **Open in Editor**: Opens in configured editor (VS Code, etc.)
  - **Open in Terminal**: Opens configured terminal in repo directory
- Displays relative paths from repo root for easy navigation

### 🛠️ Developer Tools (`DevToolsPage`)
Quick launch for common development applications:
- **VS Code** (or configured editor)
- **PowerShell**
- **Windows Terminal** (or configured terminal)

### 🌐 Local Servers (`LocalServersPage`)
- Opens configurable localhost development URLs
- Default includes common ports: 3000, 5173, 8000
- Fully customizable via `config.json`

### ⭐ Favorite Websites (`FavoriteWebsitesPage`)
- Quick-access bookmarks for frequently visited sites
- Opens URLs in default browser
- Configurable via `config.json`

### ⚡ Custom Commands (`CustomCommandsPage`)
Execute user-defined commands with support for:
- **`url`**: Open web addresses
- **`folder`**: Open directories in Explorer
- **`command`**: Run executables with arguments
- **`terminal-in-folder`**: Launch terminal in specific directory

### ⚙️ Configuration (`ConfigPage`)
Comprehensive configuration management:
- **View Settings**: Current repo root, editor, terminal commands
- **Open Config Folder**: Navigate to config directory
- **Open Config File**: Edit with default application
- **Open Config in Editor**: Edit with configured editor
- **Open Debug Log**: View configuration and error logs
- **Reload Config**: Apply changes without restarting
- **Reset to Defaults**: Restore default configuration

## Security Notes

- **No Credentials Stored**: Configuration files do not contain passwords or sensitive data
- **Windows Packaged Storage**: Config stored in isolated app storage
- **Process Execution**: Uses `UseShellExecute = true` for safe process launching
- **Path Validation**: Validates directories and executables before execution

## Contributing & Workflow

Contributions are welcome! To contribute:

1. **Fork the repository**
2. **Create a feature branch**: `git checkout -b feature/your-feature-name`
3. **Make your changes** with clear, focused commits
4. **Test thoroughly**: Run automated tests (`dotnet test`) and verify in Command Palette
5. **Submit a pull request** with a description of changes

### Development Guidelines

- Follow existing code patterns and structure
- Keep features modular (one page per feature)
- Update `CHANGELOG.md` with changes
- Test configuration loading and error handling
- Ensure AOT compatibility (source-generated JSON)

See `CHANGELOG.md` for recent changes and version history.

## Technology Stack

- **.NET 9** (Windows App SDK)
- **C# 12** with nullable reference types
- **Command Palette Extensions API** (Microsoft.CommandPalette.Extensions)
- **WinRT COM Server** (out-of-process communication)
- **MSIX Packaging** (Windows Store distribution)
- **Source-Generated JSON** (AOT-compatible serialization)

## License

Licensed under the **GNU General Public License v3.0**. See the [LICENSE](LICENSE) file for the full text.

## Acknowledgments

Built for [Microsoft PowerToys Command Palette](https://learn.microsoft.com/windows/powertoys/command-palette/overview)

---

**Made with ❤️ for developers by developers**

## Configuration

Dev Launchpad automatically creates a `config.json` file in Windows packaged app storage:

**Location**: `%LOCALAPPDATA%\Packages\DevLaunchpad_<publisher-id>\LocalState\DevLaunchpad\config.json`

You can find the exact path by navigating to **Configuration** in the extension.

### Editing settings in Command Palette

You don't have to hand-edit JSON for the common options. Open the **Settings** command (or the
gear in the Command Palette extension manager) to edit the **repository root**, **editor command**,
and **terminal command** directly in-palette. Changes are saved straight back to `config.json`, so
the form and the file always agree. The list-based options (local servers, favorite websites,
custom commands) are still edited in `config.json`.

> **Tip:** The **Repositories** page supports live search — start typing to filter discovered
> repos by name or path.

### Configuration Options

```json
{
  "RepoRoot": "C:\\Projects",           // Root folder to scan for Git repositories
  "EditorCommand": "code",               // Command to launch your editor (e.g., "code", "notepad++")
  "TerminalCommand": "wt",               // Terminal command (e.g., "wt", "powershell")

  "LocalUrls": [                         // Development server URLs
    {
      "Title": "Frontend",
      "Url": "http://localhost:5173"
    },
    {
      "Title": "Backend API",
      "Url": "http://localhost:8000"
    },
    {
      "Title": "React App",
      "Url": "http://localhost:3000"
    }
  ],

  "FavoriteWebsites": [                  // Quick-access bookmarks
    {
      "Title": "GitHub",
      "Url": "https://github.com"
    },
    {
      "Title": "ChatGPT",
      "Url": "https://chatgpt.com"
    }
  ],

  "CustomCommands": [                    // User-defined commands
    {
      "Title": "Open PowerShell",
      "Type": "command",                 // Types: url, folder, command, terminal-in-folder
      "Target": "powershell.exe",
      "Arguments": ""
    },
    {
      "Title": "Open Projects Folder",
      "Type": "folder",
      "Target": "C:\\Projects",
      "Arguments": ""
    },
    {
      "Title": "Terminal in Projects",
      "Type": "terminal-in-folder",
      "Target": "C:\\Projects",
      "Arguments": ""
    }
  ]
}
```

### Custom Command Types

- **`url`**: Opens URLs in the default browser
- **`folder`**: Opens folders in Windows Explorer
- **`command`**: Executes programs/scripts with optional arguments
- **`terminal-in-folder`**: Opens your configured terminal in a specific directory

### Editing Configuration

1. **Via Extension**:
   - Open Dev Launchpad → Configuration
   - Select "Open Config File in Editor"

2. **Manual Edit**:
   - Navigate to config folder (shown in Configuration page)
   - Edit `config.json` with your preferred editor
   - Use "Reload Config" to apply changes

3. **Reset to Defaults**:
   - Configuration → "Reset Config to Defaults"

## Screenshots

[Add screenshots here showing the extension in action]

- Main menu
- Repository browser
- Configuration page
- Custom commands in action

Screenshots and design assets are stored in `docs/images/`.
