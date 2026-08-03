using Fallout.Common;
using Fallout.Solutions;

namespace Automation.Fallout.Components.Parameters;

public interface IHasSolution : IFalloutBuild
{
    [Solution]
    Solution Solution => TryGetValue(() => Solution);
}