using Fallout.Common;
using Fallout.Common.IO;

namespace Automation.Fallout.Components.Parameters;

public interface IDoTag : IFalloutBuild
{
    [Parameter("Force tag for local builds - Default is 'false'")]
    bool ForceTagRelease => TryGetValue<bool?>(() => ForceTagRelease) ?? false;

}