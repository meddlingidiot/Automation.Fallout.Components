using Automation.Fallout.Components.Components;
using Automation.Fallout.Components.Parameters;
using Fallout.Common;

namespace Automation.Fallout.Components.DefaultBuilds;

public class PackageBuild : AzurePipelinesBuild, IShowVersion,
    IClean, ICompile, IRestore, IScanForSecrets, IRunUnitTests, IRunIntegrationTests, 
    IGenerateCoverageReport, ITest, IUpdateChangelog,
    IPackage, ITagRelease, IAnnounceRelease
{
    public static int Main() => Execute<PackageBuild>(
        x => ((IPackage)x).ReleasePackage);

    int IHasTests.MinCoverageThreshold => 20;
    //bool IHasTests.BreakBuildOnWarnings => false;

}
