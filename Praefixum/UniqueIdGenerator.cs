// =======================
// PraefixumSourceGenerator.cs
// =======================
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Praefixum;

// Uses UniqueIdAttribute and UniqueIdFormat from Praefixum.Attributes

[Generator]
public sealed class PraefixumSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Attribute definitions are provided by the Praefixum.Attributes assembly

        // Find method calls that need interception
        var methodCallsProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is InvocationExpressionSyntax,
                transform: static (ctx, _) => GetMethodCallInfo(ctx))
            .Where(static m => m is not null)
            .Collect();

        // Generate interceptors
        context.RegisterSourceOutput(methodCallsProvider, static (ctx, calls) =>
        {
            if (calls.IsEmpty) return;

            var interceptorSource = GenerateInterceptorSource(calls.Where(c => c is not null).Cast<MethodCallInfo>().ToArray());
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
            return null;        // Get the interceptable location using Roslyn API
        var interceptableLocation = semanticModel.GetInterceptableLocation(invocation, CancellationToken.None);
        if (interceptableLocation == null)
            return null;

        // Extract the data parameter from the intercepts location attribute syntax
        var attributeSyntax = interceptableLocation.GetInterceptsLocationAttributeSyntax();
        var dataValue = ExtractDataFromAttributeSyntax(attributeSyntax);        return new MethodCallInfo(
            method.ContainingType.ToDisplayString(),
            method.Name,
            method.ReturnType.ToDisplayString(),
            dataValue,
            method.Parameters.ToArray(),
            parametersWithUniqueId.Select(x => new ParameterInfo(
                x.index,
                GetUniqueIdFormat(x.param),
                GetPrefix(x.param),
                GetDeterministic(x.param)
            )).ToList()
        );}

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

    private static bool GetDeterministic(IParameterSymbol parameter)
    {
        var attr = parameter.GetAttributes().FirstOrDefault(a => 
            a.AttributeClass?.Name == "UniqueIdAttribute");
        
        if (attr?.ConstructorArguments.Length > 2)
        {
            return (bool)attr.ConstructorArguments[2].Value!;
        }
        return true;
    }    private static string GenerateInterceptorSource(MethodCallInfo[] calls)
    {        var sb = new StringBuilder();
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Runtime.CompilerServices;");
        sb.AppendLine("using System.Security.Cryptography;");
        sb.AppendLine("using System.Text;");
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
        sb.AppendLine("    }");        sb.AppendLine("}");
        sb.AppendLine();
        
        sb.AppendLine("namespace Praefixum");
        sb.AppendLine("{");
        sb.AppendLine("file static class PraefixumInterceptor");
        sb.AppendLine("{");
        sb.AppendLine("    private static readonly Dictionary<string, int> _callsiteCounts = new();");
        sb.AppendLine("    private static readonly object _lock = new();");
        sb.AppendLine();        for (int i = 0; i < calls.Length; i++)
        {
            var call = calls[i];

            sb.AppendLine($"    [InterceptsLocation(1,\"{call.InterceptsLocationData}\")]");
            sb.AppendLine($"    public static {call.ReturnType} {call.MethodName}_{i}(");
            
            // Generate exact method signature
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
                    if (defaultValue == null)
                        sb.Append(" = null");
                    else if (defaultValue is string s)
                        sb.Append($" = \"{s}\"");
                    else
                        sb.Append($" = {defaultValue}");
                }
            }
            sb.AppendLine(")");
            sb.AppendLine("    {");            // Check if ALL unique ID parameters are provided - if so, call original method
            var uniqueIdParams = call.Parameters.Where(p => p.GetAttributes().Any(attr => 
                attr.AttributeClass?.Name == "UniqueIdAttribute")).ToList();
            
            if (uniqueIdParams.Count > 1)
            {
                var conditions = string.Join(" && ", uniqueIdParams.Select(p => $"{p.Name} != null"));
                sb.AppendLine($"        if ({conditions})");
            }
            else
            {
                sb.AppendLine($"        if ({uniqueIdParams[0].Name} != null)");
            }
            
            if (call.ReturnType == "void")
            {
                sb.AppendLine($"        {{");
                sb.AppendLine($"            {call.ContainingType}.{call.MethodName}({string.Join(", ", call.Parameters.Select(p => p.Name))});");
                sb.AppendLine($"            return;");
                sb.AppendLine($"        }}");
            }
            else
            {
                sb.AppendLine($"            return {call.ContainingType}.{call.MethodName}({string.Join(", ", call.Parameters.Select(p => p.Name))});");
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
                    // This parameter has [UniqueId] - generate ID if null
                    sb.AppendLine($"        // Generate {param.Name} if null");
                    sb.AppendLine($"        var {param.Name}Final = {param.Name};");
                    sb.AppendLine($"        if ({param.Name}Final == null)");
                    sb.AppendLine("        {");
                    sb.AppendLine($"            var key{paramIndex} = \"{call.InterceptsLocationData}:{paramIndex}\";");
                    sb.AppendLine($"            int count{paramIndex};");
                    sb.AppendLine();
                    sb.AppendLine("            lock (_lock)");
                    sb.AppendLine("            {");
                    sb.AppendLine($"                if (!_callsiteCounts.TryGetValue(key{paramIndex}, out count{paramIndex}))");
                    sb.AppendLine($"                    count{paramIndex} = 0;");
                    sb.AppendLine($"                _callsiteCounts[key{paramIndex}] = count{paramIndex} + 1;");
                    sb.AppendLine("            }");
                    sb.AppendLine();
                    
                    var format = uniqueIdInfo.Format;
                    var prefix = uniqueIdInfo.Prefix != null ? $"\"{uniqueIdInfo.Prefix}\"" : "null";
                    sb.AppendLine($"            {param.Name}Final = GenerateId(key{paramIndex}, count{paramIndex}, UniqueIdFormat.{format}, {prefix});");
                    sb.AppendLine("        }");
                    sb.AppendLine();
                    
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
                sb.AppendLine($"        {call.ContainingType}.{call.MethodName}({args});");
            }
            else
            {
                sb.AppendLine($"        return {call.ContainingType}.{call.MethodName}({args});");
            }
            sb.AppendLine("    }");
            sb.AppendLine();
        }

        sb.AppendLine("    private static string GenerateId(string key, int count, UniqueIdFormat format = UniqueIdFormat.HtmlId, string? prefix = null)");
        sb.AppendLine("    {");        sb.AppendLine("        string baseId = format switch");
        sb.AppendLine("        {");
        sb.AppendLine("            UniqueIdFormat.Guid => DeterministicGuid(key, count),");
        sb.AppendLine("            UniqueIdFormat.Timestamp => DeterministicTimestamp(key, count),");
        sb.AppendLine("            UniqueIdFormat.ShortHash => ShortHash(key, count),");
        sb.AppendLine("            UniqueIdFormat.HtmlId => HtmlSafeId(ShortHash(key, count)),");
        sb.AppendLine("            _ => throw new ArgumentOutOfRangeException()");
        sb.AppendLine("        }; // Fixed GUID generation");
        sb.AppendLine();
        sb.AppendLine("        return prefix is null ? baseId : $\"{prefix}{baseId}\";");
        sb.AppendLine("    }");
        sb.AppendLine();        sb.AppendLine("    private static string ShortHash(string key, int count)");
        sb.AppendLine("    {");
        sb.AppendLine("        using var sha256 = SHA256.Create();");
        sb.AppendLine("        var inputBytes = Encoding.UTF8.GetBytes($\"{key}:{count}\");");
        sb.AppendLine("        var hashBytes = sha256.ComputeHash(inputBytes);");
        sb.AppendLine("        return Convert.ToBase64String(hashBytes)");
        sb.AppendLine("            .Replace(\"+\", \"a\").Replace(\"/\", \"b\").Replace(\"=\", string.Empty)");
        sb.AppendLine("            .Substring(0, 8);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private static string DeterministicGuid(string key, int count)");
        sb.AppendLine("    {");
        sb.AppendLine("        using var sha256 = SHA256.Create();");
        sb.AppendLine("        var inputBytes = Encoding.UTF8.GetBytes($\"{key}:{count}:guid\");");
        sb.AppendLine("        var hashBytes = sha256.ComputeHash(inputBytes);");
        sb.AppendLine("        // Take first 16 bytes for GUID");
        sb.AppendLine("        var guidBytes = new byte[16];");
        sb.AppendLine("        Array.Copy(hashBytes, guidBytes, 16);");
        sb.AppendLine("        var guid = new Guid(guidBytes);");        sb.AppendLine("        return guid.ToString(\"N\");");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private static string DeterministicTimestamp(string key, int count)");
        sb.AppendLine("    {");
        sb.AppendLine("        // Generate deterministic timestamp-like value from hash");
        sb.AppendLine("        using var sha256 = SHA256.Create();");
        sb.AppendLine("        var inputBytes = Encoding.UTF8.GetBytes($\"{key}:{count}:timestamp\");");
        sb.AppendLine("        var hashBytes = sha256.ComputeHash(inputBytes);");
        sb.AppendLine("        // Convert first 8 bytes to long and ensure it's positive and looks like a timestamp");
        sb.AppendLine("        var timestampLong = Math.Abs(BitConverter.ToInt64(hashBytes, 0));");
        sb.AppendLine("        // Scale to reasonable timestamp range (around current era)");
        sb.AppendLine("        var baseTimestamp = 1700000000000L; // Base timestamp (year 2023)");
        sb.AppendLine("        var maxOffset = 100000000000L; // Max offset (about 3 years)");
        sb.AppendLine("        var deterministicTimestamp = baseTimestamp + (timestampLong % maxOffset);");
        sb.AppendLine("        return deterministicTimestamp.ToString();");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private static string HtmlSafeId(string id)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (id.Length > 0 && !char.IsLetter(id[0]))");
        sb.AppendLine("            return \"x\" + id;");
        sb.AppendLine("        return id;");        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine("}");        return sb.ToString();
    }
}

internal record MethodCallInfo(
    string ContainingType,
    string MethodName,
    string ReturnType,
    string InterceptsLocationData,
    IParameterSymbol[] Parameters,
    List<ParameterInfo> UniqueIdParameters
);

internal record ParameterInfo(
    int Index,
    UniqueIdFormat Format,
    string? Prefix,
    bool Deterministic
);