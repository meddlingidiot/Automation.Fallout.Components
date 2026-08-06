using Automation.Fallout.Components.Parameters;
using Fallout.Common;

namespace Automation.Fallout.Components.DefaultBuilds;

/// <summary>
/// Base class for all Fallout builds that run on GitHub Actions.
/// Provides GitHub-specific configuration and integration points.
/// </summary>
public abstract class GitHubActionsBuild : FalloutBuild, IGitHubActionsConfig
{
    /// <summary>
    /// Helper property to detect if running in GitHub Actions
    /// </summary>
    protected static bool IsGitHubActions =>
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS"));

    /// <summary>
    /// GitHub Run ID
    /// </summary>
    protected static string GitHubRunId =>
        Environment.GetEnvironmentVariable("GITHUB_RUN_ID") ?? "0";

    /// <summary>
    /// GitHub Run Number
    /// </summary>
    protected static string GitHubRunNumber =>
        Environment.GetEnvironmentVariable("GITHUB_RUN_NUMBER") ?? "1";
}
