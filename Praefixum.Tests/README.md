# Praefixum Test Suite

This project contains a comprehensive test suite for the Praefixum UniqueIdGenerator Roslyn source generator.

## Test Categories

The test suite is organized into the following categories:

1. **Basic Tests** (`UniqueIdGeneratorBasicTests.cs`)
   - Core functionality tests for the source generator
   - Attribute recognition and code generation
   - Basic method parameter replacement
   - Class and namespace handling

2. **Format Tests** (`UniqueIdFormatTests.cs`)
   - Tests for all supported ID formats (Hex8, Hex16, Hex32, Guid, HtmlId)
   - Verifies the structure and length of each format
   - Validates that the generated IDs match expected patterns
   - Format-specific edge cases and validation

3. **Edge Case Tests** (`UniqueIdGeneratorEdgeCaseTests.cs`)
   - Tests empty namespaces and global namespace handling
   - Tests generic classes and complex type scenarios
   - Tests non-default parameter names and positions
   - Tests unusual code structures and scenarios
   - Boundary conditions and error cases

4. **Performance Tests** (`UniqueIdGeneratorPerformanceTests.cs`)
   - Performance benchmarks for ID generation
   - Concurrency and thread-safety tests
   - Large-scale generation scenarios
   - Memory usage and efficiency validation

## Running Tests

To run all tests:

```bash
dotnet test
```

To run specific test categories:

```bash
# Basic functionality tests
dotnet test --filter "FullyQualifiedName~UniqueIdGeneratorBasicTests"

# Format validation tests  
dotnet test --filter "FullyQualifiedName~UniqueIdFormatTests"

# Edge case and boundary tests
dotnet test --filter "FullyQualifiedName~UniqueIdGeneratorEdgeCaseTests"

# Performance and concurrency tests
dotnet test --filter "FullyQualifiedName~UniqueIdGeneratorPerformanceTests"
```

To run tests with detailed output:

```bash
dotnet test --logger "console;verbosity=detailed"
```

## Test Architecture

The test suite uses a custom `TestHelpers` class and Microsoft.CodeAnalysis.Testing framework which:

1. Compiles test C# code using the Roslyn compiler
2. Applies the Praefixum UniqueIdGenerator source generator
3. Captures the generated source code
4. Verifies the generated code against expected patterns
5. Validates compilation success and absence of errors

This allows comprehensive testing of the source generator without requiring separate projects for each test case.

## Understanding Generated Code

The source generator produces the following for each test:

1. **InterceptsLocationAttribute**: Embedded in the same file as interceptors (for .NET 9 compatibility)
2. **Implementation files**: Named with pattern `{ClassName}_UniqueIds.g.cs`
3. **Interceptor methods**: Generate unique IDs and replace method calls at compile time

To find the correct implementation file in tests:

```csharp
var generatedSource = result.GeneratedSources.FirstOrDefault(
    s => s.HintName.Contains("MyClass_UniqueIds.g.cs"));
```

## Test Helpers

The `TestHelpers` class provides utilities for:

- **CreateTestCompilation**: Sets up Roslyn compilation with proper references
- **GetGeneratorOutput**: Runs the source generator and captures output
- **ValidateGeneration**: Common validation patterns for generated code
- **AssertContains/AssertNotContains**: String validation helpers

## Coverage Areas

### Format Testing

- **Hex8**: 8-character hexadecimal IDs
- **Hex16**: 16-character hexadecimal IDs  
- **Hex32**: 32-character hexadecimal IDs
- **Guid**: Standard GUID format
- **HtmlId**: HTML-compatible identifiers

### Scenario Testing

- Multiple attributes in same class
- Static and non-static classes
- Various namespace scenarios (including global namespace)
- Generic classes and complex types
- Parameter positioning and naming
- Concurrency and thread safety
- Performance under load

## Common Issues and Solutions

### 1. Generator Output Verification

The source generator creates interceptor files that are ephemeral (exist only during compilation). Tests verify the generated code structure and content without requiring the generated files to be written to disk.

### 2. InterceptsLocationAttribute Integration

In .NET 9, the `InterceptsLocationAttribute` is embedded within the same file as the interceptors, not as a separate file. Tests should expect this unified structure.

### 3. Static vs. Non-Static Classes

- **Static classes**: Generated code uses `static class` in interceptor declarations
- **Non-static classes**: Generated code uses regular class declarations for interceptors
- Tests validate the appropriate class modifier based on the source class type

### 4. Namespace Handling

- **Global namespace**: Classes are handled with appropriate global namespace syntax
- **Named namespaces**: Block-scoped namespace declarations are used in generated code
- **Nested namespaces**: Full namespace qualification is maintained

### 5. Generic Classes

Generic classes are supported with proper type parameter handling. Tests verify that generic constraints and type parameters are correctly preserved in generated interceptors.

### 6. Parameter Positioning

The `UniqueId` attribute can be applied to any parameter position. Tests verify that the correct parameter is replaced regardless of its position in the method signature.

## Troubleshooting Tests

If tests are failing, check the following:

1. **Generated file location**: Look for the correct hint name pattern (`{ClassName}_UniqueIds.g.cs`)
2. **Compilation errors**: Ensure the test source code compiles without the generator first
3. **Generator execution**: Verify the generator ran successfully and produced output
4. **Content verification**: Check that assertions match the current generator output format
5. **Reference setup**: Ensure all required references are included in the test compilation

For more detailed troubleshooting, see the main project's [TROUBLESHOOTING.md](../docs/TROUBLESHOOTING.md) guide.
