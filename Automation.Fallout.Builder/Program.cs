using System.CommandLine;
using Automation.Fallout.Builder.Commands;
using Automation.Fallout.Builder.Models;

namespace Automation.Fallout.Builder;

class Program
{
    static int Main(string[] args)
    {
        var rootCommand = new RootCommand("Automation Fallout Builder - Simplify Fallout build setup");

        var setupCommand = new Command("setup", "Setup Fallout build for the current project");
        setupCommand.SetAction(async (parseResult, cancellationToken) =>
        {
            return await SetupCommand.ExecuteAsync();
        });

        rootCommand.Subcommands.Add(setupCommand);
        rootCommand.Subcommands.Add(BuildMigrateCommand());

        return rootCommand.Parse(args).Invoke();
    }

    private static Command BuildMigrateCommand()
    {
        var pathOption = new Option<string?>("--path", "-p")
        {
            Description = "Repository root to migrate. Defaults to the repository containing the current directory."
        };

        var dryRunOption = new Option<bool>("--dry-run")
        {
            Description = "Report what would change without writing any file."
        };

        var noBackupOption = new Option<bool>("--no-backup")
        {
            Description = "Do not copy originals into .fallout/Backup before rewriting them."
        };

        var keepRootItemsOption = new Option<bool>("--keep-root-items")
        {
            Description = "Leave azure-pipelines.yml, GitVersion.yml and nuget.config untouched."
        };

        var refreshPackagesOption = new Option<bool>("--refresh-packages")
        {
            Description = "After repairing the project, run 'dotnet add package' to move to the newest feed versions."
        };

        var falloutVersionOption = new Option<string?>("--fallout-version")
        {
            Description = $"Fallout.Common version to pin. Default: {MigrationDefaults.FalloutCommonVersion}."
        };

        var componentsVersionOption = new Option<string?>("--components-version")
        {
            Description = $"Automation.Fallout.Components version to pin. Default: {MigrationDefaults.ComponentsVersion}."
        };

        var globalToolVersionOption = new Option<string?>("--global-tool-version")
        {
            Description = $"Fallout.Cli version for .config/dotnet-tools.json. Default: {MigrationDefaults.GlobalToolVersion}."
        };

        var migrateCommand = new Command("migrate",
            "Repair a repository that was renamed from Nuke to Fallout but no longer compiles");

        migrateCommand.Options.Add(pathOption);
        migrateCommand.Options.Add(dryRunOption);
        migrateCommand.Options.Add(noBackupOption);
        migrateCommand.Options.Add(keepRootItemsOption);
        migrateCommand.Options.Add(refreshPackagesOption);
        migrateCommand.Options.Add(falloutVersionOption);
        migrateCommand.Options.Add(componentsVersionOption);
        migrateCommand.Options.Add(globalToolVersionOption);

        migrateCommand.SetAction(async (parseResult, cancellationToken) =>
        {
            var options = new MigrationOptions
            {
                Path = parseResult.GetValue(pathOption),
                DryRun = parseResult.GetValue(dryRunOption),
                NoBackup = parseResult.GetValue(noBackupOption),
                KeepRootItems = parseResult.GetValue(keepRootItemsOption),
                RefreshPackages = parseResult.GetValue(refreshPackagesOption),
                FalloutCommonVersion = parseResult.GetValue(falloutVersionOption),
                ComponentsVersion = parseResult.GetValue(componentsVersionOption),
                GlobalToolVersion = parseResult.GetValue(globalToolVersionOption)
            };

            return await MigrateCommand.ExecuteAsync(options);
        });

        return migrateCommand;
    }
}
