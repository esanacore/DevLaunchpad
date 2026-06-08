# Contributing to Dev Launchpad

Thank you for your interest in contributing to Dev Launchpad! This document provides guidelines and instructions for contributing to the project.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Making Changes](#making-changes)
- [Testing](#testing)
- [Submitting Changes](#submitting-changes)
- [Style Guidelines](#style-guidelines)

## Code of Conduct

Be respectful, professional, and inclusive. We're all here to build better tools for developers.

## Getting Started

1. **Fork the repository** on GitHub
2. **Clone your fork** locally:
   ```powershell
   git clone https://github.com/YOUR-USERNAME/DevLaunchpad.git
   cd DevLaunchpad
   ```
3. **Add upstream remote**:
   ```powershell
   git remote add upstream https://github.com/esanacore/DevLaunchpad.git
   ```

## Development Setup

### Prerequisites

- **Visual Studio 2022 or later**
- **Windows 10/11** (version 19041+)
- **.NET 9 SDK**
- **PowerToys** (with Command Palette enabled)

### Initial Setup

1. Open `DevLaunchpad.sln` in Visual Studio
2. Restore NuGet packages (should happen automatically)
3. Build the solution: `Ctrl+Shift+B`
4. Deploy: Press `F5` or Build → Deploy Solution

### Testing Your Changes

1. Build and deploy the extension
2. In Command Palette (`Win+Alt+Space`):
   - Type `reload`
   - Select "Reload extensions"
3. Type "Dev Launchpad" to access your changes

## Making Changes

### Branch Naming

Use descriptive branch names:
- `feature/repository-icons` - New features
- `fix/config-loading-error` - Bug fixes
- `docs/update-readme` - Documentation updates
- `refactor/config-management` - Code refactoring

### Commit Messages

Write clear, descriptive commit messages:

```
Add Git status indicators to repository list

- Show branch name and uncommitted changes
- Add visual indicators for repo state
- Update RepoPage to query Git status
```

**Format**:
- First line: Brief summary (50 chars or less)
- Blank line
- Detailed description with bullet points

### Code Organization

- **One feature per Page**: Keep features modular in `Pages/` folder
- **Configuration**: Extend `DevLaunchpadConfig.cs` for new settings
- **JSON Serialization**: Use source-generated JSON (update `DevLaunchpadJsonContext.cs`)
- **Error Handling**: Use try-catch and log to debug log

### Adding New Features

When adding a new feature page:

1. **Create the Page class** in `Pages/` folder:
   ```csharp
   public sealed partial class YourFeaturePage : ListPage
   {
       public YourFeaturePage()
       {
           Title = "Your Feature";
           Name = "your-feature";
       }

       public override IListItem[] GetItems()
       {
           // Implementation
       }
   }
   ```

2. **Register in CommandsProvider** (`DevLaunchpadCommandsProvider.cs`):
   ```csharp
   new CommandItem(
       title: "Your Feature",
       subtitle: "Description of your feature"
   )
   {
       Command = new YourFeaturePage()
   }
   ```

3. **Update configuration** if needed (`DevLaunchpadConfig.cs`)

4. **Update documentation** (`README.md`, `CHANGELOG.md`)

## Testing

### Automated Tests

The project includes an xUnit test suite in `DevLaunchpad.Tests/`. Run it with:

```powershell
dotnet test DevLaunchpad.Tests/DevLaunchpad.Tests.csproj -c Debug -p:Platform=x64 -r win-x64 --self-contained --verbosity normal
```

The suite covers:

| File | What it tests |
|------|---------------|
| `ConfigLogicTests.cs` | Config load/save, recent repo tracking, pinning, reset |
| `ConfigSerializationTests.cs` | JSON round-trip, AOT source-generated context, defaults |
| `GitHelperTests.cs` | Branch parsing from `HEAD`, remote URL normalization |
| `ProcessLauncherTests.cs` | Input validation (null/empty/missing-path guards) |
| `RepoScannerTests.cs` | Repository discovery: depth limit, excluded directories, metadata |

Test helpers in `Helpers/` provide isolated fixtures:
- `TempConfigDir` — redirects config I/O to a throwaway temp directory.
- `TempGitRepo` — builds `.git` directory stubs for filesystem-level tests.

### Manual Testing Checklist

Before submitting:

- [ ] Extension builds without errors (`dotnet build`)
- [ ] Automated tests pass (`dotnet test`)
- [ ] Extension deploys successfully
- [ ] All existing features still work
- [ ] New feature works as expected
- [ ] Configuration loads correctly
- [ ] Error handling works (test with invalid config)
- [ ] Debug log shows expected output

### Configuration Testing

1. Test with default configuration
2. Test with custom configuration
3. Test with malformed JSON (should handle gracefully)
4. Test configuration reload
5. Test reset to defaults

## Submitting Changes

### Pull Request Process

1. **Update your fork**:
   ```powershell
   git fetch upstream
   git checkout main
   git merge upstream/main
   ```

2. **Create a branch**:
   ```powershell
   git checkout -b feature/your-feature-name
   ```

3. **Make your changes** and commit

4. **Push to your fork**:
   ```powershell
   git push origin feature/your-feature-name
   ```

5. **Open a Pull Request** on GitHub with:
   - Clear title and description
   - Reference related issues
   - Screenshots/GIFs if UI changes
   - Update to `CHANGELOG.md`

### Pull Request Checklist

- [ ] Code follows project style and patterns
- [ ] All existing features still work
- [ ] Automated tests pass (`dotnet test`)
- [ ] New features are documented
- [ ] `CHANGELOG.md` is updated
- [ ] Build succeeds without warnings
- [ ] No sensitive data in configuration examples

## Style Guidelines

### C# Code Style

- **Naming**: PascalCase for classes/methods, camelCase for fields/variables
- **Access Modifiers**: Explicit on all members
- **Null Safety**: Use nullable reference types (`string?`, null checks)
- **Documentation**: XML comments on public APIs
- **Formatting**: Use Visual Studio defaults (4-space indent)

### Example:

```csharp
/// <summary>
/// Loads configuration from the config file.
/// </summary>
/// <returns>Configuration object or default if load fails.</returns>
public static DevLaunchpadConfig Load()
{
    try
    {
        string configPath = GetConfigPath();
        // Implementation...
    }
    catch (Exception ex)
    {
        WriteDebugLog($"Load failed: {ex}");
        return new DevLaunchpadConfig();
    }
}
```

### JSON Configuration

- Use 2-space indentation
- Include comments in documentation, not in JSON
- Keep secrets out of configuration files
- Validate configuration before use

### Documentation

- Keep README.md up to date
- Update CHANGELOG.md for all changes
- Add XML comments for public APIs
- Include examples for new features

## Questions?

If you have questions or need help:

1. Check existing issues on GitHub
2. Review the [README.md](README.md)
3. Open a new issue with the `question` label

## License

By contributing, you agree that your contributions will be licensed under the same license as the project.

---

Thank you for contributing to Dev Launchpad! 🚀
