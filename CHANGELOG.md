# Changelog

All notable changes to the UniqueIdGenerator project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-05-22

### Added
- Initial release of UniqueIdGenerator source generator
- Support for multiple ID formats:
  - Hex8 (8-character hexadecimal)
  - Hex16 (16-character hexadecimal, default)
  - Hex32 (32-character hexadecimal)
  - GUID (standard GUID format with dashes)
  - HtmlId (4-character HTML-friendly ID)
- Deterministic ID generation based on code location
- Extensive documentation in README.md
- MIT license
- CI/CD pipeline with GitHub Actions
- Unit tests for all ID generation formats

### Fixed
- NuGet packaging configuration to properly include the analyzer DLL
