using Automation.Fallout.Builder.Models;
using Automation.Fallout.Builder.Services;
using Spectre.Console;

namespace Automation.Fallout.Builder.Commands;

public static class SetupCommand
{
    private static string? FindRepositoryRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();

        // Walk up the directory tree looking for .git or .sln
        while (currentDir != null)
        {
            // Check for .git directory
            if (Directory.Exists(Path.Combine(currentDir, ".git")))
            {
                return currentDir;
            }

            // Check for .sln file
            if (Directory.GetFiles(currentDir, "*.sln").Length > 0)
            {
                return currentDir;
            }

            // Move up one directory
            var parent = Directory.GetParent(currentDir);
            if (parent == null)
                break;

            currentDir = parent.FullName;
        }

        return null;
    }

    public static async Task<int> ExecuteAsync()
    {
        AnsiConsole.Write(
            new FigletText("AFTR Fallout Setup")
                .Color(Color.Blue));

        AnsiConsole.MarkupLine("[bold]Welcome to Automation Fallout Builder Setup![/]");
        AnsiConsole.MarkupLine("This tool will help you configure your Fallout build pipeline.\n");

        var workingDirectory = FindRepositoryRoot();
        if (workingDirectory == null)
        {
            AnsiConsole.MarkupLine("[red]Could not find repository root. Please run this command from within a git repository or directory containing a .sln file.[/]");
            return 1;
        }

        AnsiConsole.MarkupLine($"[grey]Repository Root: {workingDirectory.EscapeMarkup()}[/]\n");

        // Step 1: Ensure Nuke is installed
        if (!await NuGetPackageInstaller.InstallFalloutGlobalToolAsync())
        {
            AnsiConsole.MarkupLine("[red]Failed to install Fallout. Please install it manually and try again.[/]");
            return 1;
        }

        // Step 2: Update Nuke global tool
        await NuGetPackageInstaller.UpdateFalloutGlobalToolAsync();

        // Step 3: Run Nuke setup if not already done
        var buildDir = Path.Combine(workingDirectory, "build");
        if (!Directory.Exists(buildDir))
        {
            if (!await NuGetPackageInstaller.RunFalloutSetupAsync(workingDirectory))
            {
                AnsiConsole.MarkupLine("[red]Fallout setup failed. Please check the errors above.[/]");
                return 1;
            }
        }
        else
        {
            AnsiConsole.MarkupLine("[green]Build directory already exists, skipping Fallout setup[/]\n");
        }

        // Step 4: Find build project
        var buildCsproj = Path.Combine(buildDir, "_build.csproj");
        if (!File.Exists(buildCsproj))
        {
            AnsiConsole.MarkupLine($"[red]Build project not found at {buildCsproj.EscapeMarkup()}[/]");
            return 1;
        }

        // Step 5: Upgrade build project to net10.0
        await NuGetPackageInstaller.UpgradeProjectTargetFrameworkAsync(buildCsproj);

        // Step 6: Add PackageDownloads
        await NuGetPackageInstaller.AddPackageDownloadAsync(buildCsproj, "GitVersion.Tool", "6.5.1");
        await NuGetPackageInstaller.AddPackageDownloadAsync(buildCsproj, "ReportGenerator", "5.5.1");

        // Step 7: Get user configuration
        var config = await PromptForConfigurationAsync(workingDirectory);

        // Step 8: Install required packages
        await InstallRequiredPackagesAsync(buildCsproj, config);

        // Step 9: Install local tools
        await InstallLocalToolsAsync(workingDirectory);

        // Step 10: Delete Configuration.cs if it exists
        var configurationCs = Path.Combine(buildDir, "Configuration.cs");
        await NuGetPackageInstaller.DeleteFileIfExistsAsync(configurationCs);

        // Step 11: Copy default root items
        await NuGetPackageInstaller.CopyDefaultRootItemsAsync(workingDirectory);

        // Step 12: Update .gitignore
        await NuGetPackageInstaller.UpdateGitIgnoreAsync(workingDirectory);

        // Step 13: Generate Build.cs
        await GenerateBuildFileAsync(buildDir, config);

        AnsiConsole.MarkupLine("\n[green bold]Setup completed successfully![/]");
        AnsiConsole.MarkupLine("\n[yellow]Next steps:[/]");
        AnsiConsole.MarkupLine("  1. Review the generated build/Build.cs file");
        AnsiConsole.MarkupLine("  2. Run [cyan]nuke --help[/] to see available targets");
        AnsiConsole.MarkupLine("  3. Run [cyan]nuke[/] to execute the default build");

        return 0;
    }

    private static async Task<BuildConfiguration> PromptForConfigurationAsync(string workingDirectory)
    {
        var config = new BuildConfiguration();

        // Select build type
        var builds = DefaultBuildDiscovery.GetAvailableBuilds();
        var buildType = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Select the build type for your project:[/]")
                .AddChoices(builds.Select(b => $"{b.Name} - {b.Description}")));

        var selectedBuildName = buildType.Split(" - ")[0];
        config.BuildType = selectedBuildName;
        var selectedBuild = builds.First(b => b.Name == selectedBuildName);

        AnsiConsole.MarkupLine($"\n[green]Selected: {selectedBuildName.EscapeMarkup()}[/]\n");

        // Ask about warnings
        config.BreakBuildOnWarnings = AnsiConsole.Confirm(
            "[yellow]Break build on warnings?[/]",
            defaultValue: true);

        // Ask about secret leaks
        config.BreakBuildOnSecretLeaks = AnsiConsole.Confirm(
            "[yellow]Break build on secret leaks?[/]",
            defaultValue: true);

        // Ask about code coverage if tests are included
        if (selectedBuild.RequiresTests)
        {
            config.EnableCodeCoverage = AnsiConsole.Confirm(
                "[yellow]Enable code coverage requirement?[/]",
                defaultValue: false);

            if (config.EnableCodeCoverage)
            {
                config.MinCodeCoverage = AnsiConsole.Prompt(
                    new TextPrompt<int>("[yellow]Minimum code coverage percentage (0-100):[/]")
                        .DefaultValue(80)
                        .Validate(value =>
                        {
                            return value switch
                            {
                                < 0 => ValidationResult.Error("[red]Coverage must be at least 0[/]"),
                                > 100 => ValidationResult.Error("[red]Coverage cannot exceed 100[/]"),
                                _ => ValidationResult.Success()
                            };
                        }));
            }
        }

        // Ask about Velopack configuration if needed
        if (selectedBuild.RequiresVelopack)
        {
            AnsiConsole.MarkupLine("\n[yellow]Velopack Configuration:[/]");

            config.VelopackProjectName = ProjectDiscovery.PromptForProject(
                workingDirectory,
                "[yellow]Select the Velopack project (the executable project):[/]");

            // Scan for .ico files in the project directory
            var iconFiles = Directory.GetFiles(workingDirectory, "*.ico", SearchOption.AllDirectories)
                .Select(f => Path.GetRelativePath(workingDirectory, f))
                .ToList();

            if (iconFiles.Count > 0)
            {
                var choices = new List<string> { "None (use default)" };
                choices.AddRange(iconFiles);
                choices.Add("Enter custom path...");

                var iconChoice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[yellow]Select an icon for Velopack:[/]")
                        .AddChoices(choices));

                if (iconChoice == "Enter custom path...")
                {
                    config.VelopackIconPath = AnsiConsole.Ask<string>(
                        "[yellow]Enter the icon path (relative to solution root):[/]");
                }
                else if (iconChoice != "None (use default)")
                {
                    config.VelopackIconPath = iconChoice;
                }
            }
            else
            {
                if (AnsiConsole.Confirm("[yellow]Do you have a custom icon for Velopack?[/]", defaultValue: false))
                {
                    config.VelopackIconPath = AnsiConsole.Ask<string>(
                        "[yellow]Enter the icon path (relative to solution root):[/]");
                }
            }
        }

        return config;
    }

    private static async Task InstallRequiredPackagesAsync(string buildCsproj, BuildConfiguration config)
    {
        AnsiConsole.MarkupLine("\n[yellow bold]Installing required NuGet packages...[/]\n");

        // Core package (your components package)
        await NuGetPackageInstaller.AddPackageToProjectAsync(
            buildCsproj,
            "Automation.Fallout.Components");

        // GitVersion
        await NuGetPackageInstaller.AddPackageToProjectAsync(
            buildCsproj,
            "Fallout.Common");
    }

    private static async Task InstallLocalToolsAsync(string workingDirectory)
    {
        AnsiConsole.MarkupLine("\n[yellow bold]Installing local tools...[/]\n");

        // Install GitVersion.Tool 6.5.1
        await NuGetPackageInstaller.InstallToolLocallyAsync(
            workingDirectory,
            "GitVersion.Tool",
            "6.5.1");

        // Install Gitleaks for secret scanning
        AnsiConsole.MarkupLine("[yellow]Note: Gitleaks should be installed separately from https://github.com/gitleaks/gitleaks[/]");
    }

    private static async Task GenerateBuildFileAsync(string buildDir, BuildConfiguration config)
    {
        AnsiConsole.MarkupLine("\n[yellow bold]Generating Build.cs file...[/]\n");

        var builds = DefaultBuildDiscovery.GetAvailableBuilds();
        var selectedBuild = builds.First(b => b.Name == config.BuildType);

        var buildFileContent = BuildFileGenerator.GenerateBuildFile(config, selectedBuild);
        var buildFilePath = Path.Combine(buildDir, "Build.cs");

        await File.WriteAllTextAsync(buildFilePath, buildFileContent);

        AnsiConsole.MarkupLine($"[green]Build.cs generated at: {buildFilePath.EscapeMarkup()}[/]");
    }
}
