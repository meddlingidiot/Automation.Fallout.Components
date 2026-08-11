using System.Diagnostics;
using System.Xml.Linq;
using Automation.Fallout.Builder.Models;
using Spectre.Console;

namespace Automation.Fallout.Builder.Services;

public static class NuGetPackageInstaller
{
    /// <summary>
    /// Ensures the Fallout CLI is available as a global tool.
    /// </summary>
    /// <remarks>
    /// Detection reads 'dotnet tool list -g' rather than running 'fallout --version'. Inside a
    /// Fallout repository that probe restores and compiles the _build project before answering,
    /// which takes minutes, so any timeout on it reports "not installed" and triggers a redundant
    /// install that then collides on the 'fallout' shim.
    /// </remarks>
    public static async Task<bool> InstallFalloutGlobalToolAsync(string? version = null)
    {
        version ??= MigrationDefaults.GlobalToolVersion;

        AnsiConsole.MarkupLine("[yellow]Checking if the Fallout CLI is installed...[/]");

        var installedTools = await ListGlobalToolsAsync();

        if (installedTools.TryGetValue(MigrationDefaults.GlobalToolPackageId, out var installedVersion))
        {
            AnsiConsole.MarkupLine($"[green]Fallout CLI {installedVersion.EscapeMarkup()} is already installed[/]");
            return true;
        }

        // Fallout.GlobalTool was renamed to Fallout.Cli and unlisted, but it still owns the
        // 'fallout' shim, so installing the replacement fails until it is removed.
        if (installedTools.ContainsKey(SupersededGlobalToolPackageId))
        {
            AnsiConsole.MarkupLine($"[yellow]{SupersededGlobalToolPackageId} owns the 'fallout' command and has been superseded by {MigrationDefaults.GlobalToolPackageId}. Removing it...[/]");

            if (!await RunDotNetCommandAsync($"tool uninstall {SupersededGlobalToolPackageId} --global"))
            {
                AnsiConsole.MarkupLine($"[red]Could not uninstall {SupersededGlobalToolPackageId}. Remove it manually and try again.[/]");
                return false;
            }
        }

        AnsiConsole.MarkupLine($"[yellow]Installing Fallout CLI {version.EscapeMarkup()}...[/]");

        // The 11.x releases are unlisted on nuget.org, so an unversioned install cannot resolve them.
        var success = await RunDotNetCommandAsync($"tool install {MigrationDefaults.GlobalToolPackageId} -g --version {version}");

        if (success)
        {
            AnsiConsole.MarkupLine("[green]Fallout CLI installed successfully[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Failed to install the Fallout CLI[/]");
        }

        return success;
    }

    /// <summary>
    /// Moves an already installed Fallout CLI onto the requested version.
    /// </summary>
    public static async Task<bool> UpdateFalloutGlobalToolAsync(string? version = null)
    {
        version ??= MigrationDefaults.GlobalToolVersion;

        var installedTools = await ListGlobalToolsAsync();
        if (installedTools.TryGetValue(MigrationDefaults.GlobalToolPackageId, out var installedVersion)
            && installedVersion == version)
        {
            return true;
        }

        AnsiConsole.MarkupLine($"[yellow]Updating the Fallout CLI to {version.EscapeMarkup()}...[/]");

        var success = await RunDotNetCommandAsync(
            $"tool update {MigrationDefaults.GlobalToolPackageId} --global --version {version}");

        if (success)
        {
            AnsiConsole.MarkupLine("[green]Fallout CLI updated successfully[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[yellow]Fallout CLI update failed, but continuing...[/]");
        }

        return success;
    }

    /// <summary>The package id that Fallout.Cli replaced. It still owns the 'fallout' shim.</summary>
    private const string SupersededGlobalToolPackageId = "fallout.globaltool";

    /// <summary>
    /// Parses 'dotnet tool list -g' into a package id -> version map.
    /// </summary>
    public static async Task<IReadOnlyDictionary<string, string>> ListGlobalToolsAsync()
    {
        var (succeeded, output, _) = await RunDotNetCommandCaptureAsync("tool list -g");

        return succeeded
            ? ParseGlobalToolList(output)
            : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Parses the table emitted by 'dotnet tool list' into a package id -> version map.
    /// </summary>
    public static IReadOnlyDictionary<string, string> ParseGlobalToolList(string output)
    {
        var tools = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var columns = line.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            // Skip the header row and the ---- separator.
            if (columns.Length < 2 || columns[0].StartsWith("---", StringComparison.Ordinal) ||
                columns[0].Equals("Package", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            tools[columns[0]] = columns[1];
        }

        return tools;
    }

    public static async Task<bool> RunFalloutSetupAsync(string workingDirectory)
    {
        AnsiConsole.MarkupLine("[yellow]Running Fallout setup...[/]");

        var success = await RunInteractiveCommandAsync("fallout", ":setup", workingDirectory);

        if (success)
        {
            AnsiConsole.MarkupLine("[green]Fallout setup completed successfully[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Fallout setup failed[/]");
        }

        return success;
    }

    /// <summary>
    /// References a package from a project, putting the version wherever the repository keeps it.
    /// </summary>
    /// <remarks>
    /// This edits the project XML rather than shelling out to 'dotnet add package' because the
    /// version has to land in Directory.Packages.props when the repository manages versions
    /// centrally, and because a repository being set up for the first time may not have restored
    /// anything yet.
    /// </remarks>
    /// <param name="version">Pins an exact version. When null, the newest released version on the feeds is used.</param>
    /// <param name="repositoryRoot">Where the feed lookup runs and where the search for Directory.Packages.props stops. Defaults to the project directory.</param>
    /// <param name="fallbackVersion">Used when the feeds cannot be reached, so setup still leaves a buildable project.</param>
    public static async Task<bool> AddPackageToProjectAsync(
        string projectPath,
        string packageName,
        string? version = null,
        string? repositoryRoot = null,
        string? fallbackVersion = null)
    {
        AnsiConsole.MarkupLine($"[yellow]Adding package {packageName} to project...[/]");

        try
        {
            var projectDirectory = Path.GetDirectoryName(Path.GetFullPath(projectPath))!;
            repositoryRoot ??= projectDirectory;

            var projectXml = await File.ReadAllTextAsync(projectPath);

            var resolvedVersion = await ResolvePackageVersionAsync(
                packageName, version, repositoryRoot, projectXml, fallbackVersion);

            if (resolvedVersion == null)
            {
                AnsiConsole.MarkupLine(
                    $"[red]Could not determine a version for {packageName}. Add it to the project manually.[/]");
                return false;
            }

            var propsFile = CentralPackageManagement.FindPropsFile(projectDirectory, repositoryRoot);
            var centrallyManaged = propsFile != null &&
                                   CentralPackageManagement.IsEnabled(await File.ReadAllTextAsync(propsFile));

            if (centrallyManaged)
            {
                var props = CentralPackageManagement.UpsertPackageVersion(
                    await File.ReadAllTextAsync(propsFile!), packageName, resolvedVersion);

                if (props.Changed)
                {
                    await File.WriteAllTextAsync(propsFile!, props.Xml);
                }

                AnsiConsole.MarkupLine(
                    $"[green]Pinned {packageName} {resolvedVersion} in {Path.GetFileName(propsFile)}[/]");
            }

            var project = CentralPackageManagement.UpsertPackageReference(
                projectXml, packageName, centrallyManaged ? null : resolvedVersion);

            if (project.Changed)
            {
                await File.WriteAllTextAsync(projectPath, project.Xml);
            }

            AnsiConsole.MarkupLine($"[green]Package {packageName} added successfully[/]");
            return true;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to add package {packageName}: {ex.Message.EscapeMarkup()}[/]");
            return false;
        }
    }

    /// <summary>
    /// An explicit pin wins, then the newest released version on the feeds, then whatever the project
    /// already pins, then the version this tool shipped with.
    /// </summary>
    private static async Task<string?> ResolvePackageVersionAsync(
        string packageName,
        string? requestedVersion,
        string repositoryRoot,
        string projectXml,
        string? fallbackVersion)
    {
        if (requestedVersion != null)
            return requestedVersion;

        var latest = await NuGetVersionResolver.GetLatestStableVersionAsync(packageName, repositoryRoot);
        if (latest != null)
            return latest;

        var existing = CentralPackageManagement.ReadPackageReferenceVersion(projectXml, packageName);
        if (existing != null)
        {
            AnsiConsole.MarkupLine($"[yellow]Keeping the version {packageName} is already pinned to ({existing.EscapeMarkup()})[/]");
            return existing;
        }

        if (fallbackVersion != null)
        {
            AnsiConsole.MarkupLine($"[yellow]Falling back to {packageName} {fallbackVersion.EscapeMarkup()}[/]");
        }

        return fallbackVersion;
    }

    public static async Task<bool> InstallToolLocallyAsync(string workingDirectory, string toolName, string? version = null)
    {
        var versionArg = version != null ? $" --version {version}" : "";
        var command = $"tool install {toolName}{versionArg} --create-manifest-if-needed";

        AnsiConsole.MarkupLine($"[yellow]Installing local tool {toolName}...[/]");

        var success = await RunDotNetCommandAsync(command, workingDirectory);

        if (success)
        {
            AnsiConsole.MarkupLine($"[green]Tool {toolName} installed successfully[/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]Failed to install tool {toolName}[/]");
        }

        return success;
    }

    public static async Task<bool> UpgradeProjectTargetFrameworkAsync(string projectPath, string targetFramework = "net10.0")
    {
        AnsiConsole.MarkupLine($"[yellow]Checking target framework in {Path.GetFileName(projectPath)}...[/]");

        try
        {
            var doc = XDocument.Load(projectPath);
            var targetFrameworkElement = doc.Descendants("TargetFramework").FirstOrDefault();

            if (targetFrameworkElement == null)
            {
                AnsiConsole.MarkupLine("[red]Could not find TargetFramework element in project file[/]");
                return false;
            }

            var currentFramework = targetFrameworkElement.Value;
            if (currentFramework == targetFramework)
            {
                AnsiConsole.MarkupLine($"[green]Target framework is already {targetFramework}[/]");
                return true;
            }

            AnsiConsole.MarkupLine($"[yellow]Upgrading from {currentFramework} to {targetFramework}...[/]");
            targetFrameworkElement.Value = targetFramework;
            doc.Save(projectPath);

            AnsiConsole.MarkupLine($"[green]Target framework upgraded to {targetFramework}[/]");
            return true;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error upgrading target framework: {ex.Message.EscapeMarkup()}[/]");
            return false;
        }
    }

    public static async Task<bool> AddPackageDownloadAsync(string projectPath, string packageName, string version)
    {
        AnsiConsole.MarkupLine($"[yellow]Adding PackageDownload {packageName} {version}...[/]");

        try
        {
            var doc = XDocument.Load(projectPath);
            var root = doc.Root;

            if (root == null)
            {
                AnsiConsole.MarkupLine("[red]Invalid project file[/]");
                return false;
            }

            // Find or create ItemGroup for PackageDownload
            var packageDownloadGroup = root.Elements("ItemGroup")
                .FirstOrDefault(g => g.Elements("PackageDownload").Any());

            if (packageDownloadGroup == null)
            {
                packageDownloadGroup = new XElement("ItemGroup");
                root.Add(packageDownloadGroup);
            }

            // Check if package already exists
            var existingPackage = packageDownloadGroup.Elements("PackageDownload")
                .FirstOrDefault(p => p.Attribute("Include")?.Value == packageName);

            if (existingPackage != null)
            {
                // Update version if different
                var currentVersion = existingPackage.Attribute("Version")?.Value;
                if (currentVersion == $"[{version}]")
                {
                    AnsiConsole.MarkupLine($"[green]{packageName} {version} already exists[/]");
                    return true;
                }

                existingPackage.Attribute("Version")!.Value = $"[{version}]";
                AnsiConsole.MarkupLine($"[yellow]Updated {packageName} to {version}[/]");
            }
            else
            {
                // Add new PackageDownload
                var newPackage = new XElement("PackageDownload",
                    new XAttribute("Include", packageName),
                    new XAttribute("Version", $"[{version}]"));
                packageDownloadGroup.Add(newPackage);
                AnsiConsole.MarkupLine($"[green]Added {packageName} {version}[/]");
            }

            doc.Save(projectPath);
            return true;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error adding PackageDownload: {ex.Message.EscapeMarkup()}[/]");
            return false;
        }
    }

    public static Task DeleteFileIfExistsAsync(string filePath)
    {
        if (File.Exists(filePath))
        {
            AnsiConsole.MarkupLine($"[yellow]Deleting {Path.GetFileName(filePath)}...[/]");
            File.Delete(filePath);
            AnsiConsole.MarkupLine($"[green]Deleted {Path.GetFileName(filePath)}[/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"[grey]{Path.GetFileName(filePath)} does not exist, skipping...[/]");
        }

        return Task.CompletedTask;
    }

    public static async Task UpdateGitIgnoreAsync(string workingDirectory)
    {
        AnsiConsole.MarkupLine("[yellow]Updating .gitignore...[/]");

        var gitignorePath = Path.Combine(workingDirectory, ".gitignore");

        try
        {
            var content = string.Empty;
            if (File.Exists(gitignorePath))
            {
                content = await File.ReadAllTextAsync(gitignorePath);
            }

            var additions = BuildGitIgnoreAdditions(content);

            if (additions.Length == 0)
            {
                AnsiConsole.MarkupLine("[green].gitignore already contains Fallout and Rider entries[/]");
                return;
            }

            await File.AppendAllTextAsync(gitignorePath, additions);

            AnsiConsole.MarkupLine("[green]Updated .gitignore with Fallout and Rider entries[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error updating .gitignore: {ex.Message.EscapeMarkup()}[/]");
        }
    }

    /// <summary>
    /// Returns the text that needs appending to an existing .gitignore, or an empty string when the
    /// Fallout and Rider sections are already present.
    /// </summary>
    public static string BuildGitIgnoreAdditions(string content)
    {
        var needsFalloutSection = !content.Contains("# Fallout Build");
        var needsRiderSection = !content.Contains("# JetBrains Rider");
        var needsDotNetSection = !content.Contains("# Rider DotSettings");

        if (!needsFalloutSection && !needsRiderSection && !needsDotNetSection)
        {
            return string.Empty;
        }

        var additions = new System.Text.StringBuilder();

        if (!content.EndsWith("\n") && content.Length > 0)
        {
            additions.AppendLine();
        }

        if (needsFalloutSection)
        {
            additions.AppendLine("# Fallout Build");
            additions.AppendLine(".fallout/build.schema.json");
            additions.AppendLine(".fallout/temp/");
            additions.AppendLine(".tmp/");
            additions.AppendLine("artifacts/");
            additions.AppendLine();
        }

        if (needsRiderSection)
        {
            additions.AppendLine("# JetBrains Rider");
            additions.AppendLine(".idea/");
        }
        if (needsDotNetSection)
        {
            additions.AppendLine();
            additions.AppendLine("# Rider DotSettings");
            additions.AppendLine("*.DotSettings");
        }

        return additions.ToString();
    }

    /// <summary>
    /// Reads the root config files (nuget.config, GitVersion.yml, global.json, azure-pipelines.yml)
    /// that ship embedded in this tool.
    /// </summary>
    /// <summary>
    /// Folder under DefaultRootItems holding the items every platform gets.
    /// </summary>
    private const string CommonRootItemFolder = "Common";

    /// <summary>
    /// Items that do not belong at the repository root. The embedded resource name cannot carry a
    /// directory (MSBuild flattens the path into dots), so the destination is mapped explicitly.
    /// </summary>
    private static readonly Dictionary<string, string> RootItemDestinations = new()
    {
        ["build.yml"] = Path.Combine(".github", "workflows", "build.yml")
    };

    /// <summary>
    /// Where a root item belongs relative to the repository root. Most sit at the root; the GitHub
    /// workflow has to land under .github/workflows to be picked up.
    /// </summary>
    public static string ResolveRootItemDestination(string fileName) =>
        RootItemDestinations.TryGetValue(fileName, out var mapped) ? mapped : fileName;

    /// <summary>
    /// Root items that are never overwritten once the repository has its own copy. nuget.config is
    /// the only one: it carries the feeds and credentials the repository restores from, and the
    /// defaults embedded here point somewhere else entirely.
    /// </summary>
    public static bool IsProtectedRootItem(string fileName) =>
        string.Equals(fileName, "nuget.config", StringComparison.OrdinalIgnoreCase);

    public static IReadOnlyList<DefaultRootItem> GetDefaultRootItems(BuildPlatform platform)
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var items = new List<DefaultRootItem>();
        var wanted = new[] { CommonRootItemFolder, platform.ToString() };

        foreach (var resourceName in assembly.GetManifestResourceNames().Where(name => name.Contains("DefaultRootItems")))
        {
            // Format: Automation.Fallout.Builder.DefaultRootItems.<Folder>.filename.ext
            // MSBuild turns the directory separator into a dot, so the part right after
            // "DefaultRootItems" is the platform folder and the rest is the file name.
            var parts = resourceName.Split('.');
            var index = Array.IndexOf(parts, "DefaultRootItems");
            if (index < 0 || index + 2 >= parts.Length)
                continue;

            var folder = parts[index + 1];
            if (!wanted.Contains(folder, StringComparer.OrdinalIgnoreCase))
                continue;

            var fileName = string.Join(".", parts.Skip(index + 2));

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                AnsiConsole.MarkupLine($"[red]Could not load resource {resourceName.EscapeMarkup()}[/]");
                continue;
            }

            using var buffer = new MemoryStream();
            stream.CopyTo(buffer);

            items.Add(new DefaultRootItem
            {
                FileName = fileName,
                Content = buffer.ToArray()
            });
        }

        return items;
    }

    public static async Task<bool> CopyDefaultRootItemsAsync(string workingDirectory, BuildPlatform platform)
    {
        AnsiConsole.MarkupLine($"[yellow]Copying default root items for {platform} from embedded resources...[/]");

        try
        {
            var items = GetDefaultRootItems(platform);

            if (items.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No DefaultRootItems embedded resources found[/]");
                return false;
            }

            foreach (var item in items)
            {
                var relativePath = ResolveRootItemDestination(item.FileName);

                var destFile = Path.Combine(workingDirectory, relativePath);

                // An existing nuget.config carries the repository's own feeds and credentials, so it
                // is never overwritten - replacing it would silently cut the repository off from its
                // packages.
                if (IsProtectedRootItem(item.FileName) && File.Exists(destFile))
                {
                    AnsiConsole.MarkupLine($"[yellow]Kept existing {relativePath.EscapeMarkup()}[/]");
                    continue;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(destFile)!);
                await File.WriteAllBytesAsync(destFile, item.Content);

                AnsiConsole.MarkupLine($"[green]Copied {relativePath.EscapeMarkup()}[/]");
            }

            AnsiConsole.MarkupLine("[green]All default root items copied successfully[/]");
            return true;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error copying default root items: {ex.Message.EscapeMarkup()}[/]");
            return false;
        }
    }

    private static async Task<bool> RunDotNetCommandAsync(string arguments, string? workingDirectory = null)
    {
        return await RunCommandAsync("dotnet", arguments, workingDirectory);
    }

    /// <summary>
    /// Runs a dotnet command and returns its stdout instead of echoing it to the console.
    /// </summary>
    internal static async Task<(bool Succeeded, string Output, string Error)> RunDotNetCommandCaptureAsync(
        string arguments, string? workingDirectory = null)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDirectory ?? Directory.GetCurrentDirectory()
                }
            };

            process.Start();

            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            await Task.WhenAll(outputTask, errorTask, process.WaitForExitAsync());

            return (process.ExitCode == 0, await outputTask, await errorTask);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error running command: {ex.Message.EscapeMarkup()}[/]");
            return (false, string.Empty, ex.Message);
        }
    }

    private static async Task<bool> RunInteractiveCommandAsync(string command, string arguments, string? workingDirectory = null)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    WorkingDirectory = workingDirectory ?? Directory.GetCurrentDirectory()
                }
            };

            process.Start();
            await process.WaitForExitAsync();

            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error running command: {ex.Message.EscapeMarkup()}[/]");
            return false;
        }
    }

    private static async Task<bool> RunCommandAsync(string command, string arguments, string? workingDirectory = null)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDirectory ?? Directory.GetCurrentDirectory()
                }
            };

            process.OutputDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    AnsiConsole.MarkupLine($"[grey]{args.Data.EscapeMarkup()}[/]");
                }
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    AnsiConsole.MarkupLine($"[red]{args.Data.EscapeMarkup()}[/]");
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();

            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error running command: {ex.Message.EscapeMarkup()}[/]");
            return false;
        }
    }
}
