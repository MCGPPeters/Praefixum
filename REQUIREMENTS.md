# Requirements: Praefixum Unique ID Interceptor

## Overview

This project implements a compile-time unique ID generation system for C# using .NET 9 source generator interceptors. It enables deterministic and human-friendly unique identifiers to be inserted into function parameters that are explicitly marked with a `[UniqueId]` attribute. The IDs are generated based on file, line, and column location, ensuring uniqueness even when multiple calls are on the same line. The specifications of stable version of interceptors can be found [here](https://raw.githubusercontent.com/dotnet/roslyn/main/docs/features/interceptors.md)

## Goals

* Provide a declarative way to annotate method parameters for compile-time unique ID injection
* Ensure deterministic and globally unique values based on call-site
* Conform to relevant format standards (e.g., HTML5 element ID constraints)
* Provide various format options to suit use cases like HTML IDs, GUIDs, base encodings, etc.
* Offer prefix/suffix customization at the call site
* Prevent accidental collisions with diagnostics and logging

## Functional Requirements

### 1. Attributes

* `[UniqueId]` attribute must be applicable to method parameters
* It must accept:

  * `UniqueIdFormat` enum (required)
  * `Prefix` (optional string)
  * `Suffix` (optional string)

### 2. UniqueIdFormat Enum

Defines output formats:

* `HtmlId`: Compliant with HTML5 ID rules, e.g., `idabc123`
* `Guid`: RFC 4122 string with dashes
* `Hex8`: 8-character lowercase hex
* `Hex16`: 16-character hex
* `Hex32`: Full MD5 hash as lowercase hex string
* `Base36`: Uppercase base-36 string
* `Base64`: URL-safe base-64 encoding
* `Slug`: `item-<base36>` pattern
* `AlphaOnly`: Uppercase-only A-Z characters from hash
* `NamespaceQualified`: Combines symbol path and hash slug

### 3. Method Interception

* Only intercept calls to `UniqueId.X()` when:

  * Result is passed to a parameter with `[UniqueId]`
  * The call includes `CallerFilePath`, `CallerLineNumber`
* Replace the call with a string literal at compile time using `[InterceptsLocation]`

### 4. Hash Input

Use the concatenation of:

* Full file path
* Line number
* Column number

### 5. Emission Behavior

* Insert a partial method into a generated file with a constant return value
* Emit diagnostics:

  * `UID001`: Duplicate ID detected
  * `UID002`: Parameter marked with `[UniqueId]` is not assigned a generator
* Write a log of all generated IDs to `generated-ids.log`

### 6. Safety and Determinism

* Ensure IDs are stable across builds if file, line, and column remain constant
* Avoid runtime logic for ID generation

## Non-Functional Requirements

### Performance

* Minimal impact on compile-time performance
* Only analyze relevant symbols and syntax nodes

### Compatibility

* .NET 8 or later
* Must support both nullable and non-nullable ID parameter types (e.g., `string`, `string?`)

### Extensibility

* Easy to add new ID formats
* Format logic encapsulated in `GenerateLiteral` function

## Example Usage

```csharp
public static Node math(
    Attribute[] attributes,
    Node[] children,
    [UniqueId(UniqueIdFormat.HtmlId, Prefix = "html-")] string? id = null
) => element("math", attributes, children, id ?? UniqueId.HtmlId());
```

## Out of Scope

* Runtime ID generation
* Rewriting method definitions or parameter lists
* Ensuring ID uniqueness across projects (only per project guaranteed)

## Future Considerations

* Allow `[UniqueId]` on class fields or properties
* Allow `[UniqueId]` on local variables inside methods
* Tooling to auto-repair UID002
* Enable reporting to centralized ID registry or auditing dashboard
