using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Praefixum.Tests
{
    public class UniqueIdGeneratorStaticPartialTests
    {
        private static readonly MetadataReference[] DefaultReferences = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
            MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location),
            MetadataReference.CreateFromFile(typeof(System.ComponentModel.EditorBrowsableAttribute).Assembly.Location)
        };

        [Fact]
        public void StaticPartialClass_GeneratesStaticPartialInOutput()
        {
            var generator = new UniqueIdGenerator();
            var source = @"
using Praefixum;

namespace StaticPartialTest
{
    public static partial class StaticPartialClass
    {
        public static string GetId([UniqueId] string id = null) => id ?? GetId_id_Id;
    }
}";
            var result = SourceGeneratorVerifier.RunGenerator(generator, source, DefaultReferences);
            Assert.Empty(result.Diagnostics);
            var generatedSource = result.GeneratedSources.FirstOrDefault(s => s.HintName.Contains("StaticPartialClass_UniqueIds.g.cs"));
            Assert.NotNull(generatedSource);
            var generatedCode = generatedSource.Source;
            Assert.Contains("static partial class StaticPartialClass", generatedCode);
            Assert.Contains("public const string GetId_id_Id", generatedCode);
        }
    }
}
