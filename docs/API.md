# Praefixum API Documentation

This document provides detailed API documentation for the Praefixum source generator.

## Core Attributes

### UniqueIdAttribute

Marks a method parameter to receive a unique ID generated at compile time.

```csharp
[AttributeUsage(AttributeTargets.Parameter)]
public class UniqueIdAttribute : Attribute
```

#### Constructor

```csharp
public UniqueIdAttribute(UniqueIdFormat format, string prefix = "", string suffix = "")
```

**Parameters:**

- `format` (`UniqueIdFormat`): The format for the generated ID
- `prefix` (`string`, optional): String to prepend to the generated ID
- `suffix` (`string`, optional): String to append to the generated ID

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Format` | `UniqueIdFormat` | The ID format to use |
| `Prefix` | `string` | Prefix added to the generated ID |
| `Suffix` | `string` | Suffix added to the generated ID |

#### Example Usage

```csharp
public static string CreateButton(
    string text,
    [UniqueId(UniqueIdFormat.HtmlId, Prefix = "btn-")] string? id = null)
{
    return $"<button id=\"{id ?? UniqueId.HtmlId()}\">{text}</button>";
}
```

## Enumerations

### UniqueIdFormat

Defines the available formats for generated unique IDs.

```csharp
public enum UniqueIdFormat
```

#### Values

| Value | Example | Description |
|-------|---------|-------------|
| `HtmlId` | `a1B2cD` | 6-character HTML5-compliant ID starting with a letter |
| `Hex8` | `a1b2c3d4` | 8-character lowercase hexadecimal |
| `Hex16` | `a1b2c3d4e5f6g7h8` | 16-character lowercase hexadecimal |
| `Hex32` | `a1b2c3d4e5f6g7h8...` | 32-character lowercase hexadecimal (full MD5) |
| `Guid` | `12345678-1234-5678-9abc-123456789abc` | Standard GUID format with dashes |
| `Base64` | `YWJjZGVm` | URL-safe Base64 encoding |
| `AlphaOnly` | `ABCDEF` | Uppercase letters only (A-Z) |
| `Slug` | `item-ABC123` | Slug format with "item-" prefix |

#### Format Specifications

##### HtmlId Format

- **Length**: 6 characters
- **Character set**: `a-z`, `A-Z`, `0-9`
- **Rules**: Always starts with a letter (HTML5 compliant)
- **Collision resistance**: ~2.8 billion combinations
- **Use case**: HTML element IDs, CSS selectors

```csharp
[UniqueId(UniqueIdFormat.HtmlId)]
// Generates: "a1B2cD", "x9Y8zW", etc.
```

##### Hex8 Format

- **Length**: 8 characters
- **Character set**: `0-9`, `a-f` (lowercase)
- **Collision resistance**: ~4.3 billion combinations
- **Use case**: Short hexadecimal identifiers

```csharp
[UniqueId(UniqueIdFormat.Hex8)]
// Generates: "a1b2c3d4", "f5e6d7c8", etc.
```

##### Hex16 Format

- **Length**: 16 characters
- **Character set**: `0-9`, `a-f` (lowercase)
- **Collision resistance**: ~1.8 × 10^19 combinations
- **Use case**: Medium-length unique identifiers

```csharp
[UniqueId(UniqueIdFormat.Hex16)]
// Generates: "a1b2c3d4e5f6g7h8", etc.
```

##### Hex32 Format

- **Length**: 32 characters
- **Character set**: `0-9`, `a-f` (lowercase)
- **Collision resistance**: Full MD5 hash (extremely low collision probability)
- **Use case**: Maximum uniqueness requirements

```csharp
[UniqueId(UniqueIdFormat.Hex32)]
// Generates: "a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6", etc.
```

##### Guid Format

- **Length**: 36 characters (including dashes)
- **Format**: `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`
- **Character set**: `0-9`, `a-f` (lowercase)
- **Use case**: Standard GUID requirements

```csharp
[UniqueId(UniqueIdFormat.Guid)]
// Generates: "12345678-1234-5678-9abc-123456789abc", etc.
```

##### Base64 Format

- **Length**: Variable (typically 8-12 characters)
- **Character set**: `A-Z`, `a-z`, `0-9`, `-`, `_` (URL-safe)
- **Use case**: Compact encoding, URL parameters

```csharp
[UniqueId(UniqueIdFormat.Base64)]
// Generates: "YWJjZGVm", "SGVsbG8", etc.
```

##### AlphaOnly Format

- **Length**: Variable (typically 6-8 characters)
- **Character set**: `A-Z` (uppercase only)
- **Use case**: When only letters are allowed

```csharp
[UniqueId(UniqueIdFormat.AlphaOnly)]
// Generates: "ABCDEF", "XYZABC", etc.
```

##### Slug Format

- **Length**: Variable (`item-` + 6 characters)
- **Format**: `item-XXXXXX`
- **Character set**: `A-Z`, `0-9` (uppercase)
- **Use case**: URL slugs, readable identifiers

```csharp
[UniqueId(UniqueIdFormat.Slug)]
// Generates: "item-ABC123", "item-XYZ789", etc.
```

## Static Methods

### UniqueId Class

Provides static methods for generating unique IDs. These methods are intercepted by the source generator.

```csharp
public static class UniqueId
```

#### Methods

Each format has a corresponding static method:

```csharp
public static string HtmlId([CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
public static string Hex8([CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
public static string Hex16([CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
public static string Hex32([CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
public static string Guid([CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
public static string Base64([CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
public static string AlphaOnly([CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
public static string Slug([CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
```

**Parameters:**

- `filePath` (implicit): Filled by `[CallerFilePath]` attribute
- `lineNumber` (implicit): Filled by `[CallerLineNumber]` attribute

**Returns:**

- `string`: The generated unique ID

**Note**: These methods are intercepted by the source generator and replaced with compile-time constants. The actual method bodies are never executed.

## Usage Patterns

### Basic Usage

The most common pattern is to use default parameter values with null-coalescing:

```csharp
public static string CreateElement(
    string tag,
    [UniqueId(UniqueIdFormat.HtmlId)] string? id = null)
{
    return $"<{tag} id=\"{id ?? UniqueId.HtmlId()}\">{tag}</{tag}>";
}
```

### Multiple Parameters

You can have multiple `[UniqueId]` parameters in the same method:

```csharp
public static string CreateForm(
    [UniqueId(UniqueIdFormat.HtmlId, Prefix = "form-")] string? formId = null,
    [UniqueId(UniqueIdFormat.HtmlId, Prefix = "submit-")] string? submitId = null)
{
    return $@"
        <form id=""{formId ?? UniqueId.HtmlId()}"">
            <button id=""{submitId ?? UniqueId.HtmlId()}"" type=""submit"">Submit</button>
        </form>";
}
```

### Custom Prefixes and Suffixes

Customize the generated IDs with prefixes and suffixes:

```csharp
public static string CreateInput(
    [UniqueId(UniqueIdFormat.HtmlId, Prefix = "input-", Suffix = "-field")] string? id = null)
{
    return $"<input id=\"{id ?? UniqueId.HtmlId()}\" type=\"text\" />";
}

// Generates IDs like: "input-a1B2cD-field"
```

### Conditional ID Assignment

Only generate IDs when needed:

```csharp
public static string CreateDiv(
    string content,
    bool needsId = false,
    [UniqueId(UniqueIdFormat.HtmlId)] string? id = null)
{
    var idAttr = needsId ? $" id=\"{id ?? UniqueId.HtmlId()}\"" : "";
    return $"<div{idAttr}>{content}</div>";
}
```

## Compilation Behavior

### How Interception Works

1. **Analysis**: The source generator scans for method parameters with `[UniqueId]` attributes
2. **Call Detection**: Identifies calls to `UniqueId.X()` methods that flow to these parameters
3. **Hash Generation**: Creates a deterministic hash from file path, line number, and column
4. **Code Generation**: Generates interceptor methods with compile-time constants
5. **Replacement**: The compiler replaces the original calls with the generated constants

### Generated Code Example

For this source code:

```csharp
var button = CreateButton("Click me"); // File: Program.cs, Line: 15, Column: 14
```

The generator creates:

```csharp
[InterceptsLocation(@"D:\MyProject\Program.cs", 15, 14)]
private static string UniqueId_a1b2c3d4() => "a1B2cD";
```

### Determinism

IDs are generated deterministically based on:

- **File path**: Absolute path to the source file
- **Line number**: Line number of the call site
- **Column number**: Column number of the call site

The same call site will always generate the same ID across builds.

## Diagnostics

The source generator provides diagnostic messages for common issues:

### UID001: Duplicate ID Detected

**Severity**: Error

**Description**: Multiple calls at the same location would generate the same ID.

**Example**:

```csharp
// This would cause UID001 if both calls are on the same line
var id1 = UniqueId.HtmlId(); var id2 = UniqueId.HtmlId();
```

**Solution**: Move calls to separate lines or use different formats.

### UID002: Parameter Not Assigned

**Severity**: Warning

**Description**: A parameter marked with `[UniqueId]` is not assigned a `UniqueId.X()` call.

**Example**:

```csharp
public void Method([UniqueId(UniqueIdFormat.HtmlId)] string id)
{
    // id is not assigned a UniqueId.HtmlId() call
}
```

**Solution**: Assign the parameter with the corresponding `UniqueId.X()` method call.

## Performance Characteristics

### Compile Time

- **Minimal impact**: Generator only analyzes relevant syntax nodes
- **Incremental**: Uses incremental generation for efficiency
- **Scalable**: Performance scales linearly with number of `[UniqueId]` usages

### Runtime

- **Zero overhead**: All IDs are compile-time constants
- **No allocations**: Generated code contains only string literals
- **Thread-safe**: Constants are inherently thread-safe

### Memory

- **Constant memory**: Each unique ID is a single string constant
- **Shared strings**: Identical IDs (same location) share memory
- **Minimal footprint**: No runtime data structures

## Limitations

### Current Limitations

1. **File scope**: Uniqueness is guaranteed only within a single project
2. **Same line calls**: Multiple calls on the same line may conflict
3. **Format constraints**: Each format has specific character set limitations
4. **Interceptor requirements**: Requires .NET 9 and interceptor support

### Workarounds

1. **Use different formats**: Mix formats to avoid conflicts
2. **Separate lines**: Place calls on different lines
3. **Custom prefixes**: Use prefixes to namespace IDs
4. **Manual IDs**: Provide explicit IDs when needed

## Best Practices

### Do's

- ✅ Use meaningful prefixes for categorization
- ✅ Choose appropriate formats for your use case
- ✅ Keep calls on separate lines when possible
- ✅ Use null-coalescing operator (`??`) for default values
- ✅ Test generated code in your target environment

### Don'ts

- ❌ Don't rely on specific ID values in tests
- ❌ Don't use multiple calls on the same line
- ❌ Don't assume cross-project uniqueness
- ❌ Don't use in performance-critical paths during compilation
- ❌ Don't modify generated interceptor code

### Recommended Patterns

```csharp
// Good: Clear, maintainable pattern
public static string CreateCard(
    string title,
    string content,
    [UniqueId(UniqueIdFormat.HtmlId, Prefix = "card-")] string? id = null)
{
    return $@"
        <div id=""{id ?? UniqueId.HtmlId()}"" class=""card"">
            <h3>{title}</h3>
            <p>{content}</p>
        </div>";
}

// Good: Multiple unique parameters
public static string CreateTable(
    [UniqueId(UniqueIdFormat.HtmlId, Prefix = "table-")] string? tableId = null,
    [UniqueId(UniqueIdFormat.HtmlId, Prefix = "thead-")] string? headerId = null)
{
    return $@"
        <table id=""{tableId ?? UniqueId.HtmlId()}"">
            <thead id=""{headerId ?? UniqueId.HtmlId()}"">
                <!-- table content -->
            </thead>
        </table>";
}
```

This completes the API documentation for Praefixum. The documentation covers all public APIs, usage patterns, performance characteristics, and best practices for using the library.
