using Fallout.Common;

namespace Automation.Fallout.Components.Parameters;

public interface IHasGitHubPackages : IFalloutBuild
{
    [Parameter("GitHub organization or user name")]
    string GitHubOwner => TryGetValue(() => GitHubOwner) ?? throw new Exception("GitHubOwner parameter is required");

    [Parameter("GitHub Personal Access Token for package publishing")]
    string GitHubToken => TryGetValue(() => GitHubToken) ?? Environment.GetEnvironmentVariable("GITHUB_TOKEN") ?? throw new Exception("GitHubToken parameter or GITHUB_TOKEN environment variable is required");
}
