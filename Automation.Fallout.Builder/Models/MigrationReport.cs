namespace Automation.Fallout.Builder.Models;

/// <summary>
/// Collects everything the migration touched (or would touch) so it can be rendered once at the end.
/// </summary>
public class MigrationReport
{
    private readonly List<MigrationChange> _changes = new();

    public IReadOnlyList<MigrationChange> Changes => _changes;

    public bool HasWarnings => _changes.Any(c => c.Kind == MigrationChangeKind.Warning);

    public int ChangeCount => _changes.Count(c => c.Kind is MigrationChangeKind.Added
        or MigrationChangeKind.Changed
        or MigrationChangeKind.Removed);

    public void Add(string file, string detail, MigrationChangeKind kind = MigrationChangeKind.Changed)
    {
        _changes.Add(new MigrationChange { File = file, Detail = detail, Kind = kind });
    }

    public void Skip(string file, string detail) => Add(file, detail, MigrationChangeKind.Skipped);

    public void Warn(string file, string detail) => Add(file, detail, MigrationChangeKind.Warning);
}
