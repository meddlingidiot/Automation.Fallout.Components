using Automation.Fallout.Builder.Services;

namespace Automation.Fallout.Builder.UnitTests;

public class CentralPackageManagementTests : IAsyncDisposable
{
    private readonly string _testDirectory;

    public CentralPackageManagementTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"CentralPackageManagementTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    public ValueTask DisposeAsync()
    {
        if (Directory.Exists(_testDirectory))
        {
            try { Directory.Delete(_testDirectory, true); } catch { /* Ignore cleanup errors */ }
        }

        return ValueTask.CompletedTask;
    }

    /// <summary>A repository that keeps every version in one place, exactly as the NuGet docs describe it.</summary>
    private const string PropsFile = """
                                     <Project>
                                       <PropertyGroup>
                                         <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
                                       </PropertyGroup>
                                       <ItemGroup>
                                         <PackageVersion Include="Avalonia" Version="12.1.0" />
                                       </ItemGroup>
                                     </Project>
                                     """;

    /// <summary>build/_build.csproj as 'fallout :setup' leaves it.</summary>
    private const string BuildProject = """
                                        <Project Sdk="Microsoft.NET.Sdk">
                                          <PropertyGroup>
                                            <TargetFramework>net10.0</TargetFramework>
                                          </PropertyGroup>
                                          <ItemGroup>
                                            <PackageReference Include="Fallout.Common" Version="11.0.18" />
                                          </ItemGroup>
                                        </Project>
                                        """;

    #region FindPropsFile

    [Test]
    public async Task FindPropsFile_PropsInRepositoryRoot_FoundFromTheBuildDirectory()
    {
        var propsFile = Path.Combine(_testDirectory, CentralPackageManagement.PropsFileName);
        await File.WriteAllTextAsync(propsFile, PropsFile);
        var buildDirectory = Directory.CreateDirectory(Path.Combine(_testDirectory, "build")).FullName;

        var found = CentralPackageManagement.FindPropsFile(buildDirectory, _testDirectory);

        await Assert.That(found).IsEqualTo(propsFile);
    }

    [Test]
    public async Task FindPropsFile_NoProps_ReturnsNull()
    {
        var buildDirectory = Directory.CreateDirectory(Path.Combine(_testDirectory, "build")).FullName;

        var found = CentralPackageManagement.FindPropsFile(buildDirectory, _testDirectory);

        await Assert.That(found).IsNull();
    }

    [Test]
    public async Task FindPropsFile_PropsAboveTheRepositoryRoot_IsNotOursToEdit()
    {
        await File.WriteAllTextAsync(Path.Combine(_testDirectory, CentralPackageManagement.PropsFileName), PropsFile);
        var repositoryRoot = Directory.CreateDirectory(Path.Combine(_testDirectory, "repo")).FullName;
        var buildDirectory = Directory.CreateDirectory(Path.Combine(repositoryRoot, "build")).FullName;

        var found = CentralPackageManagement.FindPropsFile(buildDirectory, repositoryRoot);

        await Assert.That(found).IsNull();
    }

    #endregion

    #region IsEnabled

    [Test]
    public async Task IsEnabled_ExplicitlyTrue_IsEnabled()
    {
        await Assert.That(CentralPackageManagement.IsEnabled(PropsFile)).IsTrue();
    }

    [Test]
    public async Task IsEnabled_ExplicitlyFalse_IsDisabled()
    {
        var propsFile = PropsFile.Replace(">true<", ">false<");

        await Assert.That(CentralPackageManagement.IsEnabled(propsFile)).IsFalse();
    }

    [Test]
    public async Task IsEnabled_PropertyOmitted_IsEnabled()
    {
        var propsFile = """
                        <Project>
                          <ItemGroup>
                            <PackageVersion Include="Avalonia" Version="12.1.0" />
                          </ItemGroup>
                        </Project>
                        """;

        await Assert.That(CentralPackageManagement.IsEnabled(propsFile)).IsTrue();
    }

    #endregion

    #region UpsertPackageVersion

    [Test]
    public async Task UpsertPackageVersion_NewPackage_JoinsTheExistingItemGroup()
    {
        var result = CentralPackageManagement.UpsertPackageVersion(PropsFile, "Automation.Fallout.Components", "1.0.2");

        using (Assert.Multiple())
        {
            await Assert.That(result.Changed).IsTrue();
            await Assert.That(result.Xml).Contains("""Include="Automation.Fallout.Components" Version="1.0.2" """.TrimEnd());
            await Assert.That(result.Xml).Contains("""Include="Avalonia" Version="12.1.0" """.TrimEnd());
        }
    }

    [Test]
    public async Task UpsertPackageVersion_SameVersion_LeavesTheFileAlone()
    {
        var result = CentralPackageManagement.UpsertPackageVersion(PropsFile, "Avalonia", "12.1.0");

        using (Assert.Multiple())
        {
            await Assert.That(result.Changed).IsFalse();
            await Assert.That(result.Xml).IsEqualTo(PropsFile);
        }
    }

    [Test]
    public async Task UpsertPackageVersion_DifferentVersion_MovesThePin()
    {
        var result = CentralPackageManagement.UpsertPackageVersion(PropsFile, "Avalonia", "12.2.0");

        using (Assert.Multiple())
        {
            await Assert.That(result.Changed).IsTrue();
            await Assert.That(result.Xml).Contains("""Include="Avalonia" Version="12.2.0" """.TrimEnd());
            await Assert.That(result.Xml).DoesNotContain("12.1.0");
        }
    }

    #endregion

    #region UpsertPackageReference

    [Test]
    public async Task UpsertPackageReference_CentrallyManaged_DropsTheVersionAttribute()
    {
        var result = CentralPackageManagement.UpsertPackageReference(BuildProject, "Fallout.Common", version: null);

        using (Assert.Multiple())
        {
            await Assert.That(result.Changed).IsTrue();
            await Assert.That(result.Xml).Contains("""Include="Fallout.Common" """.TrimEnd());
            await Assert.That(result.Xml).DoesNotContain("11.0.18");
        }
    }

    [Test]
    public async Task UpsertPackageReference_CentrallyManagedNewPackage_IsAddedWithoutAVersion()
    {
        var result = CentralPackageManagement.UpsertPackageReference(BuildProject, "Automation.Fallout.Components", version: null);

        using (Assert.Multiple())
        {
            await Assert.That(result.Changed).IsTrue();
            await Assert.That(result.Xml).Contains("""<PackageReference Include="Automation.Fallout.Components" />""");
        }
    }

    [Test]
    public async Task UpsertPackageReference_NotCentrallyManaged_KeepsTheVersionOnTheReference()
    {
        var result = CentralPackageManagement.UpsertPackageReference(BuildProject, "Automation.Fallout.Components", "1.0.2");

        using (Assert.Multiple())
        {
            await Assert.That(result.Changed).IsTrue();
            await Assert.That(result.Xml).Contains("""Include="Automation.Fallout.Components" Version="1.0.2" """.TrimEnd());
        }
    }

    [Test]
    public async Task UpsertPackageReference_AlreadyVersionless_ReportsNoChange()
    {
        var versionless = CentralPackageManagement.UpsertPackageReference(BuildProject, "Fallout.Common", version: null).Xml;

        var result = CentralPackageManagement.UpsertPackageReference(versionless, "Fallout.Common", version: null);

        using (Assert.Multiple())
        {
            await Assert.That(result.Changed).IsFalse();
            await Assert.That(result.Xml).IsEqualTo(versionless);
        }
    }

    [Test]
    public async Task UpsertPackageReference_ProjectWithoutAnItemGroup_CreatesOne()
    {
        var project = """
                      <Project Sdk="Microsoft.NET.Sdk">
                        <PropertyGroup>
                          <TargetFramework>net10.0</TargetFramework>
                        </PropertyGroup>
                      </Project>
                      """;

        var result = CentralPackageManagement.UpsertPackageReference(project, "Automation.Fallout.Components", "1.0.2");

        using (Assert.Multiple())
        {
            await Assert.That(result.Changed).IsTrue();
            await Assert.That(result.Xml).Contains("<ItemGroup>");
            await Assert.That(result.Xml).Contains("""Include="Automation.Fallout.Components" Version="1.0.2" """.TrimEnd());
        }
    }

    #endregion

    #region ReadPackageReferenceVersion

    [Test]
    public async Task ReadPackageReferenceVersion_PinnedPackage_ReturnsTheVersion()
    {
        var version = CentralPackageManagement.ReadPackageReferenceVersion(BuildProject, "Fallout.Common");

        await Assert.That(version).IsEqualTo("11.0.18");
    }

    [Test]
    public async Task ReadPackageReferenceVersion_UnreferencedPackage_ReturnsNull()
    {
        var version = CentralPackageManagement.ReadPackageReferenceVersion(BuildProject, "Automation.Fallout.Components");

        await Assert.That(version).IsNull();
    }

    #endregion
}
