# Dev Launchpad Architecture

This document provides a technical overview of the Dev Launchpad extension architecture, design patterns, and implementation details.

## System Overview

Dev Launchpad is a Command Palette extension built on .NET 9 with Windows App SDK. It operates as an out-of-process COM server that interfaces with the PowerToys Command Palette host.

## Core Components

### 1. DevLaunchpad.cs - Extension Entry Point

**Purpose**: Implements IExtension interface for Command Palette integration

**Responsibilities**:
- COM class registration (GUID: `3ccae5b0-6a8e-4a44-98f7-d5a7b7bcbb43`)
- Provider lifecycle management
- Extension disposal signaling

### 2. Program.cs - COM Server Bootstrap

**Purpose**: Manages out-of-process COM server lifecycle

**Responsibilities**:
- Parses `-RegisterProcessAsComServer` argument
- Registers COM classes with Shmuelie.WinRTServer
- Manages extension singleton instance
- Handles server start/stop/cleanup

### 3. DevLaunchpadCommandsProvider.cs

**Purpose**: Defines top-level commands exposed to Command Palette

**Responsibilities**:
- Implements CommandProvider base class
- Defines TopLevelCommands() array
- Sets extension display name and icon
- Maps commands to page implementations

### 4. Page Implementations

Each page inherits from ListPage and implements specific features: RepoPage, DevToolsPage, LocalServersPage, FavoriteWebsitesPage, CustomCommandsPage, ConfigPage.

### 5. DevLaunchpadConfig.cs

**Purpose**: Configuration persistence and management

**Features**:
- **Auto-creation**: Creates default config on first run
- **Windows Storage**: Uses ApplicationData.Current.LocalFolder (config.json and debug.log)
- **Manages hot reload, validation, and debug logging**

### 6. DevLaunchpadJsonContext.cs

**Purpose**: AOT-compatible JSON serialization using System.Text.Json source generators.

## Data Flow

1. **Configuration Loading**: Config is loaded from JSON into memory on application start.
2. **Command Execution**: PowerToys interfaces via COM; user selections trigger page generation and action execution.

## Key Technologies

- **Runtime**: .NET 9 (Windows App SDK)
- **Interface**: Microsoft.CommandPalette.Extensions
- **Communication**: WinRT COM Server (out-of-process)
- **Packaging**: MSIX Packaging (Windows Store distribution)
- **Serialization**: Source-Generated JSON (AOT-compatible)

## Repository Structure

- `DevLaunchpad/`: Main extension project (Core logic).
- `DevLaunchpad.Tests/`: Automated xUnit tests.
- `docs/`: Supplemental documentation.
- `constitution/`: Universal engineering rules.

---

**Last Updated**: 2026-06-10
**Document Version**: 1.1