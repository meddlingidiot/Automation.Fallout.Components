using System.Xml.Linq;

namespace Automation.Fallout.Builder.Services;

/// <summary>The rewritten MSBuild file, plus whether anything actually changed.</summary>
public class XmlEditResult
{
    public string Xml { get; set; } = string.Empty;

    public bool Changed { get; set; }
}

/// <summary>
/// Adds packages to a project the way the repository expects them.
/// </summary>
/// <remarks>
/// A repository with a Directory.Packages.props keeps every version in that one file, and a
/// PackageReference that carries its own Version fails restore with NU1008. So the version goes in
/// as a PackageVersion and the project only gets the bare Include.
/// </remarks>
public static class CentralPackageManagement
{
    public const string PropsFileName = "Directory.Packages.props";

    /// <summary>
    /// Walks up from <paramref name="startDirectory"/> looking for the Directory.Packages.props that
    /// applies to a project in that directory. The walk stops at <paramref name="repositoryRoot"/>:
    /// MSBuild would keep going to the drive root, but a props file outside the repository is not
    /// ours to edit.
    /// </summary>
    public static string? FindPropsFile(string startDirectory, string repositoryRoot)
    {
        var root = Path.GetFullPath(repositoryRoot);
        var current = Path.GetFullPath(startDirectory);

        while (true)
        {
            var candidate = Path.Combine(current, PropsFileName);
            if (File.Exists(candidate))
                return candidate;

            if (string.Equals(current, root, StringComparison.OrdinalIgnoreCase))
                return null;

            var parent = Directory.GetParent(current);
            if (parent == null)
                return null;

            current = parent.FullName;
        }
    }

    /// <summary>
    /// Whether the props file actually manages versions centrally. Only an explicit
    /// ManagePackageVersionsCentrally of false opts out - the property is routinely left out of
    /// hand-written props files, and NuGet's own template sets it to true.
    /// </summary>
    public static bool IsEnabled(string propsXml)
    {
        var doc = XDocument.Parse(propsXml);

        var property = ByLocalName(doc, "ManagePackageVersionsCentrally").FirstOrDefault();
        if (property == null)
            return true;

        return !bool.TryParse(property.Value.Trim(), out var enabled) || enabled;
    }

    /// <summary>Adds or corrects the PackageVersion entry for a package in Directory.Packages.props.</summary>
    public static XmlEditResult UpsertPackageVersion(string propsXml, string packageId, string version)
    {
        var doc = XDocument.Parse(propsXml);
        var root = doc.Root ?? throw new InvalidOperationException($"{PropsFileName} has no root element.");

        var existing = ByLocalName(doc, "PackageVersion")
            .FirstOrDefault(e => string.Equals(Include(e), packageId, StringComparison.OrdinalIgnoreCase));

        if (existing == null)
        {
            ItemGroupFor(root, "PackageVersion").Add(new XElement(root.Name.Namespace.GetName("PackageVersion"),
                new XAttribute("Include", packageId),
                new XAttribute("Version", version)));

            return new XmlEditResult { Xml = Serialize(doc), Changed = true };
        }

        if (existing.Attribute("Version")?.Value == version)
            return new XmlEditResult { Xml = propsXml, Changed = false };

        existing.SetAttributeValue("Version", version);
        return new XmlEditResult { Xml = Serialize(doc), Changed = true };
    }

    /// <summary>
    /// Adds or corrects a PackageReference. A null <paramref name="version"/> means the repository
    /// manages versions centrally, so any Version attribute already on the reference is dropped.
    /// </summary>
    public static XmlEditResult UpsertPackageReference(string projectXml, string packageId, string? version)
    {
        var doc = XDocument.Parse(projectXml);
        var root = doc.Root ?? throw new InvalidOperationException("The project file has no root element.");

        var existing = ByLocalName(doc, "PackageReference")
            .FirstOrDefault(e => string.Equals(Include(e), packageId, StringComparison.OrdinalIgnoreCase));

        if (existing == null)
        {
            var reference = new XElement(root.Name.Namespace.GetName("PackageReference"),
                new XAttribute("Include", packageId));

            if (version != null)
            {
                reference.SetAttributeValue("Version", version);
            }

            ItemGroupFor(root, "PackageReference").Add(reference);
            return new XmlEditResult { Xml = Serialize(doc), Changed = true };
        }

        var currentVersion = existing.Attribute("Version");

        if (version == null)
        {
            if (currentVersion == null)
                return new XmlEditResult { Xml = projectXml, Changed = false };

            currentVersion.Remove();
            return new XmlEditResult { Xml = Serialize(doc), Changed = true };
        }

        if (currentVersion?.Value == version)
            return new XmlEditResult { Xml = projectXml, Changed = false };

        existing.SetAttributeValue("Version", version);
        return new XmlEditResult { Xml = Serialize(doc), Changed = true };
    }

    /// <summary>Reads the version a project already pins for a package, if any.</summary>
    public static string? ReadPackageReferenceVersion(string projectXml, string packageId)
    {
        var doc = XDocument.Parse(projectXml);

        return ByLocalName(doc, "PackageReference")
            .FirstOrDefault(e => string.Equals(Include(e), packageId, StringComparison.OrdinalIgnoreCase))
            ?.Attribute("Version")?.Value;
    }

    /// <summary>The ItemGroup already holding items of this kind, or a new one appended to the file.</summary>
    private static XElement ItemGroupFor(XElement root, string itemName)
    {
        var group = ByLocalName(root, "ItemGroup")
            .FirstOrDefault(g => g.Elements().Any(e => e.Name.LocalName == itemName));

        if (group != null)
            return group;

        group = new XElement(root.Name.Namespace.GetName("ItemGroup"));
        root.Add(group);
        return group;
    }

    private static IEnumerable<XElement> ByLocalName(XContainer container, string localName) =>
        container.Descendants().Where(e => e.Name.LocalName == localName);

    private static string Include(XElement element) => element.Attribute("Include")?.Value ?? string.Empty;

    private static string Serialize(XDocument doc)
    {
        var declaration = doc.Declaration != null ? doc.Declaration + Environment.NewLine : string.Empty;
        return declaration + doc + Environment.NewLine;
    }
}
