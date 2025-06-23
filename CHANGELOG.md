# Changelog

All notable changes to the UniqueIdGenerator project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-05-22

### Added
- Initial release of UniqueIdGenerator
- Source generator for creating compile-time unique IDs
- Support for multiple ID formats:
  - Hex8 (8-character hexadecimal)
  - Hex16 (16-character hexadecimal)
  - Hex32 (32-character hexadecimal)
  - GUID (standard GUID format)
  - HtmlId (4-character HTML-friendly ID)
- Comprehensive test suite
- Demo project with usage examples
- Complete documentation in README.md

### Fixed
- NuGet packaging configuration
- Project structure for proper analyzer packaging

## [0.1.0] - 2025-05-21

### Added
- Initial development version
- Basic source generator implementation
- Hex16 format support
