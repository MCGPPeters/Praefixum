# Changelog

All notable changes to Praefixum are documented here.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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

---

Historical changelog entries prior to 1.1.7 were not maintained reliably. For earlier changes, refer to the git history.
