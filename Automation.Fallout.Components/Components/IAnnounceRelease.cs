using Automation.Fallout.Components.Parameters;
using Fallout.Common;

namespace Automation.Fallout.Components.Components;

/// <summary>
/// Announces a release via email, Teams, Slack, etc.
/// Override the Announce target to customize announcement behavior.
/// </summary>
public interface IAnnounceRelease : IFalloutBuild, IHasGitVersion
{
    Target Announce => t => t
        .OnlyWhenDynamic(() => GitVersion.BranchName.Equals("main", StringComparison.OrdinalIgnoreCase))
        .Description("Announce Release via Email/Teams/etc.")
        .Executes(() => PerformAnnouncement());

    /// <summary>
    /// Override this to customize how releases are announced.
    /// </summary>
    void PerformAnnouncement()
    {
        Serilog.Log.Information("Announcing release...");
        // Default implementation - override to add Teams/Slack/Email notifications
    }
}
