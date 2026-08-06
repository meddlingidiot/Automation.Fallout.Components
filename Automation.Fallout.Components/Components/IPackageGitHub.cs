using Automation.Fallout.Components.Parameters;
using Fallout.Common;
using Fallout.Common.Tools.DotNet;
using Fallout.Common.Utilities;

namespace Automation.Fallout.Components.Components;

/// <summary>
/// Pushes the packaged output to GitHub Packages for the configured owner.
/// </summary>
public interface IPackageGitHub : IPackage, IHasGitHubPackages
{
    Target ReleasePackage => t => t
        .DependsOn<IPackage>(x => x.Package)
        .When(IsServerBuild || ForceTagRelease, _ => _
            .Triggers<ITagRelease>(x => x.TagRelease))
        .Description("Deploy NuGet packages to GitHub Packages")
        .Executes(() =>
        {
            // GitHub Packages rejects pushes without a token, and a local run rarely has one.
            if (!IsServerBuild && !ForceTagRelease)
            {
                Serilog.Log.Information(
                    "Skipping NuGet push - not a server build. Use --force-tag-release to push locally.");
                return;
            }

            Serilog.Log.Information("Deploying NuGet packages to GitHub Packages...");

            var feedName = IsMainBranch ? "Production" : "Prerelease";

            foreach (var package in PackagesToPush)
            {
                Serilog.Log.Information("Pushing {Package} to GitHub Packages ({Feed})", package.Name, feedName);
                DotNetTasks.DotNetNuGetPush(s => s
                    .SetTargetPath(package)
                    .SetSource($"https://nuget.pkg.github.com/{GitHubOwner}/index.json")
                    .SetApiKey(GitHubToken)
                    .SetSkipDuplicate(true));
            }
        });
}
