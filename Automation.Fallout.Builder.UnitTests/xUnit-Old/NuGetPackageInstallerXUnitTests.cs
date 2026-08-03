// using System.Xml.Linq;
// using Automation.Fallout.Builder.Services;
//
// namespace Automation.Fallout.Builder.UnitTests;
//
// public class NuGetPackageInstallerXUnitTests : IDisposable
// {
//     private readonly string _testDirectory;
//     private readonly List<string> _filesToCleanup;
//
//     public NuGetPackageInstallerXUnitTests()
//     {
//         _testDirectory = Path.Combine(Path.GetTempPath(), $"NuGetPackageInstallerTests_{Guid.NewGuid()}");
//         Directory.CreateDirectory(_testDirectory);
//         _filesToCleanup = new List<string>();
//     }
//
//     public void Dispose()
//     {
//         foreach (var file in _filesToCleanup)
//         {
//             if (File.Exists(file))
//             {
//                 try { File.Delete(file); } catch { /* Ignore cleanup errors */ }
//             }
//         }
//
//         if (Directory.Exists(_testDirectory))
//         {
//             try { Directory.Delete(_testDirectory, true); } catch { /* Ignore cleanup errors */ }
//         }
//     }
//
//     private string CreateTestProjectFile(string targetFramework = "net8.0")
//     {
//         var projectPath = Path.Combine(_testDirectory, $"test_{Guid.NewGuid()}.csproj");
//         var content = $@"<Project Sdk=""Microsoft.NET.Sdk"">
//   <PropertyGroup>
//     <TargetFramework>{targetFramework}</TargetFramework>
//   </PropertyGroup>
// </Project>";
//         File.WriteAllText(projectPath, content);
//         _filesToCleanup.Add(projectPath);
//         return projectPath;
//     }
//
//     #region UpgradeProjectTargetFrameworkAsync Tests
//
//     [Fact]
//     public async Task UpgradeProjectTargetFrameworkAsync_ValidProject_UpgradesFramework()
//     {
//         // Arrange
//         var projectPath = CreateTestProjectFile("net8.0");
//
//         // Act
//         var result = await NuGetPackageInstaller.UpgradeProjectTargetFrameworkAsync(projectPath, "net10.0");
//
//         // Assert
//         Assert.True(result);
//         var doc = XDocument.Load(projectPath);
//         var targetFramework = doc.Descendants("TargetFramework").First().Value;
//         Assert.Equal("net10.0", targetFramework);
//     }
//
//     [Fact]
//     public async Task UpgradeProjectTargetFrameworkAsync_AlreadyCorrectFramework_ReturnsTrue()
//     {
//         // Arrange
//         var projectPath = CreateTestProjectFile("net10.0");
//
//         // Act
//         var result = await NuGetPackageInstaller.UpgradeProjectTargetFrameworkAsync(projectPath, "net10.0");
//
//         // Assert
//         Assert.True(result);
//         var doc = XDocument.Load(projectPath);
//         var targetFramework = doc.Descendants("TargetFramework").First().Value;
//         Assert.Equal("net10.0", targetFramework);
//     }
//
//     [Fact]
//     public async Task UpgradeProjectTargetFrameworkAsync_CustomFramework_UpgradesCorrectly()
//     {
//         // Arrange
//         var projectPath = CreateTestProjectFile("net6.0");
//
//         // Act
//         var result = await NuGetPackageInstaller.UpgradeProjectTargetFrameworkAsync(projectPath, "net9.0");
//
//         // Assert
//         Assert.True(result);
//         var doc = XDocument.Load(projectPath);
//         var targetFramework = doc.Descendants("TargetFramework").First().Value;
//         Assert.Equal("net9.0", targetFramework);
//     }
//
//     [Fact]
//     public async Task UpgradeProjectTargetFrameworkAsync_NonExistentFile_ReturnsFalse()
//     {
//         // Arrange
//         var projectPath = Path.Combine(_testDirectory, "nonexistent.csproj");
//
//         // Act
//         var result = await NuGetPackageInstaller.UpgradeProjectTargetFrameworkAsync(projectPath);
//
//         // Assert
//         Assert.False(result);
//     }
//
//     [Fact]
//     public async Task UpgradeProjectTargetFrameworkAsync_InvalidXml_ReturnsFalse()
//     {
//         // Arrange
//         var projectPath = Path.Combine(_testDirectory, $"invalid_{Guid.NewGuid()}.csproj");
//         File.WriteAllText(projectPath, "This is not valid XML");
//         _filesToCleanup.Add(projectPath);
//
//         // Act
//         var result = await NuGetPackageInstaller.UpgradeProjectTargetFrameworkAsync(projectPath);
//
//         // Assert
//         Assert.False(result);
//     }
//
//     [Fact]
//     public async Task UpgradeProjectTargetFrameworkAsync_MissingTargetFramework_ReturnsFalse()
//     {
//         // Arrange
//         var projectPath = Path.Combine(_testDirectory, $"noframework_{Guid.NewGuid()}.csproj");
//         var content = @"<Project Sdk=""Microsoft.NET.Sdk"">
//   <PropertyGroup>
//   </PropertyGroup>
// </Project>";
//         File.WriteAllText(projectPath, content);
//         _filesToCleanup.Add(projectPath);
//
//         // Act
//         var result = await NuGetPackageInstaller.UpgradeProjectTargetFrameworkAsync(projectPath);
//
//         // Assert
//         Assert.False(result);
//     }
//
//     #endregion
//
//     #region AddPackageDownloadAsync Tests
//
//     [Fact]
//     public async Task AddPackageDownloadAsync_NewPackage_AddsSuccessfully()
//     {
//         // Arrange
//         var projectPath = CreateTestProjectFile();
//
//         // Act
//         var result = await NuGetPackageInstaller.AddPackageDownloadAsync(projectPath, "TestPackage", "1.0.0");
//
//         // Assert
//         Assert.True(result);
//         var doc = XDocument.Load(projectPath);
//         var packageDownload = doc.Descendants("PackageDownload")
//             .FirstOrDefault(p => p.Attribute("Include")?.Value == "TestPackage");
//         Assert.NotNull(packageDownload);
//         Assert.Equal("[1.0.0]", packageDownload.Attribute("Version")?.Value);
//     }
//
//     [Fact]
//     public async Task AddPackageDownloadAsync_ExistingPackageSameVersion_ReturnsTrue()
//     {
//         // Arrange
//         var projectPath = CreateTestProjectFile();
//         await NuGetPackageInstaller.AddPackageDownloadAsync(projectPath, "TestPackage", "1.0.0");
//
//         // Act
//         var result = await NuGetPackageInstaller.AddPackageDownloadAsync(projectPath, "TestPackage", "1.0.0");
//
//         // Assert
//         Assert.True(result);
//         var doc = XDocument.Load(projectPath);
//         var packageDownloads = doc.Descendants("PackageDownload")
//             .Where(p => p.Attribute("Include")?.Value == "TestPackage")
//             .ToList();
//         Assert.Single(packageDownloads);
//     }
//
//     [Fact]
//     public async Task AddPackageDownloadAsync_ExistingPackageDifferentVersion_UpdatesVersion()
//     {
//         // Arrange
//         var projectPath = CreateTestProjectFile();
//         await NuGetPackageInstaller.AddPackageDownloadAsync(projectPath, "TestPackage", "1.0.0");
//
//         // Act
//         var result = await NuGetPackageInstaller.AddPackageDownloadAsync(projectPath, "TestPackage", "2.0.0");
//
//         // Assert
//         Assert.True(result);
//         var doc = XDocument.Load(projectPath);
//         var packageDownload = doc.Descendants("PackageDownload")
//             .FirstOrDefault(p => p.Attribute("Include")?.Value == "TestPackage");
//         Assert.NotNull(packageDownload);
//         Assert.Equal("[2.0.0]", packageDownload.Attribute("Version")?.Value);
//     }
//
//     [Fact]
//     public async Task AddPackageDownloadAsync_MultiplePackages_AddsAll()
//     {
//         // Arrange
//         var projectPath = CreateTestProjectFile();
//
//         // Act
//         await NuGetPackageInstaller.AddPackageDownloadAsync(projectPath, "Package1", "1.0.0");
//         await NuGetPackageInstaller.AddPackageDownloadAsync(projectPath, "Package2", "2.0.0");
//         var result = await NuGetPackageInstaller.AddPackageDownloadAsync(projectPath, "Package3", "3.0.0");
//
//         // Assert
//         Assert.True(result);
//         var doc = XDocument.Load(projectPath);
//         var packageDownloads = doc.Descendants("PackageDownload").ToList();
//         Assert.Equal(3, packageDownloads.Count);
//         Assert.Contains(packageDownloads, p => p.Attribute("Include")?.Value == "Package1");
//         Assert.Contains(packageDownloads, p => p.Attribute("Include")?.Value == "Package2");
//         Assert.Contains(packageDownloads, p => p.Attribute("Include")?.Value == "Package3");
//     }
//
//     [Fact]
//     public async Task AddPackageDownloadAsync_NonExistentFile_ReturnsFalse()
//     {
//         // Arrange
//         var projectPath = Path.Combine(_testDirectory, "nonexistent.csproj");
//
//         // Act
//         var result = await NuGetPackageInstaller.AddPackageDownloadAsync(projectPath, "TestPackage", "1.0.0");
//
//         // Assert
//         Assert.False(result);
//     }
//
//     [Fact]
//     public async Task AddPackageDownloadAsync_InvalidXml_ReturnsFalse()
//     {
//         // Arrange
//         var projectPath = Path.Combine(_testDirectory, $"invalid_{Guid.NewGuid()}.csproj");
//         File.WriteAllText(projectPath, "This is not valid XML");
//         _filesToCleanup.Add(projectPath);
//
//         // Act
//         var result = await NuGetPackageInstaller.AddPackageDownloadAsync(projectPath, "TestPackage", "1.0.0");
//
//         // Assert
//         Assert.False(result);
//     }
//
//     #endregion
//
//     #region DeleteFileIfExistsAsync Tests
//
//     [Fact]
//     public async Task DeleteFileIfExistsAsync_ExistingFile_DeletesSuccessfully()
//     {
//         // Arrange
//         var filePath = Path.Combine(_testDirectory, $"testfile_{Guid.NewGuid()}.txt");
//         File.WriteAllText(filePath, "test content");
//
//         // Act
//         await NuGetPackageInstaller.DeleteFileIfExistsAsync(filePath);
//
//         // Assert
//         Assert.False(File.Exists(filePath));
//     }
//
//     [Fact]
//     public async Task DeleteFileIfExistsAsync_NonExistentFile_DoesNotThrow()
//     {
//         // Arrange
//         var filePath = Path.Combine(_testDirectory, "nonexistent.txt");
//
//         // Act & Assert
//         await NuGetPackageInstaller.DeleteFileIfExistsAsync(filePath);
//         Assert.False(File.Exists(filePath));
//     }
//
//     #endregion
//
//     #region UpdateGitIgnoreAsync Tests
//
//     [Fact]
//     public async Task UpdateGitIgnoreAsync_NoExistingFile_CreatesWithAllSections()
//     {
//         // Arrange
//         var gitignorePath = Path.Combine(_testDirectory, ".gitignore");
//         _filesToCleanup.Add(gitignorePath);
//
//         // Act
//         await NuGetPackageInstaller.UpdateGitIgnoreAsync(_testDirectory);
//
//         // Assert
//         Assert.True(File.Exists(gitignorePath));
//         var content = await File.ReadAllTextAsync(gitignorePath);
//         Assert.Contains("# Nuke Build", content);
//         Assert.Contains(".nuke/build.schema.json", content);
//         Assert.Contains(".nuke/temp/", content);
//         Assert.Contains(".tmp/", content);
//         Assert.Contains("artifacts/", content);
//         Assert.Contains("# JetBrains Rider", content);
//         Assert.Contains(".idea/", content);
//         Assert.Contains("# Rider DotSettings", content);
//         Assert.Contains("*.DotSettings", content);
//     }
//
//     [Fact]
//     public async Task UpdateGitIgnoreAsync_ExistingFileWithoutSections_AppendsAllSections()
//     {
//         // Arrange
//         var gitignorePath = Path.Combine(_testDirectory, ".gitignore");
//         await File.WriteAllTextAsync(gitignorePath, "bin/\nobj/\n");
//         _filesToCleanup.Add(gitignorePath);
//
//         // Act
//         await NuGetPackageInstaller.UpdateGitIgnoreAsync(_testDirectory);
//
//         // Assert
//         var content = await File.ReadAllTextAsync(gitignorePath);
//         Assert.Contains("bin/", content);
//         Assert.Contains("obj/", content);
//         Assert.Contains("# Nuke Build", content);
//         Assert.Contains("# JetBrains Rider", content);
//         Assert.Contains("# Rider DotSettings", content);
//     }
//
//     [Fact]
//     public async Task UpdateGitIgnoreAsync_ExistingFileWithNukeSection_DoesNotDuplicateNuke()
//     {
//         // Arrange
//         var gitignorePath = Path.Combine(_testDirectory, ".gitignore");
//         await File.WriteAllTextAsync(gitignorePath, "# Nuke Build\n.nuke/build.schema.json\n");
//         _filesToCleanup.Add(gitignorePath);
//
//         // Act
//         await NuGetPackageInstaller.UpdateGitIgnoreAsync(_testDirectory);
//
//         // Assert
//         var content = await File.ReadAllTextAsync(gitignorePath);
//         var nukeCount = System.Text.RegularExpressions.Regex.Matches(content, "# Nuke Build").Count;
//         Assert.Equal(1, nukeCount);
//         Assert.Contains("# JetBrains Rider", content);
//         Assert.Contains("# Rider DotSettings", content);
//     }
//
//     [Fact]
//     public async Task UpdateGitIgnoreAsync_ExistingFileWithRiderSection_DoesNotDuplicateRider()
//     {
//         // Arrange
//         var gitignorePath = Path.Combine(_testDirectory, ".gitignore");
//         await File.WriteAllTextAsync(gitignorePath, "# JetBrains Rider\n.idea/\n");
//         _filesToCleanup.Add(gitignorePath);
//
//         // Act
//         await NuGetPackageInstaller.UpdateGitIgnoreAsync(_testDirectory);
//
//         // Assert
//         var content = await File.ReadAllTextAsync(gitignorePath);
//         var riderCount = System.Text.RegularExpressions.Regex.Matches(content, "# JetBrains Rider").Count;
//         Assert.Equal(1, riderCount);
//         Assert.Contains("# Nuke Build", content);
//         Assert.Contains("# Rider DotSettings", content);
//     }
//
//     [Fact]
//     public async Task UpdateGitIgnoreAsync_ExistingFileWithDotSettingsSection_DoesNotDuplicateDotSettings()
//     {
//         // Arrange
//         var gitignorePath = Path.Combine(_testDirectory, ".gitignore");
//         await File.WriteAllTextAsync(gitignorePath, "# Rider DotSettings\n*.DotSettings\n");
//         _filesToCleanup.Add(gitignorePath);
//
//         // Act
//         await NuGetPackageInstaller.UpdateGitIgnoreAsync(_testDirectory);
//
//         // Assert
//         var content = await File.ReadAllTextAsync(gitignorePath);
//         var dotSettingsCount = System.Text.RegularExpressions.Regex.Matches(content, "# Rider DotSettings").Count;
//         Assert.Equal(1, dotSettingsCount);
//         Assert.Contains("# Nuke Build", content);
//         Assert.Contains("# JetBrains Rider", content);
//     }
//
//     [Fact]
//     public async Task UpdateGitIgnoreAsync_ExistingFileWithAllSections_DoesNotModify()
//     {
//         // Arrange
//         var gitignorePath = Path.Combine(_testDirectory, ".gitignore");
//         var originalContent = "# Nuke Build\n.nuke/\n# JetBrains Rider\n.idea/\n# Rider DotSettings\n*.DotSettings\n";
//         await File.WriteAllTextAsync(gitignorePath, originalContent);
//         _filesToCleanup.Add(gitignorePath);
//
//         // Act
//         await NuGetPackageInstaller.UpdateGitIgnoreAsync(_testDirectory);
//
//         // Assert
//         var content = await File.ReadAllTextAsync(gitignorePath);
//         Assert.Equal(originalContent, content);
//     }
//
//     [Fact]
//     public async Task UpdateGitIgnoreAsync_FileWithoutTrailingNewline_AddsNewlineBeforeAppending()
//     {
//         // Arrange
//         var gitignorePath = Path.Combine(_testDirectory, ".gitignore");
//         await File.WriteAllTextAsync(gitignorePath, "bin/");
//         _filesToCleanup.Add(gitignorePath);
//
//         // Act
//         await NuGetPackageInstaller.UpdateGitIgnoreAsync(_testDirectory);
//
//         // Assert
//         var content = await File.ReadAllTextAsync(gitignorePath);
//         Assert.StartsWith("bin/", content);
//         Assert.Contains("# Nuke Build", content);
//         // Ensure proper spacing
//         var lines = content.Split('\n');
//         Assert.True(lines.Length > 1);
//     }
//
//     #endregion
//
//     #region Edge Cases and Validation Tests
//
//     [Fact]
//     public async Task AddPackageDownloadAsync_EmptyProjectFile_CreatesItemGroup()
//     {
//         // Arrange
//         var projectPath = Path.Combine(_testDirectory, $"empty_{Guid.NewGuid()}.csproj");
//         File.WriteAllText(projectPath, "<Project />");
//         _filesToCleanup.Add(projectPath);
//
//         // Act
//         var result = await NuGetPackageInstaller.AddPackageDownloadAsync(projectPath, "TestPackage", "1.0.0");
//
//         // Assert
//         Assert.True(result);
//         var doc = XDocument.Load(projectPath);
//         var itemGroup = doc.Descendants("ItemGroup").FirstOrDefault();
//         Assert.NotNull(itemGroup);
//         var packageDownload = itemGroup.Elements("PackageDownload").FirstOrDefault();
//         Assert.NotNull(packageDownload);
//     }
//
//     [Fact]
//     public async Task UpgradeProjectTargetFrameworkAsync_PreservesOtherElements()
//     {
//         // Arrange
//         var projectPath = Path.Combine(_testDirectory, $"complex_{Guid.NewGuid()}.csproj");
//         var content = @"<Project Sdk=""Microsoft.NET.Sdk"">
//   <PropertyGroup>
//     <TargetFramework>net8.0</TargetFramework>
//     <OutputType>Exe</OutputType>
//     <Nullable>enable</Nullable>
//   </PropertyGroup>
//   <ItemGroup>
//     <PackageReference Include=""SomePackage"" Version=""1.0.0"" />
//   </ItemGroup>
// </Project>";
//         File.WriteAllText(projectPath, content);
//         _filesToCleanup.Add(projectPath);
//
//         // Act
//         var result = await NuGetPackageInstaller.UpgradeProjectTargetFrameworkAsync(projectPath, "net10.0");
//
//         // Assert
//         Assert.True(result);
//         var doc = XDocument.Load(projectPath);
//         Assert.Equal("net10.0", doc.Descendants("TargetFramework").First().Value);
//         Assert.Equal("Exe", doc.Descendants("OutputType").First().Value);
//         Assert.Equal("enable", doc.Descendants("Nullable").First().Value);
//         Assert.NotNull(doc.Descendants("PackageReference").FirstOrDefault());
//     }
//
//     [Fact]
//     public async Task AddPackageDownloadAsync_PreservesExistingItemGroups()
//     {
//         // Arrange
//         var projectPath = Path.Combine(_testDirectory, $"withitems_{Guid.NewGuid()}.csproj");
//         var content = @"<Project Sdk=""Microsoft.NET.Sdk"">
//   <PropertyGroup>
//     <TargetFramework>net8.0</TargetFramework>
//   </PropertyGroup>
//   <ItemGroup>
//     <PackageReference Include=""SomePackage"" Version=""1.0.0"" />
//   </ItemGroup>
// </Project>";
//         File.WriteAllText(projectPath, content);
//         _filesToCleanup.Add(projectPath);
//
//         // Act
//         var result = await NuGetPackageInstaller.AddPackageDownloadAsync(projectPath, "TestPackage", "1.0.0");
//
//         // Assert
//         Assert.True(result);
//         var doc = XDocument.Load(projectPath);
//         var itemGroups = doc.Descendants("ItemGroup").ToList();
//         Assert.True(itemGroups.Count >= 1);
//         Assert.NotNull(doc.Descendants("PackageReference").FirstOrDefault());
//         Assert.NotNull(doc.Descendants("PackageDownload").FirstOrDefault());
//     }
//
//     #endregion
// }
