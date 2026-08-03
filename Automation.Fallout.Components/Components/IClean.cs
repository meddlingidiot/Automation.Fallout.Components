using Automation.Fallout.Components.Parameters;
using Fallout.Common;
using Fallout.Common.IO;
using Fallout.Common.Tools.DotNet;

namespace Automation.Fallout.Components.Components;

public interface IClean : IFalloutBuild, IHasSolution, IHasConfiguration, IHasArtifacts
{
    Target Clean => t => t
        .Description("Clean build artifacts")
        .Executes(() =>
        {
            // Clean solution output directories (bin/obj)
            DotNetTasks.DotNetClean(s => DotNetCleanSettingsExtensions
                .SetProject<DotNetCleanSettings>(s, (string)Solution)
                .SetConfiguration(Configuration));

            // Delete artifacts directory if it exists
            ArtifactsDirectory.CreateOrCleanDirectory();

            // Optionally clean specific directories
            TestResultDirectory.CreateOrCleanDirectory();
            PackagePublishDirectory.CreateOrCleanDirectory();
            CoverageReportDirectory.CreateOrCleanDirectory();
            VelopackPublishDirectory.CreateOrCleanDirectory();
        });
}