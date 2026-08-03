using Fallout.Common;
using Fallout.Common.Tools.GitVersion;

namespace Automation.Fallout.Components.Parameters;

public interface IHasGitVersion : IFalloutBuild
{
    [GitVersion(Framework = "net8.0")] 
    GitVersion GitVersion => TryGetValue(() => GitVersion);
}
