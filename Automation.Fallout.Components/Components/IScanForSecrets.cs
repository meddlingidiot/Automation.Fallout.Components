using System.Runtime.InteropServices;
using Automation.Fallout.Components.Parameters;
using Fallout.Common;
using Fallout.Common.IO;
using Fallout.Common.Tooling;

namespace Automation.Fallout.Components.Components;

public interface IScanForSecrets : IFalloutBuild, IHasTests
{
    // 1. Configuration: Allows overriding version via parameter: --gitleaks-version 8.18.2
    [Parameter("Version of Gitleaks to use.")] 
    string GitleaksVersion => TryGetValue(() => GitleaksVersion) ?? "8.18.1";

    // 2. Tool Definition: "Lazy" path resolution. 
    // We don't throw here. We return a path, and let the execution step decide if it's valid.
    AbsolutePath GitleaksToolPath => GetGitleaksPath();

    // 3. The Tool Delegate: Allows usage like Gitleaks("detect ...")
    Tool Gitleaks => ToolResolver.GetPathTool(GitleaksToolPath);

    Target ScanForSecrets => t => t
        .After<ITest>(x => x.CoverageReport)
        .OnlyWhenDynamic(() => BreakBuildOnSecretLeaks)
        .Executes(() =>
        {
            
            // 4. Provisioning: "Zero Setup" logic.
            // If the tool isn't found, download it automatically.
            EnsureGitleaksInstalled().Wait();

            // 5. Execution: Using the Tool delegate which logs neatly
            Serilog.Log.Information("Scanning for secrets...");
            
            //--no-git --source
            Gitleaks($"detect --source {RootDirectory} --verbose --redact --exit-code 0");
        });

    // --- Helper Logic ---

    private AbsolutePath GetGitleaksPath()
    {
        // A. Check if user provided a custom path via parameter
        // [Parameter] string GitleaksPath ... could be added if needed.

        // B. Check PATH (System installed)
        try 
        {
            var path = ToolPathResolver.GetPathExecutable("gitleaks");
            if (!string.IsNullOrWhiteSpace(path)) return path;
        }
        catch { /* Ignore 'Not Found' errors here to allow fallback */ }

        // C. Check Local Cache (provisioned by us previously)
        var localExe = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "gitleaks.exe" : "gitleaks";
        return TemporaryDirectory / "gitleaks" / localExe;
    }

    private async Task EnsureGitleaksInstalled()
    {
        if (GitleaksToolPath.FileExists()) return;

        Serilog.Log.Information($"Gitleaks not found globally or locally. Downloading v{GitleaksVersion}...");

        string os = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "windows" :
                    RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "linux" : "darwin";
        string arch = RuntimeInformation.OSArchitecture == Architecture.Arm64 ? "arm64" : "x64";
        string ext = os == "windows" ? "zip" : "tar.gz";

        // Gitleaks release naming convention
        string downloadUrl = $"https://github.com/gitleaks/gitleaks/releases/download/v{GitleaksVersion}/gitleaks_{GitleaksVersion}_{os}_{arch}.{ext}";
        AbsolutePath archivePath = TemporaryDirectory / $"gitleaks.{ext}";
        AbsolutePath installDir = TemporaryDirectory / "gitleaks";

        try 
        {
            // Download
            using var client = new HttpClient();
            var response = await client.GetByteArrayAsync(downloadUrl);
            await File.WriteAllBytesAsync(archivePath, response);

            // Extract
            installDir.CreateDirectory();
            archivePath.UncompressTo(installDir);

            // Permission (Linux/Mac)
            if (os != "windows")
            {
                var exePath = installDir / "gitleaks";
                ProcessTasks.StartProcess("chmod", $"+x {exePath}").AssertZeroExitCode();
            }
            
            Serilog.Log.Information($"Gitleaks installed to {installDir}");
        }
        finally
        {
            archivePath.DeleteFile();
        }
    }
}
