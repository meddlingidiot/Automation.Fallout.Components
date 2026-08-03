using System.Linq;
using Automation.Fallout.Components.DefaultBuilds;
using Automation.Fallout.Components.Parameters;
using Fallout.Common;
using Fallout.Common.IO;
using Fallout.Solutions;
using Fallout.Common.Tooling;
using Fallout.Common.Tools.DotNet;
using Fallout.Common.Utilities;
using static Fallout.Common.Tools.DotNet.DotNetTasks;

namespace Automation.Fallout.Components.Components;

public interface IVelopack : IFalloutBuild, IHasSolution, IHasConfiguration, IHasGitVersion, IHasVelopack, IHasArtifacts,
    IHasCodeSigning,
    IDoTag
{
    Target PreVelopack => t => t
        .DependsOn<ITest>(x => x.Test)
        .DependsOn<IUpdateChangelog>()
        .Description("Download existing Velopack releases")
        .Executes(() =>
        {
            if (string.IsNullOrEmpty(AzureBlobSasTokenLocal))
            {
                Serilog.Log.Warning("AzureBlobSasToken not provided, skipping download of existing releases");
                return;
            }

            var velopackProject = Solution.GetProject(VelopackProjectName);
            if (string.IsNullOrEmpty(velopackProject?.Name) || string.IsNullOrEmpty(VelopackProjectName))
            {
                Serilog.Log.Warning("VelopackProjectName not provided, skipping building...");
                Assert.False(true, "VelopackProjectName not provided");
                return;
            }

            var version = GitVersion?.FullSemVer ?? "0.0.0-local";
            var releaseType = version.Contains("beta") ? "Prerelease" : "Stable";
            var channel = $"{VelopackProjectName}-{releaseType}-{VelopackChannel}";
            var downloadDir = VelopackPublishDirectory;

            Serilog.Log.Information("Downloading existing releases from Azure Blob Storage...");
            Serilog.Log.Information("Channel: {Channel}", channel);
            Serilog.Log.Information("Download Directory: {Directory}", downloadDir);

            // Ensure vpk is installed before use
            Serilog.Log.Information("Installing/updating Velopack CLI...");
            ProcessTasks.StartProcess("dotnet", "tool update -g vpk")//--version 1.0.1")
                .AssertZeroExitCode();

            var vpkToolPath = ToolPathResolver.GetPathExecutable("vpk");

            downloadDir.CreateDirectory();

            var vpkArgs = $"download az " +
                          $"--container {VelopackBlobContainer} " +
                          $"--account {AzureBlobAccount} " +
                          $"--sas \"{AzureBlobSasTokenLocal}\" " +
                          $"--channel {channel} " +
                          $"--outputDir \"{downloadDir}\"";

            if (!string.IsNullOrEmpty(AzureBlobEndpoint))
            {
                vpkArgs += $" --endpoint {AzureBlobEndpoint}";
            }

            if (!string.IsNullOrEmpty(AzureBlobTimeout))
            {
                vpkArgs += $" --timeout {AzureBlobTimeout}";
            }

            Serilog.Log.Information("Running: vpk {Args}", vpkArgs.Replace(AzureBlobSasTokenLocal, "***"));

            ProcessTasks.StartProcess(vpkToolPath, vpkArgs, workingDirectory: RootDirectory)
                .WaitForExit();
        });

    Target BuildVelopack => t => t
        .DependsOn(PreVelopack)
        .Description("Build Velopack release package")
        .Executes(() =>
        {
            var velopackProject = Solution.GetProject(VelopackProjectName);
            if (string.IsNullOrEmpty(velopackProject?.Name) || string.IsNullOrEmpty(VelopackProjectName))
            {
                Serilog.Log.Warning("VelopackProjectName not provided, skipping building...");
                Assert.False(true, "VelopackProjectName not provided");
                return;
            }
            
            var version = GitVersion?.FullSemVer.Replace('+', '-') ?? "0.0.0-local";
            var isMainBranch = GitVersion?.BranchName.Equals("main", StringComparison.OrdinalIgnoreCase) ?? false;
            var releaseType = isMainBranch ? "Stable" : "Prerelease";
            var channel = $"{VelopackProjectName}-{releaseType}-{VelopackChannel}";
            var netRuntime = $"win-x64";
            
            var targetFrameworks = velopackProject.GetTargetFrameworks();
            var useNetRuntime = targetFrameworks.Any(x => x.StartsWith("net10.0", StringComparison.OrdinalIgnoreCase));

            Serilog.Log.Information("Building Velopack package for {Project} version {Version}", VelopackProjectName,
                version);
            Serilog.Log.Information("Release Type: {ReleaseType}", releaseType);
            Serilog.Log.Information("Channel: {Channel}", channel);

            // Publish the application to temp directory (not artifacts)
            var publishDirectory = RootDirectory / ".tmp" / "velopack-build";
            publishDirectory.CreateOrCleanDirectory();
            Serilog.Log.Information("Publishing application to {Directory}", publishDirectory);

            DotNetPublish(s => s
                .SetProject(velopackProject)
                .SetConfiguration(Configuration)
                .SetOutput(publishDirectory)
                .SetRuntime("win-x64")
                .SetSelfContained(true)
                .SetProperty("PublishSingleFile", "false")
                .SetAssemblyVersion(GitVersion?.AssemblySemVer)
                .SetFileVersion(GitVersion?.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion?.InformationalVersion));

            // Find the main executable
            var executables = Directory.GetFiles(publishDirectory, "*.exe", SearchOption.TopDirectoryOnly);
            Assert.NotEmpty(executables, $"No .exe files found in publish directory: {publishDirectory}");

            var mainExe = executables.FirstOrDefault(e =>
                              Path.GetFileName(e).Equals($"{VelopackProjectName}.exe",
                                  StringComparison.OrdinalIgnoreCase))
                          ?? executables.First();
            var mainExeName = Path.GetFileName(mainExe);

            Serilog.Log.Information("Main executable: {MainExe}", mainExeName);

            // Build Velopack package
            var vpkArgs = $"pack " +
                          $"--packId {VelopackProjectName} " +
                          $"--packVersion {version} " +
                          $"--packDir \"{publishDirectory}\" " +
                          $"--mainExe {mainExeName} " +
                          $"--channel {channel} " +
                          //(useNetRuntime ? $"--runtime {netRuntime} " : "") +
                          $"--outputDir \"{VelopackPublishDirectory}\"";

            //Serilog.Log.Information("SslComUsername: {SslComUsername}}", SslComUsername);
            // Add signing parameters if configured
            //if (!string.IsNullOrEmpty(SslComUsername) && !string.IsNullOrEmpty(SslComPassword))
            //{
            //    vpkArgs += $" --signParams \"/fd sha256 /tr http://timestamp.ssl.com /td sha256 /n \\\"{CodeSigningCertificateName}\\\"\"";
            //    Serilog.Log.Information("Code signing enabled");
            //}
            
            if (!string.IsNullOrEmpty(VelopackIconPath) && File.Exists(VelopackIconPath))
            {
                vpkArgs += $" --icon \"{VelopackIconPath}\"";
                Serilog.Log.Information("Using icon: {VelopackIconPath}", VelopackIconPath);
            }

            var vpkToolPath = ToolPathResolver.GetPathExecutable("vpk");

            Serilog.Log.Information("Creating Velopack package...");
            Serilog.Log.Information("Running: vpk {Args}", vpkArgs);

            var process = ProcessTasks.StartProcess(vpkToolPath, vpkArgs, workingDirectory: RootDirectory, logOutput: true);
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                var output = string.Join(Environment.NewLine, process.Output.Select(o => o.Text));
                Serilog.Log.Error("Velopack command output: {Output}", output);

                if (output.Contains("equal or greater to the current version"))
                {
                    Serilog.Log.Warning("Version {Version} already exists in channel. Skipping package creation.",
                        version);
                }
                else
                {
                    Serilog.Log.Error("Velopack pack failed with exit code {ExitCode}", process.ExitCode);
                    throw new Exception($"Velopack pack failed with exit code {process.ExitCode}: {output}");
                }
            }
            else
            {
                Serilog.Log.Information("Velopack package built successfully");
            }

            // List created files
            var createdFiles = Directory.GetFiles(VelopackPublishDirectory, "*", SearchOption.TopDirectoryOnly);
            foreach (var file in createdFiles)
            {
                Serilog.Log.Information("  - {File}", Path.GetFileName(file));
            }
        });

    Target ReleaseVelopack => t => t
        .DependsOn(BuildVelopack)
        .When(IsServerBuild || ForceTagRelease, _ => _
            .Triggers<ITagRelease>(x => x.TagRelease))
        .Description("Upload Velopack releases")
        .Executes(() =>
        {
            if (string.IsNullOrEmpty(AzureBlobSasTokenLocal))
            {
                Serilog.Log.Warning("AzureBlobSasToken not provided, skipping upload");
                return;
            }
            var velopackProject = Solution.GetProject(VelopackProjectName);
            if (string.IsNullOrEmpty(velopackProject?.Name) || string.IsNullOrEmpty(VelopackProjectName))
            {
                Serilog.Log.Warning("VelopackProjectName not provided, skipping building...");
                Assert.False(true, "VelopackProjectName not provided");
                return;
            }
            
            var isMainBranch = GitVersion?.BranchName.Equals("main", StringComparison.OrdinalIgnoreCase) ?? false;
            var releaseType = isMainBranch ? "Stable" : "Prerelease";
            var channel = $"{VelopackProjectName}-{releaseType}-{VelopackChannel}";
            var keepMaxReleases = KeepMaxReleases; // PreRelease could keep fewer, but 1 was not enough

            var assetsFile = VelopackPublishDirectory / $"assets.{channel}.json";
            if (!assetsFile.Exists())
            {
                Serilog.Log.Warning("No assets file found for channel '{Channel}', skipping upload (pack was likely skipped because this version already exists)", channel);
                return;
            }

            Serilog.Log.Information("Uploading Velopack releases to Azure Blob Storage...");
            Serilog.Log.Information("Channel: {Channel}", channel);
            Serilog.Log.Information("Keep Max Releases: {KeepMaxReleases}", keepMaxReleases);
            Serilog.Log.Information("Source Directory: {Directory}", VelopackPublishDirectory);

            var vpkArgs = $"upload az " +
                          $"--outputDir \"{VelopackPublishDirectory}\" " +
                          $"--container {VelopackBlobContainer} " +
                          $"--account {AzureBlobAccount} " +
                          $"--sas \"{AzureBlobSasTokenLocal}\" " +
                          $"--channel {channel} " +
                          $"--keepMaxReleases {keepMaxReleases}";

            if (!string.IsNullOrEmpty(AzureBlobEndpoint))
            {
                vpkArgs += $" --endpoint {AzureBlobEndpoint}";
            }

            if (!string.IsNullOrEmpty(AzureBlobTimeout))
            {
                vpkArgs += $" --timeout {AzureBlobTimeout}";
            }

            var vpkToolPath = ToolPathResolver.GetPathExecutable("vpk");

            Serilog.Log.Information("Running: vpk {Args}", vpkArgs.Replace(AzureBlobSasTokenLocal, "***"));

            ProcessTasks.StartProcess(vpkToolPath, vpkArgs, workingDirectory: RootDirectory)
                .AssertZeroExitCode();

            Serilog.Log.Information("Successfully uploaded Velopack releases");
            Serilog.Log.Information("Update URL: {Url}/{Container}/{Channel}", AzureBlobEndpoint, VelopackBlobContainer,
                channel);
        });
}
