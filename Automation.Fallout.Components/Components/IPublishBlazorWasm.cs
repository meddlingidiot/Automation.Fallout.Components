using Automation.Fallout.Components.Parameters;
using Fallout.Common;
using Fallout.Common.IO;
using Fallout.Common.Tooling;
using Fallout.Common.Tools.DotNet;
using static Fallout.Common.Tools.DotNet.DotNetTasks;

namespace Automation.Fallout.Components.Components;

/// <summary>
/// Publishes a Blazor WASM project and deploys the wwwroot output to a separate GitHub repository.
/// Requires <see cref="IHasStaticSitePublish"/> parameters to be configured on the build.
/// </summary>
public interface IPublishBlazorWasm : IFalloutBuild, IHasConfiguration, IHasGitVersion, IHasStaticSitePublish
{
    Target PublishBlazorWasm => t => t
        .Description("Publish Blazor WASM and deploy wwwroot contents to a static site GitHub repository")
        .Executes(() =>
        {
            var publishDir = RootDirectory / ".tmp" / "blazor-publish";
            publishDir.CreateOrCleanDirectory();

            var projectFile = RootDirectory / BlazorProjectPath;
            Serilog.Log.Information("Publishing {Project}", BlazorProjectPath);
            
            ProcessTasks.StartProcess("dotnet", "workload install wasm-tools")
                .AssertZeroExitCode();;
            
            DotNetPublish(s => s
                .SetProject(projectFile)
                .SetConfiguration(Configuration)
                .SetOutput(publishDir));

            var wwwrootDir = publishDir / "wwwroot";
            Assert.DirectoryExists(wwwrootDir, $"wwwroot not found in publish output at: {wwwrootDir}");

            var fileCount = Directory.GetFiles(wwwrootDir, "*", SearchOption.AllDirectories).Length;
            Serilog.Log.Information("Found wwwroot with {Count} files", fileCount);

            // Clone the static site repo into a temp dir
            var cloneDir = RootDirectory / ".tmp" / "static-site-clone";
            cloneDir.CreateOrCleanDirectory();

            var repoUri = new Uri(StaticSiteRepoUrl);
            var authenticatedUrl = $"{repoUri.Scheme}://{StaticSiteGitHubToken}@{repoUri.Authority}{repoUri.PathAndQuery}";

            Serilog.Log.Information("Cloning static site repository...");
            ProcessTasks.StartProcess("git", $"clone --depth 1 {authenticatedUrl} .", workingDirectory: cloneDir)
                .AssertZeroExitCode();

            // Remove all existing content except .git and .github
            foreach (var file in Directory.EnumerateFiles(cloneDir, "*", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(cloneDir, file);
                if (!relative.StartsWith(".git"))
                    File.Delete(file);
            }
            foreach (var dir in Directory.GetDirectories(cloneDir)
                         .Where(d => Path.GetFileName(d) != ".git" && Path.GetFileName(d) != ".github"))
                Directory.Delete(dir, recursive: true);

            // Copy wwwroot contents into repo root
            Serilog.Log.Information("Copying wwwroot contents to repo...");
            foreach (var srcFile in Directory.EnumerateFiles(wwwrootDir, "*", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(wwwrootDir, srcFile);
                var destFile = cloneDir / relative;
                destFile.Parent.CreateDirectory();
                File.Copy(srcFile, destFile, overwrite: true);
            }

            // Configure git identity for the commit
            ProcessTasks.StartProcess("git", "config --local user.name \"CI Build\"", workingDirectory: cloneDir)
                .AssertZeroExitCode();
            ProcessTasks.StartProcess("git", "config --local user.email \"ci@build.local\"", workingDirectory: cloneDir)
                .AssertZeroExitCode();

            // Stage all changes
            ProcessTasks.StartProcess("git", "add --all", workingDirectory: cloneDir)
                .AssertZeroExitCode();

            // Skip commit if nothing changed
            var statusProcess = ProcessTasks.StartProcess("git", "status --porcelain", workingDirectory: cloneDir);
            statusProcess.AssertWaitForExit();
            if (!statusProcess.Output.Any(x => !string.IsNullOrWhiteSpace(x.Text)))
            {
                Serilog.Log.Information("No changes detected, skipping commit");
                return;
            }

            var version = GitVersion?.FullSemVer ?? "local";
            ProcessTasks.StartProcess("git", $"commit -m \"Deploy {version}\"", workingDirectory: cloneDir)
                .AssertZeroExitCode();

            Serilog.Log.Information("Pushing to main branch...");
            ProcessTasks.StartProcess("git", "push origin main", workingDirectory: cloneDir)
                .AssertZeroExitCode();

            Serilog.Log.Information("Successfully deployed to static site repository");
        });
}
