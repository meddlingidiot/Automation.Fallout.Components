using System.Text.Json;
using Spectre.Console;

namespace Automation.Fallout.Builder.Services;

/// <summary>
/// Asks the feeds configured for a repository for the newest released version of a package.
/// </summary>
/// <remarks>
/// The lookup runs from the repository root so that the repository's own nuget.config - the one that
/// carries the AFTR feeds - is the one that applies. 'dotnet package search' leaves prereleases out
/// unless --prerelease is passed, so what comes back is already the latest stable version.
/// </remarks>
public static class NuGetVersionResolver
{
    public static async Task<string?> GetLatestStableVersionAsync(string packageId, string workingDirectory)
    {
        var (succeeded, output, error) = await NuGetPackageInstaller.RunDotNetCommandCaptureAsync(
            $"package search {packageId} --exact-match --format json", workingDirectory);

        if (!succeeded)
        {
            AnsiConsole.MarkupLine($"[yellow]Could not query the feeds for {packageId.EscapeMarkup()}[/]");

            // 'dotnet package search' aborts on the first source that rejects it, so one feed needing
            // credentials loses the results from every other feed as well.
            if (NeedsCredentials(error) || NeedsCredentials(output))
            {
                AnsiConsole.MarkupLine(
                    "[yellow]A feed rejected the request. Set the credentials it needs - a GitHub Packages feed reads %GITHUB_TOKEN% - or pass the version explicitly.[/]");
            }

            return null;
        }

        try
        {
            return SelectLatestStableVersion(output);
        }
        catch (JsonException ex)
        {
            AnsiConsole.MarkupLine($"[yellow]Could not read the feed response for {packageId.EscapeMarkup()}: {ex.Message.EscapeMarkup()}[/]");
            return null;
        }
    }

    /// <summary>
    /// Picks the highest non-prerelease version out of a 'dotnet package search --format json'
    /// response. Every configured source contributes its own result block, and --exact-match makes
    /// each block list versions rather than a single latest, so all of them have to be compared.
    /// </summary>
    public static string? SelectLatestStableVersion(string json)
    {
        using var document = JsonDocument.Parse(json);

        if (!document.RootElement.TryGetProperty("searchResult", out var sources) ||
            sources.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        string? latest = null;

        foreach (var source in sources.EnumerateArray())
        {
            if (!source.TryGetProperty("packages", out var packages) || packages.ValueKind != JsonValueKind.Array)
                continue;

            foreach (var package in packages.EnumerateArray())
            {
                var version = ReadVersion(package);
                if (version == null || IsPrerelease(version))
                    continue;

                if (latest == null || Compare(version, latest) > 0)
                {
                    latest = version;
                }
            }
        }

        return latest;
    }

    /// <summary>'version' is what --exact-match emits, 'latestVersion' what a plain search emits.</summary>
    private static string? ReadVersion(JsonElement package)
    {
        if (package.TryGetProperty("version", out var version) && version.ValueKind == JsonValueKind.String)
            return version.GetString();

        if (package.TryGetProperty("latestVersion", out var latestVersion) && latestVersion.ValueKind == JsonValueKind.String)
            return latestVersion.GetString();

        return null;
    }

    private static bool IsPrerelease(string version) => version.Contains('-', StringComparison.Ordinal);

    public static bool NeedsCredentials(string output) =>
        output.Contains("401", StringComparison.Ordinal) ||
        output.Contains("Unauthorized", StringComparison.OrdinalIgnoreCase) ||
        output.Contains("403", StringComparison.Ordinal);

    private static int Compare(string left, string right)
    {
        var leftParts = NumericParts(left);
        var rightParts = NumericParts(right);

        for (var i = 0; i < Math.Max(leftParts.Length, rightParts.Length); i++)
        {
            var leftPart = i < leftParts.Length ? leftParts[i] : 0;
            var rightPart = i < rightParts.Length ? rightParts[i] : 0;

            if (leftPart != rightPart)
                return leftPart.CompareTo(rightPart);
        }

        return 0;
    }

    private static int[] NumericParts(string version) =>
        version.Split('+')[0]
            .Split('.')
            .Select(part => int.TryParse(part, out var number) ? number : 0)
            .ToArray();
}
