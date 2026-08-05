namespace Automation.Fallout.Builder.Models;

/// <summary>
/// The CI platform a generated build targets. It decides which packaging interface the build
/// implements, which base class it derives from, and which root items get copied into the repository.
/// </summary>
public enum BuildPlatform
{
    GitHubActions,
    AzureDevOps
}
