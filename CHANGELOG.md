# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased] - 1.0.13-beta.2

### 🐛 Bug Fixes

- fix(ci): resolve blob SAS token from secrets variable group ([fe22158](../../commit/fe22158))

### 📝 Other Changes

- build: update tool and test package versions ([0471dbf](../../commit/0471dbf))

## [1.0.12] - 2026-08-10

### 📝 Other Changes

- Revert back to the installers container ([b870817](../../commit/b870817))

## [1.0.11] - 2026-08-10

### ✨ Features

- feat(velopack): choose default blob container by build host ([192c972](../../commit/192c972))

## [1.0.10] - 2026-08-07

### 🔧 Chores

- chore(build): bump Automation.Fallout.Components package version ([0916589](../../commit/0916589))

## [1.0.9] - 2026-08-07

### 🐛 Bug Fixes

- fix(azure-pipelines): resolve blob SAS token from variable group ([4fe10dd](../../commit/4fe10dd))

## [1.0.8] - 2026-08-07

### 📝 Other Changes

- ci(github-actions): simplify build runner configuration ([671a5d4](../../commit/671a5d4))

## [1.0.7] - 2026-08-06

### 📝 Other Changes

- ci(github-actions): grant workflows write permission for build job ([5673e2b](../../commit/5673e2b))

## [1.0.6] - 2026-08-06

### 📝 Other Changes

- ci(github-actions): run build on self-hosted Windows runner ([14d7ef8](../../commit/14d7ef8))

## [1.0.5] - 2026-08-05

### 🐛 Bug Fixes

- fix(velopack): resolve vpk on demand and fall back to dotnet tools ([f929732](../../commit/f929732))

## [1.0.4] - 2026-08-05

### 📝 Other Changes

- ci(pipeline): MultiPlatform deploy ([b8d5120](../../commit/b8d5120))

## [1.0.3] - 2026-08-05

### ✨ Features

- feat(builder): add shared Fallout banner for migrate command ([dd427d1](../../commit/dd427d1))
- feat(builder): add central package management support ([91a2e13](../../commit/91a2e13))
- feat(builder): add global tool parsing and safer Fallout install flow ([b5b54ea](../../commit/b5b54ea))

### 🐛 Bug Fixes

- fix(builder): rename global tool command to autofallout ([6ad71fb](../../commit/6ad71fb))
- fix(builder): support centrally managed package versions in migrator ([3b1a41d](../../commit/3b1a41d))
- fix(build): switch package release to Azure Pipelines ([489c42d](../../commit/489c42d))

### 🔧 Chores

- chore(gitignore): restore and clarify Fallout build ignores ([e4eaa71](../../commit/e4eaa71))
- chore(build): ignore fallout temp files in build ([7732f21](../../commit/7732f21))

### 📝 Other Changes

- Untrack ignored .fallout files and tidy .gitignore ([6a1d49c](../../commit/6a1d49c))
- Fix the build? ([4aadcf1](../../commit/4aadcf1))
- Merged GitHub and AzureDevOps into one codebase. Made it more symmetrical. And got package management somewhat working. Still needs work on migrate on an existing central package management leaves versions in the project file. ([afdae8a](../../commit/afdae8a))
- Fallout migrate command ([09b2994](../../commit/09b2994))

## [1.0.2] - 2026-08-02

### 📝 Other Changes

- Point Builder package metadata at the Fallout repo ([80a11cf](../../commit/80a11cf))

## [1.0.1] - 2026-08-02

### 📝 Other Changes

- Fix GitVersion injection under Fallout 11 / System.Text.Json ([4a3d42f](../../commit/4a3d42f))

## [1.0.0] - 2026-08-02

### 📝 Other Changes

- Need it commited so I can create a tag... ([8ed3101](../../commit/8ed3101))

