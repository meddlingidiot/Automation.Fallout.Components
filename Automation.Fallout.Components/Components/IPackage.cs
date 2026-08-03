using Automation.Fallout.Components.DefaultBuilds;
using Automation.Fallout.Components.Parameters;
using Fallout.Common;
using Fallout.Common.IO;
using Fallout.Common.Tools.DotNet;
using Fallout.Common.Tools.Git;
using Fallout.Common.Utilities;
using static Fallout.Common.Tools.DotNet.DotNetTasks;


namespace Automation.Fallout.Components.Components;

public interface IPackage : IFalloutBuild, IVelopack, IHasSolution, IHasConfiguration, 
    IHasGitVersion, IHasArtifacts, IHasAzureDevOpsFeeds, IDoTag
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
                //.EnableNoBuild()); //see if this fixes it

        });
    
    Target ReleasePackage => t => t
        .DependsOn(Package)
        .When(IsServerBuild || ForceTagRelease, _ => _
            .Triggers<ITagRelease>(x => x.TagRelease))
        .Description("Deploy NuGet packages")
        .Executes(() =>
        {
            Serilog.Log.Information("Deploying NuGet packages...");

            var currentBranch = GitTasks.GitCurrentBranch();
            var isMainBranch = currentBranch.Equals("main", StringComparison.OrdinalIgnoreCase);            
            
            var feedId = isMainBranch ? ProductionFeedId : PrereleaseFeedId;
            var feedName = isMainBranch ? "Production" : "Prerelease";
        
            var packages = ArtifactsDirectoryParam.GlobFiles("**/*.nupkg");
        
            foreach (var package in packages)
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

