using Automation.Fallout.Builder.Models;
using Automation.Fallout.Builder.Services;
using Spectre.Console;

namespace Automation.Fallout.Builder.Commands;

/// <summary>
/// Repairs a repository that has been renamed from Nuke to Fallout but no longer compiles.
/// </summary>
public static class MigrateCommand
{
    public static async Task<int> ExecuteAsync(MigrationOptions options)
    {
        AnsiConsole.Write(
            new FigletText("AFTR Fallout Migrate")
                .Color(Color.Orange1));

        var workingDirectory = options.Path ?? RepositoryLocator.FindRepositoryRoot();
        if (workingDirectory == null || !Directory.Exists(workingDirectory))
        {
            AnsiConsole.MarkupLine("[red]Could not find repository root. Run this from within a git repository or a directory containing a .sln or .slnx file, or pass --path.[/]");
            return 1;
        }

        var buildDirectory = Path.Combine(workingDirectory, "build");
        var buildProject = Path.Combine(buildDirectory, "_build.csproj");
        if (!File.Exists(buildProject))
        {
            AnsiConsole.MarkupLine($"[red]No build project found at {buildProject.EscapeMarkup()}.[/]");
            AnsiConsole.MarkupLine("[yellow]This repository has not been set up yet - run [cyan]aftrfallout setup[/] instead.[/]");
            return 1;
        }

        AnsiConsole.MarkupLine($"[grey]Repository Root: {workingDirectory.EscapeMarkup()}[/]");
        if (options.DryRun)
        {
            AnsiConsole.MarkupLine("[yellow bold]Dry run - no files will be written.[/]");
        }
        AnsiConsole.WriteLine();

        var report = new MigrationReport();
        var workspace = new MigrationWorkspace(workingDirectory, options, report);

        MigrateBuildProject(buildProject, workspace, report, options);
        MigrateBuildSources(buildDirectory, workspace, report);
        MigrateToolManifest(workspace, report, options);
        RootItemMigrator.MigrateScripts(workspace, report);
        RootItemMigrator.MigrateConfigDirectory(workspace);

        if (options.KeepRootItems)
        {
            report.Skip("azure-pipelines.yml, GitVersion.yml, nuget.config", "Left in place (--keep-root-items)");
        }
        else
        {
            RootItemMigrator.CopyDefaultRootItems(workspace);
        }

        MigrateGitIgnore(workspace, report);

        if (options.RefreshPackages)
        {
            await RefreshPackagesAsync(buildProject, options, report, workspace);
        }

        RenderReport(report, workspace, options);

        return 0;
    }

    private static void MigrateBuildProject(string buildProject, MigrationWorkspace workspace, MigrationReport report,
        MigrationOptions options)
    {
        var original = workspace.ReadText(buildProject);
        if (original == null)
            return;

        try
        {
            var result = BuildProjectMigrator.Migrate(original, options);

            if (result.Notes.Count == 0)
            {
                report.Skip(workspace.Relative(buildProject), "Already migrated");
                return;
            }

            workspace.WriteText(buildProject, result.Xml, string.Join("; ", result.Notes));
        }
        catch (Exception ex)
        {
            report.Warn(workspace.Relative(buildProject), $"Could not be migrated: {ex.Message}");
        }
    }

    private static void MigrateBuildSources(string buildDirectory, MigrationWorkspace workspace, MigrationReport report)
    {
        var sources = Directory.GetFiles(buildDirectory, "*.cs", SearchOption.AllDirectories)
            .Where(file => !IsGenerated(buildDirectory, file));

        foreach (var source in sources)
        {
            var original = workspace.ReadText(source);
            if (original == null)
                continue;

            var migrated = NamespaceMigrator.Rewrite(original);
            if (!string.Equals(original, migrated, StringComparison.Ordinal))
            {
                workspace.WriteText(source, migrated, "Rewrote Nuke namespaces and types to Fallout");
            }

            foreach (var literal in NamespaceMigrator.FindUnescapedPathLiterals(migrated))
            {
                report.Warn(workspace.Relative(source),
                    $"{literal} is not a verbatim string - the backslashes are read as escape sequences. Prefix it with @");
            }
        }
    }

    private static bool IsGenerated(string buildDirectory, string file)
    {
        var relative = Path.GetRelativePath(buildDirectory, file);
        return relative.StartsWith("bin" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
               || relative.StartsWith("obj" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    private static void MigrateToolManifest(MigrationWorkspace workspace, MigrationReport report, MigrationOptions options)
    {
        var manifestPath = workspace.Resolve(".config", "dotnet-tools.json");
        var original = workspace.ReadText(manifestPath);
        if (original == null)
            return;

        try
        {
            var migrated = RootItemMigrator.MigrateToolManifest(original, options.ResolvedGlobalToolVersion, out var changed);
            if (!changed || migrated == null)
                return;

            workspace.WriteText(manifestPath, migrated,
                $"Replaced the legacy global tool with {MigrationDefaults.GlobalToolPackageId} {options.ResolvedGlobalToolVersion}");
        }
        catch (Exception ex)
        {
            report.Warn(workspace.Relative(manifestPath), $"Could not be migrated: {ex.Message}");
        }
    }

    private static void MigrateGitIgnore(MigrationWorkspace workspace, MigrationReport report)
    {
        var gitignorePath = workspace.Resolve(".gitignore");
        var original = workspace.ReadText(gitignorePath) ?? string.Empty;

        var additions = NuGetPackageInstaller.BuildGitIgnoreAdditions(original);
        if (additions.Length == 0)
            return;

        workspace.WriteText(gitignorePath, original + additions, "Added Fallout and Rider ignore entries");
    }

    private static async Task RefreshPackagesAsync(string buildProject, MigrationOptions options, MigrationReport report,
        MigrationWorkspace workspace)
    {
        if (options.DryRun)
        {
            report.Skip(workspace.Relative(buildProject), "Would refresh packages to the newest feed versions (--refresh-packages)");
            return;
        }

        AnsiConsole.MarkupLine("\n[yellow bold]Refreshing packages to the newest feed versions...[/]\n");

        // Components first: it is what constrains the Fallout.Common floor.
        if (options.ComponentsVersion == null)
        {
            await NuGetPackageInstaller.AddPackageToProjectAsync(buildProject, "Automation.Fallout.Components",
                repositoryRoot: workspace.Root, fallbackVersion: MigrationDefaults.ComponentsVersion);
        }

        if (options.FalloutCommonVersion == null)
        {
            await NuGetPackageInstaller.AddPackageToProjectAsync(buildProject, "Fallout.Common",
                repositoryRoot: workspace.Root, fallbackVersion: MigrationDefaults.FalloutCommonVersion);
        }

        report.Add(workspace.Relative(buildProject), "Refreshed package versions from the configured feeds");
    }

    private static void RenderReport(MigrationReport report, MigrationWorkspace workspace, MigrationOptions options)
    {
        AnsiConsole.WriteLine();

        if (report.Changes.Count == 0)
        {
            AnsiConsole.MarkupLine("[green]Nothing to migrate - this repository is already on Fallout.[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("[bold]Action[/]")
            .AddColumn("[bold]File[/]")
            .AddColumn("[bold]Detail[/]");

        foreach (var change in report.Changes)
        {
            table.AddRow(
                Describe(change.Kind),
                change.File.EscapeMarkup(),
                change.Detail.EscapeMarkup());
        }

        AnsiConsole.Write(table);

        if (options.DryRun)
        {
            AnsiConsole.MarkupLine($"\n[yellow]Dry run complete - {report.ChangeCount} file(s) would change. Re-run without [cyan]--dry-run[/] to apply.[/]");
            return;
        }

        AnsiConsole.MarkupLine($"\n[green bold]Migration complete - {report.ChangeCount} file(s) changed.[/]");

        if (workspace.BackupDirectory != null)
        {
            AnsiConsole.MarkupLine($"[grey]Originals backed up to {workspace.Relative(workspace.BackupDirectory).EscapeMarkup()}[/]");
        }

        if (report.HasWarnings)
        {
            AnsiConsole.MarkupLine("\n[yellow bold]Review the warnings above - they need a human.[/]");
        }

        AnsiConsole.MarkupLine("\n[yellow]Next steps:[/]");
        AnsiConsole.MarkupLine("  1. Run [cyan]dotnet build build/_build.csproj[/] - it fails faster than the full solution");
        AnsiConsole.MarkupLine("  2. Run [cyan]dotnet run --project build/_build.csproj -- --help[/] to confirm the Fallout engine loads");
        AnsiConsole.MarkupLine("  3. Diff [cyan]azure-pipelines.yml[/] against your backup if the pipeline was customised");
    }

    private static string Describe(MigrationChangeKind kind) => kind switch
    {
        MigrationChangeKind.Added => "[green]added[/]",
        MigrationChangeKind.Changed => "[green]changed[/]",
        MigrationChangeKind.Removed => "[red]removed[/]",
        MigrationChangeKind.Skipped => "[grey]skipped[/]",
        MigrationChangeKind.Warning => "[yellow]warning[/]",
        _ => kind.ToString().ToLowerInvariant()
    };
}
