using System.Text.Json;
using Automation.Fallout.Components.Parameters;
using Fallout.Common;
using Fallout.Common.IO;
using Fallout.Common.Tools.ReportGenerator;
using static Fallout.Common.Tools.ReportGenerator.ReportGeneratorTasks;

namespace Automation.Fallout.Components.Components;

/// <summary>
/// Generates code coverage reports from test results.
/// Override the CoverageReport target to customize report generation.
/// </summary>
public interface IGenerateCoverageReport : IFalloutBuild, IHasTests, IHasArtifacts
{
    Target CoverageReport => t => t
        .DependsOn<IRunUnitTests>(x => x.UnitTests)
        .DependsOn<IRunIntegrationTests>(x => x.IntegrationTests)
        .Description("Generate coverage report")
        .Executes(() =>
        {
            // Ensure we have coverage files before running the tool
            if (TestResultDirectory.GlobFiles("**/coverage.cobertura.xml").Count == 0)
            {
                Serilog.Log.Warning("No coverage files found in {TestResultDirectory}. Skipping ReportGenerator.", TestResultDirectory);
                return;
            }

            // Combine all coverage.cobertura.xml files into one report
            ReportGenerator(s => s
                .SetReports(TestResultDirectory / "**" / "coverage.cobertura.xml")
                .SetTargetDirectory(CoverageReportDirectory)
                .SetReportTypes(ReportTypes.HtmlInline, ReportTypes.Cobertura, ReportTypes.JsonSummary));


            // Read the JSON summary to get coverage percentage
            var summaryFile = CoverageReportDirectory / "Summary.json";
            var json = JsonDocument.Parse(File.ReadAllText(summaryFile));
            var lineCoverage = json.RootElement
                .GetProperty("summary")
                .GetProperty("linecoverage")
                .GetDouble();

            Serilog.Log.Information("Line Coverage: {Coverage:F2}%", lineCoverage);

            if (lineCoverage < MinCoverageThreshold)
            {
                throw new Exception($"Coverage {lineCoverage:F2}% is below threshold {MinCoverageThreshold}%");
            }
        });
}
