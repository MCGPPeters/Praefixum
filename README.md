# Praefixum - Unique ID Generator for .NET

[![Build Status](https://github.com/MCGPPeters/Praefixum/actions/workflows/ci.yml/badge.svg)](https://github.com/MCGPPeters/Praefixum/actions)
[![NuGet](https://img.shields.io/nuget/v/Praefixum.svg)](https://www.nuget.org/packages/Praefixum)
[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](LICENSE)

**Praefixum** is a .NET 10 source generator that emits interceptors for automatic unique ID generation. It enables deterministic, human-friendly identifiers to be generated for method parameters marked with the `[UniqueId]` attribute.

## 🚀 Features

- **Build-time interceptors** - Generated code handles IDs without manual wiring
- **Deterministic by call site** - Same code location generates stable IDs  
- **Multiple parameter support** - Handle multiple `[UniqueId]` parameters in a single method
- **Multiple formats** - Support for HTML IDs, GUIDs, timestamps, and hash-based IDs
- **Prefix support** - Add custom prefixes to generated IDs
- **Interceptor-based** - Uses .NET 10 preview interceptors for seamless integration
- **Thread-safe** - Generated IDs are constants, no concurrency issues
- **Zero dependencies** - Pure source generator implementation

## 📦 Installation

Install via NuGet Package Manager:

```bash
dotnet add package Praefixum
```

Or via Package Manager Console:

```powershell
Install-Package Praefixum
```

## 🎯 Quick Start

### 1. Define a method with UniqueId parameter

```csharp
using Praefixum;

public static string CreateButton(
    string text,
    [UniqueId(UniqueIdFormat.HtmlId)] string? id = null)
{
    return $"<button id=\"{id}\">{text}</button>";
}
```

### 2. Use the method

```csharp
// Each call generates a unique ID via the generated interceptor
var button1 = CreateButton("Click me");     // <button id="a1b2c3d4">Click me</button>
var button2 = CreateButton("Submit");       // <button id="x9y8z1w2">Submit</button>
```

### 3. Compilation magic ✨

The source generator emits interceptors that replace null `[UniqueId]` parameters with generated IDs based on the call location.

## 🎨 Supported ID Formats

| Format | Example | Description |
|--------|---------|-------------|
| `HtmlId` | `a1b2c3d4` | HTML5-compliant ID (starts with letter, 8 characters) |
| `Guid` | `a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6` | 32-character deterministic GUID (no dashes) |
| `Timestamp` | `1735052327842` | Unix timestamp in milliseconds |
| `ShortHash` | `YWJjZGVm` | 8-character Base64-encoded hash |

## 📖 Usage Examples

### HTML ID Generation

```csharp
public static string Div(
    string content,
    [UniqueId(UniqueIdFormat.HtmlId, prefix: "div-")] string? id = null)
{
    return $"<div id=\"{id}\">{content}</div>";
}

// Usage
var html = Div("Hello World"); // <div id="div-a1b2c3d4">Hello World</div>
```

### Multiple Parameters

**NEW**: Praefixum now supports multiple `[UniqueId]` parameters in a single method!

```csharp
public static string Form(
    [UniqueId(UniqueIdFormat.HtmlId, prefix: "form-")] string? formId = null,
    [UniqueId(UniqueIdFormat.HtmlId)] string? nameInputId = null,
    [UniqueId(UniqueIdFormat.HtmlId)] string? emailInputId = null,
    [UniqueId(UniqueIdFormat.Guid)] string? submitId = null)
{
    return $@"
        <form id=""{formId}"">
            <input id=""{nameInputId}"" name=""name"" type=""text"" />
            <input id=""{emailInputId}"" name=""email"" type=""email"" />
            <button id=""{submitId}"" type=""submit"">Submit</button>
        </form>";
}

// Usage - all IDs are automatically generated and unique
var form = Form(); 
// Generates: form-x1y2z3w4, a5b6c7d8, e9f0g1h2, i3j4k5l6m7n8o9p0q1r2s3t4u5v6w7x8
```

### Custom Prefixes

```csharp
public static string Input(
    [UniqueId(UniqueIdFormat.HtmlId, prefix: "input-")] string? id = null)
{
    return $"<input id=\"{id}\" type=\"text\" />";
}
```

### Different Formats in Same Method

```csharp
public static string Widget(
    [UniqueId(UniqueIdFormat.HtmlId)] string? elementId = null,
    [UniqueId(UniqueIdFormat.Timestamp)] string? timestampId = null,
    [UniqueId(UniqueIdFormat.Guid)] string? uuid = null)
{
    return $@"
        <div id=""{elementId}"" data-timestamp=""{timestampId}"" data-uuid=""{uuid}"">
            Widget Content
        </div>";
}
```

## 🏗️ How It Works

1. **Attribute Detection**: The source generator scans for methods with `[UniqueId]` parameters
2. **Call Site Analysis**: Identifies calls to methods with `[UniqueId]` parameters
3. **Interceptor Generation**: Creates interceptor methods for each call site using .NET 10 preview interceptors
4. **ID Generation**: Generates IDs from call-site location data at build time
5. **Invocation**: The interceptor fills null parameters with literal IDs and calls the original method

### Generated Code Example

For this source code:

```csharp
var form = CreateFormWithMultipleIds(); // Line 42, Column 12
```

The generator creates an interceptor that:
1. Checks if all `[UniqueId]` parameters are provided
2. If any are null, generates unique IDs based on call site + parameter index
3. Calls the original method with generated or provided IDs

```csharp
[InterceptsLocation(1, "D:\\MyProject\\Program.cs(42,12)")]
public static string CreateFormWithMultipleIds_0(
    string? formId = null,
    string? nameInputId = null, 
    string? emailInputId = null,
    string? submitButtonId = null)
{
    // Check if all parameters provided
    if (formId != null && nameInputId != null && emailInputId != null && submitButtonId != null)
        return OriginalClass.CreateFormWithMultipleIds(formId, nameInputId, emailInputId, submitButtonId);
    
    // Generate IDs for null parameters
    var formIdFinal = formId ?? GenerateId("key:0", 1, UniqueIdFormat.HtmlId, "form-");
    var nameInputIdFinal = nameInputId ?? GenerateId("key:1", 1, UniqueIdFormat.HtmlId, null);
    var emailInputIdFinal = emailInputId ?? GenerateId("key:2", 1, UniqueIdFormat.HtmlId, null);  
    var submitButtonIdFinal = submitButtonId ?? GenerateId("key:3", 1, UniqueIdFormat.Guid, null);
    
    return OriginalClass.CreateFormWithMultipleIds(formIdFinal, nameInputIdFinal, emailInputIdFinal, submitButtonIdFinal);
}
```

## 🧪 Testing

The project includes a comprehensive test suite covering:

- **Basic functionality** - Core ID generation and validation  
- **Multiple parameter support** - Methods with multiple `[UniqueId]` parameters
- **Format compliance** - Each format meets its specification
- **Edge cases** - Error conditions and unusual scenarios  
- **Performance** - Concurrency and load testing
- **Integration** - Real-world usage patterns with actual `[UniqueId]` attributes

Run tests with:

```bash
dotnet test
```

## 🔧 Requirements

- **.NET 10.0 or later** (for interceptor support)
- **C# preview** (interceptors are preview)
- **EnablePreviewFeatures** and **InterceptorsNamespaces** enabled in your project

```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <LangVersion>preview</LangVersion>
  <EnablePreviewFeatures>true</EnablePreviewFeatures>
  <InterceptorsNamespaces>$(InterceptorsNamespaces);Praefixum</InterceptorsNamespaces>
</PropertyGroup>
```

## 🤝 Contributing

We welcome contributions! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

### Development Setup

1. Clone the repository
2. Run `dotnet restore`
3. Run `dotnet build` to build all projects
4. Run `dotnet test` to execute the test suite

## 📋 Project Structure

```text
Praefixum/
├── Praefixum/                    # Main source generator project
│   ├── UniqueIdGenerator.cs      # Core source generator logic
│   └── UniqueIdAttribute.cs      # Attribute definitions
├── Praefixum.Demo/              # Demo application
│   ├── Program.cs               # Console demo
│   └── Html.cs                  # HTML generation examples
├── Praefixum.Tests/             # Comprehensive test suite
│   ├── UniqueIdGeneratorBasicTests.cs
│   ├── UniqueIdGeneratorEdgeCaseTests.cs
│   ├── UniqueIdFormatTests.cs
│   ├── UniqueIdGeneratorPerformanceTests.cs
│   └── TestHelpers.cs
└── docs/                        # Documentation
```

## 📈 Performance

- **No runtime ID generation** - IDs are compile-time literals in generated code
- **Fast compilation** - Minimal impact on build time
- **Memory efficient** - No runtime allocations for ID generation
- **Thread-safe** - Generated constants are inherently thread-safe

## 🐛 Troubleshooting

### Common Issues

**Q: IDs are not being generated**
A: Ensure you're using .NET 10 and have enabled preview features and interceptors in your project.

**Q: Duplicate ID errors**
A: This indicates the same call site is generating multiple IDs. Check your code structure.

**Q: Build errors with interceptors**
A: Verify your project targets .NET 10, `LangVersion` is `preview`, and preview features are enabled.

## 📄 License

This project is licensed under the Apache License 2.0 - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Built with [Roslyn](https://github.com/dotnet/roslyn) source generators
- Uses .NET 10 preview [interceptor feature](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-12.0/interceptors)
- Inspired by compile-time code generation patterns

Made with ❤️ for the .NET community
