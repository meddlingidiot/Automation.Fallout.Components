namespace Automation.Fallout.Builder.Models;

public enum MigrationChangeKind
{
    Added,
    Changed,
    Removed,
    Skipped,
    Warning
}

public class MigrationChange
{
    public string File { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public MigrationChangeKind Kind { get; set; } = MigrationChangeKind.Changed;
}
