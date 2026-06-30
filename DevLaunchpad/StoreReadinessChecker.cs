using System;
using System.Collections.Generic;
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
