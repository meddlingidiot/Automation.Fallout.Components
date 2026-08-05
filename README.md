# Automation.Fallout.Components

A [Fallout](https://github.com/ChrisonSimtian/Fallout) build system library and global tool for .NET projects, providing reusable build components, pre-configured build templates, and automated setup across multiple projects.

> **Migrated from NUKE.** This repository previously targeted the NUKE build system, which is no longer maintained. It now targets **Fallout**, the hard-fork successor. See [Migration from NUKE](#-migration-from-nuke) for what changed and how to update a consuming repo.

## 🚀 Overview

This repository contains two main projects:

1. **Automation.Fallout.Components** - A library of reusable Fallout build components
2. **Automation.Fallout.Builder** - A global .NET CLI tool for automated build setup

Together, they enable standardized, maintainable build pipelines across 80+ projects with minimal configuration.

## 📦 Projects

### Automation.Fallout.Components

A NuGet package providing modular, composable build components for Fallout builds targeting .NET 10.0.

**Key Features:**
- Component-based architecture using interface composition
- Pre-built templates for common build scenarios
- Reusable targets for compilation, testing, packaging, and deployment
- Azure Pipelines *and* GitHub Actions integration, selected during setup
- GitVersion and semantic versioning support

**Available Components:**

| Component | Purpose |
|-----------|---------|
| `IShowVersion` | Display version information from GitVersion |
| `IClean` | Clean build artifacts |
| `IRestore` | Restore NuGet packages |
| `ICompile` | Compile .NET projects with configurable warning behavior |
| `IScanForSecrets` | Scan for secrets using Gitleaks |
| `IRunUnitTests` | Execute unit tests |
| `IRunIntegrationTests` | Execute integration tests |
| `IGenerateCoverageReport` | Generate code coverage reports with ReportGenerator |
| `ITest` | Orchestrate full test pipeline |
| `IUpdateChangelog` | Update CHANGELOG.md from Git history |
| `IPackage` | Create NuGet packages (production only - see the platform components below) |
| `IVelopack` | Build Velopack installers for application deployment |
| `ITagRelease` | Create Git tags for releases |
| `IAnnounceRelease` | Announce releases (placeholder for notifications) |
| `IGitTagging` | Core Git tagging functionality |
| `IPostRelease` | Post-release cleanup and tasks |
| `ITestExecution` | Shared test execution logic supporting both VSTest and Microsoft Testing Platform (MTP) |

**Platform-specific components** - pick the pair matching your [CI platform](#-ci-platform):

| Component | Platform | Purpose |
|-----------|----------|---------|
| `IPackageAzureDevOps` | Azure DevOps | Push packages to Azure Artifacts feeds |
| `IPackageGitHub` | GitHub | Push packages to GitHub Packages |
| `ICreateGitHubRelease` | GitHub | Create a GitHub release from the tag, with milestone notes and assets |
| `IPublishBlazorWasm` | GitHub | Publish Blazor WASM and deploy `wwwroot` to a static-site repository |
| `AzurePipelinesBuild` | Azure DevOps | Build base class with Azure Pipelines helpers |
| `GitHubActionsBuild` | GitHub | Build base class with GitHub Actions helpers |

**Pre-configured Build Templates:**

| Template | Includes | Use Case |
|----------|----------|----------|
| `CompileBuild` | Version, Clean, Compile, Restore, Secret Scanning | Simple libraries or basic projects |
| `TestBuild` | CompileBuild + Unit/Integration Tests + Coverage | Libraries with test suites |
| `PackageBuild` | TestBuild + Changelog + NuGet Package + Git Tag | Libraries published to NuGet |
| `VelopackBuild` | TestBuild + Velopack Installer + Git Tag | Desktop applications with auto-updates |
| `PackageAndVelopackBuild` | TestBuild + NuGet + Velopack + Git Tag | Projects requiring both library and app distribution |

**Dependencies:**
- Fallout.Build 11.0.18
- Fallout.Common 11.0.18
- SharpZipLib 1.4.2

### Automation.Fallout.Builder

A global .NET tool (`autofallout`) that automates the setup of Fallout build infrastructure with intelligent prompts and configuration.

> **Note:** The tool command has been renamed twice: `aftrnuke` -> `aftrfallout` -> `autofallout`. Update any scripts or pipelines that still invoke either of the older names.

**Key Features:**
- Interactive setup with Spectre.Console
- Automatic detection and installation of dependencies
- Generates customized `Build.cs` based on project needs
- Installs and configures required tools (GitVersion, Gitleaks)
- Copies default configuration files
- Upgrades build projects to .NET 10.0
- Migrates existing Nuke repositories with `autofallout migrate`

**Installation/Updates:**

```bash
dotnet tool install --global Automation.Fallout.Builder
```

**Usage:**

```bash
cd YourProject
autofallout setup
```

The tool will guide you through:
1. Build type selection (Compile, Test, Package, Velopack, etc.)
2. Quality gate configuration (warnings, secrets, coverage)
3. Project-specific settings
4. Automatic dependency installation

**What It Does:**
- Installs/updates the Fallout CLI (`Fallout.Cli`, exposing the `fallout` command)
- Runs `fallout :setup` to scaffold the `build/` directory
- Creates `build/` directory structure with `_build.csproj` targeting .NET 10.0
- Adds required NuGet packages (`Automation.Fallout.Components` at the newest released version, `Fallout.Common` pinned to the CLI version), writing the versions into `Directory.Packages.props` when the repository manages package versions centrally
- Adds `PackageDownload` entries for GitVersion.Tool (6.8.2) and ReportGenerator (5.5.11)
- Installs local tools via dotnet-tools.json
- Copies default files: `.gitleaks.toml`, `nuget.config`, `GitVersion.yml`, `azure-pipelines.yml`
- Generates `Build.cs` implementing selected build template
- Adds `.fallout/`, Rider, and DotSettings sections to `.gitignore`
- Removes legacy `Configuration.cs` if present

**Dependencies:**
- Spectre.Console 0.57.2
- System.CommandLine 2.0.10

## 🛠️ Getting Started

### Quick Start

1. Install the global tool:
   ```bash
   dotnet tool install --global Automation.Fallout.Builder
   ```

2. Navigate to your project:
   ```bash
   cd MyProject
   ```

3. Run setup:
   ```bash
   autofallout setup
   ```

4. Follow the interactive prompts to configure your build. The first question is which
   **CI platform** the repository builds on - see [CI platform](#-ci-platform).

5. Run your build:
   ```bash
   fallout
   ```

### 🔀 CI platform

Setup asks up front whether the repository builds on **Azure DevOps** or **GitHub Actions**,
because the two differ in more than a pipeline file. The answer decides:

| | Azure DevOps | GitHub Actions |
|---|---|---|
| Build base class | `AzurePipelinesBuild` | `GitHubActionsBuild` |
| Packaging component | `IPackageAzureDevOps` | `IPackageGitHub` |
| Package destination | Azure Artifacts feeds | GitHub Packages |
| Credentials | feed IDs, `az` api key | `GitHubOwner`, `GitHubToken` |
| CI definition copied | `azure-pipelines.yml` | `.github/workflows/build.yml` |
| `nuget.config` copied | AFTR feeds + nuget.org | nuget.org |
| Extra components | - | `ICreateGitHubRelease` when the build tags |

Both platforms share every other component, so a build only differs where it has to.

The `migrate` command takes the same choice as `--platform GitHubActions|AzureDevOps`
(default `AzureDevOps`).

### Manual Setup (Without Tool)

If you prefer manual setup:

1. Install Fallout globally:
   ```bash
   dotnet tool install -g Fallout.Cli
   ```

2. Setup Fallout in your project:
   ```bash
   fallout :setup
   ```

3. Add the Components package to `build/_build.csproj`:
   ```xml
   <PackageReference Include="Automation.Fallout.Components" Version="1.0.85" />
   ```

4. Create your `Build.cs` implementing desired interfaces:
   ```csharp
   using Fallout.Common;
   using Automation.Fallout.Components.DefaultBuilds;

   class Build : TestBuild
   {
       public static int Main() => Execute<Build>(x => ((ITest)x).Test);
   }
   ```

## 🔄 Migration from NUKE

NUKE is no longer maintained; Fallout is its hard-fork successor. The API surface is largely identical — most migration work is renaming namespaces and package references.

### Package references

| NUKE | Fallout |
|------|---------|
| `Nuke.Common` | `Fallout.Common` |
| `Nuke.Build` | `Fallout.Build` |
| `Nuke.GlobalTool` | `Fallout.Cli` |
| `Automation.Nuke.Components` | `Automation.Fallout.Components` |
| `Automation.Nuke.Builder` | `Automation.Fallout.Builder` |

### Namespaces

| NUKE | Fallout |
|------|---------|
| `Nuke.Common` | `Fallout.Common` |
| `Nuke.Common.CI` | `Fallout.Common.CI` |
| `Nuke.Common.Execution` | `Fallout.Common.Execution` |
| `Nuke.Common.IO` | `Fallout.Common.IO` |
| `Nuke.Common.Tooling` | `Fallout.Common.Tooling` |
| `Nuke.Common.ProjectModel` | `Fallout.Solutions` |
| `Automation.Nuke.Components.*` | `Automation.Fallout.Components.*` |

> ⚠️ **`Fallout.Solutions` is the one non-obvious rename.** Fallout 11.0 inlined the vendored solution parser and realigned the namespace to match the assembly name. It is *not* `Fallout.Common.ProjectModel` — that namespace does not exist in 11.x. This is the most common migration error.

### Types

| NUKE | Fallout |
|------|---------|
| `INukeBuild` | `IFalloutBuild` |
| `NukeBuild` | `FalloutBuild` |

### Conventions

| NUKE | Fallout |
|------|---------|
| `.nuke/` directory | `.fallout/` directory |
| `.nuke/parameters.json` | `.fallout/parameters.json` |
| `nuke` CLI command | `fallout` CLI command |
| `nuke :add-package` | `fallout :add-package` |

### Target framework

Fallout 11.x ships **`net10.0` assets only**. A build project left on `net8.0` will restore without error but resolve zero compile assets, producing a confusing cascade of `CS0234: The type or namespace name 'Common' does not exist in the namespace 'Fallout'`. Set:

```xml
<TargetFramework>net10.0</TargetFramework>
```

### Automated migration

Fallout publishes a migration tool that performs the namespace and package rewrites:

```bash
dotnet tool install -g Fallout.Migrate
```

Review its output — it is known to emit `Fallout.Common.ProjectModel` rather than `Fallout.Solutions`, which you must correct by hand.

## 🎯 Build Configuration

### Parameters

All build templates support common parameters through the `IHas*` interfaces:

- **Solution** - Target solution file (auto-detected)
- **Configuration** - Build configuration (Debug/Release, default: Release)
- **BreakBuildOnWarnings** - Fail build on compiler warnings (default: false)
- **BreakBuildOnSecretLeak** - Fail build on detected secrets (default: true)
- **MinCoverageThreshold** - Minimum code coverage percentage (default: 80)
- **VelopackIconPath** - Path to application icon for Velopack builds
- **VelopackDownloadUrl** - Base URL for Velopack update downloads

### Custom Components

Create your own reusable build components by implementing `IFalloutBuild`:

```csharp
// In your build project or shared library
using Fallout.Common;
using Fallout.Common.IO;
using static Fallout.Common.Tools.DotNet.DotNetTasks;

namespace Automation.Fallout.Components.Components;

public interface IMyLint : IFalloutBuild
{
    AbsolutePath Src => RootDirectory / "src";

    Target MyLint => _ => _
        .Description("Run custom lint checks")
        .Executes(() =>
        {
            DotNet($"format {Src} --verify-no-changes");
        });
}
```

Then add it to your build class:

```csharp
class Build : TestBuild, IMyLint
{
    Target Default => _ => _
        .DependsOn(((IMyLint)this).MyLint)
        .DependsOn(((ITest)this).Test);
}
```

## 📋 NuGet Configuration

This solution uses a multi-source NuGet configuration with package source mapping:

**Sources:**
- **AFTR Production** - Production packages from Azure Artifacts
- **AFTR Prerelease** - Prerelease/beta packages from Azure Artifacts
- **nuget.org** - Public NuGet packages

**Package Routing:**
- `Automation.*`, `FuelTaxAutomation.*`, `PMFuelTax*.*` → AFTR feeds
- All other packages (including `Fallout.*`) → nuget.org

See `nuget.config` for details.

## 🔧 Requirements

**To run the builder tool:**
- .NET SDK 8.0 or higher

**To build/run generated Fallout builds:**
- .NET SDK 10.0
- Git (for versioning and tagging) — the repository must have at least one commit, or GitVersion injection fails with `Could not find commit information`
- Gitleaks (for secret scanning) - install separately

**Auto-installed by the tool:**
- Fallout.Cli (latest) — the renamed successor to the unlisted `Fallout.GlobalTool`
- GitVersion.Tool 6.8.2 (PackageDownload)
- ReportGenerator 5.5.11 (PackageDownload)

## 📁 Generated Project Structure

After running `autofallout setup`, your project will have:

```
YourProject/
├── .fallout/
│   ├── parameters.json            # Fallout build parameters (solution, secrets)
│   └── build.schema.json          # Generated schema for editor autocomplete
├── build/
│   ├── Build.cs                   # Generated build script
│   └── _build.csproj              # Build project (net10.0)
├── .gitleaks.toml                 # Gitleaks configuration
├── nuget.config                   # NuGet source configuration
├── GitVersion.yml                 # GitVersion configuration
├── azure-pipelines.yml            # Sample Azure Pipelines YAML
├── build.cmd                      # Windows build script
├── build.ps1                      # PowerShell build script
└── build.sh                       # Unix build script
```

## 🚢 CI/CD Integration

### Azure Pipelines

All build templates inherit from `AzurePipelinesBuild` which provides:
- Automatic detection of Azure Pipelines environment
- Build reason and commit info
- Conditional execution based on CI context

Example `azure-pipelines.yml`:

```yaml
trigger:
  - main

pool:
  vmImage: 'windows-latest'

steps:
- task: UseDotNet@2
  displayName: 'Install .NET SDK'
  inputs:
    version: '10.x'

- script: dotnet tool restore
  displayName: 'Restore local tools'

- script: .\build.cmd
  displayName: 'Run Fallout build'
```

> **Checkout depth:** GitVersion needs full history. Set `fetchDepth: 0` on your checkout step, otherwise version resolution fails on CI.

### Non-Interactive Mode

For CI environments, pre-create your `_build.csproj` and commit your `Build.cs`. The tool becomes a no-op if everything is already configured.

## 📊 Code Coverage

The `IGenerateCoverageReport` component uses ReportGenerator to create coverage reports:

- Generates HTML reports in `artifacts/coverage/`
- Enforces minimum coverage thresholds via `MinCoverageThreshold` parameter
- Ignores files in `artifacts/` directory

Coverage is automatically collected by `coverlet.collector` during test execution.

## 🔐 Secret Scanning

The `IScanForSecrets` component uses Gitleaks to prevent committing sensitive data:

- Scans entire repository history
- Uses `.gitleaks.toml` configuration
- Can break builds with `BreakBuildOnSecretLeak` parameter
- Install Gitleaks separately: https://github.com/gitleaks/gitleaks

## 📦 Packaging & Releases

### NuGet Packages

`IPackage` component:
- Updates CHANGELOG.md from Git commits
- Generates NuGet packages
- Outputs to `artifacts/` directory
- Uses GitVersion for semantic versioning

`IPackage` only *produces* packages. Pushing them lives in a platform-specific component,
because the destination differs - implement one of:

- **`IPackageAzureDevOps`** - pushes to Azure Artifacts, picking the production or prerelease
  feed from the current branch (`IHasAzureDevOpsFeeds`).
- **`IPackageGitHub`** - pushes to GitHub Packages for `GitHubOwner` using `GitHubToken`
  (`IHasGitHubPackages`). Skips the push on local runs unless `--force-tag-release` is passed.

The setup wizard picks the right one for you from the [CI platform](#-ci-platform) answer.

### Velopack Deployments

`IVelopack` component:
- Builds Windows installers with Squirrel/Velopack
- Supports custom icons and download URLs
- Handles .NET runtime bundling
- Conditional runtime selection based on target framework

## 🧪 Testing

### Microsoft Testing Platform (MTP) Support

The `ITestExecution` interface supports both VSTest and Microsoft Testing Platform (MTP) based test frameworks (e.g., TUnit, TUnit-based runners).

By default, tests run via `dotnet test` using the VSTest protocol. If your test projects use an MTP-based framework, override `UseMicrosoftTestingPlatform` in your build class:

```csharp
class Build : TestBuild
{
    bool ITestExecution.UseMicrosoftTestingPlatform => true;

    public static int Main() => Execute<Build>(x => ((ITest)x).Test);
}
```

**Why this matters:** On the .NET 10 SDK, `dotnet test` no longer supports the VSTest protocol for MTP-based test apps. When `UseMicrosoftTestingPlatform` is `true`, the test executable is invoked directly, bypassing VSTest entirely. MTP mode provides:
- Direct test binary execution (no VSTest adapter required)
- Built-in Cobertura coverage output (`--coverage-output-format cobertura` on server builds)
- TRX report generation per project and target framework
- Multi-TFM support — each target framework is executed independently

> **Note:** When using MTP mode, ensure your projects are built before running tests (`EnableNoBuild` is not used in MTP mode — the build step runs the binary from the output directory).

### Running Unit Tests

This repository's own test project uses **TUnit**, which is MTP-based. On the .NET 10 SDK, `dotnet test` fails with `Testing with VSTest target is no longer supported`. Run the test binary directly instead:

```bash
dotnet build Automation.Fallout.Components.sln
./Automation.Fallout.Builder.UnitTests/bin/Debug/net8.0/Automation.Fallout.Builder.UnitTests.exe
```

The test project validates:
- Build file generation logic
- Default build discovery
- NuGet package installation and `.gitignore` management

## 🤝 Contributing

This system is designed for use across 80+ internal projects. When contributing:

1. Ensure backward compatibility
2. Update both `Automation.Fallout.Components` and `Automation.Fallout.Builder` in sync
3. Test against multiple project types
4. Update CHANGELOG.md with changes
5. Keep components focused and composable

## 📝 Version History

See [CHANGELOG.md](CHANGELOG.md) for detailed version history.

**Current Version:** 1.0.85

**Recent Changes:**
- Migrated from NUKE to the Fallout build system (Fallout.Common / Fallout.Build 11.0.18)
- Renamed `Automation.Nuke.Components` → `Automation.Fallout.Components` and `Automation.Nuke.Builder` → `Automation.Fallout.Builder`
- Retargeted the build project to `net10.0` (required by Fallout 11.x)
- Updated `Nuke.Common.ProjectModel` usages to `Fallout.Solutions`

## 📖 Additional Documentation

- [Builder Tool README](Automation.Fallout.Builder/README.md) - Detailed `autofallout` documentation
- [Fallout Documentation](https://github.com/ChrisonSimtian/Fallout) - Official Fallout build system docs
- [GitVersion Docs](https://gitversion.net/) - Semantic versioning configuration

## 🐛 Troubleshooting

**`CS0234: The type or namespace name 'Common' does not exist in the namespace 'Fallout'`**
- Your build project is not targeting `net10.0`. Fallout 11.x ships net10.0 assets only, so restore silently succeeds while resolving no compile assets.

**`CS0234: The type or namespace name 'ProjectModel' does not exist in the namespace 'Fallout.Common'`**
- Replace `using Fallout.Common.ProjectModel;` with `using Fallout.Solutions;` (renamed in Fallout 11.0).

**"Fallout not found"**
- Ensure .NET global tools are in PATH
- Run `dotnet tool install -g Fallout.Cli`

**`Could not find commit information`**
- The repository has no commits, or was cloned with a shallow fetch. GitVersion requires real history — make an initial commit, and set `fetchDepth: 0` on CI checkout.

**`Missing package reference/download` for GitVersion.Tool**
- Add `<PackageDownload Include="GitVersion.Tool" Version="[6.8.2]" />` to `build/_build.csproj`.

**Build fails targeting net10.0**
- Install .NET 10 SDK

**Package not found**
- Verify `nuget.config` has correct Azure Artifacts feeds
- Check package source mapping configuration — `Fallout.*` resolves from nuget.org, not the AFTR feeds

**Gitleaks errors**
- Install Gitleaks: https://github.com/gitleaks/gitleaks
- Verify `.gitleaks.toml` exists

**Coverage reports not generated**
- Ensure ReportGenerator is in PackageDownload
- Check `artifacts/coverage/` directory permissions

## 📄 License

[Your License Here]

## 👤 Authors

Luke Lanphear

## 🔗 Repository

https://dev.azure.com/AFTR/Automation/_git/Automation.Fallout.Components
