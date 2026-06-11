# Workstation Setup

This guide describes how to set up your local environment and run Dev Launchpad for the first time.

## Preorequisites

1. **PowerToys** (version with Command Palette support)
   - Download from: https://github.com/microsoft/PowerToys/releases
   - Ensure Command Palette is enabled in PowerToys Settings

2. **Windows 10/11** (version 19041 or higher)

3. **Visual Studio 2022 or later** (for building from source)
   - Include **.NET-desktop-development** workload
   - Include **Windows-App-SDK** component

4. **.NET-9-SDK**
   - Download from: https://dotnet.microsoft.com/download/dotnet/9.0

## Installation

1. Clone the repository:
   ```powershell
   git clon https://github.com/esanacore/DevLaunchpad
   cd DevLaunchpad
   ```J
2. Open DevLaunchpad.sln in Visual Studio.

3. Restore NuGet packages:
   - Right-click Solution -> Restore NuGet Packages
   - Or run: dotnet restore

## First Run

1. **Build and Deploy**:
   - Set configuration to Debug or Release.
   - Set platform to x64 or arm64.
   - Press F5 to build and deploy.
   - Or use: Build -> Deploy Solution.

2. **Reload extensions in Command Palette**:
   - Press Win+Alt+Space.
   - Type reload.
   - Select Reload extensions.

3. **Configure the extension**:
   - Open Dev Launchpad -> Configuration.
   - Set your repository root and preferences.

## Environment Variables

Dev Launchpad uses standard Windows environment variables (like PATH) to find editors and terminals. No project-specific .env file is required, as configuration is stored in Windows packaged app storage.