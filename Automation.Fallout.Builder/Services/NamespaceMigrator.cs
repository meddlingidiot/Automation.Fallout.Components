using System.Text.RegularExpressions;

namespace Automation.Fallout.Builder.Services;

/// <summary>
/// Rewrites Nuke namespaces and types to their Fallout equivalents, and repairs the one rename a
/// blind Nuke -> Fallout find/replace gets wrong.
/// </summary>
public static class NamespaceMigrator
{
    /// <summary>
    /// Ordered, most specific first. Nuke.Common.* maps onto Fallout.Common.* for everything
    /// (CI, Execution, Git, IO, Tooling, Tools, Utilities, ChangeLog) except ProjectModel, which
    /// moved out from under Common entirely and became Fallout.Solutions.
    /// </summary>
    private static readonly (string Old, string New)[] Replacements =
    {
        ("Nuke.Common.ProjectModel", "Fallout.Solutions"),
        // A straight Nuke -> Fallout rename produces this namespace, which does not exist and fails
        // with CS0234: 'ProjectModel' does not exist in the namespace 'Fallout.Common'.
        ("Fallout.Common.ProjectModel", "Fallout.Solutions"),
        ("Nuke.ProjectModel", "Fallout.Solutions"),
        ("Automation.Nuke.Components", "Automation.Fallout.Components"),
        ("Nuke.Common", "Fallout.Common"),
        ("INukeBuild", "IFalloutBuild"),
        ("NukeBuild", "FalloutBuild"),
        ("\".nuke\"", "\".fallout\"")
    };

    /// <summary>
    /// Matches a non-verbatim string literal holding what looks like a Windows path, where a
    /// backslash is followed by a letter. Those are parsed as escape sequences (\a, \r, \t, ...)
    /// so the literal compiles but the path is wrong at runtime.
    /// </summary>
    private static readonly Regex UnescapedPathLiteral = new(
        @"(?<!@)""[^""\r\n]*\\[a-zA-Z][^""\r\n]*\.[a-zA-Z0-9]+[^""\r\n]*""",
        RegexOptions.Compiled);

    public static string Rewrite(string source)
    {
        foreach (var (oldValue, newValue) in Replacements)
        {
            source = source.Replace(oldValue, newValue, StringComparison.Ordinal);
        }

        return source;
    }

    public static bool NeedsMigration(string source) =>
        !string.Equals(Rewrite(source), source, StringComparison.Ordinal);

    /// <summary>
    /// Finds string literals that should almost certainly be verbatim (@"...") because they contain
    /// backslash sequences that C# will silently interpret as escapes.
    /// </summary>
    public static IReadOnlyList<string> FindUnescapedPathLiterals(string source)
    {
        return UnescapedPathLiteral.Matches(source)
            .Select(m => m.Value)
            // A doubled backslash is already escaped correctly.
            .Where(literal => !literal.Contains(@"\\", StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }
}
