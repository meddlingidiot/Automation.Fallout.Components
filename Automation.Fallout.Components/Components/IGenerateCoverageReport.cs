using System.Runtime.InteropServices;
using System.Text.Json;
using Automation.Fallout.Components.Parameters;
using Fallout.Common;
using Fallout.Common.IO;
using Fallout.Common.Tooling;
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

            if (UploadToCodecov)
            {
                EnsureCodecovUploaderInstalled().Wait();
                UploadCoverageToCodecov(CoverageReportDirectory / "Cobertura.xml");
            }
        });

    AbsolutePath CodecovUploaderPath
    {
        get
        {
            var exe = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "codecov.exe" : "codecov";
            return TemporaryDirectory / "codecov" / exe;
        }
    }

    private async Task EnsureCodecovUploaderInstalled()
    {
        if (CodecovUploaderPath.FileExists()) return;

        var os = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "windows"
               : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "linux"
               : "macos";
        var exe = os == "windows" ? "codecov.exe" : "codecov";
        var downloadUrl = $"https://uploader.codecov.io/latest/{os}/{exe}";

        Serilog.Log.Information("Downloading Codecov uploader from {Url}...", downloadUrl);

        var installDir = TemporaryDirectory / "codecov";
        installDir.CreateDirectory();

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "fallout-build");
        var bytes = await client.GetByteArrayAsync(downloadUrl);
        await File.WriteAllBytesAsync(CodecovUploaderPath, bytes);

        if (os != "windows")
            ProcessTasks.StartProcess("chmod", $"+x \"{CodecovUploaderPath}\"").AssertZeroExitCode();

        Serilog.Log.Information("Codecov uploader installed to {Path}", CodecovUploaderPath);
    }

    private void UploadCoverageToCodecov(AbsolutePath coberturaFile)
    {
        // -Z: exit non-zero on upload errors (uploader defaults to exit 0 even on failure)
        var args = $"--file \"{coberturaFile}\" --rootDir \"{RootDirectory}\" -Z";

        if (!string.IsNullOrEmpty(CodecovToken))
            args += $" --token \"{CodecovToken}\"";

        Serilog.Log.Information("Uploading coverage to Codecov...");
        ProcessTasks.StartProcess(CodecovUploaderPath, args, logOutput: true).AssertZeroExitCode();
    }
}
