# Troubleshooting Guide

This guide helps resolve common issues when using Praefixum.

## Common Issues

### Issue: IDs are not being generated

Symptoms:
- Methods run but IDs remain null.
- No interceptor code is generated.

Possible causes and fixes:

1. **Incorrect .NET version**
   ```xml
   <!-- Correct -->
   <TargetFramework>net10.0</TargetFramework>
   ```

2. **Preview features not enabled**
   ```xml
   <PropertyGroup>
     <LangVersion>preview</LangVersion>
     <EnablePreviewFeatures>true</EnablePreviewFeatures>
   </PropertyGroup>
   ```

3. **Interceptors namespace not allowed**
   ```xml
   <PropertyGroup>
     <InterceptorsNamespaces>$(InterceptorsNamespaces);Praefixum</InterceptorsNamespaces>
   </PropertyGroup>
   ```

4. **Parameter is never null**
   ```csharp
   // IDs are only generated when the [UniqueId] parameter is null
   public static string CreateDiv([UniqueId] string? id = null)
   {
       return $"<div id=\"{id}\"></div>";
   }
   ```

### Issue: Build errors with interceptors

Symptoms:
- Compilation errors mentioning `InterceptsLocationAttribute`.
- Errors like `CS9234/CS9235/CS9236`.

Fixes:

1. Ensure the project targets .NET 10 with preview features enabled.
2. Clean and rebuild:
   ```bash
   dotnet clean
   dotnet build
   ```

### Issue: Generated code not visible

If you want to inspect generated output, enable compiler-generated files:

```xml
<PropertyGroup>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)Generated</CompilerGeneratedFilesOutputPath>
</PropertyGroup>
```

After a build, check:

```
obj/Debug/net10.0/Generated/Praefixum/Praefixum.PraefixumSourceGenerator/
└── PraefixumInterceptor.g.cs
```

Note: `UniqueIdAttribute` is part of the runtime assembly, so `UniqueIdAttribute.g.cs` is not expected.

### Issue: NuGet package not working

- Make sure the package reference is correct and restored:
  ```xml
  <PackageReference Include="Praefixum" Version="1.1.7" />
  ```
- Ensure preview features and interceptors are enabled (see above).
- Clean and rebuild after upgrading.

## Minimal Example

```csharp
using Praefixum;

public static class Demo
{
    public static string CreateDiv([UniqueId(UniqueIdFormat.HtmlId)] string? id = null)
    {
        return $"<div id=\"{id}\">Test</div>";
    }
}
```

## CI/CD Notes

If builds succeed locally but fail in CI:

- Ensure the SDK is .NET 10 and preview features are supported.
- Verify the pipeline uses a compatible SDK image (for example: `mcr.microsoft.com/dotnet/sdk:10.0`).

## Getting Help

If you still have issues:

- Review the project README.
- Search existing GitHub issues.
- Open a new issue with a minimal repro and build output.
