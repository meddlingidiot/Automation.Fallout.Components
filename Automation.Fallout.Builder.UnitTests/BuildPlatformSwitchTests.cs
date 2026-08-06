using Automation.Fallout.Builder.Models;
using Automation.Fallout.Builder.Services;

namespace Automation.Fallout.Builder.UnitTests;

/// <summary>
/// The setup wizard asks which CI platform a repository builds on. Everything downstream of that
/// answer - base class, packaging interface, entry-point target - has to follow it.
/// </summary>
public class BuildPlatformSwitchTests
{
    private static DefaultBuildInfo PackageBuildInfo() => new()
    {
        Name = "PackageBuild",
        Description = "Full pipeline with NuGet package creation",
        RequiresTests = true,
        RequiresPackaging = true,
        RequiresVelopack = false
    };

    private static string Generate(BuildPlatform platform) =>
        BuildFileGenerator.GenerateBuildFile(
            new BuildConfiguration { BuildType = "PackageBuild", Platform = platform },
            PackageBuildInfo());

    [Test]
    public async Task AzureDevOps_IsTheDefaultPlatform()
    {
        await Assert.That(new BuildConfiguration().Platform).IsEqualTo(BuildPlatform.AzureDevOps);
    }

    [Test]
    public async Task GitHubActions_UsesGitHubBaseClassAndPackaging()
    {
        var result = Generate(BuildPlatform.GitHubActions);

        using (Assert.Multiple())
        {
            await Assert.That(result).Contains("public class Build : GitHubActionsBuild,");
            await Assert.That(result).Contains("IPackageGitHub");
            await Assert.That(result).Contains("x => ((IPackageGitHub)x).ReleasePackage");
            await Assert.That(result).DoesNotContain("AzurePipelinesBuild");
            await Assert.That(result).DoesNotContain("IPackageAzureDevOps");
        }
    }

    [Test]
    public async Task AzureDevOps_UsesAzureBaseClassAndPackaging()
    {
        var result = Generate(BuildPlatform.AzureDevOps);

        using (Assert.Multiple())
        {
            await Assert.That(result).Contains("public class Build : AzurePipelinesBuild,");
            await Assert.That(result).Contains("IPackageAzureDevOps");
            await Assert.That(result).Contains("x => ((IPackageAzureDevOps)x).ReleasePackage");
            await Assert.That(result).DoesNotContain("GitHubActionsBuild");
            await Assert.That(result).DoesNotContain("IPackageGitHub");
        }
    }

    /// <summary>
    /// The bare "IPackage" name only marks where packaging goes; it must never survive into a
    /// generated build, because it carries no ReleasePackage target.
    /// </summary>
    [Test]
    [Arguments(BuildPlatform.GitHubActions)]
    [Arguments(BuildPlatform.AzureDevOps)]
    public async Task PlaceholderPackageInterface_IsAlwaysReplaced(BuildPlatform platform)
    {
        var result = Generate(platform);

        await Assert.That(result).DoesNotContain("((IPackage)x)");
        await Assert.That(result).DoesNotContain(" IPackage,");
    }

    [Test]
    public async Task GitHubActions_AddsGitHubReleaseWhenTagging()
    {
        var result = Generate(BuildPlatform.GitHubActions);

        await Assert.That(result).Contains("ICreateGitHubRelease");
    }

    [Test]
    public async Task AzureDevOps_DoesNotAddGitHubRelease()
    {
        var result = Generate(BuildPlatform.AzureDevOps);

        await Assert.That(result).DoesNotContain("ICreateGitHubRelease");
    }

    [Test]
    public async Task CompileBuild_HasNoPackagingInterface_OnEitherPlatform()
    {
        var buildInfo = new DefaultBuildInfo
        {
            Name = "CompileBuild",
            Description = "Basic compilation",
            RequiresTests = false
        };

        foreach (var platform in new[] { BuildPlatform.GitHubActions, BuildPlatform.AzureDevOps })
        {
            var result = BuildFileGenerator.GenerateBuildFile(
                new BuildConfiguration { BuildType = "CompileBuild", Platform = platform }, buildInfo);

            await Assert.That(result).DoesNotContain("IPackageGitHub");
            await Assert.That(result).DoesNotContain("IPackageAzureDevOps");
        }
    }
}
