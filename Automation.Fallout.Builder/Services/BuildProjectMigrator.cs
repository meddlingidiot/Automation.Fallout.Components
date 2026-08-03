using System.Xml.Linq;
using Automation.Fallout.Builder.Models;

namespace Automation.Fallout.Builder.Services;

public class BuildProjectMigrationResult
{
    public string Xml { get; set; } = string.Empty;

    /// <summary>Human readable description of every edit applied to the project file.</summary>
    public List<string> Notes { get; set; } = new();
}

/// <summary>
/// Repairs a build/_build.csproj that was renamed from Nuke to Fallout.
/// </summary>
public static class BuildProjectMigrator
{
    public static BuildProjectMigrationResult Migrate(string projectXml, MigrationOptions options)
    {
        var doc = XDocument.Parse(projectXml);
        var root = doc.Root ?? throw new InvalidOperationException("The build project file has no root element.");
        var notes = new List<string>();

        RenameNukeProperties(root, notes);
        UpgradeTargetFramework(root, notes);
        MigratePackageReferences(root, options, notes);
        UpsertPackageDownload(root, "GitVersion.Tool", MigrationDefaults.GitVersionToolVersion, notes);
        UpsertPackageDownload(root, "ReportGenerator", MigrationDefaults.ReportGeneratorVersion, notes);

        return new BuildProjectMigrationResult { Xml = Serialize(doc), Notes = notes };
    }

    /// <summary>NukeRootDirectory, NukeScriptDirectory, NukeTelemetryVersion, ...</summary>
    private static void RenameNukeProperties(XElement root, List<string> notes)
    {
        var properties = ByLocalName(root, "PropertyGroup")
            .SelectMany(g => g.Elements())
            .Where(e => e.Name.LocalName.StartsWith("Nuke", StringComparison.Ordinal))
            .ToList();

        foreach (var property in properties)
        {
            var oldName = property.Name.LocalName;
            var newName = string.Concat("Fallout", oldName.AsSpan("Nuke".Length));
            property.Name = property.Name.Namespace.GetName(newName);
            notes.Add($"Renamed <{oldName}> to <{newName}>");
        }
    }

    private static void UpgradeTargetFramework(XElement root, List<string> notes)
    {
        var targetFramework = ByLocalName(root, "TargetFramework").FirstOrDefault();
        if (targetFramework == null || targetFramework.Value == MigrationDefaults.TargetFramework)
            return;

        notes.Add($"Upgraded TargetFramework from {targetFramework.Value} to {MigrationDefaults.TargetFramework}");
        targetFramework.Value = MigrationDefaults.TargetFramework;
    }

    private static void MigratePackageReferences(XElement root, MigrationOptions options, List<string> notes)
    {
        var references = ByLocalName(root, "PackageReference").ToList();

        // Automation.Nuke.Components -> Automation.Fallout.Components. The Nuke-era version (1.0.47)
        // does not exist for the Fallout package, so it always has to be re-pinned.
        foreach (var reference in references.Where(r => Include(r) == "Automation.Nuke.Components"))
        {
            SetInclude(reference, "Automation.Fallout.Components");
            notes.Add("Renamed package Automation.Nuke.Components to Automation.Fallout.Components");
        }

        // Anything else still on a Nuke package is dead weight - Fallout.Common replaces Nuke.Common.
        foreach (var reference in references.Where(r => Include(r).StartsWith("Nuke.", StringComparison.OrdinalIgnoreCase)).ToList())
        {
            notes.Add($"Removed package reference {Include(reference)}");
            reference.Remove();
        }

        UpsertPackageReference(root, "Fallout.Common", options.ResolvedFalloutCommonVersion, notes);
        UpsertPackageReference(root, "Automation.Fallout.Components", options.ResolvedComponentsVersion, notes);
    }

    private static void UpsertPackageReference(XElement root, string packageId, string version, List<string> notes)
    {
        var existing = ByLocalName(root, "PackageReference")
            .FirstOrDefault(r => string.Equals(Include(r), packageId, StringComparison.OrdinalIgnoreCase));

        if (existing == null)
        {
            var group = ByLocalName(root, "ItemGroup").FirstOrDefault(g => g.Elements().Any(e => e.Name.LocalName == "PackageReference"));
            if (group == null)
            {
                group = new XElement(root.Name.Namespace.GetName("ItemGroup"));
                root.Add(group);
            }

            group.Add(new XElement(root.Name.Namespace.GetName("PackageReference"),
                new XAttribute("Include", packageId),
                new XAttribute("Version", version)));

            notes.Add($"Added package {packageId} {version}");
            return;
        }

        var currentVersion = existing.Attribute("Version")?.Value;
        if (currentVersion == version)
            return;

        existing.SetAttributeValue("Version", version);
        notes.Add(currentVersion == null
            ? $"Pinned {packageId} to {version}"
            : $"Corrected {packageId} from {currentVersion} to {version}");
    }

    private static void UpsertPackageDownload(XElement root, string packageId, string version, List<string> notes)
    {
        var pinned = $"[{version}]";
        var existing = ByLocalName(root, "PackageDownload")
            .FirstOrDefault(p => string.Equals(Include(p), packageId, StringComparison.OrdinalIgnoreCase));

        if (existing == null)
        {
            var group = ByLocalName(root, "ItemGroup").FirstOrDefault(g => g.Elements().Any(e => e.Name.LocalName == "PackageDownload"));
            if (group == null)
            {
                group = new XElement(root.Name.Namespace.GetName("ItemGroup"));
                root.Add(group);
            }

            group.Add(new XElement(root.Name.Namespace.GetName("PackageDownload"),
                new XAttribute("Include", packageId),
                new XAttribute("Version", pinned)));

            notes.Add($"Added PackageDownload {packageId} {version}");
            return;
        }

        if (existing.Attribute("Version")?.Value == pinned)
            return;

        existing.SetAttributeValue("Version", pinned);
        notes.Add($"Updated PackageDownload {packageId} to {version}");
    }

    private static IEnumerable<XElement> ByLocalName(XContainer container, string localName) =>
        container.Descendants().Where(e => e.Name.LocalName == localName);

    private static string Include(XElement element) => element.Attribute("Include")?.Value ?? string.Empty;

    private static void SetInclude(XElement element, string value) => element.SetAttributeValue("Include", value);

    private static string Serialize(XDocument doc)
    {
        var declaration = doc.Declaration != null ? doc.Declaration + Environment.NewLine : string.Empty;
        return declaration + doc + Environment.NewLine;
    }
}
