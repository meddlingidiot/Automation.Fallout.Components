# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased] - 0.0.1-beta.4

### Fixed

- `aftrfallout setup` now copies the default `nuget.config` before resolving packages. The AFTR feeds
  were previously written after `dotnet add package` ran, so `Automation.Fallout.Components` silently
  failed to be added in a repository that did not already have the feeds configured.
- Packages are now added by editing the project XML instead of shelling out to `dotnet add package`,
  and the version lands where the repository keeps it: in `Directory.Packages.props` as a
  `PackageVersion` when the repository manages versions centrally, otherwise on the
  `PackageReference`. A `PackageReference` carrying its own `Version` fails restore with NU1008 under
  central package management.
- Setup says so at the end when a package could not be added, rather than reporting success over a
  `build/_build.csproj` that will not compile.

### Changed

- `Automation.Fallout.Components` is pinned to the newest released version on the feeds
  (`dotnet package search`, prereleases excluded), falling back to the version already referenced and
  then to the version this tool shipped with. `Fallout.Common` stays pinned to the version that
  matches the Fallout CLI being installed.

