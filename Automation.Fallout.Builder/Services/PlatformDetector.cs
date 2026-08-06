using System.Xml.Linq;
using Automation.Fallout.Builder.Models;

namespace Automation.Fallout.Builder.Services;

/// <summary>
/// Works out which CI platform a repository targets by looking at the package feeds in its
/// nuget.config. A GitHub Packages feed means the repository lives on GitHub; an Azure Artifacts
/// feed means it lives in Azure DevOps.
/// </summary>
public static class PlatformDetector
{
    private const string GitHubPackagesHost = "nuget.pkg.github.com";

    private static readonly string[] AzureArtifactsHosts =
    {
        "pkgs.dev.azure.com",
        "pkgs.visualstudio.com"
    };

    /// <summary>
    /// The nuget.config that sits alongside the solution, falling back to one at the repository
    /// root. Returns null when the repository has neither.
    /// </summary>
    public static string? FindNuGetConfig(string repositoryRoot)
    {
        if (!Directory.Exists(repositoryRoot))
            return null;

        var searchDirectories = new List<string>();

        // "Alongside the solution file" first - a repository can keep its solution in a subdirectory.
        foreach (var pattern in new[] { "*.sln", "*.slnx" })
        {
            foreach (var solution in Directory.GetFiles(repositoryRoot, pattern, SearchOption.AllDirectories))
            {
                var directory = Path.GetDirectoryName(solution);
                if (directory != null && !searchDirectories.Contains(directory))
                    searchDirectories.Add(directory);
            }
        }

        if (!searchDirectories.Contains(repositoryRoot))
            searchDirectories.Add(repositoryRoot);

        foreach (var directory in searchDirectories)
        {
            // NuGet treats the file name case-insensitively and both spellings appear in the wild.
            var match = Directory.GetFiles(directory, "*.config")
                .FirstOrDefault(f => string.Equals(Path.GetFileName(f), "nuget.config", StringComparison.OrdinalIgnoreCase));

            if (match != null)
                return match;
        }

        return null;
    }

    /// <summary>
    /// Reads the feeds out of a nuget.config. Returns null when the file cannot be parsed or names
    /// no recognisable feed, which leaves the caller free to fall back on an explicit --platform.
    /// </summary>
    public static BuildPlatform? DetectFromContent(string nugetConfigXml)
    {
        var feeds = ReadFeeds(nugetConfigXml);
        if (feeds == null)
            return null;

        var hasGitHub = feeds.Any(f => f.Contains(GitHubPackagesHost, StringComparison.OrdinalIgnoreCase));
        var hasAzure = feeds.Any(f => AzureArtifactsHosts.Any(host => f.Contains(host, StringComparison.OrdinalIgnoreCase)));

        // A repository wired to both is ambiguous for code generation, so say nothing and let the
        // caller use the explicit --platform value rather than guessing wrong.
        if (hasGitHub == hasAzure)
            return null;

        return hasGitHub ? BuildPlatform.GitHubActions : BuildPlatform.AzureDevOps;
    }

    /// <summary>The feed URLs in a nuget.config, or null when it cannot be parsed.</summary>
    private static List<string>? ReadFeeds(string nugetConfigXml)
    {
        try
        {
            return XDocument.Parse(nugetConfigXml)
                .Descendants("packageSources")
                .Elements("add")
                .Select(e => (string?)e.Attribute("value"))
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value!)
                .ToList();
        }
        catch (System.Xml.XmlException)
        {
            return null;
        }
    }

    /// <summary>
    /// Whether a nuget.config can reach GitHub Packages. Automation.Fallout.Components is published
    /// there and not to nuget.org, so a GitHub repository without this feed cannot restore its own
    /// build project.
    /// </summary>
    public static bool NamesGitHubPackagesFeed(string nugetConfigXml) =>
        ReadFeeds(nugetConfigXml)?.Any(f => f.Contains(GitHubPackagesHost, StringComparison.OrdinalIgnoreCase)) == true;

    /// <summary>
    /// Detects the platform for a repository, reporting the nuget.config it used.
    /// </summary>
    public static BuildPlatform? DetectFromRepository(string repositoryRoot, out string? nugetConfigPath)
    {
        nugetConfigPath = FindNuGetConfig(repositoryRoot);
        if (nugetConfigPath == null)
            return null;

        try
        {
            return DetectFromContent(File.ReadAllText(nugetConfigPath));
        }
        catch (IOException)
        {
            return null;
        }
    }
}
