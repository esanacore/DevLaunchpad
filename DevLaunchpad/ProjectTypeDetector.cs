// Copyright (c) Eric Sanacore
// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.IO;
using System.Linq;

namespace DevLaunchpad;

/// <summary>
/// Infers a repository's primary technology stack by inspecting the marker files in its root
/// directory (e.g. <c>package.json</c>, <c>Cargo.toml</c>, a <c>*.sln</c>).
///
/// Like <see cref="GitHelper"/>, this is read-only and never spawns a process, so it is cheap
/// enough to call for every repository while building the list. Detection is intentionally
/// shallow (root directory only) to keep the repository scan bounded.
/// </summary>
internal static class ProjectTypeDetector
{
    /// <summary>
    /// Single-file markers in priority order. The first match wins, so language ecosystems are
    /// listed ahead of generic tooling like Docker.
    /// </summary>
    private static readonly (string FileName, string Label)[] FileMarkers =
    [
        ("package.json", "Node"),
        ("Cargo.toml", "Rust"),
        ("go.mod", "Go"),
        ("pyproject.toml", "Python"),
        ("requirements.txt", "Python"),
        ("setup.py", "Python"),
        ("Pipfile", "Python"),
        ("pom.xml", "Java"),
        ("build.gradle", "Java"),
        ("build.gradle.kts", "Java"),
        ("pubspec.yaml", "Dart"),
        ("Gemfile", "Ruby"),
        ("composer.json", "PHP"),
        ("Dockerfile", "Docker"),
        ("docker-compose.yml", "Docker"),
    ];

    /// <summary>
    /// Glob markers checked before the single-file markers. Any file matching the pattern counts.
    /// </summary>
    private static readonly (string Pattern, string Label)[] GlobMarkers =
    [
        ("*.sln", ".NET"),
        ("*.csproj", ".NET"),
        ("*.fsproj", ".NET"),
        ("*.vbproj", ".NET"),
    ];

    /// <summary>
    /// Returns a short, human-friendly label for the repository's primary stack
    /// (e.g. ".NET", "Node", "Rust"), or <c>null</c> when no known marker is present.
    /// </summary>
    public static string? Detect(string repoPath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(repoPath) || !Directory.Exists(repoPath))
            {
                return null;
            }

            foreach (var (pattern, label) in GlobMarkers)
            {
                if (Directory.EnumerateFiles(repoPath, pattern).Any())
                {
                    return label;
                }
            }

            foreach (var (fileName, label) in FileMarkers)
            {
                if (File.Exists(Path.Combine(repoPath, fileName)))
                {
                    return label;
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            DevLaunchpadConfig.WriteDebugLog($"ProjectTypeDetector.Detect failed for '{repoPath}': {ex}");
            return null;
        }
    }
}
