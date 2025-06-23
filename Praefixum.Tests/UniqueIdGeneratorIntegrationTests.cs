using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Praefixum.Tests
{
    public class UniqueIdGeneratorIntegrationTests
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
        public void IntegrationTest_AllFormatsGenerate_ValidUniqueIds()
        {
            // Arrange
            var generator = new UniqueIdGenerator();
            var source = @"
using Praefixum;

namespace IntegrationTest
{
    public partial class IdGenerator
    {
        public string CreateHex16Id([UniqueId] string id = null) => id ?? CreateHex16Id_id_Id;
        
        public string CreateHex32Id([UniqueId(UniqueIdFormat.Hex32)] string id = null) => id ?? CreateHex32Id_id_Id;
        
        public string CreateGuidId([UniqueId(UniqueIdFormat.Guid)] string id = null) => id ?? CreateGuidId_id_Id;
        
        public string CreateHex8Id([UniqueId(UniqueIdFormat.Hex8)] string id = null) => id ?? CreateHex8Id_id_Id;
        
        public string CreateHtmlId([UniqueId(UniqueIdFormat.HtmlId)] string id = null) => id ?? CreateHtmlId_id_Id;
        
        // Practical use cases with prefixes
        public string CreateUserId([UniqueId] string id = null) => ""usr_"" + (id ?? CreateUserId_id_Id);
        
        public string CreateSessionId([UniqueId(UniqueIdFormat.Hex8)] string id = null) => ""sess_"" + (id ?? CreateSessionId_id_Id);
        
        public string CreateTransactionId([UniqueId(UniqueIdFormat.Guid)] string id = null) => ""tx_"" + (id ?? CreateTransactionId_id_Id);
        
        // HTML elements with the HtmlId format
        public string CreateButtonId([UniqueId(UniqueIdFormat.HtmlId)] string id = null) => id ?? CreateButtonId_id_Id;
        
        public string CreateInputId([UniqueId(UniqueIdFormat.HtmlId)] string id = null) => id ?? CreateInputId_id_Id;
    }
}";

            // Act
            var result = SourceGeneratorVerifier.RunGenerator(generator, source, DefaultReferences);

            // Assert
            Assert.Empty(result.Diagnostics);
            
            // Find the implementation file (not the attribute file)
            var generatedSource = result.GeneratedSources.FirstOrDefault(
                s => s.Source.Contains("IntegrationTest") && s.Source.Contains("IdGenerator"));
            
            Assert.NotNull(generatedSource);
            
            var generatedCode = generatedSource.Source;
            
            // Verify all formats are generated correctly
            AssertHasIdWithFormat(generatedCode, "CreateHex16Id_id_Id", @"^[a-f0-9]{16}$");
            AssertHasIdWithFormat(generatedCode, "CreateHex32Id_id_Id", @"^[a-f0-9]{32}$");
            AssertHasIdWithFormat(generatedCode, "CreateGuidId_id_Id", @"^[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}$");
            AssertHasIdWithFormat(generatedCode, "CreateHex8Id_id_Id", @"^[a-f0-9]{8}$");
            
            // HTML ID format should start with a letter and contain only valid characters
            var htmlIdMatch = Regex.Match(generatedCode, @"public const string CreateHtmlId_id_Id = ""([a-zA-Z0-9\-_:\.]+)"";");
            Assert.True(htmlIdMatch.Success);
            var htmlId = htmlIdMatch.Groups[1].Value;
            Assert.True(char.IsLetter(htmlId[0]));
            Assert.Matches("^[a-zA-Z][a-zA-Z0-9\\-_:]*$", htmlId);
        }
        
        // Helper method for checking ID formats
        private void AssertHasIdWithFormat(string generatedCode, string constantName, string formatPattern)
        {
            var match = Regex.Match(generatedCode, $@"public const string {constantName} = ""([^""]+)"";");
            Assert.True(match.Success, $"Could not find constant named {constantName}");
            Assert.Matches(formatPattern, match.Groups[1].Value);
        }
        
        [Fact]
        public void IntegrationTest_StaticVsNonStatic_GeneratesCorrectCode()
        {
            // Arrange
            var generator = new UniqueIdGenerator();
            var source = @"
using Praefixum;

namespace StaticTest
{
    public static partial class StaticClass
    {
        public static string GetId([UniqueId] string id = null) => id ?? GetId_id_Id;
    }
    
    public partial class NonStaticClass
    {
        public string GetId([UniqueId] string id = null) => id ?? GetId_id_Id;
    }
}";

            // Act
            var result = SourceGeneratorVerifier.RunGenerator(generator, source, DefaultReferences);

            // Assert
            Assert.Empty(result.Diagnostics);
            
            // Find the implementation files using hint names
            var staticClassSource = result.GeneratedSources.FirstOrDefault(
                s => s.HintName.Contains("StaticClass_UniqueIds.g.cs"));
            var nonStaticClassSource = result.GeneratedSources.FirstOrDefault(
                s => s.HintName.Contains("NonStaticClass_UniqueIds.g.cs"));
            
            Assert.NotNull(staticClassSource);
            Assert.NotNull(nonStaticClassSource);
            
            // Static class should be declared as static
            Assert.Contains("static class StaticClass", staticClassSource.Source);
            Assert.Contains("public const string GetId_id_Id", staticClassSource.Source);
            
            // Non-static class should not be declared as static
            Assert.Contains("partial class NonStaticClass", nonStaticClassSource.Source);
            Assert.Contains("public const string GetId_id_Id", nonStaticClassSource.Source);
        }
        
        [Fact]
        public void IntegrationTest_NestedNamespaces_GeneratesCorrectCode()
        {
            // Arrange
            var generator = new UniqueIdGenerator();
            var source = @"
using Praefixum;

namespace OuterNamespace
{
    namespace MiddleNamespace
    {
        namespace InnerNamespace
        {
            public partial class NestedClass
            {
                public string GetId([UniqueId] string id = null) => id ?? GetId_id_Id;
            }
        }
    }
}";

            // Act
            var result = SourceGeneratorVerifier.RunGenerator(generator, source, DefaultReferences);

            // Assert
            Assert.Empty(result.Diagnostics);
            
            // Find the implementation file (not the attribute file)
            var generatedSource = result.GeneratedSources.FirstOrDefault(
                s => s.Source.Contains("OuterNamespace") && s.Source.Contains("NestedClass"));
            
            Assert.NotNull(generatedSource);
            
            var generatedCode = generatedSource.Source;
            
            // Check namespace structure
            Assert.Contains("namespace OuterNamespace.MiddleNamespace.InnerNamespace", generatedCode);
            Assert.Contains("partial class NestedClass", generatedCode);
            Assert.Contains("public const string GetId_id_Id", generatedCode);
        }
        
        [Fact]
        public void IntegrationTest_HtmlId_ComplianceWithHtmlSpec()
        {
            // Arrange - Create a large number of HTML IDs to test their compliance
            var generator = new UniqueIdGenerator();
            var methodLines = string.Join("\n", Enumerable.Range(1, 50)
                .Select(i => $"public string GetHtmlId{i}([UniqueId(UniqueIdFormat.HtmlId)] string id = null) => id ?? GetHtmlId{i}_id_Id;"));
            
            var source = $@"
using Praefixum;

namespace HtmlIdTest
{{
    public partial class HtmlIdGenerator
    {{
        {methodLines}
    }}
}}";

            // Act
            var result = SourceGeneratorVerifier.RunGenerator(generator, source, DefaultReferences);

            // Assert
            Assert.Empty(result.Diagnostics);
            
            // Find the implementation file (not the attribute file)
            var generatedSource = result.GeneratedSources.FirstOrDefault(
                s => s.Source.Contains("HtmlIdTest") && s.Source.Contains("HtmlIdGenerator"));
            
            Assert.NotNull(generatedSource);
            
            var generatedCode = generatedSource.Source;
              // Extract all HTML IDs
            var matches = Regex.Matches(generatedCode, @"public const string GetHtmlId\d+_id_Id = ""([a-zA-Z0-9\-_]+)"";");
            Assert.Equal(50, matches.Count);
            
            // Verify each ID complies with HTML spec
            foreach (Match match in matches)
            {
                var id = match.Groups[1].Value;
                  // 1. First character must be a letter (HTML requirement)
                Assert.True(char.IsLetter(id[0]));
                
                // 2. Length should be exactly 6 characters
                Assert.Equal(6, id.Length);
                
                // 3. All characters must be valid HTML ID characters (a-z, 0-9, hyphens, underscores)
                Assert.Matches("^[a-zA-Z][a-zA-Z0-9\\-_]*$", id);
            }
            
            // Check uniqueness
            var allIds = matches.Select(m => m.Groups[1].Value).ToList();
            Assert.Equal(allIds.Count, allIds.Distinct().Count());
        }
    }
}
