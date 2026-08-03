using Automation.Fallout.Components.Parameters;
using Fallout.Common;
using Fallout.Common.Tools.DotNet;
using static Fallout.Common.Tools.DotNet.DotNetTasks;

namespace Automation.Fallout.Components.Components;

public interface ICompile : IFalloutBuild, IHasSolution, IHasConfiguration, IHasGitVersion, IHasTests
{
    Target Compile => t => t
        .DependsOn<IRestore>()
        .Description("Compile Solution")
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore()
                .SetTreatWarningsAsErrors(BreakBuildOnWarnings)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion));
        });
}
    