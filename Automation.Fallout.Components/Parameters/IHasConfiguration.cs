using Fallout.Common;

namespace Automation.Fallout.Components.Parameters;

public interface IHasConfiguration : IFalloutBuild
{
    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    Configuration Configuration => TryGetValue(() => Configuration) ??
                                   (IsLocalBuild ? Configuration.Debug : Configuration.Release);
}
