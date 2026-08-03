using System.Diagnostics;
using System.Xml.Linq;
using Automation.Fallout.Builder.Models;
using Spectre.Console;

namespace Automation.Fallout.Builder.Services;

public static class NuGetPackageInstaller
{
    public static async Task<bool> InstallFalloutGlobalToolAsync()
    {
        AnsiConsole.MarkupLine("[yellow]Checking if Fallout global tool is installed...[/]");

        var isInstalled = await IsFalloutInstalledAsync();
        if (isInstalled)
        {
            AnsiConsole.MarkupLine("[green]Fallout is already installed[/]");
            return true;
        }

        AnsiConsole.MarkupLine("[yellow]Installing Fallout global tool...[/]");
        // Fallout.GlobalTool was renamed to Fallout.Cli and unlisted; the shim is still 'fallout'.
        var success = await RunDotNetCommandAsync("tool install Fallout.Cli -g");

        if (success)
        {
            AnsiConsole.MarkupLine("[green]Fallout global tool installed successfully[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Failed to install Fallout global tool[/]");
        }

        return success;
    }

    public static async Task<bool> UpdateFalloutGlobalToolAsync()
    {
        AnsiConsole.MarkupLine("[yellow]Updating Fallout global tool...[/]");

        var success = await RunDotNetCommandAsync("tool update Fallout.Cli --global");

        if (success)
        {
            AnsiConsole.MarkupLine("[green]Fallout global tool updated successfully[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[yellow]Fallout global tool update failed, but continuing...[/]");
        }

        return success;
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

    public static async Task<bool> AddPackageToProjectAsync(string projectPath, string packageName, string? version = null)
    {
        // Remove the package first if it exists
        AnsiConsole.MarkupLine($"[yellow]Removing existing {packageName} package if present...[/]");
        var removeCommand = $"remove \"{projectPath}\" package {packageName}";
        await RunDotNetCommandAsync(removeCommand);

        // Add the package (always use newest version unless specified)
        var versionArg = version != null ? $" --version {version}" : "";
        var command = $"add \"{projectPath}\" package {packageName}{versionArg}";

        AnsiConsole.MarkupLine($"[yellow]Adding package {packageName} to project...[/]");

        var success = await RunDotNetCommandAsync(command);

        if (success)
        {
            AnsiConsole.MarkupLine($"[green]Package {packageName} added successfully[/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]Failed to add package {packageName}[/]");
        }

        return success;
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
    /// Reads the root config files (nuget.config, GitVersion.yml, azure-pipelines.yml) that ship
    /// embedded in this tool.
    /// </summary>
    public static IReadOnlyList<DefaultRootItem> GetDefaultRootItems()
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var items = new List<DefaultRootItem>();

        foreach (var resourceName in assembly.GetManifestResourceNames().Where(name => name.Contains("DefaultRootItems")))
        {
            // Format: Automation.Fallout.Builder.DefaultRootItems.filename.ext
            var parts = resourceName.Split('.');
            var fileNameParts = new List<string>();

            // Start from "DefaultRootItems" onwards
            bool foundDefaultRootItems = false;
            foreach (var part in parts)
            {
                if (part == "DefaultRootItems")
                {
                    foundDefaultRootItems = true;
                    continue;
                }
                if (foundDefaultRootItems)
                {
                    fileNameParts.Add(part);
                }
            }

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
                FileName = string.Join(".", fileNameParts),
                Content = buffer.ToArray()
            });
        }

        return items;
    }

    public static async Task<bool> CopyDefaultRootItemsAsync(string workingDirectory)
    {
        AnsiConsole.MarkupLine("[yellow]Copying default root items from embedded resources...[/]");

        try
        {
            var items = GetDefaultRootItems();

            if (items.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No DefaultRootItems embedded resources found[/]");
                return false;
            }

            foreach (var item in items)
            {
                var destFile = Path.Combine(workingDirectory, item.FileName);
                await File.WriteAllBytesAsync(destFile, item.Content);

                AnsiConsole.MarkupLine($"[green]Copied {item.FileName.EscapeMarkup()}[/]");
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

    private static async Task<bool> IsFalloutInstalledAsync()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "fallout",
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            // Use async event handlers to prevent buffer deadlock
            process.OutputDataReceived += (sender, args) => { /* Consume output */ };
            process.ErrorDataReceived += (sender, args) => { /* Consume error */ };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Wait with a 10-second timeout
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(10));
            var exitTask = process.WaitForExitAsync();
            var completedTask = await Task.WhenAny(exitTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                process.Kill(true);
                return false;
            }

            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private static async Task<bool> RunDotNetCommandAsync(string arguments, string? workingDirectory = null)
    {
        return await RunCommandAsync("dotnet", arguments, workingDirectory);
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
