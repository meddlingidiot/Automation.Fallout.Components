using Automation.Fallout.Builder.Models;
using Automation.Fallout.Builder.Services;

namespace Automation.Fallout.Builder.UnitTests;

public class PackageInterfaceMigratorTests
{
    /// <summary>A build file as it looks straight after a Nuke -> Fallout namespace rename.</summary>
    private const string MigratedBuildFile = """
                                             public class Build : GitHubActionsBuild, IShowVersion, ITest, IPackage, ITagRelease
                                             {
                                                 public static int Main() => Execute<Build>(
                                                     x => ((IPackage)x).ReleasePackage);

                                                 string IHasGitHubPackages.GitHubOwner => "meddlingidiot";
                                             }
                                             """;

    [Test]
    public async Task Rewrite_GitHubActions_RetargetsPackageAtIPackageGitHub()
    {
        var result = PackageInterfaceMigrator.Rewrite(MigratedBuildFile, BuildPlatform.GitHubActions, out var notes);

        using (Assert.Multiple())
        {
            await Assert.That(result).Contains("IPackageGitHub, ITagRelease");
            await Assert.That(result).Contains("((IPackageGitHub)x).ReleasePackage");
            await Assert.That(notes).IsNotEmpty();
        }
    }

    [Test]
    public async Task Rewrite_AzureDevOps_RetargetsPackageAtIPackageAzureDevOps()
    {
        var source = MigratedBuildFile.Replace("GitHubActionsBuild", "AzurePipelinesBuild");

        var result = PackageInterfaceMigrator.Rewrite(source, BuildPlatform.AzureDevOps, out _);

        using (Assert.Multiple())
        {
            await Assert.That(result).Contains("IPackageAzureDevOps, ITagRelease");
            await Assert.That(result).Contains("((IPackageAzureDevOps)x).ReleasePackage");
        }
    }

    [Test]
    public async Task Rewrite_AzureDevOps_KeepsAnExplicitGitHubOwnerCompiling()
    {
        // IPackageAzureDevOps does not inherit IHasGitHubPackages, so the explicit member would
        // otherwise fail with CS0540 - the same error the bare IPackage produces.
        var source = MigratedBuildFile.Replace("GitHubActionsBuild", "AzurePipelinesBuild");

        var result = PackageInterfaceMigrator.Rewrite(source, BuildPlatform.AzureDevOps, out var notes);

        using (Assert.Multiple())
        {
            await Assert.That(result).Contains("AzurePipelinesBuild, IHasGitHubPackages, IShowVersion");
            await Assert.That(notes).Count().IsEqualTo(2);
        }
    }

    [Test]
    public async Task Rewrite_GitHubActions_DoesNotAddIHasGitHubPackages()
    {
        // IPackageGitHub already inherits it.
        var result = PackageInterfaceMigrator.Rewrite(MigratedBuildFile, BuildPlatform.GitHubActions, out var notes);

        using (Assert.Multiple())
        {
            await Assert.That(result).DoesNotContain(", IHasGitHubPackages,");
            await Assert.That(notes).Count().IsEqualTo(1);
        }
    }

    [Test]
    public async Task Rewrite_IsIdempotent()
    {
        var once = PackageInterfaceMigrator.Rewrite(MigratedBuildFile, BuildPlatform.GitHubActions, out _);

        var twice = PackageInterfaceMigrator.Rewrite(once, BuildPlatform.GitHubActions, out var notes);

        using (Assert.Multiple())
        {
            await Assert.That(twice).IsEqualTo(once);
            await Assert.That(notes).IsEmpty();
            await Assert.That(twice).DoesNotContain("IPackageGitHubGitHub");
        }
    }

    [Test]
    public async Task Rewrite_LeavesAlreadyPlatformSpecificInterfacesAlone()
    {
        var source = "public class Build : AzurePipelinesBuild, IPackageAzureDevOps { }";

        var result = PackageInterfaceMigrator.Rewrite(source, BuildPlatform.AzureDevOps, out var notes);

        using (Assert.Multiple())
        {
            await Assert.That(result).IsEqualTo(source);
            await Assert.That(notes).IsEmpty();
        }
    }

    [Test]
    public async Task Rewrite_VelopackOnlyBuild_IsUntouched()
    {
        var source = """
                     public class Build : GitHubActionsBuild, ITest, IVelopack, ITagRelease
                     {
                         public static int Main() => Execute<Build>(
                             y => ((IVelopack)y).ReleaseVelopack);
                     }
                     """;

        var result = PackageInterfaceMigrator.Rewrite(source, BuildPlatform.GitHubActions, out var notes);

        using (Assert.Multiple())
        {
            await Assert.That(result).IsEqualTo(source);
            await Assert.That(notes).IsEmpty();
        }
    }

    [Test]
    public async Task Rewrite_DependsOnGenericArgument_IsRetargeted()
    {
        var source = "Target Custom => t => t.DependsOn<IPackage>(x => x.Package);";

        var result = PackageInterfaceMigrator.Rewrite(source, BuildPlatform.GitHubActions, out _);

        await Assert.That(result).Contains("DependsOn<IPackageGitHub>");
    }
}
