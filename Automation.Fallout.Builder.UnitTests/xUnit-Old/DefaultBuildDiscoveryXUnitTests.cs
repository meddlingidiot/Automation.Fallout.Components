// using Automation.Fallout.Builder.Services;
//
// namespace Automation.Fallout.Builder.UnitTests;
//
// public class DefaultBuildDiscoveryXUnitTests
// {
//     [Fact]
//     public void GetAvailableBuilds_ReturnsNonEmptyList()
//     {
//         // Act
//         var builds = DefaultBuildDiscovery.GetAvailableBuilds();
//
//         // Assert
//         Assert.NotNull(builds);
//         Assert.NotEmpty(builds);
//     }
//
//     [Fact]
//     public void GetAvailableBuilds_ReturnsFiveBuilds()
//     {
//         // Act
//         var builds = DefaultBuildDiscovery.GetAvailableBuilds();
//
//         // Assert
//         Assert.Equal(5, builds.Count);
//     }
//
//     [Fact]
//     public void GetAvailableBuilds_ContainsCompileBuild()
//     {
//         // Act
//         var builds = DefaultBuildDiscovery.GetAvailableBuilds();
//
//         // Assert
//         var compileBuild = builds.FirstOrDefault(b => b.Name == "CompileBuild");
//         Assert.NotNull(compileBuild);
//         Assert.Equal("Basic compilation with secret scanning", compileBuild.Description);
//         Assert.False(compileBuild.RequiresTests);
//         Assert.False(compileBuild.RequiresPackaging);
//         Assert.False(compileBuild.RequiresVelopack);
//     }
//
//     [Fact]
//     public void GetAvailableBuilds_ContainsTestBuild()
//     {
//         // Act
//         var builds = DefaultBuildDiscovery.GetAvailableBuilds();
//
//         // Assert
//         var testBuild = builds.FirstOrDefault(b => b.Name == "TestBuild");
//         Assert.NotNull(testBuild);
//         Assert.Equal("Compilation, testing, and code coverage", testBuild.Description);
//         Assert.True(testBuild.RequiresTests);
//         Assert.False(testBuild.RequiresPackaging);
//         Assert.False(testBuild.RequiresVelopack);
//     }
//
//     [Fact]
//     public void GetAvailableBuilds_ContainsPackageBuild()
//     {
//         // Act
//         var builds = DefaultBuildDiscovery.GetAvailableBuilds();
//
//         // Assert
//         var packageBuild = builds.FirstOrDefault(b => b.Name == "PackageBuild");
//         Assert.NotNull(packageBuild);
//         Assert.Equal("Full pipeline with NuGet package creation", packageBuild.Description);
//         Assert.True(packageBuild.RequiresTests);
//         Assert.True(packageBuild.RequiresPackaging);
//         Assert.False(packageBuild.RequiresVelopack);
//     }
//
//     [Fact]
//     public void GetAvailableBuilds_ContainsVelopackBuild()
//     {
//         // Act
//         var builds = DefaultBuildDiscovery.GetAvailableBuilds();
//
//         // Assert
//         var velopackBuild = builds.FirstOrDefault(b => b.Name == "VelopackBuild");
//         Assert.NotNull(velopackBuild);
//         Assert.Equal("Application deployment with Velopack", velopackBuild.Description);
//         Assert.True(velopackBuild.RequiresTests);
//         Assert.False(velopackBuild.RequiresPackaging);
//         Assert.True(velopackBuild.RequiresVelopack);
//     }
//
//     [Fact]
//     public void GetAvailableBuilds_ContainsPackageAndVelopackBuild()
//     {
//         // Act
//         var builds = DefaultBuildDiscovery.GetAvailableBuilds();
//
//         // Assert
//         var packageAndVelopackBuild = builds.FirstOrDefault(b => b.Name == "PackageAndVelopackBuild");
//         Assert.NotNull(packageAndVelopackBuild);
//         Assert.Equal("Complete pipeline with both NuGet and Velopack", packageAndVelopackBuild.Description);
//         Assert.True(packageAndVelopackBuild.RequiresTests);
//         Assert.True(packageAndVelopackBuild.RequiresPackaging);
//         Assert.True(packageAndVelopackBuild.RequiresVelopack);
//     }
//
//     [Fact]
//     public void GetAvailableBuilds_CompileBuild_HasCorrectComponents()
//     {
//         // Act
//         var builds = DefaultBuildDiscovery.GetAvailableBuilds();
//         var compileBuild = builds.First(b => b.Name == "CompileBuild");
//
//         // Assert
//         Assert.NotNull(compileBuild.Components);
//         Assert.Equal(5, compileBuild.Components.Count);
//         Assert.Contains("IShowVersion", compileBuild.Components);
//         Assert.Contains("IClean", compileBuild.Components);
//         Assert.Contains("IRestore", compileBuild.Components);
//         Assert.Contains("ICompile", compileBuild.Components);
//         Assert.Contains("IScanForSecrets", compileBuild.Components);
//     }
//
//     [Fact]
//     public void GetAvailableBuilds_TestBuild_HasCorrectComponents()
//     {
//         // Act
//         var builds = DefaultBuildDiscovery.GetAvailableBuilds();
//         var testBuild = builds.First(b => b.Name == "TestBuild");
//
//         // Assert
//         Assert.NotNull(testBuild.Components);
//         Assert.Equal(9, testBuild.Components.Count);
//         Assert.Contains("IShowVersion", testBuild.Components);
//         Assert.Contains("IClean", testBuild.Components);
//         Assert.Contains("IRestore", testBuild.Components);
//         Assert.Contains("ICompile", testBuild.Components);
//         Assert.Contains("IScanForSecrets", testBuild.Components);
//         Assert.Contains("IRunUnitTests", testBuild.Components);
//         Assert.Contains("IRunIntegrationTests", testBuild.Components);
//         Assert.Contains("IGenerateCoverageReport", testBuild.Components);
//         Assert.Contains("ITest", testBuild.Components);
//     }
//
//     [Fact]
//     public void GetAvailableBuilds_PackageBuild_HasCorrectComponents()
//     {
//         // Act
//         var builds = DefaultBuildDiscovery.GetAvailableBuilds();
//         var packageBuild = builds.First(b => b.Name == "PackageBuild");
//
//         // Assert
//         Assert.NotNull(packageBuild.Components);
//         Assert.Equal(13, packageBuild.Components.Count);
//         Assert.Contains("IShowVersion", packageBuild.Components);
//         Assert.Contains("IClean", packageBuild.Components);
//         Assert.Contains("IRestore", packageBuild.Components);
//         Assert.Contains("ICompile", packageBuild.Components);
//         Assert.Contains("IScanForSecrets", packageBuild.Components);
//         Assert.Contains("IRunUnitTests", packageBuild.Components);
//         Assert.Contains("IRunIntegrationTests", packageBuild.Components);
//         Assert.Contains("IGenerateCoverageReport", packageBuild.Components);
//         Assert.Contains("ITest", packageBuild.Components);
//         Assert.Contains("IUpdateChangelog", packageBuild.Components);
//         Assert.Contains("IPackage", packageBuild.Components);
//         Assert.Contains("ITagRelease", packageBuild.Components);
//         Assert.Contains("IAnnounceRelease", packageBuild.Components);
//     }
//
//     [Fact]
//     public void GetAvailableBuilds_VelopackBuild_HasCorrectComponents()
//     {
//         // Act
//         var builds = DefaultBuildDiscovery.GetAvailableBuilds();
//         var velopackBuild = builds.First(b => b.Name == "VelopackBuild");
//
//         // Assert
//         Assert.NotNull(velopackBuild.Components);
//         Assert.Equal(13, velopackBuild.Components.Count);
//         Assert.Contains("IShowVersion", velopackBuild.Components);
//         Assert.Contains("IClean", velopackBuild.Components);
//         Assert.Contains("IRestore", velopackBuild.Components);
//         Assert.Contains("ICompile", velopackBuild.Components);
//         Assert.Contains("IScanForSecrets", velopackBuild.Components);
//         Assert.Contains("IRunUnitTests", velopackBuild.Components);
//         Assert.Contains("IRunIntegrationTests", velopackBuild.Components);
//         Assert.Contains("IGenerateCoverageReport", velopackBuild.Components);
//         Assert.Contains("ITest", velopackBuild.Components);
//         Assert.Contains("IUpdateChangelog", velopackBuild.Components);
//         Assert.Contains("IVelopack", velopackBuild.Components);
//         Assert.Contains("ITagRelease", velopackBuild.Components);
//         Assert.Contains("IAnnounceRelease", velopackBuild.Components);
//     }
//
//     [Fact]
//     public void GetAvailableBuilds_PackageAndVelopackBuild_HasCorrectComponents()
//     {
//         // Act
//         var builds = DefaultBuildDiscovery.GetAvailableBuilds();
//         var packageAndVelopackBuild = builds.First(b => b.Name == "PackageAndVelopackBuild");
//
//         // Assert
//         Assert.NotNull(packageAndVelopackBuild.Components);
//         Assert.Equal(14, packageAndVelopackBuild.Components.Count);
//         Assert.Contains("IShowVersion", packageAndVelopackBuild.Components);
//         Assert.Contains("IClean", packageAndVelopackBuild.Components);
//         Assert.Contains("IRestore", packageAndVelopackBuild.Components);
//         Assert.Contains("ICompile", packageAndVelopackBuild.Components);
//         Assert.Contains("IScanForSecrets", packageAndVelopackBuild.Components);
//         Assert.Contains("IRunUnitTests", packageAndVelopackBuild.Components);
//         Assert.Contains("IRunIntegrationTests", packageAndVelopackBuild.Components);
//         Assert.Contains("IGenerateCoverageReport", packageAndVelopackBuild.Components);
//         Assert.Contains("ITest", packageAndVelopackBuild.Components);
//         Assert.Contains("IUpdateChangelog", packageAndVelopackBuild.Components);
//         Assert.Contains("IPackage", packageAndVelopackBuild.Components);
//         Assert.Contains("IVelopack", packageAndVelopackBuild.Components);
//         Assert.Contains("ITagRelease", packageAndVelopackBuild.Components);
//         Assert.Contains("IAnnounceRelease", packageAndVelopackBuild.Components);
//     }
//
//     [Fact]
//     public void GetAvailableBuilds_AllBuildsHaveUniqueName()
//     {
//         // Act
//         var builds = DefaultBuildDiscovery.GetAvailableBuilds();
//
//         // Assert
//         var uniqueNames = builds.Select(b => b.Name).Distinct().Count();
//         Assert.Equal(builds.Count, uniqueNames);
//     }
//
//     [Fact]
//     public void GetAvailableBuilds_AllBuildsHaveNonEmptyDescription()
//     {
//         // Act
//         var builds = DefaultBuildDiscovery.GetAvailableBuilds();
//
//         // Assert
//         Assert.All(builds, build =>
//         {
//             Assert.False(string.IsNullOrWhiteSpace(build.Description));
//         });
//     }
//
//     [Fact]
//     public void GetAvailableBuilds_AllBuildsHaveNonEmptyName()
//     {
//         // Act
//         var builds = DefaultBuildDiscovery.GetAvailableBuilds();
//
//         // Assert
//         Assert.All(builds, build =>
//         {
//             Assert.False(string.IsNullOrWhiteSpace(build.Name));
//         });
//     }
//
//     [Fact]
//     public void GetAvailableBuilds_AllBuildsHaveComponents()
//     {
//         // Act
//         var builds = DefaultBuildDiscovery.GetAvailableBuilds();
//
//         // Assert
//         Assert.All(builds, build =>
//         {
//             Assert.NotNull(build.Components);
//             Assert.NotEmpty(build.Components);
//         });
//     }
//
//     [Fact]
//     public void GetAvailableBuilds_AllBuildsIncludeIShowVersion()
//     {
//         // Act
//         var builds = DefaultBuildDiscovery.GetAvailableBuilds();
//
//         // Assert
//         Assert.All(builds, build =>
//         {
//             Assert.Contains("IShowVersion", build.Components);
//         });
//     }
//
//     [Fact]
//     public void GetAvailableBuilds_AllBuildsIncludeIClean()
//     {
//         // Act
//         var builds = DefaultBuildDiscovery.GetAvailableBuilds();
//
//         // Assert
//         Assert.All(builds, build =>
//         {
//             Assert.Contains("IClean", build.Components);
//         });
//     }
//
//     [Fact]
//     public void GetAvailableBuilds_AllBuildsIncludeIRestore()
//     {
//         // Act
//         var builds = DefaultBuildDiscovery.GetAvailableBuilds();
//
//         // Assert
//         Assert.All(builds, build =>
//         {
//             Assert.Contains("IRestore", build.Components);
//         });
//     }
//
//     [Fact]
//     public void GetAvailableBuilds_AllBuildsIncludeICompile()
//     {
//         // Act
//         var builds = DefaultBuildDiscovery.GetAvailableBuilds();
//
//         // Assert
//         Assert.All(builds, build =>
//         {
//             Assert.Contains("ICompile", build.Components);
//         });
//     }
//
//     [Fact]
//     public void GetAvailableBuilds_AllBuildsIncludeIScanForSecrets()
//     {
//         // Act
//         var builds = DefaultBuildDiscovery.GetAvailableBuilds();
//
//         // Assert
//         Assert.All(builds, build =>
//         {
//             Assert.Contains("IScanForSecrets", build.Components);
//         });
//     }
//
//     [Fact]
//     public void GetAvailableBuilds_OnlyCompileBuildDoesNotRequireTests()
//     {
//         // Act
//         var builds = DefaultBuildDiscovery.GetAvailableBuilds();
//
//         // Assert
//         var buildsRequiringTests = builds.Where(b => b.RequiresTests).ToList();
//         var buildsNotRequiringTests = builds.Where(b => !b.RequiresTests).ToList();
//
//         Assert.Equal(4, buildsRequiringTests.Count);
//         Assert.Single(buildsNotRequiringTests);
//         Assert.Equal("CompileBuild", buildsNotRequiringTests.First().Name);
//     }
//
//     [Fact]
//     public void GetAvailableBuilds_OnlyTwoBuildsRequirePackaging()
//     {
//         // Act
//         var builds = DefaultBuildDiscovery.GetAvailableBuilds();
//
//         // Assert
//         var buildsRequiringPackaging = builds.Where(b => b.RequiresPackaging).ToList();
//         Assert.Equal(2, buildsRequiringPackaging.Count);
//         Assert.Contains(buildsRequiringPackaging, b => b.Name == "PackageBuild");
//         Assert.Contains(buildsRequiringPackaging, b => b.Name == "PackageAndVelopackBuild");
//     }
//
//     [Fact]
//     public void GetAvailableBuilds_OnlyTwoBuildsRequireVelopack()
//     {
//         // Act
//         var builds = DefaultBuildDiscovery.GetAvailableBuilds();
//
//         // Assert
//         var buildsRequiringVelopack = builds.Where(b => b.RequiresVelopack).ToList();
//         Assert.Equal(2, buildsRequiringVelopack.Count);
//         Assert.Contains(buildsRequiringVelopack, b => b.Name == "VelopackBuild");
//         Assert.Contains(buildsRequiringVelopack, b => b.Name == "PackageAndVelopackBuild");
//     }
//
//     [Fact]
//     public void GetAvailableBuilds_CalledMultipleTimes_ReturnsEquivalentData()
//     {
//         // Act
//         var builds1 = DefaultBuildDiscovery.GetAvailableBuilds();
//         var builds2 = DefaultBuildDiscovery.GetAvailableBuilds();
//
//         // Assert
//         Assert.Equal(builds1.Count, builds2.Count);
//         for (int i = 0; i < builds1.Count; i++)
//         {
//             Assert.Equal(builds1[i].Name, builds2[i].Name);
//             Assert.Equal(builds1[i].Description, builds2[i].Description);
//             Assert.Equal(builds1[i].RequiresTests, builds2[i].RequiresTests);
//             Assert.Equal(builds1[i].RequiresPackaging, builds2[i].RequiresPackaging);
//             Assert.Equal(builds1[i].RequiresVelopack, builds2[i].RequiresVelopack);
//             Assert.Equal(builds1[i].Components.Count, builds2[i].Components.Count);
//         }
//     }
//
//     [Fact]
//     public void GetAvailableBuilds_BuildsWithTests_IncludeITest()
//     {
//         // Act
//         var builds = DefaultBuildDiscovery.GetAvailableBuilds();
//
//         // Assert
//         var buildsRequiringTests = builds.Where(b => b.RequiresTests);
//         Assert.All(buildsRequiringTests, build =>
//         {
//             Assert.Contains("ITest", build.Components);
//         });
//     }
//
//     [Fact]
//     public void GetAvailableBuilds_BuildsWithPackaging_IncludeIPackage()
//     {
//         // Act
//         var builds = DefaultBuildDiscovery.GetAvailableBuilds();
//
//         // Assert
//         var buildsRequiringPackaging = builds.Where(b => b.RequiresPackaging);
//         Assert.All(buildsRequiringPackaging, build =>
//         {
//             Assert.Contains("IPackage", build.Components);
//         });
//     }
//
//     [Fact]
//     public void GetAvailableBuilds_BuildsWithVelopack_IncludeIVelopack()
//     {
//         // Act
//         var builds = DefaultBuildDiscovery.GetAvailableBuilds();
//
//         // Assert
//         var buildsRequiringVelopack = builds.Where(b => b.RequiresVelopack);
//         Assert.All(buildsRequiringVelopack, build =>
//         {
//             Assert.Contains("IVelopack", build.Components);
//         });
//     }
//
//     [Fact]
//     public void GetAvailableBuilds_BuildsWithTests_IncludeTestingComponents()
//     {
//         // Act
//         var builds = DefaultBuildDiscovery.GetAvailableBuilds();
//
//         // Assert
//         var buildsRequiringTests = builds.Where(b => b.RequiresTests);
//         Assert.All(buildsRequiringTests, build =>
//         {
//             Assert.Contains("IRunUnitTests", build.Components);
//             Assert.Contains("IRunIntegrationTests", build.Components);
//             Assert.Contains("IGenerateCoverageReport", build.Components);
//         });
//     }
//
//     [Fact]
//     public void GetAvailableBuilds_BuildsWithPackagingOrVelopack_IncludeReleaseComponents()
//     {
//         // Act
//         var builds = DefaultBuildDiscovery.GetAvailableBuilds();
//
//         // Assert
//         var buildsRequiringRelease = builds.Where(b => b.RequiresPackaging || b.RequiresVelopack);
//         Assert.All(buildsRequiringRelease, build =>
//         {
//             Assert.Contains("ITagRelease", build.Components);
//             Assert.Contains("IAnnounceRelease", build.Components);
//         });
//     }
// }
