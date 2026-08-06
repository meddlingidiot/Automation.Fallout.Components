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

