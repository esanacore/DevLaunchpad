using System.IO;
using System.Linq;
using Xunit;

namespace DevLaunchpad.Tests;

public sealed class StoreReadinessCheckerTests
{
    [Fact]
    public void CheckRepository_FindsRequiredStoreSubmissionAssets()
    {
        string repoRoot = FindRepoRoot();

        var report = StoreReadinessChecker.CheckRepository(repoRoot);

        Assert.Empty(report.Failures);
        Assert.Contains(report.PassedChecks, c => c.Contains("privacy", System.StringComparison.OrdinalIgnoreCase));
        Assert.Contains(report.PassedChecks, c => c.Contains("x64", System.StringComparison.OrdinalIgnoreCase));
        Assert.Contains(report.PassedChecks, c => c.Contains("arm64", System.StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void CheckRepository_ReportsMissingManifest()
    {
        string tempRoot = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempRoot);

        try
        {
            var report = StoreReadinessChecker.CheckRepository(tempRoot);

            Assert.Contains(report.Failures, f => f.Contains("Package.appxmanifest"));
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "DevLaunchpad.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? Directory.GetCurrentDirectory();
    }
}
