using Fallout.Common;

namespace Automation.Fallout.Components.Parameters;

public interface IHasTests : IFalloutBuild
{
    [Parameter("Break build on secret leaks - Default is 'true'")]
    bool BreakBuildOnSecretLeaks => TryGetValue<bool?>(() => BreakBuildOnSecretLeaks) ?? true;
    
    [Parameter("Break build on warnings - Default is 'true'")]
    bool BreakBuildOnWarnings => TryGetValue<bool?>(() => BreakBuildOnWarnings) ?? true;

    [Parameter] int MinCoverageThreshold => TryGetValue<int?>(() => MinCoverageThreshold) ?? 0;
}
