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
    string? prefix = null,
    bool deterministic = true)
```

Parameters:

- `format` (`UniqueIdFormat`, optional): The format for the generated ID (default: `Guid`).
- `prefix` (`string`, optional): String to prepend to the generated ID.
- `deterministic` (`bool`, optional): Reserved for future use. IDs are deterministic by call site.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Format` | `UniqueIdFormat` | The ID format to use |
| `Prefix` | `string?` | Prefix added to the generated ID |
| `Deterministic` | `bool` | Reserved for future use |

#### Example Usage

```csharp
public static string CreateButton(
    string text,
    [UniqueId(UniqueIdFormat.HtmlId, prefix: "btn-")] string? id = null)
{
    return $"<button id=\"{id}\">{text}</button>";
}

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
    ShortHash
}
```

#### Values

| Value | Example | Description |
|-------|---------|-------------|
| `Guid` | `a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6` | 32-character deterministic GUID (no dashes) |
| `HtmlId` | `xa1b2c3d4` | HTML-compliant ID starting with a letter |
| `Timestamp` | `1735052327842` | Timestamp-like numeric string |
| `ShortHash` | `YWJjZGVm` | 8-character hash string |

## Behavior

- The generator emits interceptor methods at build time.
- At invocation time, the generated interceptor supplies literal IDs for null parameters and calls the original method.
- IDs are derived from the call-site location at build time.

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
