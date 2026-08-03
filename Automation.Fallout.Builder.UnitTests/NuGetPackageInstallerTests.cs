using System.Xml.Linq;
using Automation.Fallout.Builder.Services;

namespace Automation.Fallout.Builder.UnitTests;

public class NuGetPackageInstallerTests : IAsyncDisposable
{
    private readonly string _testDirectory;
    private readonly List<string> _filesToCleanup;

    public NuGetPackageInstallerTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"NuGetPackageInstallerTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        _filesToCleanup = new List<string>();
    }

    public ValueTask DisposeAsync()
    {
        foreach (var file in _filesToCleanup)
        {
            if (File.Exists(file))
            {
                try { File.Delete(file); } catch { /* Ignore cleanup errors */ }
            }
        }

        if (Directory.Exists(_testDirectory))
        {
            try { Directory.Delete(_testDirectory, true); } catch { /* Ignore cleanup errors */ }
        }

        return ValueTask.CompletedTask;
    }

    private string CreateTestProjectFile(string targetFramework = "net8.0")
    {
        var projectPath = Path.Combine(_testDirectory, $"test_{Guid.NewGuid()}.csproj");
        var content = $@"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>{targetFramework}</TargetFramework>
  </PropertyGroup>
</Project>";
        File.WriteAllText(projectPath, content);
        _filesToCleanup.Add(projectPath);
        return projectPath;
    }

    #region UpgradeProjectTargetFrameworkAsync Tests

    [Test]
    public async Task UpgradeProjectTargetFrameworkAsync_ValidProject_UpgradesFramework()
    {
        var projectPath = CreateTestProjectFile("net8.0");

        var result = await NuGetPackageInstaller.UpgradeProjectTargetFrameworkAsync(projectPath, "net10.0");

        var doc = XDocument.Load(projectPath);
        var targetFramework = doc.Descendants("TargetFramework").First().Value;
        using (Assert.Multiple())
        {
            await Assert.That(result).IsTrue();
            await Assert.That(targetFramework).IsEqualTo("net10.0");
        }
    }

    [Test]
    public async Task UpgradeProjectTargetFrameworkAsync_AlreadyCorrectFramework_ReturnsTrue()
    {
        var projectPath = CreateTestProjectFile("net10.0");

        var result = await NuGetPackageInstaller.UpgradeProjectTargetFrameworkAsync(projectPath, "net10.0");

        var doc = XDocument.Load(projectPath);
        var targetFramework = doc.Descendants("TargetFramework").First().Value;
        using (Assert.Multiple())
        {
            await Assert.That(result).IsTrue();
            await Assert.That(targetFramework).IsEqualTo("net10.0");
        }
    }

    [Test]
    public async Task UpgradeProjectTargetFrameworkAsync_CustomFramework_UpgradesCorrectly()
    {
        var projectPath = CreateTestProjectFile("net6.0");

        var result = await NuGetPackageInstaller.UpgradeProjectTargetFrameworkAsync(projectPath, "net9.0");

        var doc = XDocument.Load(projectPath);
        var targetFramework = doc.Descendants("TargetFramework").First().Value;
        using (Assert.Multiple())
        {
            await Assert.That(result).IsTrue();
            await Assert.That(targetFramework).IsEqualTo("net9.0");
        }
    }

    [Test]
    public async Task UpgradeProjectTargetFrameworkAsync_NonExistentFile_ReturnsFalse()
    {
        var projectPath = Path.Combine(_testDirectory, "nonexistent.csproj");

        var result = await NuGetPackageInstaller.UpgradeProjectTargetFrameworkAsync(projectPath);

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task UpgradeProjectTargetFrameworkAsync_InvalidXml_ReturnsFalse()
    {
        var projectPath = Path.Combine(_testDirectory, $"invalid_{Guid.NewGuid()}.csproj");
        File.WriteAllText(projectPath, "This is not valid XML");
        _filesToCleanup.Add(projectPath);

        var result = await NuGetPackageInstaller.UpgradeProjectTargetFrameworkAsync(projectPath);

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task UpgradeProjectTargetFrameworkAsync_MissingTargetFramework_ReturnsFalse()
    {
        var projectPath = Path.Combine(_testDirectory, $"noframework_{Guid.NewGuid()}.csproj");
        var content = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
  </PropertyGroup>
</Project>";
        File.WriteAllText(projectPath, content);
        _filesToCleanup.Add(projectPath);

        var result = await NuGetPackageInstaller.UpgradeProjectTargetFrameworkAsync(projectPath);

        await Assert.That(result).IsFalse();
    }

    #endregion

    #region AddPackageDownloadAsync Tests

    [Test]
    public async Task AddPackageDownloadAsync_NewPackage_AddsSuccessfully()
    {
        var projectPath = CreateTestProjectFile();

        var result = await NuGetPackageInstaller.AddPackageDownloadAsync(projectPath, "TestPackage", "1.0.0");

        var doc = XDocument.Load(projectPath);
        var packageDownload = doc.Descendants("PackageDownload")
            .FirstOrDefault(p => p.Attribute("Include")?.Value == "TestPackage");
        using (Assert.Multiple())
        {
            await Assert.That(result).IsTrue();
            await Assert.That(packageDownload).IsNotNull();
            await Assert.That(packageDownload!.Attribute("Version")?.Value).IsEqualTo("[1.0.0]");
        }
    }

    [Test]
    public async Task AddPackageDownloadAsync_ExistingPackageSameVersion_ReturnsTrue()
    {
        var projectPath = CreateTestProjectFile();
        await NuGetPackageInstaller.AddPackageDownloadAsync(projectPath, "TestPackage", "1.0.0");

        var result = await NuGetPackageInstaller.AddPackageDownloadAsync(projectPath, "TestPackage", "1.0.0");

        var doc = XDocument.Load(projectPath);
        var packageDownloads = doc.Descendants("PackageDownload")
            .Where(p => p.Attribute("Include")?.Value == "TestPackage")
            .ToList();
        using (Assert.Multiple())
        {
            await Assert.That(result).IsTrue();
            await Assert.That(packageDownloads).HasSingleItem();
        }
    }

    [Test]
    public async Task AddPackageDownloadAsync_ExistingPackageDifferentVersion_UpdatesVersion()
    {
        var projectPath = CreateTestProjectFile();
        await NuGetPackageInstaller.AddPackageDownloadAsync(projectPath, "TestPackage", "1.0.0");

        var result = await NuGetPackageInstaller.AddPackageDownloadAsync(projectPath, "TestPackage", "2.0.0");

        var doc = XDocument.Load(projectPath);
        var packageDownload = doc.Descendants("PackageDownload")
            .FirstOrDefault(p => p.Attribute("Include")?.Value == "TestPackage");
        using (Assert.Multiple())
        {
            await Assert.That(result).IsTrue();
            await Assert.That(packageDownload).IsNotNull();
            await Assert.That(packageDownload!.Attribute("Version")?.Value).IsEqualTo("[2.0.0]");
        }
    }

    [Test]
    public async Task AddPackageDownloadAsync_MultiplePackages_AddsAll()
    {
        var projectPath = CreateTestProjectFile();

        await NuGetPackageInstaller.AddPackageDownloadAsync(projectPath, "Package1", "1.0.0");
        await NuGetPackageInstaller.AddPackageDownloadAsync(projectPath, "Package2", "2.0.0");
        var result = await NuGetPackageInstaller.AddPackageDownloadAsync(projectPath, "Package3", "3.0.0");

        var doc = XDocument.Load(projectPath);
        var packageDownloads = doc.Descendants("PackageDownload").ToList();
        using (Assert.Multiple())
        {
            await Assert.That(result).IsTrue();
            await Assert.That(packageDownloads.Count).IsEqualTo(3);
            await Assert.That(packageDownloads.Any(p => p.Attribute("Include")?.Value == "Package1")).IsTrue();
            await Assert.That(packageDownloads.Any(p => p.Attribute("Include")?.Value == "Package2")).IsTrue();
            await Assert.That(packageDownloads.Any(p => p.Attribute("Include")?.Value == "Package3")).IsTrue();
        }
    }

    [Test]
    public async Task AddPackageDownloadAsync_NonExistentFile_ReturnsFalse()
    {
        var projectPath = Path.Combine(_testDirectory, "nonexistent.csproj");

        var result = await NuGetPackageInstaller.AddPackageDownloadAsync(projectPath, "TestPackage", "1.0.0");

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task AddPackageDownloadAsync_InvalidXml_ReturnsFalse()
    {
        var projectPath = Path.Combine(_testDirectory, $"invalid_{Guid.NewGuid()}.csproj");
        File.WriteAllText(projectPath, "This is not valid XML");
        _filesToCleanup.Add(projectPath);

        var result = await NuGetPackageInstaller.AddPackageDownloadAsync(projectPath, "TestPackage", "1.0.0");

        await Assert.That(result).IsFalse();
    }

    #endregion

    #region DeleteFileIfExistsAsync Tests

    [Test]
    public async Task DeleteFileIfExistsAsync_ExistingFile_DeletesSuccessfully()
    {
        var filePath = Path.Combine(_testDirectory, $"testfile_{Guid.NewGuid()}.txt");
        File.WriteAllText(filePath, "test content");

        await NuGetPackageInstaller.DeleteFileIfExistsAsync(filePath);

        await Assert.That(File.Exists(filePath)).IsFalse();
    }

    [Test]
    public async Task DeleteFileIfExistsAsync_NonExistentFile_DoesNotThrow()
    {
        var filePath = Path.Combine(_testDirectory, "nonexistent.txt");

        await NuGetPackageInstaller.DeleteFileIfExistsAsync(filePath);

        await Assert.That(File.Exists(filePath)).IsFalse();
    }

    #endregion

    #region UpdateGitIgnoreAsync Tests

    [Test]
    public async Task UpdateGitIgnoreAsync_NoExistingFile_CreatesWithAllSections()
    {
        var gitignorePath = Path.Combine(_testDirectory, ".gitignore");
        _filesToCleanup.Add(gitignorePath);

        await NuGetPackageInstaller.UpdateGitIgnoreAsync(_testDirectory);

        var content = await File.ReadAllTextAsync(gitignorePath);
        using (Assert.Multiple())
        {
            await Assert.That(File.Exists(gitignorePath)).IsTrue();
            await Assert.That(content).Contains("# Fallout Build");
            await Assert.That(content).Contains(".fallout/build.schema.json");
            await Assert.That(content).Contains(".fallout/temp/");
            await Assert.That(content).Contains(".tmp/");
            await Assert.That(content).Contains("artifacts/");
            await Assert.That(content).Contains("# JetBrains Rider");
            await Assert.That(content).Contains(".idea/");
            await Assert.That(content).Contains("# Rider DotSettings");
            await Assert.That(content).Contains("*.DotSettings");
        }
    }

    [Test]
    public async Task UpdateGitIgnoreAsync_ExistingFileWithoutSections_AppendsAllSections()
    {
        var gitignorePath = Path.Combine(_testDirectory, ".gitignore");
        await File.WriteAllTextAsync(gitignorePath, "bin/\nobj/\n");
        _filesToCleanup.Add(gitignorePath);

        await NuGetPackageInstaller.UpdateGitIgnoreAsync(_testDirectory);

        var content = await File.ReadAllTextAsync(gitignorePath);
        using (Assert.Multiple())
        {
            await Assert.That(content).Contains("bin/");
            await Assert.That(content).Contains("obj/");
            await Assert.That(content).Contains("# Fallout Build");
            await Assert.That(content).Contains("# JetBrains Rider");
            await Assert.That(content).Contains("# Rider DotSettings");
        }
    }

    [Test]
    public async Task UpdateGitIgnoreAsync_ExistingFileWithFalloutSection_DoesNotDuplicateFallout()
    {
        var gitignorePath = Path.Combine(_testDirectory, ".gitignore");
        await File.WriteAllTextAsync(gitignorePath, "# Fallout Build\n.fallout/build.schema.json\n");
        _filesToCleanup.Add(gitignorePath);

        await NuGetPackageInstaller.UpdateGitIgnoreAsync(_testDirectory);

        var content = await File.ReadAllTextAsync(gitignorePath);
        var falloutCount = System.Text.RegularExpressions.Regex.Matches(content, "# Fallout Build").Count;
        using (Assert.Multiple())
        {
            await Assert.That(falloutCount).IsEqualTo(1);
            await Assert.That(content).Contains("# JetBrains Rider");
            await Assert.That(content).Contains("# Rider DotSettings");
        }
    }

    [Test]
    public async Task UpdateGitIgnoreAsync_ExistingFileWithRiderSection_DoesNotDuplicateRider()
    {
        var gitignorePath = Path.Combine(_testDirectory, ".gitignore");
        await File.WriteAllTextAsync(gitignorePath, "# JetBrains Rider\n.idea/\n");
        _filesToCleanup.Add(gitignorePath);

        await NuGetPackageInstaller.UpdateGitIgnoreAsync(_testDirectory);

        var content = await File.ReadAllTextAsync(gitignorePath);
        var riderCount = System.Text.RegularExpressions.Regex.Matches(content, "# JetBrains Rider").Count;
        using (Assert.Multiple())
        {
            await Assert.That(riderCount).IsEqualTo(1);
            await Assert.That(content).Contains("# Fallout Build");
            await Assert.That(content).Contains("# Rider DotSettings");
        }
    }

    [Test]
    public async Task UpdateGitIgnoreAsync_ExistingFileWithDotSettingsSection_DoesNotDuplicateDotSettings()
    {
        var gitignorePath = Path.Combine(_testDirectory, ".gitignore");
        await File.WriteAllTextAsync(gitignorePath, "# Rider DotSettings\n*.DotSettings\n");
        _filesToCleanup.Add(gitignorePath);

        await NuGetPackageInstaller.UpdateGitIgnoreAsync(_testDirectory);

        var content = await File.ReadAllTextAsync(gitignorePath);
        var dotSettingsCount = System.Text.RegularExpressions.Regex.Matches(content, "# Rider DotSettings").Count;
        using (Assert.Multiple())
        {
            await Assert.That(dotSettingsCount).IsEqualTo(1);
            await Assert.That(content).Contains("# Fallout Build");
            await Assert.That(content).Contains("# JetBrains Rider");
        }
    }

    [Test]
    public async Task UpdateGitIgnoreAsync_ExistingFileWithAllSections_DoesNotModify()
    {
        var gitignorePath = Path.Combine(_testDirectory, ".gitignore");
        var originalContent = "# Fallout Build\n.fallout/\n# JetBrains Rider\n.idea/\n# Rider DotSettings\n*.DotSettings\n";
        await File.WriteAllTextAsync(gitignorePath, originalContent);
        _filesToCleanup.Add(gitignorePath);

        await NuGetPackageInstaller.UpdateGitIgnoreAsync(_testDirectory);

        var content = await File.ReadAllTextAsync(gitignorePath);
        await Assert.That(content).IsEqualTo(originalContent);
    }

    [Test]
    public async Task UpdateGitIgnoreAsync_FileWithoutTrailingNewline_AddsNewlineBeforeAppending()
    {
        var gitignorePath = Path.Combine(_testDirectory, ".gitignore");
        await File.WriteAllTextAsync(gitignorePath, "bin/");
        _filesToCleanup.Add(gitignorePath);

        await NuGetPackageInstaller.UpdateGitIgnoreAsync(_testDirectory);

        var content = await File.ReadAllTextAsync(gitignorePath);
        var lines = content.Split('\n');
        using (Assert.Multiple())
        {
            await Assert.That(content).StartsWith("bin/");
            await Assert.That(content).Contains("# Fallout Build");
            await Assert.That(lines.Length > 1).IsTrue();
        }
    }

    #endregion

    #region Edge Cases and Validation Tests

    [Test]
    public async Task AddPackageDownloadAsync_EmptyProjectFile_CreatesItemGroup()
    {
        var projectPath = Path.Combine(_testDirectory, $"empty_{Guid.NewGuid()}.csproj");
        File.WriteAllText(projectPath, "<Project />");
        _filesToCleanup.Add(projectPath);

        var result = await NuGetPackageInstaller.AddPackageDownloadAsync(projectPath, "TestPackage", "1.0.0");

        var doc = XDocument.Load(projectPath);
        var itemGroup = doc.Descendants("ItemGroup").FirstOrDefault();
        var packageDownload = itemGroup!.Elements("PackageDownload").FirstOrDefault();
        using (Assert.Multiple())
        {
            await Assert.That(result).IsTrue();
            await Assert.That(itemGroup).IsNotNull();
            await Assert.That(packageDownload).IsNotNull();
        }
    }

    [Test]
    public async Task UpgradeProjectTargetFrameworkAsync_PreservesOtherElements()
    {
        var projectPath = Path.Combine(_testDirectory, $"complex_{Guid.NewGuid()}.csproj");
        var content = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <OutputType>Exe</OutputType>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include=""SomePackage"" Version=""1.0.0"" />
  </ItemGroup>
</Project>";
        File.WriteAllText(projectPath, content);
        _filesToCleanup.Add(projectPath);

        var result = await NuGetPackageInstaller.UpgradeProjectTargetFrameworkAsync(projectPath, "net10.0");

        var doc = XDocument.Load(projectPath);
        using (Assert.Multiple())
        {
            await Assert.That(result).IsTrue();
            await Assert.That(doc.Descendants("TargetFramework").First().Value).IsEqualTo("net10.0");
            await Assert.That(doc.Descendants("OutputType").First().Value).IsEqualTo("Exe");
            await Assert.That(doc.Descendants("Nullable").First().Value).IsEqualTo("enable");
            await Assert.That(doc.Descendants("PackageReference").FirstOrDefault()).IsNotNull();
        }
    }

    [Test]
    public async Task AddPackageDownloadAsync_PreservesExistingItemGroups()
    {
        var projectPath = Path.Combine(_testDirectory, $"withitems_{Guid.NewGuid()}.csproj");
        var content = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include=""SomePackage"" Version=""1.0.0"" />
  </ItemGroup>
</Project>";
        File.WriteAllText(projectPath, content);
        _filesToCleanup.Add(projectPath);

        var result = await NuGetPackageInstaller.AddPackageDownloadAsync(projectPath, "TestPackage", "1.0.0");

        var doc = XDocument.Load(projectPath);
        var itemGroups = doc.Descendants("ItemGroup").ToList();
        using (Assert.Multiple())
        {
            await Assert.That(result).IsTrue();
            await Assert.That(itemGroups.Count >= 1).IsTrue();
            await Assert.That(doc.Descendants("PackageReference").FirstOrDefault()).IsNotNull();
            await Assert.That(doc.Descendants("PackageDownload").FirstOrDefault()).IsNotNull();
        }
    }

    #endregion
}
