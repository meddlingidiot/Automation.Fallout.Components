namespace Automation.Fallout.Builder.Models;

/// <summary>
/// Known-good versions and paths used when migrating a Nuke repository to Fallout.
/// </summary>
public static class MigrationDefaults
{
    /// <summary>
    /// The Fallout.Common version that matches the current Automation.Fallout.Components package.
    /// A blind Nuke -> Fallout rename leaves the old Nuke.Common version behind (e.g. 10.1.0), which
    /// resolves an unrelated 10.x Fallout.Common from nuget.org and fails restore with NU1605.
    /// </summary>
    public const string FalloutCommonVersion = "11.0.18";

    /// <summary>
    /// The Automation.Fallout.Components version to pin when the project still references the
    /// Nuke-era Automation.Nuke.Components package.
    /// </summary>
    public const string ComponentsVersion = "1.0.2";

    /// <summary>
    /// The Fallout CLI package id. Fallout.GlobalTool was renamed to Fallout.Cli in 10.3.41 and the
    /// old id was frozen and unlisted, so the manifest has to point at the new one. The shim command
    /// is still 'fallout', so nothing that invokes it needs to change.
    /// </summary>
    public const string GlobalToolPackageId = "fallout.cli";

    /// <summary>
    /// Version of Fallout.Cli written into .config/dotnet-tools.json in place of nuke.globaltool.
    /// </summary>
    public const string GlobalToolVersion = "11.0.18";

    public const string TargetFramework = "net10.0";

    public const string GitVersionToolVersion = "6.8.2";

    public const string ReportGeneratorVersion = "5.5.11";

    /// <summary>Legacy Nuke configuration directory, replaced by <see cref="FalloutConfigDirectory"/>.</summary>
    public const string NukeConfigDirectory = ".nuke";

    public const string FalloutConfigDirectory = ".fallout";
}
