using Automation.Fallout.Components.Parameters;
using Fallout.Common;

namespace Automation.Fallout.Components.DefaultBuilds;

/// <summary>
/// Base class for all Nuke builds that run on Azure Pipelines.
/// Provides Azure-specific configuration and integration points.
///
/// In Nuke 10+, the recommended approach is to keep CI/CD orchestration in YAML
/// and handle all build logic in Nuke targets. This class serves as a
/// configuration point for Azure-specific settings that can be referenced
/// from both the YAML pipeline and Nuke build logic.
/// </summary>
public abstract class AzurePipelinesBuild : FalloutBuild, IAzurePipelinesConfig
{
    // All Azure Pipelines configuration is provided by IAzurePipelinesConfig
    // Derived classes can override individual settings as needed

    /// <summary>
    /// Helper property to detect if running in Azure Pipelines
    /// </summary>
    protected static bool IsAzurePipelines =>
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TF_BUILD"));

    /// <summary>
    /// Azure Pipelines Build ID
    /// </summary>
    protected static string AzurePipelinesBuildId =>
        Environment.GetEnvironmentVariable("BUILD_BUILDID") ?? "0";

    /// <summary>
    /// Azure Pipelines Build Number
    /// </summary>
    protected static string AzurePipelinesBuildNumber =>
        Environment.GetEnvironmentVariable("BUILD_BUILDNUMBER") ?? "1.0.0";
}
