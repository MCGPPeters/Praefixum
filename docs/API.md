# Praefixum API Documentation

This document describes the public API and build-time behavior of Praefixum.

## Core Attributes

### UniqueIdAttribute

Marks a method parameter to receive a generated unique ID when the value is null at the call site.

```csharp
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class UniqueIdAttribute : Attribute
```

#### Constructor

```csharp
public UniqueIdAttribute(
    UniqueIdFormat format = UniqueIdFormat.Guid,
    string? prefix = null)
```

Parameters:

- `format` (`UniqueIdFormat`, optional): The format for the generated ID (default: `Guid`).
- `prefix` (`string`, optional): String to prepend to the generated ID.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Format` | `UniqueIdFormat` | The ID format to use |
| `Prefix` | `string?` | Prefix added to the generated ID |

#### Example Usage

```csharp
// Static method
public static string CreateButton(
    string text,
    [UniqueId(UniqueIdFormat.HtmlId, prefix: "btn-")] string? id = null)
{
    return $"<button id=\"{id}\">{text}</button>";
}

// Instance method
public class HtmlBuilder
{
    public string CreateElement(
        [UniqueId(UniqueIdFormat.HtmlId)] string? id = null,
        string content = "")
    {
        return $"<div id=\"{id}\">{content}</div>";
    }
}

// Multiple parameters
public static string CreateForm(
    [UniqueId(UniqueIdFormat.HtmlId, prefix: "form-")] string? formId = null,
    [UniqueId(UniqueIdFormat.HtmlId)] string? inputId = null,
    [UniqueId(UniqueIdFormat.Guid)] string? buttonId = null)
{
    return $@"
        <form id=\"{formId}\">
            <input id=\"{inputId}\" type=\"text\" />
            <button id=\"{buttonId}\">Submit</button>
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
    ShortHash,
    Sequential,
    Semantic
}
```

#### Values

| Value | Example | Description |
|-------|---------|-------------|
| `Guid` | `a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6` | 32-character deterministic GUID (no dashes) |
| `HtmlId` | `xa1b2c3d4` | HTML-compliant ID starting with a letter |
| `Timestamp` | `1735052327842` | Timestamp-like numeric string |
| `ShortHash` | `YWJjZGVm` | 8-character hash string |
| `Sequential` | `042817` | 6-digit deterministic number |
| `Semantic` | `create-button-a3f2` | Kebab-case method name + short hash |

## Compiler Diagnostics

Praefixum emits diagnostics when `[UniqueId]` is applied incorrectly:

| ID | Severity | Condition | Description |
|----|----------|-----------|-------------|
| `PRAEF001` | Warning | Non-string parameter | `[UniqueId]` is only supported on `string` parameters. Applied to a non-string parameter, the attribute is ignored and no interceptor is generated for that parameter. |
| `PRAEF002` | Warning | Non-nullable string | `[UniqueId]` requires a nullable `string?` parameter so the generator can detect when to supply a value. A non-nullable `string` parameter is excluded from interception. |
| `PRAEF003` | Info | No default value | `[UniqueId]` parameters should have a default value (typically `null`) so callers can omit the argument and let the generator fill it in. |

## Behavior

- The generator emits interceptor methods at build time for both static and instance methods.
- At invocation time, the generated interceptor supplies literal IDs for null parameters and calls the original method.
- IDs are derived from the call-site location at build time, making them deterministic.
- For instance methods, the generator emits extension-method-style interceptors with a `this` parameter.

## Generated Output

- The `[UniqueId]` attribute and `UniqueIdFormat` enum are part of the runtime assembly (`Praefixum`).
- The generator emits `PraefixumInterceptor.g.cs` into compiler-generated output.

## Project Requirements

To use interceptors, your project should enable preview features and allow the Praefixum namespace for interceptors:

```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <LangVersion>preview</LangVersion>
  <EnablePreviewFeatures>true</EnablePreviewFeatures>
  <InterceptorsNamespaces>$(InterceptorsNamespaces);Praefixum</InterceptorsNamespaces>
</PropertyGroup>
```
