using Fallout.Common;

namespace Automation.Fallout.Components.Parameters;

/// <summary>
/// Provides GitHub Actions-specific configuration parameters.
/// </summary>
public interface IGitHubActionsConfig : IFalloutBuild
{
    [Parameter(".NET SDK version to install")]
    string DotNetSdkVersion => TryGetValue(() => DotNetSdkVersion) ?? "10.0.x";
}
