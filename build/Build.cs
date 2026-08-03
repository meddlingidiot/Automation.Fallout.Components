using System;
using System.Linq;
using Automation.Fallout.Components.Components;
using Automation.Fallout.Components.DefaultBuilds;
using Automation.Fallout.Components.Parameters;
using Fallout.Common;
using Fallout.Common.CI;
using Fallout.Common.Execution;
using Fallout.Common.IO;
using Fallout.Solutions;
using Fallout.Common.Tooling;
using Fallout.Common.Utilities.Collections;
using static Fallout.Common.EnvironmentInfo;
using static Fallout.Common.IO.PathConstruction;

 public class Build : AzurePipelinesBuild, IShowVersion, IClean, ICompile, IRestore, IScanForSecrets, IRunUnitTests, 
     IRunIntegrationTests, IGenerateCoverageReport, ITest, IUpdateChangelog, IPackage, ITagRelease, IAnnounceRelease,
     IHasCodeSigning
 {
 
     public static int Main() => Execute<Build>(
         x => ((IPackage)x).ReleasePackage);
 
     int IHasTests.MinCoverageThreshold => 35;
     bool ITestExecution.UseMicrosoftTestingPlatform => true;
 }


