using Automation.Fallout.Components.Parameters;
using Fallout.Common;
using Fallout.Common.IO;
using Fallout.Common.Tools.DotNet;
using Fallout.Common.Tools.Git;
using static Fallout.Common.Tools.DotNet.DotNetTasks;

namespace Automation.Fallout.Components.Components;

/// <summary>
/// Produces the NuGet packages. Pushing them is deliberately not part of this interface:
/// the destination differs per platform, so implement <see cref="IPackageGitHub"/> or
/// <see cref="IPackageAzureDevOps"/> to get a ReleasePackage target alongside this one.
/// </summary>
public interface IPackage : IFalloutBuild, IVelopack, IHasSolution, IHasConfiguration,
    IHasGitVersion, IHasArtifacts, IDoTag
{
    Target Package => t => t
        .DependsOn<ITest>(x => x.Test)
        .DependsOn<IUpdateChangelog>()
        .Description("Package NuGet packages")
        .Executes(() =>
        {
            DotNetPack(s => s
                .SetProject(Solution)
                .SetConfiguration(Configuration)
                .SetOutputDirectory(PackagePublishDirectory)
                // Safeguard: fall back to a local version if GitVersion isn't available
                .SetVersion(GitVersion.FullSemVer ?? "0.0.0-local"));
        });

    /// <summary>
    /// Packages produced by <see cref="Package"/>, in the order they should be pushed.
    /// </summary>
    IReadOnlyCollection<AbsolutePath> PackagesToPush => ArtifactsDirectoryParam.GlobFiles("**/*.nupkg");

    /// <summary>
    /// Whether the current branch is the release branch. Prerelease feeds are used otherwise.
    /// </summary>
    bool IsMainBranch =>
        GitTasks.GitCurrentBranch().Equals("main", StringComparison.OrdinalIgnoreCase);
}
