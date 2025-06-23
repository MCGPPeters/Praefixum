# Troubleshooting Guide

This guide helps resolve common issues when using Praefixum.

## 🚨 Common Issues

### Issue: IDs are not being generated

**Symptoms:**
- Build succeeds but no unique IDs are generated
- `UniqueId.HtmlId()` calls return null or empty strings
- No interceptor code is generated

**Possible Causes:**

1. **Incorrect .NET version**
   ```xml
   <!-- ❌ Wrong - Interceptors require .NET 9 -->
   <TargetFramework>net8.0</TargetFramework>
   
   <!-- ✅ Correct -->
   <TargetFramework>net9.0</TargetFramework>
   ```

2. **Missing language version**
   ```xml
   <!-- ✅ Add this to your project file -->
   <PropertyGroup>
       <LangVersion>latest</LangVersion>
   </PropertyGroup>
   ```

3. **Incorrect parameter usage**
   ```csharp
   // ❌ Wrong - parameter not marked with [UniqueId]
   public void Method(string id = null)
   {
       id = UniqueId.HtmlId(); // Won't be intercepted
   }
   
   // ✅ Correct - parameter marked with [UniqueId]
   public void Method([UniqueId(UniqueIdFormat.HtmlId)] string id = null)
   {
       id = id ?? UniqueId.HtmlId(); // Will be intercepted
   }
   ```

**Solutions:**

1. Verify your project targets .NET 9.0 or later
2. Add `<LangVersion>latest</LangVersion>` to your project file
3. Ensure parameters are marked with `[UniqueId]` attribute
4. Check that `UniqueId.X()` calls flow to marked parameters

### Issue: Build errors with InterceptsLocation

**Symptoms:**
- Compilation errors mentioning `InterceptsLocationAttribute`
- Errors about missing interceptor attributes
- Build fails with interceptor-related messages

**Common Error Messages:**
```
CS9234: Cannot intercept: the indicated call is not interceptable
CS9235: Cannot intercept: the indicated call does not match the signature
CS9236: Cannot intercept: the indicated location does not exist
```

**Solutions:**

1. **Check .NET version compatibility**:
   ```bash
   dotnet --version  # Should be 9.0 or later
   ```

2. **Verify project configuration**:
   ```xml
   <Project Sdk="Microsoft.NET.Sdk">
     <PropertyGroup>
       <TargetFramework>net9.0</TargetFramework>
       <LangVersion>latest</LangVersion>
     </PropertyGroup>
   </Project>
   ```

3. **Clean and rebuild**:
   ```bash
   dotnet clean
   dotnet build
   ```

### Issue: Duplicate ID errors (UID001)

**Symptoms:**
- Diagnostic error UID001: Duplicate ID detected
- Multiple calls generating the same ID

**Example Problem:**
```csharp
// ❌ This causes UID001 - same line, same location
var id1 = UniqueId.HtmlId(); var id2 = UniqueId.HtmlId();
```

**Solutions:**

1. **Separate calls to different lines**:
   ```csharp
   // ✅ Different lines = different IDs
   var id1 = UniqueId.HtmlId();
   var id2 = UniqueId.HtmlId();
   ```

2. **Use different formats**:
   ```csharp
   // ✅ Different formats = different IDs
   var id1 = UniqueId.HtmlId();
   var id2 = UniqueId.Hex8();
   ```

3. **Use prefixes to differentiate**:
   ```csharp
   // ✅ Different prefixes = different IDs
   public void Method(
       [UniqueId(UniqueIdFormat.HtmlId, Prefix = "btn-")] string btnId = null,
       [UniqueId(UniqueIdFormat.HtmlId, Prefix = "div-")] string divId = null)
   ```

### Issue: Parameter not assigned warning (UID002)

**Symptoms:**
- Warning UID002: Parameter marked with [UniqueId] is not assigned a generator
- Parameter has attribute but no corresponding call

**Example Problem:**
```csharp
// ❌ Parameter marked but not assigned
public void Method([UniqueId(UniqueIdFormat.HtmlId)] string id)
{
    // No assignment to id parameter
}
```

**Solutions:**

1. **Add the corresponding call**:
   ```csharp
   // ✅ Assign with null-coalescing
   public void Method([UniqueId(UniqueIdFormat.HtmlId)] string id = null)
   {
       id = id ?? UniqueId.HtmlId();
   }
   ```

2. **Use default parameter pattern**:
   ```csharp
   // ✅ Common pattern for optional IDs
   public string CreateElement(
       [UniqueId(UniqueIdFormat.HtmlId)] string id = null)
   {
       return $"<div id=\"{id ?? UniqueId.HtmlId()}\">Content</div>";
   }
   ```

### Issue: Generated code not visible

**Symptoms:**
- IDs work but you can't see the generated code
- Want to inspect what the generator is creating

**Solution - Enable generated file output**:

```xml
<PropertyGroup>
    <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
    <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)Generated</CompilerGeneratedFilesOutputPath>
</PropertyGroup>
```

Generated files will be in: `obj/Debug/net9.0/Generated/Praefixum/`

### Issue: NuGet package not working

**Symptoms:**
- Source generator works locally but not when consumed via NuGet
- Package reference doesn't generate code

**Solutions:**

1. **Check package reference**:
   ```xml
   <!-- ✅ Correct package reference -->
   <PackageReference Include="Praefixum" Version="2.0.0" />
   ```

2. **Verify project configuration**:
   ```xml
   <PropertyGroup>
       <TargetFramework>net9.0</TargetFramework>
       <LangVersion>latest</LangVersion>
   </PropertyGroup>
   ```

3. **Clean and restore**:
   ```bash
   dotnet clean
   dotnet restore
   dotnet build
   ```

## 🔧 Debugging Techniques

### View Generated Code

To see what the source generator is creating:

1. **Add to your project file**:
   ```xml
   <PropertyGroup>
       <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
       <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)Generated</CompilerGeneratedFilesOutputPath>
   </PropertyGroup>
   ```

2. **Build the project**:
   ```bash
   dotnet build
   ```

3. **Check the generated files**:
   ```
   obj/Debug/net9.0/Generated/Praefixum/Praefixum.PraefixumSourceGenerator/
   ├── UniqueIdAttribute.g.cs
   └── UniqueIdInterceptors.g.cs
   ```

### Verify Source Generator Loading

Check if the source generator is being loaded:

```bash
# Enable detailed build output
dotnet build --verbosity diagnostic
```

Look for messages about source generators being loaded.

### Test with Minimal Example

Create a minimal test case:

```csharp
using Praefixum;

public class Test
{
    public static string CreateDiv([UniqueId(UniqueIdFormat.HtmlId)] string id = null)
    {
        return $"<div id=\"{id ?? UniqueId.HtmlId()}\">Test</div>";
    }
    
    public static void Main()
    {
        Console.WriteLine(CreateDiv()); // Should output unique ID
    }
}
```

## 🏗️ IDE-Specific Issues

### Visual Studio Issues

**Issue: IntelliSense not working**
- **Solution**: Restart Visual Studio after adding the package
- **Alternative**: Clear ComponentModelCache: Delete `%localappdata%\Microsoft\VisualStudio\17.0_xxxxx\ComponentModelCache`

**Issue: Generated code not updating**
- **Solution**: Build → Clean Solution, then Build → Rebuild Solution

### Rider Issues

**Issue: Source generator not recognized**
- **Solution**: File → Invalidate Caches and Restart
- **Alternative**: Enable "Use Roslyn Analyzer for Unity" in settings

### VS Code Issues

**Issue: C# extension not recognizing generated code**
- **Solution**: Restart the C# extension
- **Command**: `Ctrl+Shift+P` → "C#: Restart Language Server"

## 📊 Performance Issues

### Issue: Slow compilation

**Symptoms:**
- Build times significantly increased after adding Praefixum
- Source generator taking too long

**Solutions:**

1. **Check number of UniqueId usages**:
   - Generator performance scales with usage count
   - Consider batching or reducing calls if excessive

2. **Verify project structure**:
   - Avoid deep nesting or complex syntax
   - Keep source files reasonably sized

3. **Use build performance tools**:
   ```bash
   dotnet build --verbosity diagnostic 2>&1 | grep -i "generator\|time"
   ```

### Issue: Memory usage during build

**Symptoms:**
- High memory usage during compilation
- Build process consuming excessive RAM

**Solutions:**

1. **Limit parallel builds**:
   ```bash
   dotnet build -m:1  # Use single thread
   ```

2. **Increase available memory**:
   - Close other applications during build
   - Use build server with more RAM for CI/CD

## 🧪 Testing Issues

### Issue: Tests failing with generated IDs

**Symptoms:**
- Unit tests fail because IDs are not deterministic in tests
- Generated IDs different between test runs

**Solutions:**

1. **Don't test specific ID values**:
   ```csharp
   // ❌ Don't test exact values
   Assert.Equal("a1B2cD", CreateElement().GetId());
   
   // ✅ Test format compliance
   var element = CreateElement();
   Assert.Matches(@"^[a-zA-Z][a-zA-Z0-9]{5}$", element.GetId());
   ```

2. **Test ID presence and format**:
   ```csharp
   [Fact]
   public void CreateElement_GeneratesValidHtmlId()
   {
       var html = CreateElement();
       var idMatch = Regex.Match(html, @"id=""([^""]+)""");
       
       Assert.True(idMatch.Success);
       Assert.Matches(@"^[a-zA-Z][a-zA-Z0-9]{5}$", idMatch.Groups[1].Value);
   }
   ```

3. **Use dependency injection for testability**:
   ```csharp
   // Production code
   public interface IIdGenerator
   {
       string GenerateId();
   }
   
   // Test code can mock this interface
   ```

## 🌐 Environment-Specific Issues

### Issue: Works locally but not in CI/CD

**Symptoms:**
- Builds successfully on development machine
- Fails in CI/CD pipeline

**Common Causes & Solutions:**

1. **Different .NET versions**:
   ```yaml
   # GitHub Actions example
   - uses: actions/setup-dotnet@v3
     with:
       dotnet-version: '9.0.x'  # Ensure 9.0 or later
   ```

2. **Missing build tools**:
   ```dockerfile
   # Docker example
   FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
   ```

3. **File path differences**:
   - Generator uses absolute paths
   - Ensure consistent workspace structure

### Issue: Different behavior across operating systems

**Symptoms:**
- IDs generated differently on Windows vs Linux/macOS
- Path separator issues

**Solutions:**

1. **File paths are normalized internally** - this shouldn't be an issue
2. **If you see differences, please report as a bug**

## 📞 Getting Help

### Before Reporting Issues

1. **Check this troubleshooting guide**
2. **Search existing GitHub issues**
3. **Try with a minimal reproduction case**
4. **Verify .NET 9 and latest package version**

### Information to Include in Bug Reports

```
- Praefixum version: X.X.X
- .NET version: 9.0.X
- IDE: Visual Studio/Rider/VS Code (version)
- Operating System: Windows/macOS/Linux
- Project type: Console/Web/Library
- Minimal reproduction code
- Full error messages
- Build output (if relevant)
```

### Where to Get Help

- **GitHub Issues**: [https://github.com/MCGPPeters/Praefixum/issues](https://github.com/MCGPPeters/Praefixum/issues)
- **GitHub Discussions**: For questions and community support
- **Documentation**: Check README.md and API documentation

## 🔍 Advanced Debugging

### Source Generator Debugging

For contributors or advanced users who want to debug the source generator:

1. **Enable generator debugging**:
   ```xml
   <PropertyGroup>
       <IsRoslynComponent>true</IsRoslynComponent>
   </PropertyGroup>
   ```

2. **Add conditional debugging code** (remove before committing):
   ```csharp
   #if DEBUG
   if (!System.Diagnostics.Debugger.IsAttached)
   {
       System.Diagnostics.Debugger.Launch();
   }
   #endif
   ```

3. **Use logging in generator**:
   ```csharp
   context.ReportDiagnostic(Diagnostic.Create(
       new DiagnosticDescriptor("DEBUG001", "Debug", $"Value: {value}", 
       "Debug", DiagnosticSeverity.Info, true), Location.None));
   ```

### Analyzing Generated Assembly

To inspect the final compiled output:

```bash
# Use ILSpy, dotPeek, or similar tools
# Look for interceptor methods in the generated assembly
```

This troubleshooting guide should help resolve most common issues with Praefixum. If you encounter an issue not covered here, please open a GitHub issue with detailed information.
