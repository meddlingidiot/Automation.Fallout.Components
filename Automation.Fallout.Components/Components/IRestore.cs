using Automation.Fallout.Components.Parameters;
using Fallout.Common;
using Fallout.Common.Tools.DotNet;

namespace Automation.Fallout.Components.Components;

public interface IRestore : IFalloutBuild, IHasSolution
{
    Target Restore => t => t
        .DependsOn<IClean>()
        .Description("Restore NuGet packages")
        .Executes(() =>
        {
            DotNetTasks.DotNetRestore(s => DotNetRestoreSettingsExtensions
                .SetProjectFile<DotNetRestoreSettings>(s, (string)Solution));
        });
}