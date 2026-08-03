namespace Automation.Fallout.Builder.Services;

public static class RepositoryLocator
{
    /// <summary>
    /// Walks up the directory tree looking for a .git directory, a .sln or a .slnx file.
    /// </summary>
    public static string? FindRepositoryRoot(string? startingDirectory = null)
    {
        var currentDir = startingDirectory ?? Directory.GetCurrentDirectory();

        while (currentDir != null)
        {
            // Check for .git directory
            if (Directory.Exists(Path.Combine(currentDir, ".git")))
            {
                return currentDir;
            }

            // Check for .sln or .slnx file
            if (Directory.GetFiles(currentDir, "*.sln").Length > 0 ||
                Directory.GetFiles(currentDir, "*.slnx").Length > 0)
            {
                return currentDir;
            }

            // Move up one directory
            var parent = Directory.GetParent(currentDir);
            if (parent == null)
                break;

            currentDir = parent.FullName;
        }

        return null;
    }
}
