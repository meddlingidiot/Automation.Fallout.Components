using Automation.Fallout.Components.Parameters;
using Fallout.Common;
using Fallout.Common.Tools.DotNet;
using Fallout.Common.Utilities;

namespace Automation.Fallout.Components.Components;

/// <summary>
/// Pushes the packaged output to GitHub Packages for the configured owner.
/// </summary>
public interface IPackageGitHub : IPushPackagesGitHub
{
    Target ReleasePackage => t => t
        .DependsOn<IPackage>(x => x.Package)
        .When(IsServerBuild || ForceTagRelease, _ => _
            .Triggers<ITagRelease>(x => x.TagRelease))
        .Description("Deploy NuGet packages to GitHub Packages")
        .Executes(() => PushPackagesToGitHub());
}
