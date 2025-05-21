using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace UniqueIdGenerator.Tests
{
    public class UniqueIdGeneratorFormatTests
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
        public void DefaultFormat_GeneratesHex16Format()
        {
            // Arrange
            var generator = new UniqueIdGenerator();
            var source = @"
using UniqueIdGenerator;

namespace MyNamespace
{
    public partial class MyClass
    {
        public string GetId([UniqueId] string id = null) => id ?? GetId_id_Id;
    }
}";

            // Act
            var result = SourceGeneratorVerifier.RunGenerator(generator, source, DefaultReferences);

            // Assert
            Assert.Empty(result.Diagnostics);
            
            // There should be 2 generated sources (attribute + implementation)
            Assert.Equal(2, result.GeneratedSources.Length);
            
            // Find the implementation file (not the attribute file)
            var generatedSource = result.GeneratedSources.FirstOrDefault(
                s => s.Source.Contains("MyNamespace") && s.Source.Contains("MyClass"));
            
            Assert.NotNull(generatedSource);
            
            var generatedCode = generatedSource.Source;
            Assert.Contains("public const string GetId_id_Id", generatedCode);
            
            // Extract the generated ID
            var match = Regex.Match(generatedCode, @"public const string GetId_id_Id = ""([a-f0-9]+)"";");
            Assert.True(match.Success);
            
            // Verify it has 16 hex characters (Hex16 format)
            var generatedId = match.Groups[1].Value;
            Assert.Equal(16, generatedId.Length);
            Assert.Matches("^[a-f0-9]{16}$", generatedId);
        }
        
        [Fact]
        public void Hex32Format_GeneratesCorrectId()
        {
            // Arrange
            var generator = new UniqueIdGenerator();
            var source = @"
using UniqueIdGenerator;

namespace MyNamespace
{
    public partial class MyClass
    {
        public string GetId([UniqueId(UniqueIdFormat.Hex32)] string id = null) => id ?? GetId_id_Id;
    }
}";

            // Act
            var result = SourceGeneratorVerifier.RunGenerator(generator, source, DefaultReferences);

            // Assert
            Assert.Empty(result.Diagnostics);
            
            // Find the implementation file (not the attribute file)
            var generatedSource = result.GeneratedSources.FirstOrDefault(
                s => s.Source.Contains("MyNamespace") && s.Source.Contains("MyClass"));
            
            Assert.NotNull(generatedSource);
            
            var generatedCode = generatedSource.Source;
            Assert.Contains("public const string GetId_id_Id", generatedCode);
            
            // Extract the generated ID
            var match = Regex.Match(generatedCode, @"public const string GetId_id_Id = ""([a-f0-9]+)"";");
            Assert.True(match.Success);
            
            // Verify it has 32 hex characters (Hex32 format)
            var generatedId = match.Groups[1].Value;
            Assert.Equal(32, generatedId.Length);
            Assert.Matches("^[a-f0-9]{32}$", generatedId);
        }
        
        [Fact]
        public void GuidFormat_GeneratesCorrectId()
        {
            // Arrange
            var generator = new UniqueIdGenerator();
            var source = @"
using UniqueIdGenerator;

namespace MyNamespace
{
    public partial class MyClass
    {
        public string GetId([UniqueId(UniqueIdFormat.Guid)] string id = null) => id ?? GetId_id_Id;
    }
}";

            // Act
            var result = SourceGeneratorVerifier.RunGenerator(generator, source, DefaultReferences);

            // Assert
            Assert.Empty(result.Diagnostics);
            
            // Find the implementation file (not the attribute file)
            var generatedSource = result.GeneratedSources.FirstOrDefault(
                s => s.Source.Contains("MyNamespace") && s.Source.Contains("MyClass"));
            
            Assert.NotNull(generatedSource);
            
            var generatedCode = generatedSource.Source;
            Assert.Contains("public const string GetId_id_Id", generatedCode);
            
            // Extract the generated ID
            var match = Regex.Match(generatedCode, @"public const string GetId_id_Id = ""([0-9a-f\-]+)"";");
            Assert.True(match.Success);
            
            // Verify it has the correct GUID format
            var generatedId = match.Groups[1].Value;
            Assert.Matches("^[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}$", generatedId);
        }
        
        [Fact]
        public void Hex8Format_GeneratesCorrectId()
        {
            // Arrange
            var generator = new UniqueIdGenerator();
            var source = @"
using UniqueIdGenerator;

namespace MyNamespace
{
    public partial class MyClass
    {
        public string GetId([UniqueId(UniqueIdFormat.Hex8)] string id = null) => id ?? GetId_id_Id;
    }
}";

            // Act
            var result = SourceGeneratorVerifier.RunGenerator(generator, source, DefaultReferences);

            // Assert
            Assert.Empty(result.Diagnostics);
            
            // Find the implementation file (not the attribute file)
            var generatedSource = result.GeneratedSources.FirstOrDefault(
                s => s.Source.Contains("MyNamespace") && s.Source.Contains("MyClass"));
            
            Assert.NotNull(generatedSource);
            
            var generatedCode = generatedSource.Source;
            Assert.Contains("public const string GetId_id_Id", generatedCode);
            
            // Extract the generated ID
            var match = Regex.Match(generatedCode, @"public const string GetId_id_Id = ""([a-f0-9]+)"";");
            Assert.True(match.Success);
            
            // Verify it has 8 hex characters (Hex8 format)
            var generatedId = match.Groups[1].Value;
            Assert.Equal(8, generatedId.Length);
            Assert.Matches("^[a-f0-9]{8}$", generatedId);
        }
        
        [Fact]
        public void HtmlIdFormat_GeneratesCorrectId()
        {
            // Arrange
            var generator = new UniqueIdGenerator();
            var source = @"
using UniqueIdGenerator;

namespace MyNamespace
{
    public partial class MyClass
    {
        public string GetId([UniqueId(UniqueIdFormat.HtmlId)] string id = null) => id ?? GetId_id_Id;
    }
}";

            // Act
            var result = SourceGeneratorVerifier.RunGenerator(generator, source, DefaultReferences);

            // Assert
            Assert.Empty(result.Diagnostics);
            
            // Find the implementation file (not the attribute file)
            var generatedSource = result.GeneratedSources.FirstOrDefault(
                s => s.Source.Contains("MyNamespace") && s.Source.Contains("MyClass"));
            
            Assert.NotNull(generatedSource);
            
            var generatedCode = generatedSource.Source;
            Assert.Contains("public const string GetId_id_Id", generatedCode);
            
            // Extract the generated ID
            var match = Regex.Match(generatedCode, @"public const string GetId_id_Id = ""([a-zA-Z0-9\-_:\.]+)"";");
            Assert.True(match.Success);
            
            // Verify it starts with a letter (HTML ID requirement)
            var generatedId = match.Groups[1].Value;
            Assert.Matches("^[a-zA-Z]", generatedId);
            
            // Verify it only contains valid HTML ID characters
            Assert.Matches("^[a-zA-Z0-9\\-_:]+$", generatedId);
        }
    }
}
