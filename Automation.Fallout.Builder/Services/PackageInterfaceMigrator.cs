using System.Text.RegularExpressions;
using Automation.Fallout.Builder.Models;

namespace Automation.Fallout.Builder.Services;

/// <summary>
/// Retargets a build class at the split packaging interfaces. The Nuke-era IPackage carried the
/// ReleasePackage target and the GitHub Packages parameters itself; Fallout moved both onto
/// <c>IPackageGitHub</c> and <c>IPackageAzureDevOps</c> because the push destination differs per
/// platform. A build class left on the bare IPackage has no ReleasePackage target to reference and
/// fails with CS0540 on any explicit IHasGitHubPackages member.
/// </summary>
public static class PackageInterfaceMigrator
{
    /// <summary>
    /// IPackage on its own. The word boundary keeps IPackageGitHub and IPackageAzureDevOps out, so
    /// running the migration twice is a no-op.
    /// </summary>
    private static readonly Regex BarePackageInterface = new(@"\bIPackage\b", RegexOptions.Compiled);

    /// <summary>The class declaration and its base list, which is one line in every generated build file.</summary>
    private static readonly Regex ClassDeclaration = new(
        @"\bclass\s+\w+\s*:\s*(?<bases>[^\r\n{]+)",
        RegexOptions.Compiled);

    /// <summary>An explicit interface implementation such as <c>string IHasGitHubPackages.GitHubOwner => ...</c>.</summary>
    private const string GitHubPackagesMember = "IHasGitHubPackages.";

    /// <summary>
    /// The interface carrying the ReleasePackage target for the platform. Mirrors
    /// <see cref="BuildFileGenerator"/> so a migrated repository ends up where a freshly set up one would.
    /// </summary>
    public static string PackageInterfaceFor(BuildPlatform platform) => platform switch
    {
        BuildPlatform.GitHubActions => "IPackageGitHub",
        _ => "IPackageAzureDevOps"
    };

    public static string Rewrite(string source, BuildPlatform platform, out IReadOnlyList<string> notes)
    {
        var changes = new List<string>();
        notes = changes;

        if (!BarePackageInterface.IsMatch(source))
            return source;

        var packageInterface = PackageInterfaceFor(platform);
        var rewritten = BarePackageInterface.Replace(source, packageInterface);
        changes.Add($"Retargeted IPackage at {packageInterface} - ReleasePackage moved off IPackage when packaging was split per platform");

        rewritten = KeepGitHubPackagesMembersCompiling(rewritten, changes);

        return rewritten;
    }

    /// <summary>
    /// An Azure DevOps build no longer inherits IHasGitHubPackages through its packaging interface,
    /// so a leftover explicit GitHubOwner or GitHubToken member would fail to compile. Keeping the
    /// interface on the class is harmless - the members are unused parameters - and preserves a
    /// value the repository chose to spell out.
    /// </summary>
    private static string KeepGitHubPackagesMembersCompiling(string source, List<string> changes)
    {
        if (!source.Contains(GitHubPackagesMember, StringComparison.Ordinal))
            return source;

        var declaration = ClassDeclaration.Match(source);
        if (!declaration.Success)
            return source;

        var bases = declaration.Groups["bases"];
        var names = bases.Value.Split(',').Select(b => b.Trim()).ToList();

        // IPackageGitHub and ICreateGitHubRelease both inherit IHasGitHubPackages already.
        if (names.Any(name => name is "IHasGitHubPackages" or "IPackageGitHub" or "ICreateGitHubRelease"))
            return source;

        names.Insert(Math.Min(1, names.Count), "IHasGitHubPackages");
        changes.Add("Added IHasGitHubPackages to the build class - it is no longer inherited through the packaging interface, and the class implements it explicitly");

        return string.Concat(
            source.AsSpan(0, bases.Index),
            string.Join(", ", names),
            source.AsSpan(bases.Index + bases.Length));
    }
}
