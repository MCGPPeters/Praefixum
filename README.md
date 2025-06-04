# Praefixum - Unique ID Generator for .NET

[![Build Status](https://github.com/MCGPPeters/Praefixum/actions/workflows/ci.yml/badge.svg)](https://github.com/MCGPPeters/Praefixum/actions)
[![NuGet](https://img.shields.io/nuget/v/Praefixum.svg)](https://www.nuget.org/packages/Praefixum)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

**Praefixum** is a powerful .NET 9 source generator that provides compile-time unique ID generation using interceptors. It enables deterministic and human-friendly unique identifiers to be automatically inserted into method parameters marked with the `[UniqueId]` attribute.

## 🚀 Features

- **Compile-time ID generation** - No runtime overhead
- **Deterministic** - Same code location always generates the same ID
- **Multiple formats** - Support for HTML IDs, GUIDs, Hex, Base64, and more
- **Interceptor-based** - Uses .NET 9's interceptor feature for seamless integration
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
    return $"<button id=\"{id ?? UniqueId.HtmlId()}\">{text}</button>";
}
```

### 2. Use the method

```csharp
// Each call generates a unique ID at compile time
var button1 = CreateButton("Click me");     // <button id="a1B2cD">Click me</button>
var button2 = CreateButton("Submit");       // <button id="x9Y8zW">Submit</button>
```

### 3. Compilation magic ✨

The source generator automatically intercepts calls to `UniqueId.HtmlId()` and replaces them with compile-time constants based on the call location.

## 🎨 Supported ID Formats

| Format | Example | Description |
|--------|---------|-------------|
| `HtmlId` | `a1B2cD` | 6-character HTML5-compliant ID (starts with letter) |
| `Hex8` | `a1b2c3d4` | 8-character lowercase hexadecimal |
| `Hex16` | `a1b2c3d4e5f6g7h8` | 16-character lowercase hexadecimal |
| `Hex32` | `a1b2c3d4e5f6g7h8...` | 32-character lowercase hexadecimal |
| `Guid` | `12345678-1234-5678-9abc-123456789abc` | Standard GUID format |
| `Base64` | `YWJjZGVm` | URL-safe Base64 encoding |
| `AlphaOnly` | `ABCDEF` | Uppercase letters only |
| `Slug` | `item-ABC123` | Slug format with prefix |

## 📖 Usage Examples

### HTML ID Generation

```csharp
public static string Div(
    string content,
    [UniqueId(UniqueIdFormat.HtmlId, Prefix = "div-")] string? id = null)
{
    return $"<div id=\"{id ?? UniqueId.HtmlId()}\">{content}</div>";
}

// Usage
var html = Div("Hello World"); // <div id="div-a1B2cD">Hello World</div>
```

### Multiple Parameters

```csharp
public static string Form(
    [UniqueId(UniqueIdFormat.HtmlId)] string? formId = null,
    [UniqueId(UniqueIdFormat.HtmlId)] string? submitId = null)
{
    return $@"
        <form id=""{formId ?? UniqueId.HtmlId()}"">
            <button id=""{submitId ?? UniqueId.HtmlId()}"" type=""submit"">Submit</button>
        </form>";
}
```

### Custom Prefixes and Suffixes

```csharp
public static string Input(
    [UniqueId(UniqueIdFormat.HtmlId, Prefix = "input-", Suffix = "-field")] string? id = null)
{
    return $"<input id=\"{id ?? UniqueId.HtmlId()}\" type=\"text\" />";
}
```

## 🏗️ How It Works

1. **Attribute Detection**: The source generator scans for methods with `[UniqueId]` parameters
2. **Call Site Analysis**: Identifies calls to `UniqueId.X()` methods that flow to these parameters
3. **Hash Generation**: Creates a deterministic hash from file path, line number, and column
4. **Code Interception**: Uses .NET 9's interceptor feature to replace calls with compile-time constants
5. **Unique ID Generation**: Formats the hash according to the specified format

### Generated Code Example

For this source code:

```csharp
var html = CreateDiv("Hello"); // Line 42, Column 12
```

The generator creates:

```csharp
[InterceptsLocation(@"D:\MyProject\Program.cs", 42, 12)]
private static string UniqueId_a1b2c3d4() => "a1B2cD";
```

## 🧪 Testing

The project includes a comprehensive test suite with 44+ tests covering:

- **Basic functionality** - Core ID generation and validation
- **Format compliance** - Each format meets its specification
- **Edge cases** - Error conditions and unusual scenarios  
- **Performance** - Concurrency and load testing
- **Integration** - Real-world usage patterns

Run tests with:

```bash
dotnet test
```

## 🔧 Requirements

- **.NET 9.0 or later** (for interceptor support)
- **C# 12.0 or later**
- **Visual Studio 2022 17.8+** or **Rider 2023.3+**

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

- **Zero runtime overhead** - All IDs are compile-time constants
- **Fast compilation** - Minimal impact on build time
- **Memory efficient** - No runtime allocations for ID generation
- **Thread-safe** - Generated constants are inherently thread-safe

## 🐛 Troubleshooting

### Common Issues

**Q: IDs are not being generated**
A: Ensure you're using .NET 9 and have enabled interceptors in your project.

**Q: Duplicate ID errors**
A: This indicates the same call site is generating multiple IDs. Check your code structure.

**Q: Build errors with interceptors**
A: Verify your project targets .NET 9 and uses the latest C# language version.

### Diagnostic Codes

- `UID001`: Duplicate ID detected
- `UID002`: Parameter marked with `[UniqueId]` is not assigned a generator

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Built with [Roslyn](https://github.com/dotnet/roslyn) source generators
- Uses .NET 9's [interceptor feature](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-12.0/interceptors)
- Inspired by compile-time code generation patterns

Made with ❤️ for the .NET community