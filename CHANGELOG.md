# Changelog

All notable changes to the Praefixum project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.18] - 2025-05-27

### Fixed

- **Critical**: Fixed source generator NuGet package consumption
  - Resolved dual reference conflicts that prevented source generator from working when consumed as NuGet package
  - Fixed package structure: source generator now correctly placed in `analyzers/dotnet/cs/` folder only
  - Added proper MSBuild integration with build targets for NuGet packages
  - Removed unreliable PostInitialization code that was causing issues in packaged scenarios
  - Source generator now works correctly when consumed via NuGet package reference
- Improved package configuration:
  - Changed `IncludeBuildOutput` to `true` for proper assembly inclusion
  - Changed `DevelopmentDependency` to `false` to ensure package works in consuming projects
  - Added proper build and buildTransitive folders for MSBuild integration
- Cleaned up package contents and removed old package versions

### Added

- Created comprehensive test project to validate NuGet package functionality
- Added proper fallback pattern using null-coalescing operator (??) for generated constants

## [1.1.0] - 2025-05-23

### Changed

- Updated HtmlId format to be fully HTML5-compliant:
  - Increased length from 4 to 6 characters for better uniqueness
  - Added support for hyphens (-) and underscores (_) in IDs
  - Ensured all IDs start with a letter (HTML5 requirement)
  - Updated documentation and tests to reflect new format

## [1.0.2] - 2025-05-22

### Changed
- Updated publishing workflow
- Fixed GitHub Actions configuration

## [1.0.1] - 2025-05-22

### Fixed
- Fixed nullability warnings in source generator
- Improved NuGet package configuration
- Added proper symbol dictionary comparers for Roslyn compiler APIs
- Added PUBLISHING.md with detailed instructions for GitHub and NuGet deployment
- Added CHANGELOG.md and CHECKLIST.md for better release management

## [1.0.0] - 2025-05-22

### Added
- Initial release of Praefixum source generator
- Support for multiple ID formats:
  - Hex8 (8-character hexadecimal)
  - Hex16 (16-character hexadecimal, default)
  - Hex32 (32-character hexadecimal)
  - GUID (standard GUID format with dashes)
  - HtmlId (6-character HTML5-compliant ID)
- Deterministic ID generation based on code location
- Extensive documentation in README.md
- MIT license
- CI/CD pipeline with GitHub Actions
- Unit tests for all ID generation formats
