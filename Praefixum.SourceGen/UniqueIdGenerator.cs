using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Praefixum
{
    [Generator]
    public class UniqueIdGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Register the attribute source
            context.RegisterPostInitializationOutput(ctx =>
            {
                ctx.AddSource("UniqueIdAttribute.cs", SourceText.From(@"
using System;

namespace Praefixum
{
    /// <summary>
    /// Format options for UniqueId generation
    /// </summary>
    public enum UniqueIdFormat
    {
        /// <summary>
        /// 16-character hexadecimal string (default)
        /// </summary>
        Hex16 = 0,
        
        /// <summary>
        /// 32-character hexadecimal string (full MD5 hash)
        /// </summary>
        Hex32 = 1,
          /// <summary>
        /// Standard GUID format with dashes
        /// </summary>
        Guid = 2,
        
        /// <summary>
        /// 8-character hexadecimal string (short format)
        /// </summary>
        Hex8 = 3,
          /// <summary>
        /// Compact format optimized for HTML5 element IDs: 
        /// 6-character string that starts with a letter, followed by lowercase a-z, 0-9, hyphens (""-""), or underscores (""_"")
        /// Fully compliant with HTML5 ID specifications
        /// </summary>
        HtmlId = 4
    }

    /// <summary>
    /// Attribute to mark a parameter that will receive a unique ID at compile time.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class UniqueIdAttribute : Attribute
    {
        /// <summary>
        /// The format of the generated unique ID
        /// </summary>
        public UniqueIdFormat Format { get; }

        /// <summary>
        /// Creates a new UniqueIdAttribute with the default format (Hex16)
        /// </summary>
        public UniqueIdAttribute()
        {
            Format = UniqueIdFormat.Hex16;
        }

        /// <summary>
        /// Creates a new UniqueIdAttribute with the specified format
        /// </summary>
        /// <param name=""format"">The format to use for the generated ID</param>
        public UniqueIdAttribute(UniqueIdFormat format)
        {
            Format = format;
        }
    }
}", Encoding.UTF8));
            });

            // Create a pipeline for parameter syntax nodes that have attributes
            var parameterSyntax = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => s is ParameterSyntax p && p.AttributeLists.Count > 0,
                    transform: static (ctx, _) => ctx.Node as ParameterSyntax)
                .Where(p => p != null);

            // Combine the parameter syntax with the compilation
            var compilationAndParameters = context.CompilationProvider.Combine(parameterSyntax.Collect());

            // Generate source code based on the parameters
            context.RegisterSourceOutput(compilationAndParameters, (spc, source) =>
            {
                var (compilation, parameters) = source;
                if (parameters.IsEmpty)
                    return;

                // Find the UniqueIdAttribute type
                var attributeSymbol = compilation.GetTypeByMetadataName("Praefixum.UniqueIdAttribute");
                if (attributeSymbol == null)
                    return;

                // Process each parameter to find those with the UniqueIdAttribute
                var uniqueParams = new Dictionary<IMethodSymbol, Dictionary<string, string>>(SymbolEqualityComparer.Default);
                var typeToParams = new Dictionary<INamedTypeSymbol, Dictionary<IMethodSymbol, Dictionary<string, string>>>(SymbolEqualityComparer.Default);

                foreach (var parameter in parameters)
                {
                    if (parameter.SyntaxTree == null) continue;
                    var model = compilation.GetSemanticModel(parameter.SyntaxTree);
                    if (model == null) continue;
                    
                    var paramSymbol = model.GetDeclaredSymbol(parameter) as IParameterSymbol;
                    if (paramSymbol == null) continue;

                    // Check if parameter has UniqueIdAttribute
                    var uniqueIdAttribute = paramSymbol.GetAttributes()
                        .FirstOrDefault(a => a.AttributeClass != null && 
                                            a.AttributeClass.ToDisplayString() == "Praefixum.UniqueIdAttribute");
                    
                    if (uniqueIdAttribute == null)
                        continue;

                    var methodSymbol = paramSymbol.ContainingSymbol as IMethodSymbol;
                    if (methodSymbol == null) continue;

                    var typeSymbol = methodSymbol.ContainingType;
                    if (typeSymbol == null) continue;

                    // Generate a unique ID based on file path, method name, parameter name, and line number
                    var filePath = parameter.SyntaxTree.FilePath ?? "";
                    var lineNumber = parameter.SyntaxTree.GetLineSpan(parameter.Span).StartLinePosition.Line;
                    // Also include character position to handle multiple attributes on the same line
                    var charPosition = parameter.SyntaxTree.GetLineSpan(parameter.Span).StartLinePosition.Character;
                    var uniqueIdSource = $"{filePath}:{methodSymbol.Name}:{paramSymbol.Name}:{lineNumber}:{charPosition}";
                    
                    // Get the format from the attribute constructor if specified
                    UniqueIdFormat format = UniqueIdFormat.Hex16;
                    if (uniqueIdAttribute.ConstructorArguments.Length > 0)
                    {
                        if (uniqueIdAttribute.ConstructorArguments[0].Value is int formatValue)
                        {
                            format = (UniqueIdFormat)formatValue;
                        }
                    }
                    
                    var uniqueId = GenerateId(uniqueIdSource, format);

                    // Store the unique ID for this parameter
                    if (!typeToParams.TryGetValue(typeSymbol, out var methodParams))
                    {
                        methodParams = new Dictionary<IMethodSymbol, Dictionary<string, string>>(SymbolEqualityComparer.Default);
                        typeToParams[typeSymbol] = methodParams;
                    }

                    if (!methodParams.TryGetValue(methodSymbol, out var paramIds))
                    {
                        paramIds = new Dictionary<string, string>();
                        methodParams[methodSymbol] = paramIds;
                    }

                    paramIds[paramSymbol.Name] = uniqueId;
                }

                // Generate source for each type that has parameters with UniqueIdAttribute
                foreach (var typeEntry in typeToParams)
                {

                    var typeSymbol = typeEntry.Key;
                    var methodParams = typeEntry.Value;

                    var sb = new StringBuilder();
                    sb.AppendLine("using System;");
                    sb.AppendLine();

                    var ns = typeSymbol.ContainingNamespace.ToDisplayString();
                    if (!string.IsNullOrEmpty(ns))
                    {
                        sb.AppendLine($"namespace {ns}");
                        sb.AppendLine("{");
                    }

                    var className = typeSymbol.Name;
                    var isStatic = typeSymbol.IsStatic;
                    // Always emit partial, add static if needed
                    var classModifiers = isStatic ? "static partial" : "partial";
                    sb.AppendLine($"    {classModifiers} class {className}");
                    sb.AppendLine("    {");

                    foreach (var methodEntry in methodParams)
                    {
                        var methodSymbol = methodEntry.Key;
                        var paramIds = methodEntry.Value;

                        foreach (var paramEntry in paramIds)
                        {
                            var paramName = paramEntry.Key;
                            var uniqueId = paramEntry.Value;

                            sb.AppendLine($"        // Auto-generated for parameter {paramName} in method {methodSymbol.Name}");
                            sb.AppendLine($"        public const string {methodSymbol.Name}_{paramName}_Id = \"{uniqueId}\";");
                        }
                    }

                    sb.AppendLine("    }");

                    if (!string.IsNullOrEmpty(ns))
                    {
                        sb.AppendLine("}");
                    }

                    spc.AddSource($"{className}_UniqueIds.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
                }
            });
        }

        private static string GenerateId(string input, UniqueIdFormat format)
        {
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);
                
                switch (format)
                {
                    case UniqueIdFormat.Hex32:
                        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                        
                    case UniqueIdFormat.Guid:
                        return new Guid(hashBytes).ToString("D");
                          case UniqueIdFormat.Hex8:
                        return BitConverter.ToString(hashBytes, 0, 4).Replace("-", "").ToLowerInvariant();
                        
                    case UniqueIdFormat.HtmlId:
                        // Generate a HTML5-compliant ID (6 chars, starts with letter, alphanumeric with hyphens/underscores)
                        return ConvertToHtmlId(hashBytes, 6);
                        
                    case UniqueIdFormat.Hex16:
                    default:
                        return BitConverter.ToString(hashBytes, 0, 8).Replace("-", "").ToLowerInvariant();                }
            }
        }
        
        // Converts bytes to a HTML5-friendly ID format (alphanumeric, starting with a letter, includes hyphens and underscores)
        private static string ConvertToHtmlId(byte[] bytes, int length = 6)
        {
            // HTML5 IDs must begin with a letter and can contain letters, digits, hyphens, underscores, and periods
            // For better compatibility across older browsers, we'll use:
            // - First character: a-z (letter)
            // - Other characters: a-z, 0-9, hyphen (-), underscore (_)
            
            const string validChars = "abcdefghijklmnopqrstuvwxyz0123456789-_";
            
            // Ensure ID starts with a letter (HTML ID requirement)
            const string validStartChars = "abcdefghijklmnopqrstuvwxyz";
            
            var result = new StringBuilder();
            
            // First character must be a letter for HTML IDs
            byte firstByte = bytes[0];
            result.Append(validStartChars[firstByte % validStartChars.Length]);
            
            // Use a more sophisticated algorithm to pack more uniqueness into remaining characters
            for (int i = 0; i < length - 1 && i < Math.Min(bytes.Length - 1, 6); i++)
            {
                // Each character will use 6 bits (2^6 = 64 possibilities, which covers our 38 chars)
                // Extract 6 bits starting from different offsets in the MD5 hash
                int byteOffset = 1 + i * 2; // Skip first byte (used for first char) and use wider distribution
                if (byteOffset < bytes.Length)
                {
                    int value = (bytes[byteOffset] & 0x3F); // Take lower 6 bits
                    // Use the value to index into our character set
                    result.Append(validChars[value % validChars.Length]);
                }
            }
            
            return result.ToString();
        }
    }
}
