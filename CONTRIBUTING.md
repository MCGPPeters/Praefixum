# Contributing to Praefixum

Thank you for your interest in contributing to Praefixum! This document provides guidelines for maintaining and contributing to the project.

## 📁 Project Structure

```text
Praefixum/
├── Praefixum/                              # Main source generator project
│   ├── UniqueIdGenerator.cs                # Core Roslyn source generator implementation
│   ├── UniqueIdAttribute.cs                # Attribute definitions for marking parameters
│   └── Praefixum.csproj                   # Project file with source generator configuration
├── Praefixum.Demo/                        # Demo application showcasing usage
│   ├── Program.cs                         # Console demo application
│   ├── Html.cs                            # HTML generation examples
│   └── Praefixum.Demo.csproj             # Demo project file
├── Praefixum.Tests/                       # Comprehensive test suite
│   ├── UniqueIdGeneratorBasicTests.cs     # Core functionality tests
│   ├── UniqueIdGeneratorEdgeCaseTests.cs  # Edge cases and error conditions
│   ├── UniqueIdFormatTests.cs             # Format validation and compliance
│   ├── UniqueIdGeneratorPerformanceTests.cs # Performance and concurrency tests
│   ├── TestHelpers.cs                     # Reusable testing utilities
│   └── Praefixum.Tests.csproj            # Test project file
└── docs/                                  # Documentation files
    ├── README.md                          # Main documentation
    ├── CONTRIBUTING.md                    # This file
    ├── CHANGELOG.md                       # Version history
    └── REQUIREMENTS.md                    # Project requirements
```

## 🚀 Getting Started

### Prerequisites

- **.NET 10.0 SDK** or later
- **Visual Studio 2022 17.8+** or **JetBrains Rider 2023.3+**
- **Git** for version control

### Setting Up Development Environment

1. **Clone the repository**:
   ```bash
   git clone https://github.com/MCGPPeters/Praefixum.git
   cd Praefixum
   ```

2. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

3. **Build the solution**:
   ```bash
   dotnet build
   ```

4. **Run tests**:
   ```bash
   dotnet test
   ```

5. **Run the demo**:
   ```bash
   dotnet run --project Praefixum.Demo
   ```

## 🧪 Testing

### Test Categories

The test suite is organized into four main categories:

1. **Basic Tests** (`UniqueIdGeneratorBasicTests.cs`) - 12 tests
   - Core ID generation functionality
   - Attribute parsing and validation
   - Basic interceptor behavior

2. **Format Tests** (`UniqueIdFormatTests.cs`) - 11 tests
   - All supported ID formats (HtmlId, Hex8, Hex16, Hex32, Guid, etc.)
   - Format compliance and validation
   - Collision detection

3. **Edge Case Tests** (`UniqueIdGeneratorEdgeCaseTests.cs`) - 11 tests
   - Error conditions and invalid scenarios
   - Unusual code structures
   - Robustness testing

4. **Performance Tests** (`UniqueIdGeneratorPerformanceTests.cs`) - 10 tests
   - Concurrency and thread safety
   - Performance benchmarks
   - Memory efficiency

### Running Specific Test Categories

```bash
# Run all tests
dotnet test

# Run specific test files
dotnet test --filter "FullyQualifiedName~UniqueIdGeneratorBasicTests"
dotnet test --filter "FullyQualifiedName~UniqueIdFormatTests"
dotnet test --filter "FullyQualifiedName~UniqueIdGeneratorEdgeCaseTests"
dotnet test --filter "FullyQualifiedName~UniqueIdGeneratorPerformanceTests"

# Run tests with verbose output
dotnet test --verbosity normal
```

## 🔧 Development Guidelines

### Code Style

- Follow standard **C# coding conventions**
- Use **meaningful variable and method names**
- Add **XML documentation comments** to public APIs
- Use **nullable reference types** where appropriate
- Follow **async/await patterns** for asynchronous operations

### Source Generator Best Practices

- **Minimize build-time impact** - Only analyze relevant syntax nodes
- **Use incremental generation** where possible
- **Handle errors gracefully** with proper diagnostics
- **Write deterministic code** - same input should always produce same output
- **Avoid runtime dependencies** in generated code

### Adding New Features

When adding new features, follow this process:

1. **Write tests first** - Add tests in the appropriate test file
2. **Implement the feature** - Update the source generator logic
3. **Update documentation** - Modify README.md and code comments
4. **Test thoroughly** - Ensure all tests pass
5. **Update demo** - Add examples in the demo project if applicable

## 🆕 Adding New ID Formats

To add a new ID format:

1. **Add to enum**: Update `UniqueIdFormat` in `UniqueIdAttribute.cs`
2. **Implement generation**: Add case in `GenerateIdString` method in `UniqueIdGenerator.cs`
3. **Write tests**: Add format-specific tests in `UniqueIdFormatTests.cs`
4. **Update documentation**: Add format description to README.md
5. **Add demo usage**: Include examples in `Praefixum.Demo`

### Example: Adding a new format

```csharp
// 1. Add to enum
public enum UniqueIdFormat
{
    // ...existing formats...
    MyCustomFormat
}

// 2. Implement in generator
private static string GenerateIdString(string hash, UniqueIdFormat format, string prefix, string suffix)
{
    return format switch
    {
        // ...existing cases...
        UniqueIdFormat.MyCustomFormat => $"{prefix}custom_{hash[..8]}_{suffix}",
        _ => throw new ArgumentException($"Unsupported format: {format}")
    };
}

// 3. Add tests
[Fact]
public void MyCustomFormat_GeneratesCorrectFormat()
{
    // Test implementation
}
```

## 🔍 Debugging Source Generators

### Viewing Generated Code

Enable generated file output in your test project:

```xml
<PropertyGroup>
    <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
    <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)Generated</CompilerGeneratedFilesOutputPath>
</PropertyGroup>
```

Generated files will be in `obj/Debug/net10.0/Generated/`

### Debugging Tips

- Use **Debugger.Launch()** in generator code (remove before committing)
- Check **compilation diagnostics** in test failures
- Use **incremental generator** debugging features
- Test with **multiple target frameworks** if applicable

## 📝 Documentation

### Documentation Standards

- Keep **README.md** up-to-date with latest features
- Update **CHANGELOG.md** for all releases
- Include **code examples** for new features
- Use **clear, concise language**
- Add **troubleshooting sections** for common issues

### API Documentation

- Add **XML documentation** to all public APIs
- Include **parameter descriptions** and **return value** information
- Provide **usage examples** in documentation
- Document **exceptions** that may be thrown

## 🚀 Release Process

### Versioning

The project uses **Semantic Versioning (SemVer)**:

- **Patch (x.y.Z)**: Bug fixes, documentation updates
- **Minor (x.Y.0)**: New features, backward-compatible changes
- **Major (X.0.0)**: Breaking changes

### Release Steps

1. **Update version** in project files
2. **Update CHANGELOG.md** with new version
3. **Run full test suite** to ensure quality
4. **Create release tag**:
   ```bash
   git tag -a v2.1.0 -m "Release v2.1.0"
   git push origin v2.1.0
   ```
5. **GitHub Actions** will automatically build and publish to NuGet

## 🤝 Contributing Process

### Pull Request Guidelines

1. **Fork the repository** and create a feature branch
2. **Make your changes** following the guidelines above
3. **Add/update tests** for your changes
4. **Update documentation** if needed
5. **Ensure all tests pass**:
   ```bash
   dotnet test
   dotnet build
   ```
6. **Submit a pull request** with:
   - Clear description of changes
   - Reference to any related issues
   - Screenshots/examples if applicable

### Code Review Process

- All pull requests require **review approval**
- **CI/CD pipeline** must pass (build + tests)
- **Documentation** must be updated for new features
- **Breaking changes** require major version bump

## 🐛 Reporting Issues

When reporting bugs or requesting features:

1. **Check existing issues** first
2. **Use issue templates** when available
3. **Provide clear reproduction steps**
4. **Include system information** (.NET version, OS, etc.)
5. **Add code examples** demonstrating the issue

## 💡 Feature Requests

We welcome feature suggestions! Please:

1. **Search existing issues** for similar requests
2. **Describe the use case** clearly
3. **Propose implementation** if you have ideas
4. **Consider backward compatibility**

## 📞 Getting Help

- **GitHub Issues**: For bugs and feature requests
- **GitHub Discussions**: For questions and community support
- **Documentation**: Check README.md and code comments

Thank you for contributing to Praefixum! 🎉
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
