# UniqueIdGenerator

[![NuGet](https://img.shields.io/nuget/v/UniqueIdGenerator.svg)](https://www.nuget.org/packages/UniqueIdGenerator)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A C# source generator that creates deterministic, compile-time unique IDs for your methods with various format options. No runtime overhead, no external dependencies!

## Features

- 🚀 **Compile-time ID generation** - IDs are generated at build time, not runtime
- 🧩 **Multiple ID formats** - Choose from Hex8, Hex16, Hex32, GUID, or HTML-friendly formats
- 🔄 **Deterministic output** - Same inputs always produce the same IDs
- 📝 **Easy to use** - Just add an attribute to a parameter
- 🔍 **Zero runtime dependencies** - No reflection or runtime overhead
- 🎯 **Works with .NET Standard 2.0+** - Compatible with .NET Core, .NET 5+, and .NET Framework

## Installation

Install the NuGet package:

```bash
dotnet add package UniqueIdGenerator
```

### Via NuGet (Package Manager)

```shell
Install-Package UniqueIdGenerator
```

### Via .NET CLI

```shell
dotnet add package UniqueIdGenerator
```

### Direct Project Reference

Add a reference to the `UniqueIdGenerator.SourceGen` project:

```xml
<ItemGroup>
  <ProjectReference Include="..\UniqueIdGenerator.SourceGen\UniqueIdGenerator.SourceGen.csproj" 
                    OutputItemType="Analyzer" 
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

## Basic Usage

1. Add the UniqueIdGenerator namespace:

```csharp
using UniqueIdGenerator;
```

2. Create a method with a parameter marked with the `[UniqueId]` attribute and a default value:

```csharp
public string GenerateId([UniqueId] string id = null)
{
    // At compile time, the source generator will provide a constant unique ID
    return id ?? GenerateId_id_Id; // The constant is auto-generated
}
```

3. Call the method without providing a value for the parameter to get a unique ID:

```csharp
var id = GenerateId(); // Returns a unique ID based on code location
```

## Advanced Usage

### Choosing ID Formats

You can specify the format of the generated ID by providing a parameter to the `[UniqueId]` attribute:

```csharp
// Generate a 32-character hexadecimal ID
public string GetFullId([UniqueId(UniqueIdFormat.Hex32)] string id = null)
{
    return id ?? GetFullId_id_Id;
}

// Generate a GUID-formatted ID
public string GetGuidId([UniqueId(UniqueIdFormat.Guid)] string id = null)
{
    return id ?? GetGuidId_id_Id;
}

// Generate a short 8-character ID
public string GetShortId([UniqueId(UniqueIdFormat.Hex8)] string id = null)
{
    return id ?? GetShortId_id_Id;
}

// Generate HTML-friendly element IDs (4 characters)
public string GetHtmlElementId([UniqueId(UniqueIdFormat.HtmlId)] string id = null)
{
    return id ?? GetHtmlElementId_id_Id;
}
```

### Multiple Attributes on the Same Line

The source generator is designed to handle multiple `[UniqueId]` attributes used on the same line of code. For example:

```csharp
// These two methods can be on the same line and will still generate unique IDs
public string FirstId([UniqueId] string id = null) => id ?? FirstId_id_Id;
public string SecondId([UniqueId] string id = null) => id ?? SecondId_id_Id;
```

To ensure uniqueness, the generator considers both the line number and the character position of each attribute in the source file.

### Creating Prefixed IDs

You can combine the generated IDs with prefixes for different types of entities:

```csharp
public string CreateUserId([UniqueId] string id = null)
{
    return "usr_" + (id ?? CreateUserId_id_Id);
}

public string CreateOrderId([UniqueId(UniqueIdFormat.Guid)] string id = null)
{
    return "order_" + (id ?? CreateOrderId_id_Id);
}
```

### HTML Element IDs

The `HtmlId` format is specifically optimized for HTML elements, generating compact 4-character IDs that comply with HTML standards:

```csharp
// Generate HTML-friendly element IDs for different elements
public string GenerateButtonId([UniqueId(UniqueIdFormat.HtmlId)] string id = null) => id ?? GenerateButtonId_id_Id;
public string GenerateInputId([UniqueId(UniqueIdFormat.HtmlId)] string id = null) => id ?? GenerateInputId_id_Id;
public string GenerateDivId([UniqueId(UniqueIdFormat.HtmlId)] string id = null) => id ?? GenerateDivId_id_Id;
```

Sample HTML with the generated IDs:

```html
<form id="mscl">
  <div id="osx1">
    <input id="gft9" type="text" />
    <button id="z96u">Submit</button>
  </div>
</form>
```

Key features of the HTML ID format:
- Just 4 characters long for minimal markup size
- Always starts with a letter (required by HTML standards)
- Uses only lowercase letters and numbers (a-z, 0-9)
- Optimized algorithm to maximize uniqueness in a short format

## How It Works

The source generator:

1. Identifies parameters marked with the `[UniqueId]` attribute
2. Generates a unique ID based on:
   - File path
   - Method name
   - Parameter name
   - Line number
   - Character position (to handle multiple attributes on the same line)
3. Creates a partial class with public constants for each unique ID
4. The constant follows the naming pattern: `{MethodName}_{ParameterName}_Id`

When you compile your code, the source generator adds these constants to your classes, allowing you to reference them in your methods.

For HTML element IDs, the generator uses a specialized algorithm to create compact 4-character IDs that are optimized for HTML usage while still maintaining good uniqueness properties.

## Benefits Over Manual ID Generation

- **Compile-time Generation**: IDs are generated at compile time, so there's no runtime overhead
- **Deterministic**: The same code location always produces the same ID
- **Automatic**: No need to manually create and manage IDs
- **IDE Support**: Auto-completion works for the generated constants
- **Type Safety**: The generated IDs are strongly typed

## Requirements

- .NET SDK 6.0 or later
- C# 8.0 or later

## Usage

Basic usage is simple:

```csharp
using UniqueIdGenerator;

public partial class MyClass
{
    public string GetId([UniqueId] string id = null)
    {
        return id ?? GetId_id_Id;
    }
}
```

The source generator will create a constant string containing the unique ID:

```csharp
public partial class MyClass
{
    public const string GetId_id_Id = "8082640127c5d25b"; // Auto-generated unique ID
}
```

### ID Formats

The generator supports multiple ID formats:

```csharp
// Default 16-character hexadecimal (Hex16)
public string GetDefault([UniqueId] string id = null) => id ?? GetDefault_id_Id;

// 32-character hexadecimal (full MD5 hash)
public string GetHex32([UniqueId(UniqueIdFormat.Hex32)] string id = null) => id ?? GetHex32_id_Id;

// Standard GUID format with dashes
public string GetGuid([UniqueId(UniqueIdFormat.Guid)] string id = null) => id ?? GetGuid_id_Id;

// 8-character hexadecimal (short format)
public string GetHex8([UniqueId(UniqueIdFormat.Hex8)] string id = null) => id ?? GetHex8_id_Id;

// HTML-friendly ID (starts with a letter, uses alphanumeric)
public string GetHtmlId([UniqueId(UniqueIdFormat.HtmlId)] string id = null) => id ?? GetHtmlId_id_Id;
```

### Practical Examples

Create prefixed IDs for common use cases:

```csharp
// User ID with prefix
public string CreateUserId([UniqueId] string id = null) => "usr_" + (id ?? CreateUserId_id_Id);

// Session ID with prefix
public string CreateSessionId([UniqueId(UniqueIdFormat.Hex8)] string id = null) => "sess_" + (id ?? CreateSessionId_id_Id);

// Transaction ID with prefix
public string CreateTransactionId([UniqueId(UniqueIdFormat.Guid)] string id = null) => "tx_" + (id ?? CreateTransactionId_id_Id);
```

### HTML Element IDs

The `HtmlId` format is specifically designed for HTML elements:

```csharp
// Generate HTML-friendly IDs for elements
public string GenerateButtonId([UniqueId(UniqueIdFormat.HtmlId)] string id = null) => id ?? GenerateButtonId_id_Id;

// Usage
var buttonId = GenerateButtonId(); // e.g., "btn4a"
Console.WriteLine($"<button id=\"{buttonId}\">Click me</button>");
```

## Advanced Features

### Explicit Interface Implementation

Works with explicitly implemented interface methods:

```csharp
public interface IHasId
{
    string GetId(string id = null);
}

public partial class MyClass : IHasId
{
    string IHasId.GetId([UniqueId] string id) => id ?? GetId_id_Id;
}
```

### Static Classes

Works with static classes and methods:

```csharp
public static partial class IdGenerator
{
    public static string GetId([UniqueId] string id = null) => id ?? GetId_id_Id;
}
```

### Nested Classes

Works with nested classes:

```csharp
public partial class Outer
{
    public partial class Inner
    {
        public string GetId([UniqueId] string id = null) => id ?? GetId_id_Id;
    }
}
```

## Performance

Since all IDs are generated at compile time, there is zero runtime performance impact. The generated IDs are constants, which are optimized by the compiler.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Contributing

We welcome contributions to the UniqueIdGenerator project! Here's how you can help:

### Development Workflow

1. Fork the repository
2. Clone your fork: `git clone https://github.com/yourusername/UniqueIdGenerator.git`
3. Create a feature branch: `git checkout -b feature/your-feature-name`
4. Make your changes
5. Run tests: `dotnet test`
6. Commit your changes: `git commit -m "Add your meaningful commit message"`
7. Push to your fork: `git push origin feature/your-feature-name`
8. Create a Pull Request

### Running Tests

The project includes comprehensive tests. To run them:

```bash
dotnet test
```

### Building Locally

To build the project locally:

```bash
dotnet build
```

### Generating NuGet Package

To generate the NuGet package locally:

```bash
dotnet pack UniqueIdGenerator.SourceGen/UniqueIdGenerator.SourceGen.csproj -c Release
```

### Versioning

This project follows [Semantic Versioning](https://semver.org/). The version is automatically determined using GitVersion based on Git tags and commit history.

## Release Process

Releases are automated using GitHub Actions. To create a new release:

1. Update documentation if needed
2. Ensure all tests pass
3. Create a new Git tag following semantic versioning (e.g., `v1.2.3`)
4. Push the tag to GitHub

The CI/CD pipeline will automatically:
- Build the project
- Run tests
- Generate the NuGet package
- Publish the package to NuGet.org (if building from a tag)

For detailed publishing instructions, see [PUBLISHING.md](PUBLISHING.md).

