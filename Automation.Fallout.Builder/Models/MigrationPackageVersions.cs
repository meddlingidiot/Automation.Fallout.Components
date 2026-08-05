namespace Automation.Fallout.Builder.Models;

/// <summary>
/// The versions the migration pins into build/_build.csproj.
/// </summary>
public sealed record MigrationPackageVersions(string Components, string FalloutCommon)
{
    /// <summary>
    /// What the caller asked for, falling straight back to the versions this tool shipped with. Used
    /// when no feed lookup has run.
    /// </summary>
    public static MigrationPackageVersions FromOptions(MigrationOptions options) =>
        new(options.ResolvedComponentsVersion, options.ResolvedFalloutCommonVersion);
}
