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
public UniqueIdAttribute(UniqueIdFormat format = UniqueIdFormat.Guid, string? prefix = null, bool deterministic = true)
```

**Parameters:**

- `format` (`UniqueIdFormat`, optional): The format for the generated ID (default: `Guid`)
- `prefix` (`string`, optional): String to prepend to the generated ID 
- `deterministic` (`bool`, optional): Whether to generate deterministic IDs (default: `true`)

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Format` | `UniqueIdFormat` | The ID format to use |
| `Prefix` | `string?` | Prefix added to the generated ID |
| `Deterministic` | `bool` | Whether generation is deterministic |

#### Example Usage

```csharp
public static string CreateButton(
    string text,
    [UniqueId(UniqueIdFormat.HtmlId, prefix: "btn-")] string? id = null)
{
    return $"<button id=\"{id}\">{text}</button>";
}

// Multiple parameters with different formats
public static string CreateForm(
    [UniqueId(UniqueIdFormat.HtmlId, prefix: "form-")] string? formId = null,
    [UniqueId(UniqueIdFormat.HtmlId)] string? inputId = null,
    [UniqueId(UniqueIdFormat.Guid)] string? buttonId = null)
{
    return $@"
        <form id=""{formId}"">
            <input id=""{inputId}"" type=""text"" />
            <button id=""{buttonId}"">Submit</button>
        </form>";
}
```

## Enumerations

### UniqueIdFormat

Defines the available formats for generated unique IDs.

```csharp
public enum UniqueIdFormat
{
    Guid,
    HtmlId, 
    Timestamp,
    ShortHash
}
```

#### Values

| Value | Example | Description |
|-------|---------|-------------|
| `Guid` | `a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6` | 32-character deterministic GUID (no dashes) |
| `HtmlId` | `xa1b2c3d4` | HTML5-compliant ID starting with a letter (8+ chars) |
| `Timestamp` | `1735052327842` | Unix timestamp in milliseconds |
| `ShortHash` | `YWJjZGVm` | 8-character Base64-encoded hash |

#### Format Specifications

##### Guid Format

- **Length**: 32 characters
- **Character set**: `0-9`, `a-f` (lowercase hexadecimal)
- **Rules**: Deterministic based on call site and parameter index
- **Collision resistance**: Extremely high (SHA256-based)
- **Use case**: When maximum uniqueness is required

```csharp
[UniqueId(UniqueIdFormat.Guid)]
// Generates: "a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6"
```

##### HtmlId Format

- **Length**: 8+ characters (variable, starts with 'x' if needed)
- **Character set**: `a-z`, `A-Z`, `0-9`, `-`, `_` (Base64-safe)
- **Rules**: Always starts with a letter (HTML5 compliant)
- **Collision resistance**: High for typical usage
- **Use case**: HTML element IDs, CSS selectors

```csharp
[UniqueId(UniqueIdFormat.HtmlId)]
// Generates: "xa1b2c3d4", "abCdEfGh", etc.
```

##### Timestamp Format

- **Length**: Variable (typically 13 digits)
- **Character set**: `0-9` (digits only)
- **Rules**: Unix timestamp in milliseconds
- **Collision resistance**: Time-based (unique per millisecond)
- **Use case**: Time-sensitive identifiers, logs, caching

```csharp
[UniqueId(UniqueIdFormat.Timestamp)]
// Generates: "1735052327842", "1735052327843", etc.
```

##### ShortHash Format

- **Length**: 8 characters
- **Character set**: `A-Z`, `a-z`, `0-9`, `+`, `/` (Base64)
- **Rules**: Base64-encoded SHA256 hash (first 8 characters)
- **Collision resistance**: Good for most use cases
- **Use case**: Compact identifiers, URL parameters

```csharp
[UniqueId(UniqueIdFormat.ShortHash)]
// Generates: "YWJjZGVm", "SGVsbG8", etc.
```

## Usage Patterns

### Single Parameter

```csharp
public static string CreateDiv(
    [UniqueId(UniqueIdFormat.HtmlId)] string? id = null,
    string content = "")
{
    return $"<div id=\"{id}\">{content}</div>";
}

// Usage
var div = CreateDiv(content: "Hello"); // <div id="xa1b2c3d4">Hello</div>
```

### Multiple Parameters

**NEW in v2.1+**: Support for multiple `[UniqueId]` parameters in a single method.

```csharp
public static string CreateFormWithMultipleIds(
    [UniqueId(UniqueIdFormat.HtmlId, prefix: "form-")] string? formId = null,
    [UniqueId(UniqueIdFormat.HtmlId)] string? nameInputId = null,
    [UniqueId(UniqueIdFormat.HtmlId)] string? emailInputId = null,
    [UniqueId(UniqueIdFormat.Guid)] string? submitButtonId = null)
{
    return $@"
        <form id=""{formId}"">
            <input id=""{nameInputId}"" name=""name"" type=""text"" />
            <input id=""{emailInputId}"" name=""email"" type=""email"" />
            <button id=""{submitButtonId}"" type=""submit"">Submit</button>
        </form>";
}

// Usage - all parameters get unique IDs automatically
var form = CreateFormWithMultipleIds();
// Generates: 
// - formId: "form-xa1b2c3d4"
// - nameInputId: "xe5f6g7h8"  
// - emailInputId: "xi9j0k1l2"
// - submitButtonId: "m3n4o5p6q7r8s9t0u1v2w3x4y5z6a7b8"
```

### Mixed Parameters (Some with UniqueId, Some Without)

```csharp
public static string CreateComplexElement(
    string tag,                                                    // Required parameter
    [UniqueId(UniqueIdFormat.HtmlId)] string? id = null,          // UniqueId parameter
    string? className = null,                                      // Optional parameter
    string content = "")                                           // Optional parameter
{
    var classAttr = string.IsNullOrEmpty(className) ? "" : $" class=\"{className}\"";
    return $"<{tag} id=\"{id}\"{classAttr}>{content}</{tag}>";
}

// Usage
var element = CreateComplexElement("section", className: "highlight", content: "Important");
// Generates: <section id="xa1b2c3d4" class="highlight">Important</section>
```

### Custom Prefixes

```csharp
public static string CreateWidget(
    [UniqueId(UniqueIdFormat.HtmlId, prefix: "widget-")] string? id = null,
    [UniqueId(UniqueIdFormat.Timestamp, prefix: "ts-")] string? timestampId = null)
{
    return $"<div id=\"{id}\" data-timestamp=\"{timestampId}\">Widget Content</div>";
}

// Usage  
var widget = CreateWidget();
// Generates: <div id="widget-xa1b2c3d4" data-timestamp="ts-1735052327842">Widget Content</div>
```

### Explicit Values (Overriding Generation)

```csharp
// When you provide explicit values, they are used instead of generating new ones
var button1 = CreateButton("Click", id: "my-custom-id");      // Uses "my-custom-id"
var button2 = CreateButton("Submit");                         // Generates unique ID

var form = CreateFormWithMultipleIds(
    formId: "contact-form",           // Explicit ID used
    emailInputId: "user-email");      // Other parameters get generated IDs
```

## Return Type Support

The source generator preserves all return types correctly:

```csharp
// String return type
public static string GetElementId([UniqueId] string? id = null) 
    => id ?? "default";

// Int return type  
public static int GetHashFromId([UniqueId(UniqueIdFormat.HtmlId)] string? id = null) 
    => id?.GetHashCode() ?? 0;

// Bool return type
public static bool IsValidId([UniqueId(UniqueIdFormat.Guid)] string? id = null) 
    => !string.IsNullOrEmpty(id);

// Void return type
public static void ProcessElement([UniqueId(prefix: "process-")] string? id = null) 
    => Console.WriteLine($"Processing {id}");

// Async return types
public static async Task<string> CreateElementAsync([UniqueId] string? id = null) 
{
    await Task.Delay(1);
    return $"<div id=\"{id}\"></div>";
}

// Generic return types
public static List<T> CreateListWithId<T>(T[] items, [UniqueId] string? id = null) 
{
    Console.WriteLine($"List ID: {id}");
    return new List<T>(items);
}
```

## Advanced Scenarios

### Deterministic vs Non-Deterministic

```csharp
// Deterministic (default) - same call site always generates same ID
[UniqueId(UniqueIdFormat.HtmlId, deterministic: true)]  

// Non-deterministic - includes additional entropy (not fully implemented)
[UniqueId(UniqueIdFormat.HtmlId, deterministic: false)]
```

### Call Site Uniqueness

Each call site gets unique IDs, even for the same method:

```csharp
var button1 = CreateButton("First");   // ID: xa1b2c3d4
var button2 = CreateButton("Second");  // ID: xe5f6g7h8 (different call site)

// Even in loops, each iteration gets the same ID (deterministic)
for (int i = 0; i < 3; i++)
{
    var btn = CreateButton($"Button {i}"); // Same ID: xi9j0k1l2 for all iterations
}
```

### Parameter Index Uniqueness

For multiple parameters, each parameter gets a unique ID based on its position:

```csharp
var form = CreateFormWithMultipleIds(); 
// formId (index 0):        "form-xa1b2c3d4"
// nameInputId (index 1):   "xe5f6g7h8"  
// emailInputId (index 2):  "xi9j0k1l2"
// submitButtonId (index 3): "m3n4o5p6q7r8s9t0u1v2w3x4y5z6a7b8"
```

## Error Handling

### Compilation Errors

The source generator will produce compilation errors for:

- Methods with `[UniqueId]` parameters that don't have string types
- Invalid format specifications
- Circular dependencies in generated code

### Runtime Behavior

- If explicit values are provided for all `[UniqueId]` parameters, the original method is called directly
- If any `[UniqueId]` parameters are null, the interceptor generates IDs and calls the original method
- Generated IDs are always non-null and non-empty strings
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
