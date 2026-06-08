using System;
using System.IO;

namespace DevLaunchpad.Tests.Helpers;

/// <summary>
/// Sets <see cref="DevLaunchpadConfig.ConfigDirectoryOverride"/> to an isolated temp
/// directory for the lifetime of the fixture. Disposing restores the override to <c>null</c>
/// and deletes the temp directory.
///
/// Usage: wrap each test (or test class) in a <c>using</c> block.
/// </summary>
internal sealed class TempConfigDir : IDisposable
{
    public string DirectoryPath { get; }

    public TempConfigDir()
    {
        DirectoryPath = Path.Combine(Path.GetTempPath(), "DevLaunchpadConfigTests_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(DirectoryPath);
        DevLaunchpadConfig.ConfigDirectoryOverride = DirectoryPath;
    }

    /// <summary>The full path to the config.json within this temp directory.</summary>
    public string ConfigFilePath => DevLaunchpadConfig.GetConfigPath();

    public void Dispose()
    {
        DevLaunchpadConfig.ConfigDirectoryOverride = null;

        try
        {
            if (Directory.Exists(DirectoryPath))
            {
                Directory.Delete(DirectoryPath, recursive: true);
            }
        }
        catch
        {
            // Best-effort cleanup.
        }
    }
}
