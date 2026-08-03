namespace Automation.Fallout.Builder.Models;

public class DefaultBuildInfo
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Components { get; set; } = new();
    public bool RequiresTests { get; set; }
    public bool RequiresPackaging { get; set; }
    public bool RequiresVelopack { get; set; }
}
