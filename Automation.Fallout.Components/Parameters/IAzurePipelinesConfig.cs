using Fallout.Common;

namespace Automation.Fallout.Components.Parameters;

/// <summary>
/// Provides Azure Pipelines-specific configuration parameters.
/// These values are derived from the azure-pipelines.yml configuration.
/// </summary>
public interface IAzurePipelinesConfig : IFalloutBuild
{
    [Parameter("Azure Pipelines pool name")]
    string AzurePipelinesPool => TryGetValue(() => AzurePipelinesPool) ?? "Self-hosted";

    [Parameter("Azure Pipelines image - only used if not using self-hosted pool")]
    string AzurePipelinesImage => TryGetValue(() => AzurePipelinesImage) ?? "windows-latest";

    [Parameter(".NET SDK version to install")]
    string DotNetSdkVersion => TryGetValue(() => DotNetSdkVersion) ?? "8.0.x";
}
