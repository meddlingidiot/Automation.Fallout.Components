using System.Runtime.InteropServices;
using Automation.Fallout.Components.Parameters;
using Fallout.Common;
using Fallout.Common.IO;
using Fallout.Solutions;
using Fallout.Common.Tooling;
using Fallout.Common.Tools.DotNet;
using static Fallout.Common.Tools.DotNet.DotNetTasks;

namespace Automation.Fallout.Components.Components;

/// <summary>
/// Base interface for test execution with shared logic.
/// Inherit from this to create custom test execution strategies.
/// </summary>
public interface ITestExecution : IFalloutBuild, IHasSolution, IHasConfiguration, IHasTests, IHasArtifacts
{
    /// <summary>
    /// Set to true when using TUnit or any other Microsoft.Testing.Platform (MTP) based test framework.
    /// On .NET 10+ SDK, dotnet test no longer supports the VSTest protocol for MTP apps — the test
    /// executable is run directly instead to bypass this restriction.
    /// </summary>
    /// <example>
    /// In your build class:
    /// <code>bool ITestExecution.UseMicrosoftTestingPlatform => true;</code>
    /// </example>
    bool UseMicrosoftTestingPlatform => false;

    /// <summary>
    /// Shared test execution logic. Override this to customize how tests are discovered and run.
    /// </summary>
    /// <param name="projectNameFilter">Filter for test project names (e.g., "UnitTests", "IntegrationTests")</param>
    void RunTests(string projectNameFilter)
    {
        if (UseMicrosoftTestingPlatform)
            RunTestsMtp(projectNameFilter);
        else
            RunTestsVsTest(projectNameFilter);
    }

    private void RunTestsVsTest(string projectNameFilter)
    {
        DotNetTest(s => s
            .SetConfiguration(Configuration)
            .EnableNoBuild()
            .SetResultsDirectory(TestResultDirectory)
            .SetDataCollector("XPlat Code Coverage")
            .CombineWith(
                Solution.AllProjects.Where(p => p.Name.Contains(projectNameFilter)),
                (settings, project) => settings
                    .SetProjectFile(project)
                    .SetResultsDirectory(TestResultDirectory)
                    .SetLoggers($"trx;LogFileName={project.Name}.trx")
            )
        );
    }

    /// <summary>
    /// Runs MTP-based tests (e.g. TUnit) by executing the test binary directly, bypassing
    /// dotnet test / VSTest entirely. This is required on .NET 10 SDK where MTP blocks
    /// the VSTest protocol unconditionally.
    /// </summary>
    private void RunTestsMtp(string projectNameFilter)
    {
        var ext = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : "";

        foreach (var project in Solution.AllProjects.Where(p => p.Name.Contains(projectNameFilter)))
        {
            var tfms = project.GetTargetFrameworks() ?? ["net10.0"];

            foreach (var tfm in tfms)
            {
                var testExe = project.Directory / "bin" / Configuration / tfm / (project.Name + ext);
                var coverageOutput = TestResultDirectory / project.Name / tfm / "coverage.cobertura.xml";

                (TestResultDirectory / project.Name / tfm).CreateDirectory();

                Serilog.Log.Information("Running {Project} [{Tfm}]", project.Name, tfm);

                ProcessTasks.StartProcess(
                    testExe,
                    $"--coverage " +
                    $"--coverage-output-format {(IsServerBuild ? "cobertura" : "xml")} " +
                    $"--coverage-output \"{coverageOutput}\" " +
                    $"--report-trx " +
                    $"--report-trx-filename \"{project.Name}-{tfm}.trx\" " +
                    $"--results-directory \"{TestResultDirectory}\"",
                    workingDirectory: project.Directory,
                    logOutput: true
                ).AssertZeroExitCode();
            }
        }
    }
}
