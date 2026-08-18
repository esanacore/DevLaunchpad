using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DevLaunchpad;

public sealed class StoreReadinessReport
{
    public List<string> PassedChecks { get; } = [];
    public List<string> Failures { get; } = [];
    public bool IsReady => Failures.Count == 0;
}

public static class StoreReadinessChecker
{
    private static readonly XNamespace ManifestNs = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";
    private static readonly XNamespace RescapNs = "http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities";

    public static StoreReadinessReport CheckRepository(string repoRoot)
    {
        var report = new StoreReadinessReport();

        if (string.IsNullOrWhiteSpace(repoRoot) || !Directory.Exists(repoRoot))
        {
            report.Failures.Add("Repository root does not exist.");
            return report;
        }

        CheckFile(report, repoRoot, Path.Combine("DevLaunchpad", "Package.appxmanifest"), "Package.appxmanifest is present.");
        CheckFile(report, repoRoot, Path.Combine("docs", "STORE.md"), "Microsoft Store submission guide is present.");
        CheckFile(report, repoRoot, Path.Combine("docs", "PRIVACY.md"), "Privacy policy document is present.");
        CheckFile(report, repoRoot, Path.Combine("DevLaunchpad", "Properties", "PublishProfiles", "win-x64.pubxml"), "x64 publish profile is present.");
        CheckFile(report, repoRoot, Path.Combine("DevLaunchpad", "Properties", "PublishProfiles", "win-arm64.pubxml"), "arm64 publish profile is present.");

        string manifestPath = Path.Combine(repoRoot, "DevLaunchpad", "Package.appxmanifest");
        if (!File.Exists(manifestPath))
        {
            return report;
        }

        XDocument manifest;
        try
        {
            manifest = XDocument.Load(manifestPath);
        }
        catch (Exception ex) when (ex is IOException or System.Xml.XmlException)
        {
            report.Failures.Add($"Package.appxmanifest could not be parsed: {ex.Message}");
            return report;
        }

        XElement? identity = manifest.Root?.Element(ManifestNs + "Identity");
        if (identity == null)
        {
            report.Failures.Add("Package.appxmanifest is missing Identity.");
        }
        else
        {
            CheckAttribute(report, identity, "Name", "Manifest identity name is present.");
            CheckAttribute(report, identity, "Publisher", "Manifest publisher is present.");
            CheckAttribute(report, identity, "Version", "Manifest package version is present.");
            CheckVersionConsistency(report, repoRoot, identity.Attribute("Version")?.Value);
        }

        XElement? properties = manifest.Root?.Element(ManifestNs + "Properties");
        XElement? publisherDisplayName = properties?.Element(ManifestNs + "PublisherDisplayName");
        if (publisherDisplayName == null || string.IsNullOrWhiteSpace(publisherDisplayName.Value))
        {
            report.Failures.Add("Package.appxmanifest is missing PublisherDisplayName.");
        }
        else
        {
            report.PassedChecks.Add("Manifest publisher display name is present.");
        }

        var capabilities = manifest
            .Descendants(RescapNs + "Capability")
            .Select(c => c.Attribute("Name")?.Value)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (capabilities.Contains("runFullTrust"))
        {
            report.PassedChecks.Add("Restricted capability runFullTrust is declared for Command Palette COM activation.");
        }
        else
        {
            report.Failures.Add("Package.appxmanifest is missing the runFullTrust capability required by the COM server.");
        }

        return report;
    }

    private static void CheckFile(StoreReadinessReport report, string repoRoot, string relativePath, string successMessage)
    {
        if (File.Exists(Path.Combine(repoRoot, relativePath)))
        {
            report.PassedChecks.Add(successMessage);
        }
        else
        {
            report.Failures.Add($"{relativePath} is missing.");
        }
    }

    /// <summary>
    /// Verifies the repository-root <c>VERSION</c> file agrees with the manifest
    /// <c>Identity/@Version</c>. The manifest carries a four-part version (e.g. <c>1.2.0.0</c>)
    /// while the <c>VERSION</c> file carries the SemVer core (e.g. <c>1.2.0</c>); they must match on
    /// their first three components so the two sources of truth cannot silently drift.
    /// </summary>
    private static void CheckVersionConsistency(StoreReadinessReport report, string repoRoot, string? manifestVersion)
    {
        string versionFilePath = Path.Combine(repoRoot, "VERSION");
        if (!File.Exists(versionFilePath))
        {
            report.Failures.Add("VERSION file is missing.");
            return;
        }

        if (string.IsNullOrWhiteSpace(manifestVersion))
        {
            // Absence of the manifest version is already reported by the Version attribute check.
            return;
        }

        string fileVersion = File.ReadAllText(versionFilePath).Trim();
        if (VersionCore(fileVersion) != VersionCore(manifestVersion))
        {
            report.Failures.Add(
                $"VERSION file ('{fileVersion}') and manifest Identity/@Version ('{manifestVersion}') disagree.");
        }
        else
        {
            report.PassedChecks.Add("VERSION file and manifest package version agree.");
        }
    }

    /// <summary>
    /// Returns the first three dot-separated components of a version string (major.minor.patch),
    /// padding missing components with <c>0</c>, so <c>1.2</c>, <c>1.2.0</c>, and <c>1.2.0.0</c>
    /// all normalize to <c>1.2.0</c>.
    /// </summary>
    internal static string VersionCore(string version)
    {
        string[] parts = version.Trim().Split('.');
        string Part(int i) => i < parts.Length && int.TryParse(parts[i], out int n)
            ? n.ToString(CultureInfo.InvariantCulture)
            : "0";
        return $"{Part(0)}.{Part(1)}.{Part(2)}";
    }

    private static void CheckAttribute(StoreReadinessReport report, XElement element, string name, string successMessage)
    {
        if (string.IsNullOrWhiteSpace(element.Attribute(name)?.Value))
        {
            report.Failures.Add($"Package.appxmanifest Identity/@{name} is missing.");
        }
        else
        {
            report.PassedChecks.Add(successMessage);
        }
    }
}
