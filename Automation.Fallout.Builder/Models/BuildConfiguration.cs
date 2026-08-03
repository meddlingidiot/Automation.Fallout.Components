namespace Automation.Fallout.Builder.Models;

public class BuildConfiguration
{
    public string BuildType { get; set; } = string.Empty;
    public bool BreakBuildOnWarnings { get; set; } = true;
    public bool BreakBuildOnSecretLeaks { get; set; } = true;
    public int MinCodeCoverage { get; set; } = 0;
    public bool EnableCodeCoverage { get; set; } = false;
    public string? VelopackProjectName { get; set; }
    public string? VelopackIconPath { get; set; }
}
