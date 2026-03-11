# Changelog

All notable changes to Praefixum are documented here.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.1.0] - 2026-03-11

### Added

- **Sequential format** (`UniqueIdFormat.Sequential`) — 6-digit deterministic number derived from call site
- **Semantic format** (`UniqueIdFormat.Semantic`) — kebab-case method name + short hash (e.g. `create-button-a3f2`)
- **Instance method interception** — `[UniqueId]` now works on instance methods via generated extension-method-style interceptors
- **Compiler diagnostics** for attribute misuse:
  - `PRAEF001` (Warning): `[UniqueId]` on a non-string parameter
  - `PRAEF002` (Warning): `[UniqueId]` on a non-nullable string parameter
  - `PRAEF003` (Info): `[UniqueId]` parameter without a default value
- New test suites: `UniqueIdInstanceMethodTests`, `UniqueIdDiagnosticTests`
- Tests for Sequential and Semantic formats in `UniqueIdAttributeTests`
- Tests for `ulong`, `decimal`, and `enum` default value emission in `UniqueIdGeneratorDefaultValueTests`
- Demo methods `Badge` (Sequential) and `Alert` (Semantic) in `Html.cs` / `Program.cs`

### Changed

- **Removed `deterministic` parameter** from `UniqueIdAttribute` — the library is deterministic by design; the parameter was unused
- ID generation methods (`ShortHash`, `DeterministicGuid`, `DeterministicTimestamp`, `HtmlSafeId`) changed from `public` to `internal`
- `DevelopmentDependency` set to `true` in NuGet package metadata (source generators should not be a runtime dependency)
- Deduplicated ID generation logic: `TestHelpers.cs` now calls internal generator methods via `InternalsVisibleTo` instead of maintaining private copies
- Consolidated duplicate `ExtractId` helpers across test files into `TestHelpers.ExtractId`
- Updated all documentation to version 2.1.0

### Fixed

- Removed global `<NoWarn>CS8602</NoWarn>` suppression from `Praefixum.csproj`
- Removed duplicate `EmitCompilerGeneratedFiles` property in `Praefixum.Tests.csproj`
- Removed unused empty `_._` packaging artifact
- Fixed `.vscode/tasks.json` and `launch.json` referencing old project names and .NET 8.0
- Fixed `CONTRIBUTING.md` duplicated/outdated content

---

## [2.0.1] - 2026-02-25

### Fixed

- **Bool default values** now emit `false`/`true` instead of `False`/`True` in generated interceptors ([#6](https://github.com/MCGPPeters/Praefixum/issues/6))
- **Char default values** now emit `'x'` with proper single-quote syntax
- **Float/double/decimal default values** now emit correct suffixes (`f`, `d`, `m`) with invariant culture formatting
- **Long/ulong default values** now emit correct suffixes (`L`, `UL`)
- **Enum default values** now emit proper cast syntax `(EnumType)value`

### Added

- 16 new tests covering default value literal emission for all affected types

---

## [2.0.0] - 2026-02-18

### 🚀 First Major Release

This is the first stable major release of Praefixum, marking the project as production-ready.

### Added

- .NET 10 GA support with stable SDK 10.0.102
- Roslyn 5.0.0 source generator toolchain
- `global.json` to pin SDK version

### Changed

- **Breaking**: Minimum target framework for consumers is now .NET 10.0
- Upgraded `Microsoft.CodeAnalysis.CSharp` from 4.14.0 to 5.0.0
- Upgraded `Microsoft.CodeAnalysis.Analyzers` to 3.11.0
- Upgraded test infrastructure:
  - `Microsoft.NET.Test.Sdk` 17.9.0 → 18.0.1
  - `xunit` 2.7.0 → 2.9.3
  - `xunit.runner.visualstudio` 2.5.7 → 3.1.5
  - `coverlet.collector` 6.0.0 → 8.0.0
- CI pipeline updated to .NET 10.0.x
- Improved NuGet package metadata and discoverability

### Fixed

- `async void` test methods converted to `async Task` per xunit analyzer recommendations

---

## [1.2.0] - 2025-01-22

### Changed

- Switched license to Apache 2.0

## [1.1.8] - 2025-01-22

### Changed

- Made unique IDs compile-time constants for zero runtime overhead

## [1.1.7] - 2025-01-21

### Added

- Runtime `UniqueIdAttribute` and `UniqueIdFormat` types in the Praefixum assembly
- Regression test for transitive attribute duplication

### Changed

- Documentation updated for .NET 10 preview interceptors and current behavior
- Performance test baseline adjusted to avoid timing flakiness

### Fixed

- Prevented duplicate `UniqueIdAttribute` emission when referenced assemblies already define the types
- Removed NuGet pack warning by avoiding duplicate build output inclusion

## [1.1.5] - 2025-01-21

### Fixed

- Made timestamp format deterministic
- Adjusted performance test expectations

## [1.1.0] - 2025-01-20

### Added

- Multiple `[UniqueId]` parameter support in a single method
- Comprehensive documentation overhaul

### Fixed

- Test project configuration for source generators
- Duplicate `UniqueIdFormat` type warnings

## [1.0.x] - 2025-01-18 to 2025-01-20

### Added

- Initial source generator implementation with interceptor-based ID generation
- `UniqueIdAttribute` with format, prefix, and deterministic options
- Four ID formats: `Guid`, `HtmlId`, `Timestamp`, `ShortHash`
- Deterministic ID generation based on call-site location
- NuGet package with analyzer and build targets
- Demo project with HTML generation examples
- Comprehensive test suite (108 tests)

### Fixed

- Various source generator packaging and NuGet consumption issues
- GitHub Actions CI/CD pipeline configuration
- .NET interceptor compatibility across versions
