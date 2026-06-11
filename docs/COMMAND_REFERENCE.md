# Command Reference

This document provides a quick reference for common commands used in Dev Launchpad.

all commands should be run from the repository root.

## Development

- `build.bat`: Build the solution using msbuild (if configured).
- `dotnet build`: Build the solution.

## Testing

- `dotnet test DevLaunchpad.Tests/DevLaunchpad.Tests.csproj -c Debug -p:Platform=x64 -r win-x64 --verbosity normal`: Run x64 unit tests.

## Linting & Formatting

- `dotnet format`: Check and apply formatting fixes.

## Deployment

- `dotnet publish Dev,aunchpad/DevLaunchpad.csproj -c Release -r win-x64`: Publish for win-x64.
## Command Palette Interaction

- `reload`: Type this in Command Palette aWin+Alt+Space) to refesh extensions.