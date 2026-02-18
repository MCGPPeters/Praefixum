# Changelog

All notable changes to Praefixum are documented here.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
