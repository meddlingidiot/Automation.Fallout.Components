using Automation.Fallout.Builder.Services;

namespace Automation.Fallout.Builder.UnitTests;

public class GlobalToolListTests
{
    private const string ToolListOutput = """
                                          Package Id                      Version          Commands
                                          ------------------------------------------------------------------
                                          automation.fallout.builder      1.0.0            autofallout
                                          fallout.cli                     11.0.18          fallout
                                          fallout.migrate                 10.3.49          fallout-migrate
                                          gitversion.tool                 6.5.1            dotnet-gitversion
                                          """;

    [Test]
    public async Task ParseGlobalToolList_ReadsPackageIdsAndVersions()
    {
        var tools = ParseGlobalToolList();

        using (Assert.Multiple())
        {
            await Assert.That(tools).Count().IsEqualTo(4);
            await Assert.That(tools["fallout.cli"]).IsEqualTo("11.0.18");
            await Assert.That(tools["automation.fallout.builder"]).IsEqualTo("1.0.0");
        }
    }

    [Test]
    public async Task ParseGlobalToolList_SkipsHeaderAndSeparator()
    {
        var tools = ParseGlobalToolList();

        using (Assert.Multiple())
        {
            await Assert.That(tools.ContainsKey("Package")).IsFalse();
            await Assert.That(tools.Keys.Any(k => k.StartsWith("---"))).IsFalse();
        }
    }

    [Test]
    public async Task ParseGlobalToolList_IsCaseInsensitive()
    {
        var tools = ParseGlobalToolList();

        await Assert.That(tools.ContainsKey("Fallout.Cli")).IsTrue();
    }

    [Test]
    public async Task ParseGlobalToolList_HandlesEmptyOutput()
    {
        var tools = NuGetPackageInstaller.ParseGlobalToolList(string.Empty);

        await Assert.That(tools).IsEmpty();
    }

    private static IReadOnlyDictionary<string, string> ParseGlobalToolList() =>
        NuGetPackageInstaller.ParseGlobalToolList(ToolListOutput);
}
