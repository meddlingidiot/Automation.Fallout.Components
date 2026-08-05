using Automation.Fallout.Components.Parameters;
using Fallout.Common;
using Fallout.Common.Tools.DotNet;
using Fallout.Common.Utilities;

namespace Automation.Fallout.Components.Components;

/// <summary>
/// Pushes the packaged output to Azure DevOps Artifacts feeds, choosing the production or
/// prerelease feed based on the current branch.
/// </summary>
public interface IPackageAzureDevOps : IPackage, IHasAzureDevOpsFeeds
{
    Target ReleasePackage => t => t
        .DependsOn<IPackage>(x => x.Package)
        .When(IsServerBuild || ForceTagRelease, _ => _
            .Triggers<ITagRelease>(x => x.TagRelease))
        .Description("Deploy NuGet packages to Azure DevOps Artifacts")
        .Executes(() =>
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
        });
}
