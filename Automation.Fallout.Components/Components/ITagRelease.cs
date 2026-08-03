using Automation.Fallout.Components.Parameters;
using Fallout.Common;

namespace Automation.Fallout.Components.Components;

/// <summary>
/// Tags a release in Git. Inherits from <see cref="IGitTagging"/> for shared Git operations.
/// Override <see cref="IGitTagging.PerformGitTagging"/> to customize tagging behavior.
/// </summary>
public interface ITagRelease : IGitTagging, IDoTag
{
    Target TagRelease => t => t
        .DependsOn<ITest>(x => x.Test)
        .Description("Tag Release in Git")
        .OnlyWhenDynamic(() => IsServerBuild || ForceTagRelease)
        .OnlyWhenDynamic(() => GitVersion.BranchName.Equals("main", StringComparison.OrdinalIgnoreCase))
        .Executes(() => PerformGitTagging());
}
