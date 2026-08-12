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

    /// <summary>
    /// Fallout.Common drags in NuGet.Packaging 6.14.3, which drops NuGet.Frameworks 6.14.3.1 beside
    /// the build host. SDK 10.0.400's MSBuild binds NuGet.Frameworks 7.9.0.0 when it evaluates
    /// [MSBuild]::GetTargetFrameworkIdentifier, and the app-local copy wins probing, so every target
    /// that parses a project dies with a manifest mismatch. Pinning NuGet.Packaging to the version
    /// whose NuGet.Frameworks matches <see cref="SdkVersion"/> keeps the two in step.
    /// </summary>
    public const string NuGetPackagingVersion = "7.9.0";

    /// <summary>
    /// The .NET SDK written into global.json. Must stay in step with <see cref="NuGetPackagingVersion"/>:
    /// a different feature band ships a different NuGet.Frameworks and re-opens the mismatch above.
    /// </summary>
    public const string SdkVersion = "10.0.400";

    public const string GitVersionToolVersion = "6.8.2";

    public const string ReportGeneratorVersion = "5.5.11";

    /// <summary>Legacy Nuke configuration directory, replaced by <see cref="FalloutConfigDirectory"/>.</summary>
    public const string NukeConfigDirectory = ".nuke";

    public const string FalloutConfigDirectory = ".fallout";
}
