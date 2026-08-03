using Automation.Fallout.Builder.Models;
using Automation.Fallout.Builder.Services;

namespace Automation.Fallout.Builder.UnitTests;

public class BuildProjectMigratorTests
{
    /// <summary>
    /// A _build.csproj exactly as a blind Nuke -> Fallout rename leaves it: the package ids are
    /// right but the versions are still the Nuke ones.
    /// </summary>
    private const string BlindlyRenamedProject = """
                                                 <Project Sdk="Microsoft.NET.Sdk">
                                                   <PropertyGroup>
                                                     <OutputType>Exe</OutputType>
                                                     <TargetFramework>net8.0</TargetFramework>
                                                     <FalloutRootDirectory>..</FalloutRootDirectory>
                                                   </PropertyGroup>
                                                   <ItemGroup>
                                                     <PackageReference Include="Automation.Fallout.Components" Version="1.0.47" />
                                                     <PackageReference Include="Fallout.Common" Version="10.1.0" />
                                                   </ItemGroup>
                                                 </Project>
                                                 """;

    private const string UntouchedNukeProject = """
                                                <Project Sdk="Microsoft.NET.Sdk">
                                                  <PropertyGroup>
                                                    <TargetFramework>net8.0</TargetFramework>
                                                    <NukeRootDirectory>..</NukeRootDirectory>
                                                    <NukeScriptDirectory>..</NukeScriptDirectory>
                                                    <NukeTelemetryVersion>1</NukeTelemetryVersion>
                                                  </PropertyGroup>
                                                  <ItemGroup>
                                                    <PackageReference Include="Automation.Nuke.Components" Version="1.0.47" />
                                                    <PackageReference Include="Nuke.Common" Version="10.1.0" />
                                                  </ItemGroup>
                                                </Project>
                                                """;

    [Test]
    public async Task Migrate_CorrectsTheFalloutCommonVersionLeftBehindByTheRename()
    {
        var result = BuildProjectMigrator.Migrate(BlindlyRenamedProject, new MigrationOptions());

        using (Assert.Multiple())
        {
            await Assert.That(result.Xml)
                .Contains($"""Include="Fallout.Common" Version="{MigrationDefaults.FalloutCommonVersion}" """.TrimEnd());
            await Assert.That(result.Xml).DoesNotContain("""Version="10.1.0" """.TrimEnd());
            await Assert.That(result.Notes).Contains(n => n.Contains("Corrected Fallout.Common from 10.1.0"));
        }
    }

    [Test]
    public async Task Migrate_RenamesNukeMsBuildProperties()
    {
        var result = BuildProjectMigrator.Migrate(UntouchedNukeProject, new MigrationOptions());

        using (Assert.Multiple())
        {
            await Assert.That(result.Xml).Contains("<FalloutRootDirectory>");
            await Assert.That(result.Xml).Contains("<FalloutScriptDirectory>");
            await Assert.That(result.Xml).Contains("<FalloutTelemetryVersion>");
            await Assert.That(result.Xml).DoesNotContain("<Nuke");
        }
    }

    [Test]
    public async Task Migrate_ReplacesNukePackagesWithFalloutPackages()
    {
        var result = BuildProjectMigrator.Migrate(UntouchedNukeProject, new MigrationOptions());

        using (Assert.Multiple())
        {
            await Assert.That(result.Xml).DoesNotContain("Nuke.Common");
            await Assert.That(result.Xml).Contains("Automation.Fallout.Components");
            await Assert.That(result.Xml).Contains("Fallout.Common");
        }
    }

    [Test]
    public async Task Migrate_UpgradesTargetFrameworkAndAddsPackageDownloads()
    {
        var result = BuildProjectMigrator.Migrate(BlindlyRenamedProject, new MigrationOptions());

        using (Assert.Multiple())
        {
            await Assert.That(result.Xml).Contains($"<TargetFramework>{MigrationDefaults.TargetFramework}</TargetFramework>");
            await Assert.That(result.Xml).Contains($"""Include="GitVersion.Tool" Version="[{MigrationDefaults.GitVersionToolVersion}]" """.TrimEnd());
            await Assert.That(result.Xml).Contains($"""Include="ReportGenerator" Version="[{MigrationDefaults.ReportGeneratorVersion}]" """.TrimEnd());
        }
    }

    [Test]
    public async Task Migrate_HonoursExplicitVersionPins()
    {
        var options = new MigrationOptions
        {
            FalloutCommonVersion = "11.0.17",
            ComponentsVersion = "1.0.1"
        };

        var result = BuildProjectMigrator.Migrate(BlindlyRenamedProject, options);

        using (Assert.Multiple())
        {
            await Assert.That(result.Xml).Contains("""Include="Fallout.Common" Version="11.0.17" """.TrimEnd());
            await Assert.That(result.Xml).Contains("""Include="Automation.Fallout.Components" Version="1.0.1" """.TrimEnd());
        }
    }

    [Test]
    public async Task Migrate_IsIdempotent()
    {
        var options = new MigrationOptions();

        var first = BuildProjectMigrator.Migrate(BlindlyRenamedProject, options);
        var second = BuildProjectMigrator.Migrate(first.Xml, options);

        using (Assert.Multiple())
        {
            await Assert.That(second.Notes).IsEmpty();
            await Assert.That(second.Xml).IsEqualTo(first.Xml);
        }
    }
}
