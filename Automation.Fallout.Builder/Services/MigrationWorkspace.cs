using Automation.Fallout.Builder.Models;

namespace Automation.Fallout.Builder.Services;

/// <summary>
/// All file mutations performed by the migration go through here so that --dry-run and the
/// .fallout/Backup copies are honoured in exactly one place.
/// </summary>
public class MigrationWorkspace
{
    private readonly MigrationOptions _options;
    private readonly MigrationReport _report;
    private readonly string _backupRoot;

    public MigrationWorkspace(string root, MigrationOptions options, MigrationReport report)
    {
        Root = root;
        _options = options;
        _report = report;
        _backupRoot = Path.Combine(root, MigrationDefaults.FalloutConfigDirectory, "Backup",
            $"migrate-{DateTime.Now:yyyyMMdd-HHmmss}");
    }

    public string Root { get; }

    public bool DryRun => _options.DryRun;

    /// <summary>The running report, so callers can record a skip they decided on themselves.</summary>
    public MigrationReport Report => _report;

    /// <summary>Set once something has actually been backed up, so the summary can point at it.</summary>
    public string? BackupDirectory { get; private set; }

    /// <summary>Resolves a repository relative path to an absolute one.</summary>
    public string Resolve(params string[] parts) => Path.Combine(new[] { Root }.Concat(parts).ToArray());

    public string Relative(string absolutePath) => Path.GetRelativePath(Root, absolutePath);

    public void WriteText(string absolutePath, string content, string detail)
    {
        var exists = File.Exists(absolutePath);
        if (exists && string.Equals(File.ReadAllText(absolutePath), content, StringComparison.Ordinal))
        {
            _report.Skip(Relative(absolutePath), $"{detail} (already up to date)");
            return;
        }

        _report.Add(Relative(absolutePath), detail, exists ? MigrationChangeKind.Changed : MigrationChangeKind.Added);

        if (_options.DryRun)
            return;

        Backup(absolutePath);
        EnsureParentDirectory(absolutePath);
        File.WriteAllText(absolutePath, content);
    }

    public void WriteBytes(string absolutePath, byte[] content, string detail)
    {
        var exists = File.Exists(absolutePath);
        if (exists && File.ReadAllBytes(absolutePath).SequenceEqual(content))
        {
            _report.Skip(Relative(absolutePath), $"{detail} (already up to date)");
            return;
        }

        _report.Add(Relative(absolutePath), detail, exists ? MigrationChangeKind.Changed : MigrationChangeKind.Added);

        if (_options.DryRun)
            return;

        Backup(absolutePath);
        EnsureParentDirectory(absolutePath);
        File.WriteAllBytes(absolutePath, content);
    }

    public void DeleteFile(string absolutePath, string detail)
    {
        if (!File.Exists(absolutePath))
            return;

        _report.Add(Relative(absolutePath), detail, MigrationChangeKind.Removed);

        if (_options.DryRun)
            return;

        Backup(absolutePath);
        File.Delete(absolutePath);
    }

    public void DeleteDirectory(string absolutePath, string detail)
    {
        if (!Directory.Exists(absolutePath))
            return;

        _report.Add(Relative(absolutePath), detail, MigrationChangeKind.Removed);

        if (_options.DryRun)
            return;

        foreach (var file in Directory.GetFiles(absolutePath, "*", SearchOption.AllDirectories))
        {
            Backup(file);
        }

        Directory.Delete(absolutePath, recursive: true);
    }

    public string? ReadText(string absolutePath) =>
        File.Exists(absolutePath) ? File.ReadAllText(absolutePath) : null;

    private void Backup(string absolutePath)
    {
        if (_options.NoBackup || !File.Exists(absolutePath))
            return;

        var destination = Path.Combine(_backupRoot, Relative(absolutePath));
        EnsureParentDirectory(destination);
        File.Copy(absolutePath, destination, overwrite: true);
        BackupDirectory = _backupRoot;
    }

    private static void EnsureParentDirectory(string absolutePath)
    {
        var directory = Path.GetDirectoryName(absolutePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
