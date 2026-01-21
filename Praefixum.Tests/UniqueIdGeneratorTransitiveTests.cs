using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Praefixum;
using Xunit;

namespace Praefixum.Tests;

public class UniqueIdGeneratorTransitiveTests
{
    private static readonly CSharpParseOptions ParseOptions = new(LanguageVersion.Preview);

    [Fact]
    public void Generator_DoesNotEmitAttribute_WhenTypesAlreadyReferenced()
    {
        var platformReferences = GetTrustedPlatformReferences();
        var upstreamReference = CreateUpstreamUniqueIdReference(platformReferences);
        var compilation = CreateConsumerCompilation(platformReferences, upstreamReference);

        var generator = new PraefixumSourceGenerator().AsSourceGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new[] { generator }, parseOptions: ParseOptions);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);

        var runResult = driver.GetRunResult();
        var generatedFiles = runResult.Results
            .SelectMany(result => result.GeneratedSources)
            .Select(source => source.HintName)
            .ToList();

        Assert.DoesNotContain("UniqueIdAttribute.g.cs", generatedFiles);
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

    private static MetadataReference CreateUpstreamUniqueIdReference(IEnumerable<MetadataReference> references)
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
                    public bool Deterministic { get; }

                    public UniqueIdAttribute(UniqueIdFormat format = UniqueIdFormat.Guid, string? prefix = null, bool deterministic = true)
                    {
                        Format = format;
                        Prefix = prefix;
                        Deterministic = deterministic;
                    }
                }

                public enum UniqueIdFormat
                {
                    Guid,
                    HtmlId,
                    Timestamp,
                    ShortHash
                }
            }
            """;

        var syntaxTree = CSharpSyntaxTree.ParseText(source, ParseOptions);
        var compilation = CSharpCompilation.Create(
            "UpstreamUniqueId",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable));

        using var stream = new MemoryStream();
        var emitResult = compilation.Emit(stream);
        if (!emitResult.Success)
        {
            var diagnostics = string.Join(Environment.NewLine, emitResult.Diagnostics);
            throw new InvalidOperationException($"Failed to emit upstream reference:{Environment.NewLine}{diagnostics}");
        }

        return MetadataReference.CreateFromImage(stream.ToArray());
    }

    private static CSharpCompilation CreateConsumerCompilation(
        IEnumerable<MetadataReference> references,
        MetadataReference upstreamReference)
    {
        var source = """
            using Praefixum;

            public static class Consumer
            {
                public static string Call()
                {
                    return Helper.GetId();
                }
            }

            public static class Helper
            {
                public static string GetId([UniqueId] string? id = null)
                {
                    return id ?? "fallback";
                }
            }
            """;

        var syntaxTree = CSharpSyntaxTree.ParseText(source, ParseOptions);
        var allReferences = references.Concat(new[] { upstreamReference }).ToList();

        return CSharpCompilation.Create(
            "ConsumerCompilation",
            new[] { syntaxTree },
            allReferences,
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable));
    }
}
