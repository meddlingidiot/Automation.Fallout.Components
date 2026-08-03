namespace Automation.Fallout.Builder.Models;

/// <summary>
/// Options for the 'aftrfallout migrate' command.
/// </summary>
public class MigrationOptions
{
    /// <summary>Repository root. When null the root is discovered from the current directory.</summary>
    public string? Path { get; set; }

    /// <summary>Report what would change without touching any file.</summary>
    public bool DryRun { get; set; }

    /// <summary>Skip copying originals into .fallout/Backup before rewriting them.</summary>
    public bool NoBackup { get; set; }

    /// <summary>Leave azure-pipelines.yml, GitVersion.yml and nuget.config alone.</summary>
    public bool KeepRootItems { get; set; }

    /// <summary>After repairing the project, run 'dotnet add package' to move to the newest feed versions.</summary>
    public bool RefreshPackages { get; set; }

    public string? FalloutCommonVersion { get; set; }

    public string? ComponentsVersion { get; set; }

    public string? GlobalToolVersion { get; set; }

    public string ResolvedFalloutCommonVersion => FalloutCommonVersion ?? MigrationDefaults.FalloutCommonVersion;

    public string ResolvedComponentsVersion => ComponentsVersion ?? MigrationDefaults.ComponentsVersion;

    public string ResolvedGlobalToolVersion => GlobalToolVersion ?? MigrationDefaults.GlobalToolVersion;
}
