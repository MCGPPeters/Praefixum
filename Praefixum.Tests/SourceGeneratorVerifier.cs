using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Praefixum.Tests
{
    /// <summary>
    /// Helper class for testing Roslyn source generators
    /// </summary>
    public static class SourceGeneratorVerifier
    {
        public class TestResult
        {
            public ImmutableArray<Diagnostic> Diagnostics { get; }
            public ImmutableArray<GeneratedSourceResult> GeneratedSources { get; }
            public Compilation OutputCompilation { get; }

            public TestResult(ImmutableArray<Diagnostic> diagnostics, ImmutableArray<GeneratedSourceResult> generatedSources, Compilation outputCompilation)
            {
                Diagnostics = diagnostics;
                GeneratedSources = generatedSources;
                OutputCompilation = outputCompilation;
            }
        }

        public class GeneratedSourceResult
        {
            public string HintName { get; }
            public string Source { get; }

            public GeneratedSourceResult(string hintName, string source)
            {
                HintName = hintName;
                Source = source;
            }
        }

        /// <summary>
        /// Create and verify a source generator with the test input
        /// </summary>
        public static TestResult RunGenerator(
            IIncrementalGenerator generator,
            string source,
            IEnumerable<MetadataReference>? additionalReferences = null)
        {
            // Parse the test source code
            var syntaxTree = CSharpSyntaxTree.ParseText(source);
            
            // Create a list of references that are always needed
            var references = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
                MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location)
            };

            // Add any additional references provided
            if (additionalReferences != null)
            {
                references.AddRange(additionalReferences);
            }
            
            // Create the initial compilation
            var compilation = CSharpCompilation.Create(
                assemblyName: "Tests",
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithNullableContextOptions(NullableContextOptions.Enable)
            );

            // Convert the incremental generator to a source generator
            var sourceGenerator = generator.AsSourceGenerator();
            
            // Create a special GeneratorDriver that tracks and records all reported diagnostics
            var driver = CSharpGeneratorDriver.Create(
                generators: new[] { sourceGenerator },
                additionalTexts: ImmutableArray<AdditionalText>.Empty,
                parseOptions: (CSharpParseOptions)syntaxTree.Options,
                optionsProvider: null
            );

            // Run the source generator!
            var resultDriver = driver.RunGenerators(compilation);
            
            // Get the results from running the generators
            var runResult = resultDriver.GetRunResult();
            
            // Safely handle results with proper null/empty checks
            var generatedSources = ImmutableArray<GeneratedSourceResult>.Empty;
            
            if (runResult != null && !runResult.GeneratedTrees.IsDefaultOrEmpty)
            {
                // Convert all the generated syntax trees to our simplified GeneratedSourceResult type
                generatedSources = runResult.GeneratedTrees
                    .Select(static t => new GeneratedSourceResult(
                        t.FilePath, 
                        t.GetText().ToString()))
                    .ToImmutableArray();
            }
            
            // Extract diagnostics if available
            var diagnostics = runResult?.Diagnostics ?? ImmutableArray<Diagnostic>.Empty;
            
            // Return the final result
            return new TestResult(
                diagnostics,
                generatedSources,
                compilation
            );
        }
    }
}
