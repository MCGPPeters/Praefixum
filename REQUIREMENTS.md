# Requirements: Praefixum Unique ID Interceptors

## Overview

Praefixum provides a build-time source generator that emits interceptors for methods with `[UniqueId]` parameters. When a call site passes `null` for a `[UniqueId]` parameter, the generated interceptor supplies a literal unique ID and calls the original method.

## Goals

- Provide a declarative way to annotate parameters for automatic ID generation.
- Generate deterministic, human-friendly IDs based on call-site data.
- Support multiple output formats with optional prefixes.
- Avoid runtime ID generation by emitting compile-time literals.

## Functional Requirements

### 1. Attributes

- `[UniqueId]` is applicable to method parameters.
- Constructor parameters:
  - `UniqueIdFormat format` (optional, default `Guid`)
  - `string? prefix` (optional)
  - `bool deterministic` (optional, reserved for future use)

### 2. UniqueIdFormat Enum

Supported formats:

- `Guid`
- `HtmlId`
- `Timestamp`
- `ShortHash`

### 3. Method Interception

- The generator identifies invocations of methods that include `[UniqueId]` parameters.
- Interceptors are generated using the compiler-provided interceptable locations.
- Interceptors fill null `[UniqueId]` parameters and then call the original method.

### 4. ID Generation

- IDs are derived from call-site location data at build time.
- Prefixes (if provided) are prepended to generated IDs.

### 5. Emission Behavior

- The runtime assembly provides `UniqueIdAttribute` and `UniqueIdFormat`.
- The generator emits interceptor code into compiler-generated output.
- No diagnostics are currently emitted by the generator.

## Non-Functional Requirements

### Performance

- Minimal impact on compile-time performance.
- The generator should only analyze relevant syntax nodes and symbols.

### Compatibility

- .NET 10 or later (preview interceptors).
- C# preview with `EnablePreviewFeatures` enabled.
- Supports `string` and `string?` parameter types.

### Extensibility

- New ID formats should be easy to add within the generator.

## Example Usage

```csharp
public static string CreateDiv(
    [UniqueId(UniqueIdFormat.HtmlId, prefix: "html-")] string? id = null)
{
    return $"<div id=\"{id}\"></div>";
}
```

## Out of Scope

- Runtime ID generation APIs.
- Rewriting method definitions or parameter lists.
- Cross-project uniqueness guarantees.
