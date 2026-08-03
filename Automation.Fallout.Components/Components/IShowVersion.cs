using Automation.Fallout.Components.Parameters;
using Fallout.Common;

namespace Automation.Fallout.Components.Components;

public interface IShowVersion : IFalloutBuild, IHasGitVersion
{
    Target ShowVersion => t => t
        .DependentFor<IClean>()
        .Executes(() => { Serilog.Log.Information("GitVersion: {GitVersion}", GitVersion.FullSemVer); });
}