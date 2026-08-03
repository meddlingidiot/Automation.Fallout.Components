using Fallout.Common;

namespace Automation.Fallout.Components.Parameters;

public interface IHasAzureDevOpsFeeds : IFalloutBuild
{
    [Parameter("Azure DevOps feed ID for production packages")]
    string ProductionFeedId => TryGetValue(() => ProductionFeedId) ?? "c3b2a984-bc78-47a5-8bb1-f94c848b75fc";
    
    [Parameter("Azure DevOps feed ID for prerelease packages")]
    string PrereleaseFeedId => TryGetValue(() => PrereleaseFeedId) ?? "88fbe37e-e5e8-46d9-9b05-e4b10490975e";
}