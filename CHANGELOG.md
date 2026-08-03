# Changelog

All notable changes to this project will be documented in this file.

## [1.0.85] - 2026-04-13

### 📝 Other Changes

- Refactor assertions in build file generator and discovery tests to use Assert.Multiple for improved readability ([6c797f0](../../commit/6c797f0))

## [1.0.84] - 2026-04-13

### 📝 Other Changes

- Add support for Microsoft Testing Platform in test execution logic ([a958fbe](../../commit/a958fbe))

## [1.0.83] - 2026-04-09

### 📝 Other Changes

- Use resolved tool path for 'vpk' commands in IVelopack component ([45f22c4](../../commit/45f22c4))

## [1.0.82] - 2026-04-08

### 📝 Other Changes

- Deduplicate Velopack CLI installation logic in IVelopack component. ([4915cf8](../../commit/4915cf8))

## [1.0.81] - 2026-04-07

### 📝 Other Changes

- Update package references for Spectre.Console, System.CommandLine, coverlet.collector, and Microsoft.NET.Test.Sdk ([12df2e7](../../commit/12df2e7))

## [1.0.80] - 2026-03-30

### 📝 Other Changes

- Update Velopack CLI version to 0.0.1535-gb21da2a in installation process ([3107631](../../commit/3107631))
- Set assembly, file, and informational versions using `GitVersion` in Velopack build script. ([dc559cb](../../commit/dc559cb))

## [1.0.79] - 2026-03-23

### 📝 Other Changes

- Update package dependencies: System.CommandLine to 2.0.5, coverlet.collector to 8.0.1, and Microsoft.NET.Test.Sdk to 18.3.0. ([8da4fc0](../../commit/8da4fc0))
- Update Velopack CLI tool to version `0.0.1535-gb21da2a` during installation/update. ([fcd75c4](../../commit/fcd75c4))

## [1.0.78] - 2026-03-20

### 📝 Other Changes

- Restoring `channel` formatting from underscores to hyphens for consistency across Velopack operations. It was not the problem, it was a part of a bug in velopack itself. ([23c9130](../../commit/23c9130))
- Pin Velopack CLI tool version to `0.0.1521-gf115f70` during installation/update. ([6bef855](../../commit/6bef855))

## [1.0.77] - 2026-03-16

### 📝 Other Changes

- Normalize spacing in Velopack CLI installation comment for consistency. ([ccb5389](../../commit/ccb5389))
- Update dependencies and refine Velopack CLI process version handling. ([d7ba645](../../commit/d7ba645))

## [1.0.76] - 2026-03-15

### 📝 Other Changes

- Refine `channel` formatting by adding a leading underscore and simplify `downloadDir` path. ([86892ca](../../commit/86892ca))

## [1.0.75] - 2026-03-14

### 📝 Other Changes

- Use underscores instead of hyphens for `channel` formatting in Velopack. ([cdfe3d8](../../commit/cdfe3d8))

## [1.0.74] - 2026-03-14

### 📝 Other Changes

- Remove redundant cleaning of the output directory before building the Velopack package. ([53d2810](../../commit/53d2810))

## [1.0.73] - 2026-03-14

### 📝 Other Changes

- Clean output directory before building Velopack package to prevent version conflicts with old releases. ([c92871a](../../commit/c92871a))
- Log Velopack command output on failure and enable process output logging. ([669bebd](../../commit/669bebd))

## [1.0.72] - 2026-02-11

### 📝 Other Changes

- Update `GitVersion.yml`: switch `mode` to `ContinuousDelivery` for multiple branches, refine versioning settings, and add `assembly-versioning-scheme`. ([37161eb](../../commit/37161eb))

## [1.0.71] - 2026-02-10

### 📝 Other Changes

- Relax System.CommandLine dependency version in README. ([ed7bcbb](../../commit/ed7bcbb))
- Clarify installation section in README to include updates. ([ef57bc0](../../commit/ef57bc0))

## [1.0.70] - 2026-02-10

### 📝 Other Changes

- Set `mode` to `ContinuousDeployment` for the `main` branch in `GitVersion.yml`. ([b8a79c7](../../commit/b8a79c7))

## [1.0.69] - 2026-02-10

### 📝 Other Changes

- Switch `mode` from `ContinuousDeployment` to `ContinuousDelivery` in `GitVersion.yml`. ([43c8b07](../../commit/43c8b07))
- Set `mode` to `ContinuousDeployment` for `feature` and `bugfix` branches in `GitVersion.yml`. ([dbf6be5](../../commit/dbf6be5))
- Update `GitVersion.yml` to refine versioning configurations: set `assembly-versioning-scheme` to `MajorMinorPatch`, standardize branch labels, and add `tag-pre-release-weight`. ([8330d89](../../commit/8330d89))
- Modify `GitVersion.yml` to include `{CommitsSinceVersionSource}` in prerelease labels for `feature` and `bugfix` branches. ([5ae4f56](../../commit/5ae4f56))

## [1.0.68] - 2026-02-10

No changes recorded.

## [1.0.66] - 2026-02-10

### 📝 Other Changes

- Replace `+` with `-` in `GitVersion.FullSemVer` for compatibility with Velopack naming conventions. ([52d492e](../../commit/52d492e))

## [1.0.65] - 2026-02-10

### 📝 Other Changes

- Relax Velopack prerelease retention logic to honor `KeepMaxReleases` configuration. ([76b70d7](../../commit/76b70d7))

## [1.0.64] - 2026-02-09

### 📝 Other Changes

- Update NuGet tool installation command to use shorthand `-g` option. and just the other package ([f74d213](../../commit/f74d213))

## [1.0.63] - 2026-02-05

### 📝 Other Changes

- Simplify NuGet package source mapping by consolidating PM patterns. ([83c436a](../../commit/83c436a))

## [1.0.62] - 2026-02-01

### 📝 Other Changes

- Add PMTrip pattern to nuget.config package source mapping. ([2054139](../../commit/2054139))

## [1.0.61] - 2026-01-19

### 📝 Other Changes

- don't retain velopack as artifacts of builds for space reasons. They are stored in azure blob storage. ([3f8eda7](../../commit/3f8eda7))

## [1.0.60] - 2026-01-17

### 📝 Other Changes

- Comment out unused logging for `SslComUsername` in `IVelopack`. ([0ac7f50](../../commit/0ac7f50))

## [1.0.59] - 2026-01-17

### 📝 Other Changes

- Comment out code signing parameter logic in `IVelopack`. ([5790893](../../commit/5790893))

## [1.0.58] - 2026-01-10

### 📝 Other Changes

- Add parameters for SSL.com credentials and update CodeSigning defaults. ([83237a2](../../commit/83237a2))

## [1.0.57] - 2026-01-10

### 📝 Other Changes

- Add code signing support to builds and Velopack configuration ([97dcc01](../../commit/97dcc01))

## [1.0.56] - 2025-12-29

### 📝 Other Changes

- Add README and configure NuGet package source mapping to hopefully speed up load times. ([990168c](../../commit/990168c))

## [1.0.55] - 2025-12-23

### 📝 Other Changes

- Include `.vbproj` files in project discovery logic. ([3665d95](../../commit/3665d95))

## [1.0.54] - 2025-12-21

### 📝 Other Changes

- Simplify runtime string for Velopack builds and comment out unused runtime condition. ([136f0e2](../../commit/136f0e2))

## [1.0.53] - 2025-12-21

### 📝 Other Changes

- Lower `MinCoverageThreshold` to 35 and extend ignored files to include `artifacts/`. ([51b3eed](../../commit/51b3eed))

## [1.0.52] - 2025-12-21

### 📝 Other Changes

- Add runtime support for .NET 10 in Velopack builds, conditional on target frameworks. ([3429ef5](../../commit/3429ef5))

## [1.0.51] - 2025-12-21

### 📝 Other Changes

- Fix incorrect string format for `VelopackIconPath` by prefixing with `@` and update corresponding unit test. ([f740e3a](../../commit/f740e3a))

## [1.0.50] - 2025-12-18

### 📝 Other Changes

- Set `MinCoverageThreshold` to 40 in `Build` class. ([5d10932](../../commit/5d10932))

## [1.0.49] - 2025-12-18

### 📝 Other Changes

- Add unit tests for `BuildFileGenerator` and `DefaultBuildDiscovery` and extend `.gitignore` for Rider DotSettings. ([888fcd4](../../commit/888fcd4))

## [1.0.48] - 2025-12-18

### 📝 Other Changes

- Upgraded to latest Build Components. ([84cb659](../../commit/84cb659))

## [1.0.47] - 2025-12-18

### 📝 Other Changes

- Remove redundant `new` modifier from `Main` method in `BuildFileGenerator` class. ([2d4c37c](../../commit/2d4c37c))

## [1.0.46] - 2025-12-17

### 📝 Other Changes

- Set minimum coverage threshold to 20 and clean up Velopack build class. ([9fc1435](../../commit/9fc1435))

## [1.0.45] - 2025-12-17

### 📝 Other Changes

- Replace `IGenerateCodeCoverage` with `IGenerateCoverageReport` and fix `IScanSecrets` to `IScanForSecrets` in build target definitions. ([1571827](../../commit/1571827))
- Remove redundant quotes in `git config` commands within `ConfigureGitIdentity` method. ([93ca342](../../commit/93ca342))
- Update Velopack publish directory: switch to temporary `.tmp/velopack-build` directory and ensure cleanup before publish ([3642cda](../../commit/3642cda))
- Refactor build process: remove commented connection string, clean up `IScanForSecrets`, and enhance modularity with additional task-specific interfaces. ([d4bca2e](../../commit/d4bca2e))
- Modularize build process: introduce new interfaces for test execution, Git tagging, release announcements, and code coverage generation; rename `IScanSecrets` to `IScanForSecrets`; refactor existing targets for improved maintainability and readability. ([348aded](../../commit/348aded))

## [1.0.44] - 2025-12-12

### 📝 Other Changes

- Enable connection-uri rule in Gitleaks and remove commented-out connection string. ([a0ddefb](../../commit/a0ddefb))

## [1.0.43] - 2025-12-12

### 📝 Other Changes

- Remove `--no-git` flag from Gitleaks command and expand documentation with non-interactive CI usage and custom component guide. ([cd551b8](../../commit/cd551b8))

## [1.0.42] - 2025-12-10

### 📝 Other Changes

- Add missing `using Automation.Nuke.Components.DefaultBuilds` directive in `BuildFileGenerator` ([cfee7f6](../../commit/cfee7f6))

## [1.0.41] - 2025-12-10

No changes recorded.

## [1.0.40] - 2025-12-10

### 📝 Other Changes

- Refactor `BuildFileGenerator`: replace inline interface declarations with `GetInterfacesForBuild` method, streamline build class generation, and clean up unused imports. ([04ae2a8](../../commit/04ae2a8))

## [1.0.39] - 2025-12-10

### 📝 Other Changes

- Refactor build components: modularize clean, restore, and compile tasks with new `IClean`, `IRestore`, and `ICompile` interfaces; update changelog handling and enhance Azure Pipelines configurations. ([37f4ef2](../../commit/37f4ef2))

## [1.0.38] - 2025-12-09

### 📝 Other Changes

- Update `SetupCommand` to include `.gitignore` updates with Nuke and Rider entries, add `UpdateGitIgnoreAsync` in `NuGetPackageInstaller`, and adjust `.gitignore` content. ([df8761f](../../commit/df8761f))

## [1.0.37] - 2025-12-08

### 📝 Other Changes

- Update `BuildFileGenerator`: use `new` modifier in `Main` method and remove unused post-release target logic. ([98f3417](../../commit/98f3417))

## [1.0.36] - 2025-12-08

### 📝 Other Changes

- Switch to async output handling in `NuGetPackageInstaller` to prevent buffer deadlock, add timeout-based process termination, and include launch settings file for debugging. ([35475b2](../../commit/35475b2))

## [1.0.35] - 2025-12-08

### 📝 Other Changes

- Refactor Velopack icon selection: add `.ico` file scanning, custom path option, and improved prompt logic ([bdbe57d](../../commit/bdbe57d))

## [1.0.34] - 2025-12-08

### 📝 Other Changes

- Introduce `ProjectDiscovery` service, refactor Velopack prompt logic, and clean up unused package references ([4818d83](../../commit/4818d83))

## [1.0.33] - 2025-12-08

### 📝 Other Changes

- Update `Automation.Nuke.Components` package, switch to embedding `DefaultRootItems` as resources, and improve NuGet package handling logic ([055effb](../../commit/055effb))

## [1.0.32] - 2025-12-08

### 📝 Other Changes

- Enhance `BuildFileGenerator` to support additional interfaces (`IHasSolution`, `IHasConfiguration`), improve target generation logic, and update build schema with `MinCoverageThreshold`. ([b49732f](../../commit/b49732f))
- Update `build` project by adding `Automation.Nuke.Components` package, reorganizing imports, and enabling test support in `Build` class ([4a8f377](../../commit/4a8f377))

## [1.0.31] - 2025-12-07

### 📝 Other Changes

- Introduce `Automation.Nuke.Builder` project for simplifying Nuke pipeline setups, remove unused NuGet handling logic, and adjust solution structure. ([f7ab44f](../../commit/f7ab44f))

## [1.0.30] - 2025-12-07

### 📝 Other Changes

- Comment out `.EnableNoBuild()` in `IPackage` to revert potential fix. ([ae5661f](../../commit/ae5661f))

## [1.0.29] - 2025-12-07

### 📝 Other Changes

- Uncomment `.EnableNoBuild()` in `IPackage` to test potential fix. ([4b83977](../../commit/4b83977))

## [1.0.28] - 2025-12-07

### 📝 Other Changes

- Comment out `.EnableNoBuild()` in `IPackage` to test potential fix. ([711b85e](../../commit/711b85e))

## [1.0.27] - 2025-12-07

### 📝 Other Changes

- Migrate DefaultBuilds to AzurePipelinesBuild base class and introduce Azure-specific configuration. ([9a2ee4a](../../commit/9a2ee4a))

## [1.0.26] - 2025-12-07

### 📝 Other Changes

- Add Gitleaks configuration and implement ForceTagRelease parameter ([c01a7a6](../../commit/c01a7a6))

## [1.0.25] - 2025-12-07

No changes recorded.

## [1.0.2] - 2025-12-07

### 📝 Other Changes

- Remove `Configuration` class, adjust dependencies, and add `SharpZipLib` package to the build project ([728bd59](../../commit/728bd59))

## [1.0.1] - 2025-12-07

### 📝 Other Changes

- Add entries to .gitignore for Nuke temporary files and IDE configuration ([b241b28](../../commit/b241b28))
- Add entries to .gitignore for JetBrains Rider local history and Nuke temporary files ([129fe13](../../commit/129fe13))
- Add .store/ to .gitignore to exclude store files from version control ([caad29f](../../commit/caad29f))
- Add .gitignore file to exclude Visual Studio and build artifacts ([491afc1](../../commit/491afc1))
- Add .gitignore files and include `IVelopack` in PackageBuild steps ([3d42371](../../commit/3d42371))
- Extend build schema to support Velopack tasks and Azure Blob configuration ([079a14f](../../commit/079a14f))
- Upgrade build components with enhanced tagging, release handling, and project validation. ([5a72769](../../commit/5a72769))
- Add GitVersion.Tool package download to the build project ([d9d303e](../../commit/d9d303e))
- Add GitVersion.Tool installation and configure dotnet-tools.json for build automation ([fa743a3](../../commit/fa743a3))
- Integrate Automation.Nuke.Components and refactor Build class to extend PackageBuild ([d84c3d5](../../commit/d84c3d5))
- Update target framework to .NET 10 in build configuration ([88dc2a5](../../commit/88dc2a5))
- Initialize NUKE build system configuration and schema ([db72072](../../commit/db72072))
- Mark `Main` method as `new` in `Build.cs` ([ab105d1](../../commit/ab105d1))
- Set up CI with Azure Pipelines ([9d5b9f3](../../commit/9d5b9f3))
- Add cross-platform build scripts and project configurations ([9e646bb](../../commit/9e646bb))

## [1.0.0] - 2025-12-06

No changes recorded.

## [0.0.1] - 2025-12-06

### 📝 Other Changes

- .gitignore (VisualStudio) files ([8e7c231](../../commit/8e7c231))

