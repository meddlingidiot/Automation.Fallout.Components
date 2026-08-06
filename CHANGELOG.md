# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased] - 0.0.1-beta.8

### Added

- `IPackageMultiPlatform` — a single `ReleasePackage` target that pushes to Azure Artifacts or
  GitHub Packages depending on which CI system is running the build, for repositories built by
  both. Only Azure DevOps triggers `TagRelease`, so a mirrored GitHub build cannot create a
  competing `v{version}` tag.
- `IPushPackagesAzureDevOps` / `IPushPackagesGitHub` — the push logic for each platform, carried as
  overridable methods on interfaces that declare no targets, so both can be composed into one build.
- `PublishTarget` parameter (`Auto`, `AzureDevOps`, `GitHub`, `Both`, `None`) to force a
  destination. `Auto` pushes nothing on a local run.

### Changed

- `IPackageAzureDevOps` and `IPackageGitHub` now inherit their push logic from the new interfaces
  instead of inlining it. Their targets, dependencies and behavior are unchanged — a build using
  either one produces an identical target graph.
- `IVelopack` installs the Velopack CLI only when it is missing, rather than running
  `dotnet tool update -g vpk` on every `PreVelopack`. Pin a version with `--velopack-cli-version`.

### Fixed

- `IVelopack` failing with `Could not find 'vpk' via where.exe`. The CLI install sat inside
  `PreVelopack` *after* its "AzureBlobSasToken not provided" early return, so any build without a
  SAS token skipped the install and then failed in `BuildVelopack`. All three Velopack targets now
  resolve the CLI through `ResolveVelopackCli()`, which installs it on demand.
- `IVelopack` resolving `vpk` only from `PATH`. A tool installed during the build lands in the
  dotnet global tools directory, which the already-running process's `PATH` does not include on a
  fresh CI agent. The resolver now falls back to that directory.

