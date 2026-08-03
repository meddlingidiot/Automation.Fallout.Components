using Fallout.Common;
using Fallout.Common.IO;

namespace Automation.Fallout.Components.Parameters;

public interface IHasArtifacts : IFalloutBuild
{
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    [Parameter] AbsolutePath ArtifactsDirectoryParam => TryGetValue(() => ArtifactsDirectoryParam) ?? ArtifactsDirectory;
    AbsolutePath CoverageReportDirectory => ArtifactsDirectoryParam / "coverage-report";
    AbsolutePath TestResultDirectory => ArtifactsDirectoryParam / "test-results";
    AbsolutePath PackagePublishDirectory => ArtifactsDirectoryParam / "packages";
    AbsolutePath VelopackPublishDirectory => RootDirectory / ".tmp" / "velopack";
}
