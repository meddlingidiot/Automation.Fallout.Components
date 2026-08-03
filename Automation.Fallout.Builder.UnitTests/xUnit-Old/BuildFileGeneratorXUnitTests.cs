// using Automation.Fallout.Builder.Models;
// using Automation.Fallout.Builder.Services;
//
// namespace Automation.Fallout.Builder.UnitTests;
//
// public class BuildFileGeneratorXUnitTests
// {
//     [Fact]
//     public void GenerateBuildFile_CompileBuild_GeneratesCorrectOutput()
//     {
//         // Arrange
//         var config = new BuildConfiguration
//         {
//             BuildType = "CompileBuild"
//         };
//         var buildInfo = new DefaultBuildInfo
//         {
//             Name = "CompileBuild",
//             Description = "Compile only build",
//             RequiresTests = false,
//             RequiresPackaging = false,
//             RequiresVelopack = false
//         };
//
//         // Act
//         var result = BuildFileGenerator.GenerateBuildFile(config, buildInfo);
//
//         // Assert
//         Assert.Contains("using Fallout.Common;", result);
//         Assert.Contains("using Automation.Fallout.Components;", result);
//         Assert.Contains("public class Build : AzurePipelinesBuild, IShowVersion, IClean, ICompile, IRestore, IScanForSecrets", result);
//         Assert.Contains("public static int Main() => Execute<Build>(", result);
//         Assert.Contains("x => ((ICompile)x).Compile);", result);
//     }
//
//     [Fact]
//     public void GenerateBuildFile_TestBuild_GeneratesCorrectOutput()
//     {
//         // Arrange
//         var config = new BuildConfiguration
//         {
//             BuildType = "TestBuild"
//         };
//         var buildInfo = new DefaultBuildInfo
//         {
//             Name = "TestBuild",
//             Description = "Test build",
//             RequiresTests = true,
//             RequiresPackaging = false,
//             RequiresVelopack = false
//         };
//
//         // Act
//         var result = BuildFileGenerator.GenerateBuildFile(config, buildInfo);
//
//         // Assert
//         Assert.Contains("IShowVersion, IClean, ICompile, IRestore, IScanForSecrets, IRunUnitTests", result);
//         Assert.Contains("IRunIntegrationTests, IGenerateCoverageReport, ITest", result);
//         Assert.Contains("x => ((ITest)x).Test);", result);
//     }
//
//     [Fact]
//     public void GenerateBuildFile_PackageBuild_GeneratesCorrectOutput()
//     {
//         // Arrange
//         var config = new BuildConfiguration
//         {
//             BuildType = "PackageBuild"
//         };
//         var buildInfo = new DefaultBuildInfo
//         {
//             Name = "PackageBuild",
//             Description = "Package build",
//             RequiresTests = true,
//             RequiresPackaging = true,
//             RequiresVelopack = false
//         };
//
//         // Act
//         var result = BuildFileGenerator.GenerateBuildFile(config, buildInfo);
//
//         // Assert
//         Assert.Contains("IUpdateChangelog, IPackage, ITagRelease, IAnnounceRelease", result);
//         Assert.Contains("x => ((IPackage)x).ReleasePackage);", result);
//     }
//
//     [Fact]
//     public void GenerateBuildFile_VelopackBuild_GeneratesCorrectOutput()
//     {
//         // Arrange
//         var config = new BuildConfiguration
//         {
//             BuildType = "VelopackBuild",
//             VelopackProjectName = "MyApp"
//         };
//         var buildInfo = new DefaultBuildInfo
//         {
//             Name = "VelopackBuild",
//             Description = "Velopack build",
//             RequiresTests = true,
//             RequiresPackaging = false,
//             RequiresVelopack = true
//         };
//
//         // Act
//         var result = BuildFileGenerator.GenerateBuildFile(config, buildInfo);
//
//         // Assert
//         Assert.Contains("IUpdateChangelog, IVelopack, ITagRelease, IAnnounceRelease", result);
//         Assert.Contains("y => ((IVelopack)y).ReleaseVelopack);", result);
//         Assert.Contains("string IHasVelopack.VelopackProjectName => \"MyApp\";", result);
//     }
//
//     [Fact]
//     public void GenerateBuildFile_VelopackBuild_WithIcon_GeneratesCorrectOutput()
//     {
//         // Arrange
//         var config = new BuildConfiguration
//         {
//             BuildType = "VelopackBuild",
//             VelopackProjectName = "MyApp",
//             VelopackIconPath = "icon.ico"
//         };
//         var buildInfo = new DefaultBuildInfo
//         {
//             Name = "VelopackBuild",
//             Description = "Velopack build",
//             RequiresTests = true,
//             RequiresPackaging = false,
//             RequiresVelopack = true
//         };
//
//         // Act
//         var result = BuildFileGenerator.GenerateBuildFile(config, buildInfo);
//
//         // Assert
//         Assert.Contains("string IHasVelopack.VelopackProjectName => \"MyApp\";", result);
//         Assert.Contains("string IHasVelopack.VelopackIconPath => @\"icon.ico\";", result);
//     }
//
//     [Fact]
//     public void GenerateBuildFile_PackageAndVelopackBuild_GeneratesCorrectOutput()
//     {
//         // Arrange
//         var config = new BuildConfiguration
//         {
//             BuildType = "PackageAndVelopackBuild",
//             VelopackProjectName = "MyApp"
//         };
//         var buildInfo = new DefaultBuildInfo
//         {
//             Name = "PackageAndVelopackBuild",
//             Description = "Package and Velopack build",
//             RequiresTests = true,
//             RequiresPackaging = true,
//             RequiresVelopack = true
//         };
//
//         // Act
//         var result = BuildFileGenerator.GenerateBuildFile(config, buildInfo);
//
//         // Assert
//         Assert.Contains("IUpdateChangelog, IPackage, IVelopack, ITagRelease, IAnnounceRelease", result);
//         Assert.Contains("x => ((IPackage)x).ReleasePackage,", result);
//         Assert.Contains("y => ((IVelopack)y).ReleaseVelopack);", result);
//     }
//
//     [Fact]
//     public void GenerateBuildFile_WithCodeCoverage_GeneratesMinCoverageThreshold()
//     {
//         // Arrange
//         var config = new BuildConfiguration
//         {
//             BuildType = "TestBuild",
//             EnableCodeCoverage = true,
//             MinCodeCoverage = 80
//         };
//         var buildInfo = new DefaultBuildInfo
//         {
//             Name = "TestBuild",
//             Description = "Test build",
//             RequiresTests = true
//         };
//
//         // Act
//         var result = BuildFileGenerator.GenerateBuildFile(config, buildInfo);
//
//         // Assert
//         Assert.Contains("int IHasTests.MinCoverageThreshold => 80;", result);
//     }
//
//     [Fact]
//     public void GenerateBuildFile_WithoutCodeCoverage_DoesNotGenerateMinCoverageThreshold()
//     {
//         // Arrange
//         var config = new BuildConfiguration
//         {
//             BuildType = "TestBuild",
//             EnableCodeCoverage = false,
//             MinCodeCoverage = 0
//         };
//         var buildInfo = new DefaultBuildInfo
//         {
//             Name = "TestBuild",
//             Description = "Test build",
//             RequiresTests = true
//         };
//
//         // Act
//         var result = BuildFileGenerator.GenerateBuildFile(config, buildInfo);
//
//         // Assert
//         Assert.DoesNotContain("int IHasTests.MinCoverageThreshold", result);
//     }
//
//     [Fact]
//     public void GenerateBuildFile_BreakBuildOnWarningsFalse_GeneratesOverride()
//     {
//         // Arrange
//         var config = new BuildConfiguration
//         {
//             BuildType = "TestBuild",
//             BreakBuildOnWarnings = false
//         };
//         var buildInfo = new DefaultBuildInfo
//         {
//             Name = "TestBuild",
//             Description = "Test build",
//             RequiresTests = true
//         };
//
//         // Act
//         var result = BuildFileGenerator.GenerateBuildFile(config, buildInfo);
//
//         // Assert
//         Assert.Contains("bool IHasTests.BreakBuildOnWarnings => false;", result);
//     }
//
//     [Fact]
//     public void GenerateBuildFile_BreakBuildOnWarningsTrue_DoesNotGenerateOverride()
//     {
//         // Arrange
//         var config = new BuildConfiguration
//         {
//             BuildType = "TestBuild",
//             BreakBuildOnWarnings = true
//         };
//         var buildInfo = new DefaultBuildInfo
//         {
//             Name = "TestBuild",
//             Description = "Test build",
//             RequiresTests = true
//         };
//
//         // Act
//         var result = BuildFileGenerator.GenerateBuildFile(config, buildInfo);
//
//         // Assert
//         Assert.DoesNotContain("bool IHasTests.BreakBuildOnWarnings => false;", result);
//     }
//
//     [Fact]
//     public void GenerateBuildFile_BreakBuildOnSecretLeaksFalse_GeneratesOverride()
//     {
//         // Arrange
//         var config = new BuildConfiguration
//         {
//             BuildType = "TestBuild",
//             BreakBuildOnSecretLeaks = false
//         };
//         var buildInfo = new DefaultBuildInfo
//         {
//             Name = "TestBuild",
//             Description = "Test build",
//             RequiresTests = true
//         };
//
//         // Act
//         var result = BuildFileGenerator.GenerateBuildFile(config, buildInfo);
//
//         // Assert
//         Assert.Contains("bool IHasTests.BreakBuildOnSecretLeaks => false;", result);
//     }
//
//     [Fact]
//     public void GenerateBuildFile_BreakBuildOnSecretLeaksTrue_DoesNotGenerateOverride()
//     {
//         // Arrange
//         var config = new BuildConfiguration
//         {
//             BuildType = "TestBuild",
//             BreakBuildOnSecretLeaks = true
//         };
//         var buildInfo = new DefaultBuildInfo
//         {
//             Name = "TestBuild",
//             Description = "Test build",
//             RequiresTests = true
//         };
//
//         // Act
//         var result = BuildFileGenerator.GenerateBuildFile(config, buildInfo);
//
//         // Assert
//         Assert.DoesNotContain("bool IHasTests.BreakBuildOnSecretLeaks => false;", result);
//     }
//
//     [Fact]
//     public void GenerateBuildFile_ContainsXmlDocComments()
//     {
//         // Arrange
//         var config = new BuildConfiguration
//         {
//             BuildType = "CompileBuild"
//         };
//         var buildInfo = new DefaultBuildInfo
//         {
//             Name = "CompileBuild",
//             Description = "Compile only build"
//         };
//
//         // Act
//         var result = BuildFileGenerator.GenerateBuildFile(config, buildInfo);
//
//         // Assert
//         Assert.Contains("/// <summary>", result);
//         Assert.Contains("/// Build configuration for CompileBuild", result);
//         Assert.Contains("/// </summary>", result);
//         Assert.Contains("/// Support plugins are available for:", result);
//     }
//
//     [Fact]
//     public void GenerateBuildFile_UnknownBuildType_GeneratesEmptyInterfaces()
//     {
//         // Arrange
//         var config = new BuildConfiguration
//         {
//             BuildType = "UnknownBuild"
//         };
//         var buildInfo = new DefaultBuildInfo
//         {
//             Name = "UnknownBuild",
//             Description = "Unknown build"
//         };
//
//         // Act
//         var result = BuildFileGenerator.GenerateBuildFile(config, buildInfo);
//
//         // Assert
//         Assert.Contains("public class Build : AzurePipelinesBuild, ", result);
//         Assert.DoesNotContain("ICompile", result);
//         Assert.DoesNotContain("ITest", result);
//     }
//
//     [Fact]
//     public void GenerateBuildFile_WithZeroMinCodeCoverage_DoesNotGenerateThreshold()
//     {
//         // Arrange
//         var config = new BuildConfiguration
//         {
//             BuildType = "TestBuild",
//             EnableCodeCoverage = true,
//             MinCodeCoverage = 0
//         };
//         var buildInfo = new DefaultBuildInfo
//         {
//             Name = "TestBuild",
//             Description = "Test build",
//             RequiresTests = true
//         };
//
//         // Act
//         var result = BuildFileGenerator.GenerateBuildFile(config, buildInfo);
//
//         // Assert
//         Assert.DoesNotContain("int IHasTests.MinCoverageThreshold", result);
//     }
//
//     [Fact]
//     public void GenerateBuildFile_VelopackWithoutProjectName_DoesNotGenerateVelopackProperties()
//     {
//         // Arrange
//         var config = new BuildConfiguration
//         {
//             BuildType = "VelopackBuild",
//             VelopackProjectName = null
//         };
//         var buildInfo = new DefaultBuildInfo
//         {
//             Name = "VelopackBuild",
//             Description = "Velopack build",
//             RequiresVelopack = true
//         };
//
//         // Act
//         var result = BuildFileGenerator.GenerateBuildFile(config, buildInfo);
//
//         // Assert
//         Assert.DoesNotContain("string IHasVelopack.VelopackProjectName", result);
//         Assert.DoesNotContain("string IHasVelopack.VelopackIconPath", result);
//     }
//
//     [Fact]
//     public void GenerateBuildFile_VelopackWithEmptyIconPath_DoesNotGenerateIconProperty()
//     {
//         // Arrange
//         var config = new BuildConfiguration
//         {
//             BuildType = "VelopackBuild",
//             VelopackProjectName = "MyApp",
//             VelopackIconPath = string.Empty
//         };
//         var buildInfo = new DefaultBuildInfo
//         {
//             Name = "VelopackBuild",
//             Description = "Velopack build",
//             RequiresVelopack = true
//         };
//
//         // Act
//         var result = BuildFileGenerator.GenerateBuildFile(config, buildInfo);
//
//         // Assert
//         Assert.Contains("string IHasVelopack.VelopackProjectName => \"MyApp\";", result);
//         Assert.DoesNotContain("string IHasVelopack.VelopackIconPath", result);
//     }
// }
