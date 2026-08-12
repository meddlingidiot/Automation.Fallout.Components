namespace Automation.Fallout.Builder.Models;

/// <summary>
/// A root level config file (nuget.config, GitVersion.yml, global.json, azure-pipelines.yml) embedded in this tool.
/// </summary>
public class DefaultRootItem
{
    public string FileName { get; set; } = string.Empty;

    public byte[] Content { get; set; } = Array.Empty<byte>();
}
