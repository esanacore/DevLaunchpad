using System.IO;
using Xunit;

namespace DevLaunchpad.Tests;

/// <summary>
/// Tests for <see cref="ProcessLauncher"/> input validation and helper methods.
///
/// We can only test the pre-condition checks (null/empty/missing-path guards)
/// because actually launching processes requires a full Windows shell. The important
/// thing is that invalid inputs produce toasts instead of exceptions.
/// </summary>
public sealed class ProcessLauncherTests
{
    // ── OpenFolder ──────────────────────────────────────────────────

    [Fact]
    public void OpenFolder_NullPath_ReturnsToast()
    {
        var result = ProcessLauncher.OpenFolder(null!);

        // CommandResult doesn't expose its toast message directly, but we can verify
        // it doesn't throw and returns a non-null result.
        Assert.NotNull(result);
    }

    [Fact]
    public void OpenFolder_EmptyPath_ReturnsToast()
    {
        var result = ProcessLauncher.OpenFolder("");

        Assert.NotNull(result);
    }

    [Fact]
    public void OpenFolder_WhitespacePath_ReturnsToast()
    {
        var result = ProcessLauncher.OpenFolder("   ");

        Assert.NotNull(result);
    }

    [Fact]
    public void OpenFolder_NonexistentPath_ReturnsToast()
    {
        var result = ProcessLauncher.OpenFolder(@"C:\This\Path\Does\Not\Exist\12345");

        Assert.NotNull(result);
    }

    // ── OpenUrl ─────────────────────────────────────────────────────

    [Fact]
    public void OpenUrl_NullUrl_ReturnsToast()
    {
        var result = ProcessLauncher.OpenUrl(null!);

        Assert.NotNull(result);
    }

    [Fact]
    public void OpenUrl_EmptyUrl_ReturnsToast()
    {
        var result = ProcessLauncher.OpenUrl("");

        Assert.NotNull(result);
    }

    [Fact]
    public void OpenUrl_WhitespaceUrl_ReturnsToast()
    {
        var result = ProcessLauncher.OpenUrl("   ");

        Assert.NotNull(result);
    }

    // ── RunCommand ──────────────────────────────────────────────────

    [Fact]
    public void RunCommand_NullTarget_ReturnsToast()
    {
        var result = ProcessLauncher.RunCommand(null!, "");

        Assert.NotNull(result);
    }

    [Fact]
    public void RunCommand_EmptyTarget_ReturnsToast()
    {
        var result = ProcessLauncher.RunCommand("", "some args");

        Assert.NotNull(result);
    }

    // ── RunPowerShellScriptVisible ──────────────────────────────────

    [Fact]
    public void RunPowerShellScriptVisible_NullScriptPath_ReturnsToast()
    {
        var result = ProcessLauncher.RunPowerShellScriptVisible(null!, ".");

        Assert.NotNull(result);
    }

    [Fact]
    public void RunPowerShellScriptVisible_MissingScript_ReturnsToast()
    {
        var result = ProcessLauncher.RunPowerShellScriptVisible(
            @"C:\This\Script\Does\Not\Exist\12345.ps1",
            ".");

        Assert.NotNull(result);
    }

    [Fact]
    public void RunPowerShellScriptVisible_MissingWorkingDirectory_ReturnsToast()
    {
        // Use a real existing file as the script so the guard we exercise is the directory check.
        string scriptPath = Path.GetTempFileName();
        try
        {
            var result = ProcessLauncher.RunPowerShellScriptVisible(
                scriptPath,
                @"C:\This\Folder\Does\Not\Exist\12345");

            Assert.NotNull(result);
        }
        finally
        {
            File.Delete(scriptPath);
        }
    }

    // ── IsWindowsTerminal ───────────────────────────────────────────

    [Theory]
    [InlineData("wt", true)]
    [InlineData("wtd", true)]
    [InlineData("WT", true)]
    [InlineData("Wt.exe", true)]
    [InlineData("cmd", false)]
    [InlineData("powershell", false)]
    [InlineData("pwsh", false)]
    [InlineData("bash", false)]
    public void IsWindowsTerminal_DetectsCorrectly(string command, bool expected)
    {
        bool result = ProcessLauncher.IsWindowsTerminal(command);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsWindowsTerminal_FullPath_DetectsWt()
    {
        // When a full path is given, GetFileNameWithoutExtension extracts "wt".
        bool result = ProcessLauncher.IsWindowsTerminal(@"C:\Program Files\WindowsApps\wt.exe");

        Assert.True(result);
    }
}
