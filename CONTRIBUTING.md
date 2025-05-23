# Development Guide

This document provides guidelines for maintaining and contributing to the Praefixum project.

## Project Structure

- `UniqueIdGenerator.SourceGen/` - The main source generator implementation
  - `UniqueIdGenerator.cs` - The Roslyn source generator implementation
  - `UniqueIdAttribute.cs` - The attribute definition used to mark parameters
- `UniqueIdGenerator.Tests/` - Test suite for the source generator
- `UniqueIdGenerator.Demo/` - Demo project showing how to use the source generator

## Building the Project

```bash
dotnet build
```

## Running Tests

```bash
dotnet test
```

## Releasing a New Version

The project uses semantic versioning (SemVer) and GitVersion for automatic version management:

1. Decide on the type of release:
   - Patch (1.0.x): For bug fixes and minor changes that don't affect the API
   - Minor (1.x.0): For new features that are backward compatible
   - Major (x.0.0): For breaking changes

2. Create a git tag with the appropriate version:
   ```bash
   git tag -a v1.2.3 -m "Release description"
   git push origin v1.2.3
   ```

3. The GitHub Actions workflow will automatically:
   - Build the project
   - Run tests
   - Package the library
   - Publish to NuGet.org (when building from a tag)

## Code Style Guidelines

- Follow the standard C# coding conventions
- Use meaningful variable and method names
- Add XML documentation comments to public APIs
- Write unit tests for new features and bug fixes

## Performance Considerations

Since the Praefixum is used at build time:

- Try to minimize the build-time impact
- Avoid unnecessary computations
- For new ID formats, ensure they're deterministic and optimized

## Adding New Features

When adding new features:

1. Start by adding tests in the `UniqueIdGenerator.Tests` project
2. Implement the feature in the `UniqueIdGenerator.SourceGen` project
3. Update the demo project to showcase the new feature
4. Update the documentation (README.md and code comments)
5. Create a pull request with a detailed description

## Testing

The test suite includes:

- Unit tests for individual components
- Feature tests for specific functionalities
- Edge case tests for handling unusual scenarios
- Integration tests that verify the generator works as expected in real projects

## Common Maintenance Tasks

### Adding a New ID Format

1. Add the new format to the `UniqueIdFormat` enum in `UniqueIdAttribute.cs`
2. Update the `GenerateId` method in `UniqueIdGenerator.cs` to handle the new format
3. Add tests for the new format in `UniqueIdGeneratorFormatTests.cs`
4. Update the README.md to document the new format
5. Add example usage in the demo project
