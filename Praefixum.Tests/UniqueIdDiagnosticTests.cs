using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Praefixum;
using Xunit;

namespace Praefixum.Tests;

/// <summary>
/// Tests that the source generator emits correct diagnostics (PRAEF001, PRAEF002, PRAEF003)
/// when [UniqueId] is misused on method parameters.
/// </summary>
public class UniqueIdDiagnosticTests
{
    private static readonly CSharpParseOptions ParseOptions = new(LanguageVersion.Preview);

    [Fact]
    public void PRAEF001_NonStringParameter_EmitsWarning()
    {
        // Arrange — [UniqueId] on an int parameter
        var source = """
            using Praefixum;

            public static class BadUsage
            {
                public static int Process([UniqueId] int id = 0)
                {
                    return id;
                }
            }

            public static class Caller
            {
                public static int Call() => BadUsage.Process();
            }
            """;

        var diagnostics = RunGeneratorAndGetDiagnostics(source);

        Assert.Contains(diagnostics, d => d.Id == "PRAEF001");
    }

    [Fact]
    public void PRAEF002_NonNullableStringParameter_EmitsWarning()
    {
        // Arrange — [UniqueId] on a non-nullable string parameter
        var source = """
            using Praefixum;

            public static class BadUsage
            {
                public static string Process([UniqueId] string id = "default")
                {
                    return id;
                }
            }

            public static class Caller
            {
                public static string Call() => BadUsage.Process();
            }
            """;

        var diagnostics = RunGeneratorAndGetDiagnostics(source);

        Assert.Contains(diagnostics, d => d.Id == "PRAEF002");
    }

    [Fact]
    public void PRAEF003_NoDefaultValue_EmitsInfo()
    {
        // Arrange — [UniqueId] on a parameter without default value
        var source = """
            #nullable enable
            using Praefixum;

            public static class BadUsage
            {
                public static string Process([UniqueId] string? id)
                {
                    return id ?? "fallback";
                }
            }

            public static class Caller
            {
                public static string Call() => BadUsage.Process(null);
            }
            """;

        var diagnostics = RunGeneratorAndGetDiagnostics(source);

        Assert.Contains(diagnostics, d => d.Id == "PRAEF003");
    }

    [Fact]
    public void CorrectUsage_EmitsNoDiagnostics()
    {
        // Arrange — correct usage: nullable string with default null
        var source = """
            #nullable enable
            using Praefixum;

            public static class GoodUsage
            {
                public static string Process([UniqueId] string? id = null)
                {
                    return id ?? "fallback";
                }
            }

            public static class Caller
            {
                public static string Call() => GoodUsage.Process();
            }
            """;

        var diagnostics = RunGeneratorAndGetDiagnostics(source);

        // Should not contain any PRAEF diagnostics
        Assert.DoesNotContain(diagnostics, d => d.Id.StartsWith("PRAEF"));
    }

    [Fact]
    public void PRAEF001_OnMultipleParameters_EmitsMultipleWarnings()
    {
        // Arrange — [UniqueId] on two non-string parameters
        var source = """
            using Praefixum;

            public static class BadUsage
            {
                public static int Process([UniqueId] int a = 0, [UniqueId] bool b = false)
                {
                    return a + (b ? 1 : 0);
                }
            }

            public static class Caller
            {
                public static int Call() => BadUsage.Process();
            }
            """;

        var diagnostics = RunGeneratorAndGetDiagnostics(source);

        var praef001s = diagnostics.Where(d => d.Id == "PRAEF001").ToList();
        Assert.Equal(2, praef001s.Count);
    }

    // ==========================================
    // HELPER METHODS
    // ==========================================

    private static List<Diagnostic> RunGeneratorAndGetDiagnostics(string source)
    {
        var platformReferences = GetTrustedPlatformReferences();
        var praefixumReference = CreatePraefixumReference(platformReferences);

        var syntaxTree = CSharpSyntaxTree.ParseText(source, ParseOptions);
        var allReferences = platformReferences.Concat(new[] { praefixumReference }).ToList();

        var compilation = CSharpCompilation.Create(
            "TestCompilation",
            new[] { syntaxTree },
            allReferences,
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable));

        var generator = new PraefixumSourceGenerator().AsSourceGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new[] { generator }, parseOptions: ParseOptions);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out var diagnostics);

        return diagnostics.ToList();
    }

    private static List<MetadataReference> GetTrustedPlatformReferences()
    {
        var tpa = (string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
        if (string.IsNullOrWhiteSpace(tpa))
            throw new InvalidOperationException("TRUSTED_PLATFORM_ASSEMBLIES is not available.");

        return tpa.Split(Path.PathSeparator)
            .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path))
            .ToList();
    }

    private static MetadataReference CreatePraefixumReference(IEnumerable<MetadataReference> references)
    {
        var source = """
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
            """;

        var syntaxTree = CSharpSyntaxTree.ParseText(source, ParseOptions);
        var compilation = CSharpCompilation.Create(
            "PraefixumReference",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable));

        using var stream = new MemoryStream();
        var emitResult = compilation.Emit(stream);
        if (!emitResult.Success)
        {
            var diags = string.Join(Environment.NewLine, emitResult.Diagnostics);
            throw new InvalidOperationException($"Failed to emit Praefixum reference:{Environment.NewLine}{diags}");
        }

        return MetadataReference.CreateFromImage(stream.ToArray());
    }
}
