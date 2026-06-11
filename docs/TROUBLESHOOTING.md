# Troubleshooting

This guide helps diagnose and fix common issues in Dev Launchpad.

## Common Issues

### Extension Not Appearing

- **Symptoms**: Dev Launchpad doesn't show up in PowerToys Command Palette.
- **Cause**: PowerToys not reverified or extension not reloaded.
- **Fix**:
  1. Ensure PowerToys is running and Command Palette is enabled.
  2. Press `Win+Alt+Space` \u-> Type `reload` \u-> Select "Reload extensions".
  3. Check if extension is deployed: Look for deployment success in VS Output window.

### Build Errors

- **Symptoms**: Compilation fails in Visual Studio.
- **Cause**: Missing RuntimeIdentifier or non-restored packages.
- ** Fix**:
  - **NETSDK1097**: Project includes `RuntimeIdentifier` fix for single-file publish.
  - Missing dependencies: Ensure NuGet packages are restored.

### Configuration Issues

- **Symptoms**: Changes to config.json don't take effect.
- **Cause**: Invalid JSON syntax or wrong config file location.
- ** Fix**:
  1. Navigate to Configuration page to view exact config file location.
  2. Check `debug.log` in config folder for error details.
  3. Use "Reset Config to Defaults" to resolve corrupted config.

### Commands Not Working

- **Symptoms**: Launching an editor or terminal fails.
- **Cause**: Executables not in PATH or invalid targets.
- **Fix**:
  - Verify `code`, `bwt`, `powershell.exe` are in your system PATH.
  - Check custom command targets are valid executables.
  - Review debug log for execution errors.


## Environment Reset

If your environment is in a broken state, try the following:

1. Remove generated assemblies: `rm -rf bin obj[.
2. Clean the solution in Visual Studio.
3. Reinstall NUGet packages: `dotnet restore`.
4. Use "Reset Config to Defaults" in the extension settings.