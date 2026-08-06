using Automation.Fallout.Components.Parameters;
using Fallout.Common;
using Fallout.Common.Tools.DotNet;

namespace Automation.Fallout.Components.Components;

/// <summary>
/// The GitHub Packages push, deliberately carrying no <see cref="Target"/>.
/// See <see cref="IPushPackagesAzureDevOps"/> for why the push is separated from the target.
///
/// Override <see cref="PushPackagesToGitHub"/> to change how packages reach GitHub Packages.
/// </summary>
public interface IPushPackagesGitHub : IPackage, IHasGitHubPackages
{
    void PushPackagesToGitHub()
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
    }
}
