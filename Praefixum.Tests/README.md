# Praefixum Test Suite

This project contains a comprehensive test suite for the Praefixum source generator.

## Test Categories

The test suite is organized into the following categories:

1. **Format Tests** (`UniqueIdGeneratorFormatTests.cs`)
   - Tests for all supported ID formats (Hex16, Hex32, Guid, Hex8, HtmlId)
   - Verifies the structure and length of each format
   - Validates that the generated IDs match expected patterns

2. **Feature Tests** (`UniqueIdGeneratorFeatureTests.cs`)
   - Tests multiple attributes on the same line
   - Tests methods with multiple parameters with UniqueId attributes
   - Tests interaction between multiple unique ID generators in the same class

3. **Integration Tests** (`UniqueIdGeneratorIntegrationTests.cs`)
   - Tests real-world scenarios and combinations
   - Tests nested namespaces
   - Tests static versus non-static classes
   - Tests various formatting options together

4. **Edge Case Tests** (`UniqueIdGeneratorEdgeCaseTests.cs`)
   - Tests empty namespaces
   - Tests generic classes
   - Tests non-default parameter names
   - Tests unusual code structures

## Running Tests

To run all tests:

```bash
dotnet test
```

To run specific categories:

```bash
dotnet test --filter "FullyQualifiedName~UniqueIdGeneratorFormatTests"
dotnet test --filter "FullyQualifiedName~UniqueIdGeneratorFeatureTests"
dotnet test --filter "FullyQualifiedName~UniqueIdGeneratorIntegrationTests"
dotnet test --filter "FullyQualifiedName~UniqueIdGeneratorEdgeCaseTests"
```

## Test Architecture

The test suite uses a custom `SourceGeneratorVerifier` class which:

1. Compiles test C# code using the Roslyn compiler
2. Applies the Praefixum source generator
3. Gets the generated source code
4. Verifies the generated code against expected patterns

This allows testing the source generator without having to set up a separate project for each test case.

## Understanding Generated Files

The source generator produces at least two files for each test:

1. The attribute definition file (`UniqueIdAttribute.cs`)
2. One or more implementation files with naming pattern `{ClassName}_UniqueIds.g.cs`

To find the correct implementation file in tests, use the file hint name pattern:

```csharp
var generatedSource = result.GeneratedSources.FirstOrDefault(
    s => s.HintName.Contains("MyClass_UniqueIds.g.cs"));
```

## Common Issues and Solutions

### 1. Multiple Output Files

When a test is looking for a specific implementation, make sure to filter by the correct hint name pattern rather than just the content.

### 2. Static vs. Non-Static Classes

For static classes, verify that the generated code uses `static class` in the declaration. For non-static classes, it should use `partial class`.

### 3. Nested Classes

Nested classes may have their constants generated in either the outer class or their own implementation file. Tests should be flexible enough to find constants in either place.

### 4. Generic Classes

When testing generic classes, be aware that the generic type parameter may be handled differently in the output. Tests should verify the class is correctly identified without being overly strict on the exact generic format.

### 5. Global Namespace

Classes in the global namespace should be handled appropriately. The source generator may still include a namespace declaration for these classes.

## Troubleshooting Tests

If tests are failing, check the following:

1. Is the test looking for the correct generated file? Use the hint name pattern.
2. Are the assertions correctly matching the output format from the latest source generator?
3. Is there potentially another file that better represents the test output?
