using Automation.Fallout.Components.Parameters;
using Fallout.Common;
using Fallout.Common.Tools.DotNet;

namespace Automation.Fallout.Components.Components;

/// <summary>
/// The Azure DevOps package push, deliberately carrying no <see cref="Target"/>.
///
/// Keeping the push out of a target is what lets a single build combine both
/// destinations: <see cref="IPackageAzureDevOps"/> and <see cref="IPackageGitHub"/> each
/// declare a target named ReleasePackage, so they cannot be implemented together, but the
/// target-free push interfaces can. See <see cref="IPackageMultiPlatform"/>.
///
/// Override <see cref="PushPackagesToAzureDevOps"/> to change how packages reach the feed.
/// </summary>
public interface IPushPackagesAzureDevOps : IPackage, IHasAzureDevOpsFeeds
{
    void PushPackagesToAzureDevOps()
    {
        Serilog.Log.Information("Deploying NuGet packages to Azure DevOps Artifacts...");

        var isMainBranch = IsMainBranch;
        var feedId = isMainBranch ? ProductionFeedId : PrereleaseFeedId;
        var feedName = isMainBranch ? "Production" : "Prerelease";

        foreach (var package in PackagesToPush)
        {
            Serilog.Log.Information("Pushing {Package} to {Feed} feed", package.Name, feedName);
            DotNetTasks.DotNetNuGetPush(s => s
                .SetTargetPath(package)
                .SetSource($"https://pkgs.dev.azure.com/AFTR/_packaging/{feedId}/nuget/v3/index.json")
                .SetApiKey("az") // "az" is the convention for Azure DevOps
                .SetSkipDuplicate(true));
        }
    }
}
