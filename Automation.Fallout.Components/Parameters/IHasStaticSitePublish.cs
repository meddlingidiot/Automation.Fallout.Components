using Fallout.Common;

namespace Automation.Fallout.Components.Parameters;

public interface IHasStaticSitePublish : IFalloutBuild
{
    [Parameter("Relative path to the Blazor WASM .csproj file from the repository root")]
    string BlazorProjectPath => TryGetValue(() => BlazorProjectPath) ??
                                throw new Exception("BlazorProjectPath parameter is required");

    [Parameter("HTTPS URL of the GitHub repository to deploy the static site to")]
    string StaticSiteRepoUrl => TryGetValue(() => StaticSiteRepoUrl) ??
                                throw new Exception("StaticSiteRepoUrl parameter is required");

    [Parameter("GitHub Personal Access Token with write access to the static site repository"), Secret]
    string StaticSiteGitHubToken => TryGetValue(() => StaticSiteGitHubToken) ??
                                    Environment.GetEnvironmentVariable("STATIC_SITE_GITHUB_TOKEN") ??
                                    throw new Exception("StaticSiteGitHubToken parameter or STATIC_SITE_GITHUB_TOKEN environment variable is required");
}
