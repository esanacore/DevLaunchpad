# Dev Launchpad Architecture

This document provides a technical overview of the Dev Launchpad extension architecture, design patterns, and implementation details.

## Table of Contents

- [Overview](#overview)
- [Architecture Diagram](#architecture-diagram)
- [Core Components](#core-components)
- [Data Flow](#data-flow)
- [Extension Lifecycle](#extension-lifecycle)
- [Configuration System](#configuration-system)
- [Page System](#page-system)
- [Security Considerations](#security-considerations)

## Overview

Dev Launchpad is a Command Palette extension built on:
- **.NET 9** with Windows App SDK
- **Out-of-process COM server** architecture
- **MSIX packaging** for Windows Store distribution
- **Source-generated JSON** for AOT compatibility

### Technology Stack

```
┌─────────────────────────────────────────────┐
│         PowerToys Command Palette           │
│    (Host Application - In Process)          │
└─────────────────┬───────────────────────────┘
                  │ COM/WinRT Interface
                  │ (IExtension, ICommandProvider)
┌─────────────────▼───────────────────────────┐
│       Dev Launchpad Extension               │
│    (Out-of-Process COM Server)              │
│                                             │
│  ┌─────────────────────────────────────┐  │
│  │   DevLaunchpad.cs                    │  │
│  │   (IExtension Implementation)        │  │
│  └───────────────┬─────────────────────┘  │
│                  │                          │
│  ┌───────────────▼─────────────────────┐  │
│  │   DevLaunchpadCommandsProvider      │  │
│  │   (CommandProvider Base)            │  │
│  └───────────────┬─────────────────────┘  │
│                  │                          │
│  ┌───────────────▼─────────────────────┐  │
│  │         Page Implementations         │  │
│  │  - RepoPage                          │  │
│  │  - DevToolsPage                      │  │
│  │  - LocalServersPage                  │  │
│  │  - FavoriteWebsitesPage              │  │
│  │  - CustomCommandsPage                │  │
│  │  - ConfigPage                        │  │
│  └──────────────────────────────────────┘  │
│                                             │
│  ┌──────────────────────────────────────┐  │
│  │   DevLaunchpadConfig                 │  │
│  │   (Configuration Management)         │  │
│  └──────────────────────────────────────┘  │
└─────────────────────────────────────────────┘
```

## Core Components

### 1. DevLaunchpad.cs - Extension Entry Point

**Purpose**: Implements `IExtension` interface for Command Palette integration

**Responsibilities**:
- COM class registration (GUID: `3ccae5b0-6a8e-4a44-98f7-d5a7b7bcbb43`)
- Provider lifecycle management
- Extension disposal signaling

```csharp
[Guid("3ccae5b0-6a8e-4a44-98f7-d5a7b7bcbb43")]
public sealed partial class DevLaunchpad : IExtension, IDisposable
{
    public object? GetProvider(ProviderType providerType)
    {
        return providerType switch
        {
            ProviderType.Commands => _provider,
            _ => null,
        };
    }
}
```

### 2. Program.cs - COM Server Bootstrap

**Purpose**: Manages out-of-process COM server lifecycle

**Responsibilities**:
- Parses `-RegisterProcessAsComServer` argument
- Registers COM classes with Shmuelie.WinRTServer
- Manages extension singleton instance
- Handles server start/stop/cleanup

**Key Pattern**: Single extension instance returned for all COM activations

### 3. DevLaunchpadCommandsProvider.cs

**Purpose**: Defines top-level commands exposed to Command Palette

**Responsibilities**:
- Implements `CommandProvider` base class
- Defines `TopLevelCommands()` array
- Sets extension display name and icon
- Maps commands to page implementations

### 4. Page Implementations

Each page inherits from `ListPage` and implements specific features:

| Page | Purpose | Key Features |
|------|---------|--------------|
| `RepoPage` | Browse Git repos | Recursive .git detection, relative paths |
| `RepoActionsPage` | Repo actions | Open folder/editor/terminal |
| `DevToolsPage` | Quick launch tools | Configurable tool shortcuts |
| `LocalServersPage` | Local URLs | Configurable localhost links |
| `FavoriteWebsitesPage` | Website bookmarks | Quick URL access |
| `CustomCommandsPage` | Custom commands | 4 command types (url/folder/command/terminal) |
| `ConfigPage` | Settings UI | Config management, debug access |

### 5. DevLaunchpadConfig.cs

**Purpose**: Configuration persistence and management

**Features**:
- **Auto-creation**: Creates default config on first run
- **Windows Storage**: Uses `ApplicationData.Current.LocalFolder` (persistent; legacy `LocalCacheFolder` configs are migrated automatically)
- **Validation**: Ensures config integrity
- **Debug Logging**: Tracks load/save operations
- **Hot Reload**: Supports runtime config changes

**Storage Path**:
```
%LOCALAPPDATA%\Packages\DevLaunchpad_<publisherId>\LocalState\DevLaunchpad\
├── config.json       # User configuration
└── debug.log         # Debug and error log
```

### 6. DevLaunchpadJsonContext.cs

**Purpose**: AOT-compatible JSON serialization

**Implementation**:
- Uses `System.Text.Json` source generators
- No runtime reflection (trim-safe)
- Supports all configuration types
- Pretty-printed output (`WriteIndented = true`)

```csharp
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(DevLaunchpadConfig))]
[JsonSerializable(typeof(NamedUrl))]
[JsonSerializable(typeof(CustomCommandConfig))]
internal sealed partial class DevLaunchpadJsonContext : JsonSerializerContext
```

## Data Flow

### Configuration Loading

```
Application Start
    │
    ├─> Load config.json
    │   ├─> File exists? → Deserialize JSON
    │   └─> File missing? → Create default config
    │
    └─> Configuration loaded into memory
        │
        └─> Pages access via DevLaunchpadConfig.Load()
```

### Command Execution

```
User opens Command Palette (Win+Alt+Space)
    │
    ├─> PowerToys queries IExtension.GetProvider()
    │   └─> Returns DevLaunchpadCommandsProvider
    │
    ├─> User types "Dev Launchpad"
    │   └─> TopLevelCommands() provides menu items
    │
    ├─> User selects a command (e.g., "Repositories")
    │   └─> Command.Execute() called
    │       └─> RepoPage.GetItems() invoked
    │           ├─> Scans configured repo root
    │           ├─> Finds Git repositories
    │           └─> Returns list of CommandItems
    │
    └─> User selects repository → RepoActionsPage shown
        └─> User selects action → Process.Start() executed
```

## Extension Lifecycle

### Registration Phase

1. **MSIX Package Deployed** via Visual Studio
2. **AppX Manifest Parsed** by Windows
   - COM server registration (`com:ComServer`)
   - App extension declaration (`uap3:AppExtension`)
3. **Package Catalog Updated** with extension metadata

### Activation Phase

1. **PowerToys queries** Package Catalog for Command Palette extensions
2. **COM Activation** occurs when user opens Command Palette
   - `DevLaunchpad.exe -RegisterProcessAsComServer` launched
   - COM class registered with GUID
3. **Extension Instance Created** via `ComServer.RegisterClass<>()`
4. **GetProvider()** called to retrieve `ICommandProvider`

### Execution Phase

1. **Commands displayed** in Command Palette UI
2. **User interaction** triggers command execution
3. **Pages dynamically load** data (repos, config, etc.)
4. **Actions executed** (open folder, launch app, etc.)

### Disposal Phase

1. **User closes Command Palette** or extension unloads
2. **Dispose()** called on extension instance
3. **ManualResetEvent signaled** to terminate COM server
4. **Process exits** cleanly

## Configuration System

### Design Goals

- **Auto-initialization**: Works out of the box with sensible defaults
- **User-editable**: Plain JSON, easy to modify
- **Resilient**: Handles corruption, missing files, invalid JSON
- **Debuggable**: Logs all operations to debug.log

### Configuration Schema

```csharp
public sealed class DevLaunchpadConfig
{
    public string RepoRoot { get; set; }           // Git repo scan root
    public string EditorCommand { get; set; }       // Editor executable
    public string TerminalCommand { get; set; }     // Terminal executable
    public List<NamedUrl> LocalUrls { get; set; }
    public List<NamedUrl> FavoriteWebsites { get; set; }
    public List<CustomCommandConfig> CustomCommands { get; set; }
}
```

### Error Handling Strategy

```csharp
try
{
    Load config from file
    Validate JSON structure
    Deserialize to objects
}
catch (Exception ex)
{
    Log error to debug.log
    Return default configuration
    Continue execution (never crash)
}
```

## Page System

### Page Lifecycle

```
1. Page Constructor
   └─> Set Title, Name, Icon

2. GetItems() called by Command Palette
   ├─> Load configuration
   ├─> Query data sources (filesystem, config, etc.)
   ├─> Build CommandItem array
   └─> Return items to display

3. User selects item
   └─> Command.Execute() or navigate to child page
```

### Best Practices

- **Lazy Loading**: Load data in `GetItems()`, not constructor
- **Error Resilience**: Try-catch around filesystem/process operations
- **User Feedback**: Show helpful messages for empty/error states
- **Performance**: Cache expensive operations where appropriate

## Security Considerations

### Threat Model

- **No Network Access**: Extension doesn't make network calls
- **File System Access**: Limited to configured directories
- **Process Execution**: Uses `UseShellExecute = true` for OS-level validation
- **Configuration**: Stored in isolated app storage (not user-editable via registry)

### Security Practices

1. **No Credential Storage**: Never store passwords in config
2. **Path Validation**: Validate paths exist before operations
3. **Shell Execution**: Rely on OS security for process launching
4. **Input Validation**: Validate configuration values
5. **Isolated Storage**: Use Windows packaged app storage

### Known Limitations

- **PATH Environment**: Relies on system PATH for executables
- **Process Permissions**: Inherits user's security context
- **File Access**: Can access any user-accessible path

## Performance Considerations

### Repository Scanning

- **Recursive Search**: Can be slow for deep directory trees
- **Mitigation**: Stops recursion when `.git` found
- **Future**: Consider caching, background scanning

### Configuration Loading

- **File I/O**: Synchronous, blocking operation
- **Frequency**: Loaded on-demand per page
- **Future**: Cache in memory, reload on file change detection

### Process Launching

- **Shell Execute**: Relatively fast, delegated to OS
- **Error Handling**: Failures logged, don't crash extension

## Extensibility Points

### Adding New Features

1. **Create new Page class** in `Pages/` folder
2. **Register in CommandsProvider**
3. **Extend configuration** if needed
4. **Update JSON context** for serialization

### Configuration Extensions

```csharp
// Add new property to DevLaunchpadConfig
public List<NewFeature> NewFeatures { get; set; }

// Add serialization support
[JsonSerializable(typeof(NewFeature))]
[JsonSerializable(typeof(List<NewFeature>))]
```

### Custom Command Types

Extend `CustomCommandsPage` to support new command types:

```csharp
case "new-type":
    items.Add(new ListItem(new AnonymousCommand(() => 
        YourCustomHandler(entry))));
    break;
```

## Future Architecture Improvements

### Potential Enhancements

1. **Async/Await**: Use async pattern for I/O operations
2. **Background Services**: Long-running tasks (git status, indexing)
3. **Caching Layer**: In-memory cache for expensive operations
4. **Event System**: Pub/sub for config changes, file watchers
5. **Plugin System**: Load additional features from external assemblies
6. **Telemetry**: Anonymous usage analytics (opt-in)

### Scalability Considerations

- **Large Repository Collections**: Virtualization, pagination
- **Frequent Config Changes**: Debounced reload, dirty checking
- **Multi-User**: Per-user configuration, profile switching

---

**Last Updated**: 2025-01-XX  
**Document Version**: 1.0
