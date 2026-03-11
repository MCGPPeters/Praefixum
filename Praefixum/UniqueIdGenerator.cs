// =======================
// PraefixumSourceGenerator.cs
// =======================
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Security.Cryptography;
using System.Text;

namespace Praefixum;

[Generator]
public sealed class PraefixumSourceGenerator : IIncrementalGenerator
{
    // Diagnostic descriptors for [UniqueId] misuse
    private static readonly DiagnosticDescriptor NonStringParameter = new(
        id: "PRAEF001",
        title: "UniqueId applied to non-string parameter",
        messageFormat: "The [UniqueId] attribute should only be applied to parameters of type 'string?' but was applied to parameter '{0}' of type '{1}'",
        category: "Praefixum",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor NonNullableParameter = new(
        id: "PRAEF002",
        title: "UniqueId applied to non-nullable parameter",
        messageFormat: "The [UniqueId] attribute should be applied to nullable parameters (string?) but parameter '{0}' is non-nullable. The generated ID cannot be injected when the parameter is not null",
        category: "Praefixum",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor NoDefaultValue = new(
        id: "PRAEF003",
        title: "UniqueId parameter has no default value",
        messageFormat: "The [UniqueId] parameter '{0}' should have a default value of null so callers can omit it and receive a generated ID",
        category: "Praefixum",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Register the attribute source if not already provided by a referenced assembly
        context.RegisterSourceOutput(context.CompilationProvider, static (ctx, compilation) =>
        {
            var hasAttribute = HasType(compilation, "Praefixum", "UniqueIdAttribute");
            var hasFormat = HasType(compilation, "Praefixum", "UniqueIdFormat");
            if (hasAttribute || hasFormat)
                return;

            ctx.AddSource("UniqueIdAttribute.g.cs", SourceText.From("""
                #nullable enable
                namespace Praefixum
                {
                    [System.AttributeUsage(System.AttributeTargets.Parameter)]
                    public sealed class UniqueIdAttribute : System.Attribute
                    {
                        public UniqueIdFormat Format { get; }
                        public string? Prefix { get; }

                        public UniqueIdAttribute(UniqueIdFormat format = UniqueIdFormat.Guid, string? prefix = null)
                        {
                            Format = format;
                            Prefix = prefix;
                        }
                    }

                    public enum UniqueIdFormat
                    {
                        Guid,
                        HtmlId,
                        Timestamp,
                        ShortHash,
                        Sequential,
                        Semantic
                    }
                }
                """, Encoding.UTF8));
        });

        // Find method calls that need interception and collect diagnostics
        var methodCallsProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is InvocationExpressionSyntax,
                transform: static (ctx, _) => GetMethodCallInfo(ctx))
            .Where(static m => m is not null)
            .Collect();

        // Generate interceptors
        context.RegisterSourceOutput(methodCallsProvider, static (ctx, calls) =>
        {
            var validCalls = new List<MethodCallInfo>();

            foreach (var item in calls)
            {
                if (item is null) continue;

                // Report diagnostics
                foreach (var diag in item.Diagnostics)
                {
                    ctx.ReportDiagnostic(diag);
                }

                // Only include calls with valid method call info
                if (item.IsValid)
                {
                    validCalls.Add(item);
                }
            }

            if (validCalls.Count == 0) return;

            var interceptorSource = GenerateInterceptorSource(validCalls.ToArray());
            ctx.AddSource("PraefixumInterceptor.g.cs", SourceText.From(interceptorSource, Encoding.UTF8));
        });
    }

    private static MethodCallInfo? GetMethodCallInfo(GeneratorSyntaxContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        var semanticModel = context.SemanticModel;

        if (semanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol method)
            return null;

        // Check if any parameter has UniqueIdAttribute
        var parametersWithUniqueId = method.Parameters
            .Select((param, index) => new { param, index })
            .Where(x => x.param.GetAttributes().Any(attr =>
                attr.AttributeClass?.Name == "UniqueIdAttribute"))
            .ToList();

        if (!parametersWithUniqueId.Any())
            return null;

        // Collect diagnostics for misuse
        var diagnostics = new List<Diagnostic>();
        foreach (var p in parametersWithUniqueId)
        {
            var paramType = p.param.Type;
            var paramName = p.param.Name;
            var location = p.param.Locations.FirstOrDefault() ?? invocation.GetLocation();

            // Check if parameter type is string
            if (paramType.SpecialType != SpecialType.System_String)
            {
                diagnostics.Add(Diagnostic.Create(
                    NonStringParameter,
                    location,
                    paramName,
                    paramType.ToDisplayString()));
            }

            // Check if parameter is nullable
            if (paramType.NullableAnnotation != NullableAnnotation.Annotated &&
                paramType.SpecialType == SpecialType.System_String)
            {
                diagnostics.Add(Diagnostic.Create(
                    NonNullableParameter,
                    location,
                    paramName));
            }

            // Check if parameter has a default value
            if (!p.param.HasExplicitDefaultValue)
            {
                diagnostics.Add(Diagnostic.Create(
                    NoDefaultValue,
                    location,
                    paramName));
            }
        }

        // Only include parameters that are valid for interception (nullable string with default)
        var validUniqueIdParams = parametersWithUniqueId
            .Where(x => x.param.Type.SpecialType == SpecialType.System_String
                        && x.param.Type.NullableAnnotation == NullableAnnotation.Annotated)
            .ToList();

        // Get the interceptable location using Roslyn API
        var interceptableLocation = semanticModel.GetInterceptableLocation(invocation, CancellationToken.None);
        if (interceptableLocation == null)
            return null;

        // Extract the data parameter from the intercepts location attribute syntax
        var attributeSyntax = interceptableLocation.GetInterceptsLocationAttributeSyntax();
        var dataValue = ExtractDataFromAttributeSyntax(attributeSyntax);

        var isInstanceMethod = !method.IsStatic;

        var uniqueIdParameters = validUniqueIdParams.Select(x => new ParameterInfo(
            x.index,
            GetUniqueIdFormat(x.param),
            GetPrefix(x.param)
        )).ToList();

        return new MethodCallInfo(
            method.ContainingType.ToDisplayString(),
            method.Name,
            method.ReturnType.ToDisplayString(),
            dataValue,
            method.Parameters.ToArray(),
            uniqueIdParameters,
            isInstanceMethod,
            isInstanceMethod ? method.ContainingType : null,
            diagnostics,
            IsValid: uniqueIdParameters.Count > 0
        );
    }

    private static string ExtractDataFromAttributeSyntax(string attributeSyntax)
    {
        // The attribute syntax is like: [InterceptsLocation(1, "data_value")]
        // We need to extract just the data_value part

        // Find the second parameter (the data parameter)
        var startIndex = attributeSyntax.IndexOf(',');
        if (startIndex == -1) return "unknown_data";

        startIndex = attributeSyntax.IndexOf('"', startIndex);
        if (startIndex == -1) return "unknown_data";

        var endIndex = attributeSyntax.IndexOf('"', startIndex + 1);
        if (endIndex == -1) return "unknown_data";

        return attributeSyntax.Substring(startIndex + 1, endIndex - startIndex - 1);
    }

    private static bool HasType(Compilation compilation, string @namespace, string typeName)
    {
        var metadataName = $"{@namespace}.{typeName}";
        if (compilation.GetTypeByMetadataName(metadataName) is not null)
            return true;

        foreach (var reference in compilation.References)
        {
            if (compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol assemblySymbol &&
                assemblySymbol.GetTypeByMetadataName(metadataName) is not null)
            {
                return true;
            }
        }

        return compilation.GetSymbolsWithName(typeName, SymbolFilter.Type)
            .OfType<INamedTypeSymbol>()
            .Any(symbol => symbol.ContainingNamespace.ToDisplayString() == @namespace);
    }

    private static UniqueIdFormat GetUniqueIdFormat(IParameterSymbol parameter)
    {
        var attr = parameter.GetAttributes().FirstOrDefault(a =>
            a.AttributeClass?.Name == "UniqueIdAttribute");

        if (attr?.ConstructorArguments.Length > 0)
        {
            return (UniqueIdFormat)(int)attr.ConstructorArguments[0].Value!;
        }
        return UniqueIdFormat.Guid;
    }

    private static string? GetPrefix(IParameterSymbol parameter)
    {
        var attr = parameter.GetAttributes().FirstOrDefault(a =>
            a.AttributeClass?.Name == "UniqueIdAttribute");

        if (attr?.ConstructorArguments.Length > 1)
        {
            return attr.ConstructorArguments[1].Value as string;
        }
        return null;
    }

    private static string GenerateInterceptorSource(MethodCallInfo[] calls)
    {
        var sb = new StringBuilder();
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Runtime.CompilerServices;");
        sb.AppendLine();

        // Include the InterceptsLocationAttribute in the same file as the interceptors
        sb.AppendLine("namespace System.Runtime.CompilerServices");
        sb.AppendLine("{");
        sb.AppendLine("    [global::System.Diagnostics.Conditional(\"DEBUG\")]");
        sb.AppendLine("    [global::System.AttributeUsage(global::System.AttributeTargets.Method, AllowMultiple = true)]");
        sb.AppendLine("    sealed file class InterceptsLocationAttribute : global::System.Attribute");
        sb.AppendLine("    {");
        sb.AppendLine("        public InterceptsLocationAttribute(int version, string data)");
        sb.AppendLine("        {");
        sb.AppendLine("            _ = version;");
        sb.AppendLine("            _ = data;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine();

        sb.AppendLine("namespace Praefixum");
        sb.AppendLine("{");
        sb.AppendLine("file static class PraefixumInterceptor");
        sb.AppendLine("{");
        sb.AppendLine();

        for (int i = 0; i < calls.Length; i++)
        {
            var call = calls[i];

            sb.AppendLine($"    [InterceptsLocation(1,\"{call.InterceptsLocationData}\")]");

            if (call.IsInstanceMethod)
            {
                // Instance method: first parameter is 'this' receiver
                sb.Append($"    public static {call.ReturnType} {call.MethodName}_{i}(this {call.ContainingType} __instance");

                for (int j = 0; j < call.Parameters.Length; j++)
                {
                    var param = call.Parameters[j];
                    var paramType = param.Type.ToDisplayString();
                    var paramName = param.Name;

                    sb.Append($", {paramType} {paramName}");

                    // Add default value if it exists
                    if (param.HasExplicitDefaultValue)
                    {
                        var defaultValue = param.ExplicitDefaultValue;
                        sb.Append(" = ");
                        sb.Append(FormatDefaultValue(defaultValue, param.Type));
                    }
                }
                sb.AppendLine(")");
            }
            else
            {
                // Static method: standard signature
                sb.Append($"    public static {call.ReturnType} {call.MethodName}_{i}(");

                for (int j = 0; j < call.Parameters.Length; j++)
                {
                    var param = call.Parameters[j];
                    var paramType = param.Type.ToDisplayString();
                    var paramName = param.Name;

                    if (j > 0) sb.Append(", ");
                    sb.Append($"{paramType} {paramName}");

                    // Add default value if it exists
                    if (param.HasExplicitDefaultValue)
                    {
                        var defaultValue = param.ExplicitDefaultValue;
                        sb.Append(" = ");
                        sb.Append(FormatDefaultValue(defaultValue, param.Type));
                    }
                }
                sb.AppendLine(")");
            }

            sb.AppendLine("    {");

            // Check if ALL injectable unique ID parameters are provided - if so, call original method
            var uniqueIdParams = call.UniqueIdParameters
                .Select(pInfo => call.Parameters[pInfo.Index])
                .ToList();

            if (uniqueIdParams.Count > 1)
            {
                var conditions = string.Join(" && ", uniqueIdParams.Select(p => $"{p.Name} != null"));
                sb.AppendLine($"        if ({conditions})");
            }
            else if (uniqueIdParams.Count == 1)
            {
                sb.AppendLine($"        if ({uniqueIdParams[0].Name} != null)");
            }

            var callTarget = call.IsInstanceMethod
                ? $"__instance.{call.MethodName}"
                : $"{call.ContainingType}.{call.MethodName}";

            if (call.ReturnType == "void")
            {
                sb.AppendLine($"        {{");
                sb.AppendLine($"            {callTarget}({string.Join(", ", call.Parameters.Select(p => p.Name))});");
                sb.AppendLine($"            return;");
                sb.AppendLine($"        }}");
            }
            else
            {
                sb.AppendLine($"            return {callTarget}({string.Join(", ", call.Parameters.Select(p => p.Name))});");
            }
            sb.AppendLine();

            // Generate IDs for each null UniqueId parameter
            var parameterAssignments = new List<string>();
            for (int paramIndex = 0; paramIndex < call.Parameters.Length; paramIndex++)
            {
                var param = call.Parameters[paramIndex];
                var uniqueIdInfo = call.UniqueIdParameters.FirstOrDefault(u => u.Index == paramIndex);

                if (uniqueIdInfo != null)
                {
                    // This parameter has [UniqueId] - use a compile-time literal per call site
                    var literal = GenerateIdLiteral(
                        $"{call.InterceptsLocationData}:{paramIndex}",
                        uniqueIdInfo.Format,
                        uniqueIdInfo.Prefix,
                        call.MethodName);
                    var escapedLiteral = EscapeStringLiteral(literal);
                    sb.AppendLine($"        var {param.Name}Final = {param.Name} ?? \"{escapedLiteral}\";");

                    parameterAssignments.Add($"{param.Name}Final");
                }
                else
                {
                    parameterAssignments.Add(param.Name);
                }
            }

            // Call original method with all parameters (generated or original)
            var args = string.Join(", ", parameterAssignments);

            if (call.ReturnType == "void")
            {
                sb.AppendLine($"        {callTarget}({args});");
            }
            else
            {
                sb.AppendLine($"        return {callTarget}({args});");
            }
            sb.AppendLine("    }");
            sb.AppendLine();
        }

        sb.AppendLine("}");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string FormatDefaultValue(object? defaultValue, ITypeSymbol type)
    {
        if (defaultValue is null)
            return "null";
        if (defaultValue is string s)
            return $"\"{s}\"";
        if (defaultValue is bool b)
            return b ? "true" : "false";
        if (defaultValue is char c)
            return $"'{c}'";
        if (defaultValue is float f)
            return FormattableString.Invariant($"{f}f");
        if (defaultValue is double d)
            return FormattableString.Invariant($"{d}d");
        if (defaultValue is decimal m)
            return FormattableString.Invariant($"{m}m");
        if (defaultValue is long l)
            return $"{l}L";
        if (defaultValue is ulong ul)
            return $"{ul}UL";
        if (type.TypeKind == TypeKind.Enum)
            return $"({type.ToDisplayString()}){defaultValue}";
        return $"{defaultValue}";
    }

    private static string GenerateIdLiteral(string key, UniqueIdFormat format, string? prefix, string? methodName = null)
    {
        var baseId = format switch
        {
            UniqueIdFormat.Guid => DeterministicGuid(key),
            UniqueIdFormat.Timestamp => DeterministicTimestamp(key),
            UniqueIdFormat.ShortHash => ShortHash(key),
            UniqueIdFormat.HtmlId => HtmlSafeId(ShortHash(key)),
            UniqueIdFormat.Sequential => DeterministicSequential(key),
            UniqueIdFormat.Semantic => DeterministicSemantic(key, methodName),
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };

        return prefix is null ? baseId : $"{prefix}{baseId}";
    }

    internal static string ShortHash(string key)
    {
        using var sha256 = SHA256.Create();
        var inputBytes = Encoding.UTF8.GetBytes(key);
        var hashBytes = sha256.ComputeHash(inputBytes);
        return Convert.ToBase64String(hashBytes)
            .Replace("+", "a").Replace("/", "b").Replace("=", string.Empty)
            .Substring(0, 8);
    }

    internal static string DeterministicGuid(string key)
    {
        using var sha256 = SHA256.Create();
        var inputBytes = Encoding.UTF8.GetBytes($"{key}:guid");
        var hashBytes = sha256.ComputeHash(inputBytes);
        var guidBytes = new byte[16];
        Array.Copy(hashBytes, guidBytes, 16);
        var guid = new Guid(guidBytes);
        return guid.ToString("N");
    }

    internal static string DeterministicTimestamp(string key)
    {
        using var sha256 = SHA256.Create();
        var inputBytes = Encoding.UTF8.GetBytes($"{key}:timestamp");
        var hashBytes = sha256.ComputeHash(inputBytes);
        var timestampLong = Math.Abs(BitConverter.ToInt64(hashBytes, 0));
        var baseTimestamp = 1700000000000L;
        var maxOffset = 100000000000L;
        var deterministicTimestamp = baseTimestamp + (timestampLong % maxOffset);
        return deterministicTimestamp.ToString();
    }

    internal static string HtmlSafeId(string id)
    {
        if (id.Length > 0 && !char.IsLetter(id[0]))
            return "x" + id;
        return id;
    }

    internal static string DeterministicSequential(string key)
    {
        using var sha256 = SHA256.Create();
        var inputBytes = Encoding.UTF8.GetBytes($"{key}:sequential");
        var hashBytes = sha256.ComputeHash(inputBytes);
        var value = BitConverter.ToUInt32(hashBytes, 0) % 1_000_000u;
        return value.ToString("D6");
    }

    internal static string DeterministicSemantic(string key, string? methodName = null)
    {
        var hash = ShortHash(key).Substring(0, 4).ToLowerInvariant();
        var nameFragment = string.IsNullOrEmpty(methodName)
            ? "id"
            : SanitizeForId(methodName!);
        return $"{nameFragment}-{hash}";
    }

    private static string SanitizeForId(string name)
    {
        // Convert PascalCase/camelCase to kebab-case and take first meaningful segment
        var sb = new StringBuilder();
        for (int i = 0; i < name.Length && sb.Length < 16; i++)
        {
            var c = name[i];
            if (char.IsUpper(c) && i > 0)
            {
                sb.Append('-');
                sb.Append(char.ToLowerInvariant(c));
            }
            else if (char.IsLetterOrDigit(c))
            {
                sb.Append(char.ToLowerInvariant(c));
            }
        }
        return sb.Length > 0 ? sb.ToString() : "id";
    }

    private static string EscapeStringLiteral(string value)
    {
        return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}

internal record MethodCallInfo(
    string ContainingType,
    string MethodName,
    string ReturnType,
    string InterceptsLocationData,
    IParameterSymbol[] Parameters,
    List<ParameterInfo> UniqueIdParameters,
    bool IsInstanceMethod,
    ITypeSymbol? ReceiverType,
    List<Diagnostic> Diagnostics,
    bool IsValid
);

internal record ParameterInfo(
    int Index,
    UniqueIdFormat Format,
    string? Prefix
);
