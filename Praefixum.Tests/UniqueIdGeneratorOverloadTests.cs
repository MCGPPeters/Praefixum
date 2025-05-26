using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Praefixum.Tests
{
    public class UniqueIdGeneratorOverloadTests
    {        private static readonly MetadataReference[] DefaultReferences = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
            MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location),
            MetadataReference.CreateFromFile(typeof(System.ComponentModel.EditorBrowsableAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(UniqueIdGenerator).Assembly.Location)
        };

        [Fact]
        public void OverloadedMethods_GenerateUniqueConstantNames()
        {
            var generator = new UniqueIdGenerator();
            var source = @"
using Praefixum;

namespace OverloadTest
{
    public partial class MyClass
    {
        public string Foo([UniqueId] string id = null) => id ?? Foo_string_id_Id;
        public string Foo([UniqueId] int id = 0) => id.ToString() ?? Foo_int_id_Id;
        public string Foo([UniqueId] string id = null, int x = 0) => id ?? Foo_string_Int32_id_Id;
    }
}";
            var result = SourceGeneratorVerifier.RunGenerator(generator, source, DefaultReferences);
            Assert.Empty(result.Diagnostics);
            var generatedSource = result.GeneratedSources.FirstOrDefault(s => s.HintName.Contains("MyClass_UniqueIds.g.cs"));
            Assert.NotNull(generatedSource);
            var generatedCode = generatedSource.Source;
            Assert.Contains("public const string Foo_String_id_Id", generatedCode);
            Assert.Contains("public const string Foo_Int32_id_Id", generatedCode);
            Assert.Contains("public const string Foo_String_Int32_id_Id", generatedCode);
        }
    }
}
