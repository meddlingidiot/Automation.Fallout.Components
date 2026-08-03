using Automation.Fallout.Builder.Services;

namespace Automation.Fallout.Builder.UnitTests;

public class NamespaceMigratorTests
{
    [Test]
    public async Task Rewrite_NukeUsings_MapOntoFallout()
    {
        var source = """
                     using Nuke.Common;
                     using Nuke.Common.IO;
                     using Nuke.Common.CI;
                     using Nuke.Common.Tools.DotNet;
                     using Automation.Nuke.Components.Components;
                     """;

        var result = NamespaceMigrator.Rewrite(source);

        using (Assert.Multiple())
        {
            await Assert.That(result).Contains("using Fallout.Common;");
            await Assert.That(result).Contains("using Fallout.Common.IO;");
            await Assert.That(result).Contains("using Fallout.Common.CI;");
            await Assert.That(result).Contains("using Fallout.Common.Tools.DotNet;");
            await Assert.That(result).Contains("using Automation.Fallout.Components.Components;");
            await Assert.That(result).DoesNotContain("Nuke");
        }
    }

    [Test]
    public async Task Rewrite_ProjectModel_BecomesFalloutSolutions()
    {
        var source = "using Nuke.Common.ProjectModel;";

        var result = NamespaceMigrator.Rewrite(source);

        await Assert.That(result).IsEqualTo("using Fallout.Solutions;");
    }

    [Test]
    public async Task Rewrite_RepairsBlindRenameOfProjectModel()
    {
        // What a straight Nuke -> Fallout find/replace produces. The namespace does not exist and
        // the build fails with CS0234.
        var source = "using Fallout.Common.ProjectModel;";

        var result = NamespaceMigrator.Rewrite(source);

        await Assert.That(result).IsEqualTo("using Fallout.Solutions;");
    }

    [Test]
    public async Task Rewrite_BuildBaseTypes_AreRenamed()
    {
        var source = "public interface IMyLint : INukeBuild { } public class Build : NukeBuild { }";

        var result = NamespaceMigrator.Rewrite(source);

        using (Assert.Multiple())
        {
            await Assert.That(result).Contains("IFalloutBuild");
            await Assert.That(result).Contains(": FalloutBuild");
        }
    }

    [Test]
    public async Task Rewrite_ConfigDirectoryLiteral_IsRenamed()
    {
        var source = """var temp = RootDirectory / ".nuke" / "temp";""";

        var result = NamespaceMigrator.Rewrite(source);

        await Assert.That(result).Contains("\".fallout\"");
    }

    [Test]
    public async Task NeedsMigration_IsFalseForAnAlreadyMigratedFile()
    {
        var source = """
                     using Fallout.Common;
                     using Fallout.Solutions;
                     using Automation.Fallout.Components;
                     """;

        await Assert.That(NamespaceMigrator.NeedsMigration(source)).IsFalse();
    }

    [Test]
    public async Task FindUnescapedPathLiterals_FlagsNonVerbatimWindowsPaths()
    {
        // \a and \r are valid escapes, so this compiles but the path is wrong at runtime.
        var source = """string IHasVelopack.VelopackIconPath => "MyApp.WpfExample\assets\reset-password.ico";""";

        var result = NamespaceMigrator.FindUnescapedPathLiterals(source);

        await Assert.That(result).Count().IsEqualTo(1);
    }

    [Test]
    public async Task FindUnescapedPathLiterals_IgnoresVerbatimAndEscapedLiterals()
    {
        var source = """
                     var a = @"MyApp\assets\icon.ico";
                     var b = "MyApp\\assets\\icon.ico";
                     var c = "no backslashes here.txt";
                     """;

        var result = NamespaceMigrator.FindUnescapedPathLiterals(source);

        await Assert.That(result).IsEmpty();
    }
}
