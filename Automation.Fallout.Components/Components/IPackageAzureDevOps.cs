using Automation.Fallout.Components.Parameters;
using Fallout.Common;
using Fallout.Common.Tools.DotNet;
using Fallout.Common.Utilities;

namespace Automation.Fallout.Components.Components;

/// <summary>
/// Pushes the packaged output to Azure DevOps Artifacts feeds, choosing the production or
/// prerelease feed based on the current branch.
/// </summary>
public interface IPackageAzureDevOps : IPushPackagesAzureDevOps
{
    Target ReleasePackage => t => t
        .DependsOn<IPackage>(x => x.Package)
        .When(IsServerBuild || ForceTagRelease, _ => _
            .Triggers<ITagRelease>(x => x.TagRelease))
        .Description("Deploy NuGet packages to Azure DevOps Artifacts")
        .Executes(() => PushPackagesToAzureDevOps());
}
