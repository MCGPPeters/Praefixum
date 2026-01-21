# Praefixum Test Suite

This project contains the test suite for the Praefixum source generator.

## Test Categories

1. **Basic Tests** (`UniqueIdGeneratorBasicTests.cs`)
   - Core generator behavior
   - Multiple parameter support

2. **Format Tests** (`UniqueIdFormatTests.cs`)
   - `Guid`, `HtmlId`, `Timestamp`, and `ShortHash` formats
   - Prefix handling

3. **Edge Case Tests** (`UniqueIdGeneratorEdgeCaseTests.cs`)
   - Unusual code structures
   - Parameter positioning
   - Namespace handling

4. **Performance Tests** (`UniqueIdGeneratorPerformanceTests.cs`)
   - Baseline performance checks
   - Concurrency scenarios

5. **Transitive Tests** (`UniqueIdGeneratorTransitiveTests.cs`)
   - Verifies no duplicate `UniqueIdAttribute` emission when referenced assemblies already provide the types

## Running Tests

Run all tests:

```bash
dotnet test
```

Run specific categories:

```bash
# Basic functionality
dotnet test --filter "FullyQualifiedName~UniqueIdGeneratorBasicTests"

# Format validation
dotnet test --filter "FullyQualifiedName~UniqueIdFormatTests"

# Edge cases
dotnet test --filter "FullyQualifiedName~UniqueIdGeneratorEdgeCaseTests"

# Performance
dotnet test --filter "FullyQualifiedName~UniqueIdGeneratorPerformanceTests"

# Transitive behavior
dotnet test --filter "FullyQualifiedName~UniqueIdGeneratorTransitiveTests"
```

## Notes

- Interceptors require .NET 10 with preview features enabled.
- Generated output is emitted to the compiler-generated directory when `EmitCompilerGeneratedFiles` is enabled.
