namespace Automation.Fallout.Components.Parameters;

/// <summary>
/// Where <see cref="Components.IPackageMultiPlatform"/> pushes packages.
/// </summary>
public enum PackagePublishTarget
{
    /// <summary>Detect the CI platform from the environment. Pushes nothing on a local run.</summary>
    Auto,

    /// <summary>Azure DevOps Artifacts only.</summary>
    AzureDevOps,

    /// <summary>GitHub Packages only.</summary>
    GitHub,

    /// <summary>Both feeds, Azure DevOps first.</summary>
    Both,

    /// <summary>Produce packages but push nowhere. Useful for testing the build locally.</summary>
    None
}
