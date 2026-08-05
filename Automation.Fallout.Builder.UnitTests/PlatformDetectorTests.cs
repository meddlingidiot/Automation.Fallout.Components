using Automation.Fallout.Builder.Models;
using Automation.Fallout.Builder.Services;

namespace Automation.Fallout.Builder.UnitTests;

public class PlatformDetectorTests
{
    private const string GitHubFeed = "https://nuget.pkg.github.com/meddlingidiot/index.json";
    private const string AzureFeed = "https://pkgs.dev.azure.com/AFTR/_packaging/feed/nuget/v3/index.json";
    private const string NuGetOrgFeed = "https://api.nuget.org/v3/index.json";

    private static string ConfigWith(params string[] feeds)
    {
        var sources = string.Join(Environment.NewLine,
            feeds.Select((f, i) => $"        <add key=\"feed{i}\" value=\"{f}\" />"));

        return $"""
                <?xml version="1.0" encoding="utf-8"?>
                <configuration>
                    <packageSources>
                        <clear />
                {sources}
                    </packageSources>
                </configuration>
                """;
    }

    [Test]
    public async Task DetectFromContent_ReturnsGitHubActions_ForGitHubPackagesFeed()
    {
        var detected = PlatformDetector.DetectFromContent(ConfigWith(GitHubFeed));

        await Assert.That(detected).IsEqualTo(BuildPlatform.GitHubActions);
    }

    [Test]
    public async Task DetectFromContent_ReturnsAzureDevOps_ForAzureArtifactsFeed()
    {
        var detected = PlatformDetector.DetectFromContent(ConfigWith(AzureFeed));

        await Assert.That(detected).IsEqualTo(BuildPlatform.AzureDevOps);
    }

    [Test]
    public async Task DetectFromContent_IgnoresNuGetOrg()
    {
        // nuget.org says nothing about where the repository lives.
        var detected = PlatformDetector.DetectFromContent(ConfigWith(NuGetOrgFeed, GitHubFeed));

        await Assert.That(detected).IsEqualTo(BuildPlatform.GitHubActions);
    }

    [Test]
    public async Task DetectFromContent_ReturnsNull_WhenNoRecognisableFeed()
    {
        var detected = PlatformDetector.DetectFromContent(ConfigWith(NuGetOrgFeed));

        await Assert.That(detected).IsNull();
    }

    [Test]
    public async Task DetectFromContent_ReturnsNull_WhenBothPlatformsPresent()
    {
        // Ambiguous on purpose: the caller falls back to an explicit --platform rather than guessing.
        var detected = PlatformDetector.DetectFromContent(ConfigWith(GitHubFeed, AzureFeed));

        await Assert.That(detected).IsNull();
    }

    [Test]
    public async Task DetectFromContent_ReturnsNull_ForMalformedXml()
    {
        var detected = PlatformDetector.DetectFromContent("<configuration><packageSources>");

        await Assert.That(detected).IsNull();
    }

    [Test]
    public async Task DetectFromContent_MatchesHostCaseInsensitively()
    {
        var detected = PlatformDetector.DetectFromContent(ConfigWith("https://NuGet.PKG.GitHub.com/owner/index.json"));

        await Assert.That(detected).IsEqualTo(BuildPlatform.GitHubActions);
    }

    [Test]
    public async Task DetectFromContent_ReturnsAzureDevOps_ForLegacyVisualStudioFeed()
    {
        var detected = PlatformDetector.DetectFromContent(
            ConfigWith("https://pkgs.visualstudio.com/_packaging/feed/nuget/v3/index.json"));

        await Assert.That(detected).IsEqualTo(BuildPlatform.AzureDevOps);
    }

    [Test]
    public async Task FindNuGetConfig_PrefersTheOneBesideTheSolution()
    {
        using var repo = new TempRepository();
        var solutionDirectory = repo.CreateDirectory("src");
        repo.WriteFile(Path.Combine("src", "App.sln"), "solution");
        repo.WriteFile(Path.Combine("src", "nuget.config"), ConfigWith(GitHubFeed));

        var found = PlatformDetector.FindNuGetConfig(repo.Root);

        await Assert.That(found).IsNotNull();
        await Assert.That(Path.GetDirectoryName(found!)).IsEqualTo(solutionDirectory);
    }

    [Test]
    public async Task FindNuGetConfig_FallsBackToRepositoryRoot()
    {
        using var repo = new TempRepository();
        repo.WriteFile("nuget.config", ConfigWith(AzureFeed));

        var found = PlatformDetector.FindNuGetConfig(repo.Root);

        await Assert.That(found).IsNotNull();
        await Assert.That(Path.GetDirectoryName(found!)).IsEqualTo(repo.Root);
    }

    [Test]
    public async Task FindNuGetConfig_ReturnsNull_WhenAbsent()
    {
        using var repo = new TempRepository();

        var found = PlatformDetector.FindNuGetConfig(repo.Root);

        await Assert.That(found).IsNull();
    }

    [Test]
    public async Task DetectFromRepository_ReadsTheConfigBesideTheSolution()
    {
        using var repo = new TempRepository();
        repo.CreateDirectory("src");
        repo.WriteFile(Path.Combine("src", "App.sln"), "solution");
        repo.WriteFile(Path.Combine("src", "nuget.config"), ConfigWith(GitHubFeed));

        var detected = PlatformDetector.DetectFromRepository(repo.Root, out var path);

        using (Assert.Multiple())
        {
            await Assert.That(detected).IsEqualTo(BuildPlatform.GitHubActions);
            await Assert.That(path).IsNotNull();
        }
    }

    [Test]
    public async Task DetectFromRepository_ReturnsNull_WhenThereIsNoConfig()
    {
        using var repo = new TempRepository();

        var detected = PlatformDetector.DetectFromRepository(repo.Root, out var path);

        using (Assert.Multiple())
        {
            await Assert.That(detected).IsNull();
            await Assert.That(path).IsNull();
        }
    }

    [Test]
    public async Task ResolveRootItemDestination_PutsTheWorkflowUnderGitHubWorkflows()
    {
        var destination = NuGetPackageInstaller.ResolveRootItemDestination("build.yml");

        await Assert.That(destination).IsEqualTo(Path.Combine(".github", "workflows", "build.yml"));
    }

    [Test]
    public async Task ResolveRootItemDestination_LeavesRootLevelItemsAlone()
    {
        using (Assert.Multiple())
        {
            await Assert.That(NuGetPackageInstaller.ResolveRootItemDestination("GitVersion.yml")).IsEqualTo("GitVersion.yml");
            await Assert.That(NuGetPackageInstaller.ResolveRootItemDestination("azure-pipelines.yml")).IsEqualTo("azure-pipelines.yml");
        }
    }

    [Test]
    public async Task IsProtectedRootItem_CoversNuGetConfigOnly()
    {
        using (Assert.Multiple())
        {
            await Assert.That(NuGetPackageInstaller.IsProtectedRootItem("nuget.config")).IsTrue();
            await Assert.That(NuGetPackageInstaller.IsProtectedRootItem("NuGet.Config")).IsTrue();
            await Assert.That(NuGetPackageInstaller.IsProtectedRootItem("GitVersion.yml")).IsFalse();
        }
    }

    private sealed class TempRepository : IDisposable
    {
        public TempRepository()
        {
            Root = Path.Combine(Path.GetTempPath(), "fallout-detect-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Root);
        }

        public string Root { get; }

        public string CreateDirectory(string relative)
        {
            var full = Path.Combine(Root, relative);
            Directory.CreateDirectory(full);
            return full;
        }

        public void WriteFile(string relative, string content)
        {
            var full = Path.Combine(Root, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(full)!);
            File.WriteAllText(full, content);
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(Root))
                    Directory.Delete(Root, recursive: true);
            }
            catch (IOException)
            {
                // A locked temp directory is not worth failing a test over.
            }
        }
    }
}
