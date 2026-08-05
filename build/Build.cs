using Fallout.Common;
using Fallout.Solutions;
using Automation.Fallout.Components;
using Automation.Fallout.Components.Components;
using Automation.Fallout.Components.DefaultBuilds;
using Automation.Fallout.Components.Parameters;

/// <summary>
/// Build configuration for PackageBuild
/// </summary>

public class Build : GitHubActionsBuild, IShowVersion, IClean, ICompile, IRestore, IScanForSecrets, IRunUnitTests, IRunIntegrationTests, IGenerateCoverageReport, ITest, IUpdateChangelog, IPackageGitHub, ITagRelease, IAnnounceRelease, ICreateGitHubRelease
{

    public static int Main() => Execute<Build>(
        x => ((IPackageGitHub)x).ReleasePackage);

    int IHasTests.MinCoverageThreshold => 35;
    bool ITestExecution.UseMicrosoftTestingPlatform => true;
}
