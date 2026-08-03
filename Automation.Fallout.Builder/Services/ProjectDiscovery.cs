using Spectre.Console;

namespace Automation.Fallout.Builder.Services;

public static class ProjectDiscovery
{
    /// <summary>
    /// Finds all .csproj files in the repository, excluding the _build.csproj
    /// </summary>
    public static List<string> FindProjects(string repositoryRoot)
    {
        var projects = new List<string>();

        try
        {
            // Find all .csproj and .vbproj files recursively
            var csprojFiles = Directory.GetFiles(repositoryRoot, "*.csproj", SearchOption.AllDirectories);
            var vbprojFiles = Directory.GetFiles(repositoryRoot, "*.vbproj", SearchOption.AllDirectories);
            var allProjectFiles = csprojFiles.Concat(vbprojFiles);

            foreach (var csproj in allProjectFiles)
            {
                // Skip _build.csproj
                if (Path.GetFileName(csproj).Equals("_build.csproj", StringComparison.OrdinalIgnoreCase))
                    continue;

                // Skip files in bin/obj directories
                var relativePath = Path.GetRelativePath(repositoryRoot, csproj);
                if (relativePath.Contains("\\bin\\") || relativePath.Contains("/bin/") ||
                    relativePath.Contains("\\obj\\") || relativePath.Contains("/obj/"))
                    continue;

                // Get the project name (file name without extension)
                var projectName = Path.GetFileNameWithoutExtension(csproj);
                projects.Add(projectName);
            }

            // Sort alphabetically
            projects.Sort();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[yellow]Warning: Error discovering projects: {ex.Message.EscapeMarkup()}[/]");
        }

        return projects;
    }

    /// <summary>
    /// Prompts the user to select a project from the list
    /// </summary>
    public static string? PromptForProject(string repositoryRoot, string promptMessage)
    {
        AnsiConsole.MarkupLine($"[grey]Searching for projects in: {repositoryRoot.EscapeMarkup()}[/]");

        var projects = FindProjects(repositoryRoot);

        AnsiConsole.MarkupLine($"[grey]Found {projects.Count} project(s)[/]");

        if (projects.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No projects found in the repository. Please enter the project name manually.[/]");
            return AnsiConsole.Ask<string>("[yellow]Enter the project name:[/]");
        }

        // Display found projects for debugging
        if (projects.Count > 0)
        {
            AnsiConsole.MarkupLine("[grey]Available projects:[/]");
            foreach (var proj in projects.Take(5))
            {
                AnsiConsole.MarkupLine($"[grey]  - {proj.EscapeMarkup()}[/]");
            }
            if (projects.Count > 5)
            {
                AnsiConsole.MarkupLine($"[grey]  ... and {projects.Count - 5} more[/]");
            }
            AnsiConsole.WriteLine();
        }

        // Add an option to skip or enter manually
        var choices = new List<string>(projects);
        choices.Add("[grey](Enter manually)[/]");
        choices.Add("[grey](Skip - configure later)[/]");

        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(promptMessage)
                .PageSize(15)
                .MoreChoicesText("[grey](Move up and down to reveal more projects)[/]")
                .AddChoices(choices));

        if (selection.Contains("(Enter manually)"))
        {
            return AnsiConsole.Ask<string>("[yellow]Enter the project name:[/]");
        }

        if (selection.Contains("(Skip"))
        {
            return null;
        }

        return selection;
    }
}
