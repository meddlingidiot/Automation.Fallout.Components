# Automation.Nuke.Builder

A global .NET tool that simplifies the setup and configuration of Nuke build pipelines across multiple projects.

## Features

- **Interactive Setup**: Guided prompts using Spectre.Console for easy configuration
- **Multiple Build Types**: Choose from pre-configured build templates:
  - **CompileBuild**: Basic compilation with secret scanning
  - **TestBuild**: Compilation, testing, and code coverage
  - **PackageBuild**: Full pipeline with NuGet package creation
  - **VelopackBuild**: Application deployment with Velopack
  - **PackageAndVelopackBuild**: Complete pipeline with both NuGet and Velopack
- **Automated Dependencies**: Automatically installs required NuGet packages and tools
- **Configurable Options**:
  - Break build on warnings
  - Break build on secret leaks
  - Minimum code coverage requirements
  - Velopack configuration
  
Additional automation (latest updates):
- Upgrades the generated Nuke build project to target `net10.0`
- Adds `PackageDownload` entries for `GitVersion.Tool (6.5.1)` and `ReportGenerator (5.5.1)`
- Installs `GitVersion.Tool` locally (v6.5.1)
- Copies default root config files to your repository: `.gitleaks.toml`, `nuget.config`, `GitVersion.yml`, `azure-pipelines.yml`

## Installation

Install as a global .NET tool:

```bash
dotnet tool install --global Automation.Nuke.Builder
```

Or install from a local package:

```bash
dotnet tool uninstall -g Automation.Nuke.Builder
dotnet pack ./Automation.Nuke.Builder/Automation.Nuke.Builder.csproj
dotnet tool install --global --add-source "./Automation.Nuke.Builder/nupkg" Automation.Nuke.Builder
```

## Usage

Navigate to your project root directory and run:

```bash
aftrnuke setup
```

The tool will guide you through:

1. **Build Type Selection**: Choose the appropriate build template for your project
2. **Quality Gates**: Configure warning and secret leak detection
3. **Code Coverage**: Set minimum coverage thresholds (if applicable)
4. **Velopack Settings**: Configure deployment options (if applicable)

The tool will:
- Install Nuke global tool (if not already installed)
- Update Nuke global tool to the latest available
- Run `nuke :setup` to create the build infrastructure (if `build/` doesn't exist)
- Upgrade the generated build project to target `net10.0`
- Add required NuGet packages to your build project (`Automation.Nuke.Components` and `Nuke.Common`)
- Add `PackageDownload` entries for `GitVersion.Tool (6.5.1)` and `ReportGenerator (5.5.1)`
- Install local tools (installs `GitVersion.Tool` v6.5.1; Gitleaks is a separate install)
- Copy default root items: `.gitleaks.toml`, `nuget.config`, `GitVersion.yml`, `azure-pipelines.yml`
- Remove the legacy `Configuration.cs` file in `build/` if present
- Generate a customized `Build.cs` file based on your selections

### Non-interactive (CI) usage

You can re-run setup in CI or scripts to enforce conventions without prompts by answering with defaults when asked. For fully non-interactive flows, pre-create a `_build.csproj` that already references `Automation.Nuke.Components` and commit your chosen `Build.cs` so `aftrnuke setup` becomes a no-op.

## Build Types

### CompileBuild
Basic compilation pipeline with:
- Version display
- Code compilation
- Secret scanning

**Use Case**: Simple libraries or projects that only need compilation verification

### TestBuild
Testing pipeline with:
- All CompileBuild features
- Unit test execution
- Code coverage reporting

**Use Case**: Libraries with test suites

### PackageBuild
NuGet packaging pipeline with:
- All TestBuild features
- Changelog updates
- NuGet package creation
- Git tagging

**Use Case**: Libraries published to NuGet

### VelopackBuild
Application deployment pipeline with:
- All TestBuild features
- Velopack installer creation
- Git tagging

**Use Case**: Desktop applications using Velopack for updates

### PackageAndVelopackBuild
Complete pipeline with:
- All features from TestBuild
- NuGet package creation
- Velopack installer creation
- Git tagging

**Use Case**: Complex projects requiring both library and application distribution

## Example

```bash
cd MyProject
aftrnuke setup

# Follow the prompts:
# - Select build type: TestBuild
# - Break build on warnings? Yes
# - Break build on secret leaks? Yes
# - Enable code coverage requirement? Yes
# - Minimum code coverage: 80

# Run your build
nuke
```

## Roll your own step (custom component)

After the tool scaffolds your `build/` project, you can add your own reusable step as a Nuke component. Components are just small interfaces that contribute `Target`s and optional parameters. Implement your interface on the `Build` class to include the step.

1) Create a custom component interface in your build project (or a shared library):

```csharp
// build/Components/IMyLint.cs
using Nuke.Common;
using Nuke.Common.IO;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace Automation.Nuke.Components.Components; // keep namespace style consistent with other components

public interface IMyLint : INukeBuild
{
    AbsolutePath Src => RootDirectory / "src"; // example input

    Target MyLint => _ => _
        .Description("Run custom lint")
        .Executes(() =>
        {
            // Replace with your tool of choice
            DotNet($"format {Src} --verify-no-changes");
        });
}
```

2) Wire it into your `Build` by implementing the interface and (optionally) adding dependencies:

```csharp
// build/Build.cs
using Nuke.Common;
using Automation.Nuke.Components;
using Automation.Nuke.Components.Components;

public class Build : AzurePipelinesBuild, IShowVersion, IClean, IRestore, ICompile, ITest, IMyLint
{
    public new static int Main() => Execute<Build>(x => ((ITest)x).Test);

    // Optionally make your main pipeline depend on your custom step
    Target Default => _ => _
        .DependsOn(((IMyLint)this).MyLint)
        .DependsOn(((ITest)this).Test);
}
```

3) Add parameters to your component by composing with existing `IHas*` contracts. For example, to use solution/configuration values:

```csharp
public interface IMyLint : INukeBuild, IHasSolution, IHasConfiguration
{
    Target MyLint => _ => _
        .Executes(() =>
        {
            Serilog.Log.Information("Linting {Solution} in {Configuration}", Solution, Configuration);
        });
}
```

That’s it—your step is now first-class and can be shared across builds by moving the interface into a common library.

## Requirements

- .NET SDK 8.0 or higher to run this tool (`Automation.Nuke.Builder`)
- .NET SDK 10.0 to build and run the generated Nuke build (the setup upgrades `_build.csproj` to `net10.0`)
- Git (for version control features)
- Nuke.GlobalTool (installed/updated automatically by this tool)
- GitVersion.Tool (installed automatically as a local tool)
- Gitleaks (for secret scanning - install separately)

## Generated Project Structure

After running setup, your project will have:

```
MyProject/
├── build/
│   ├── Build.cs              # Generated build script
│   ├── _build.csproj         # Build project with dependencies (targets net10.0)
│   └── .nuke                 # Nuke configuration
├── .gitleaks.toml            # Default gitleaks configuration
├── nuget.config              # Default NuGet sources configuration
├── GitVersion.yml            # Default GitVersion configuration
├── azure-pipelines.yml       # Sample Azure Pipelines YAML
├── .config/
│   └── dotnet-tools.json     # Local tool manifest
└── build.cmd / build.sh      # Build entry points
```

## Customization

After generation, you can customize the `build/Build.cs` file to:
- Override default parameter values
- Add custom targets
- Extend component behavior
- Add project-specific logic

See “Roll your own step” above for a full example of creating your own component.

## Troubleshooting

**Nuke not found**: Ensure .NET global tools are in your PATH
```bash
export PATH="$PATH:$HOME/.dotnet/tools"  # Linux/Mac
# or add %USERPROFILE%\.dotnet\tools to PATH on Windows
```

**Package not found**: Ensure the Automation.Nuke.Components package is published to your NuGet feed

**Permission errors**: Run with appropriate permissions or use `--global` flag for tool installation

**Build fails targeting net10.0**: Install the .NET 10 SDK so the generated `_build.csproj` can compile.

## Development

To build and test locally:

```bash
cd Automation.Nuke.Builder
dotnet build
dotnet pack
dotnet tool install --global --add-source ./nupkg Automation.Nuke.Builder
```

## Contributing

This tool is designed to work with the Automation.Nuke.Components library. Ensure both are kept in sync when adding new features or build types.

## License

[Your License Here]
