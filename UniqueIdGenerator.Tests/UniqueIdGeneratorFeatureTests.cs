using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace UniqueIdGenerator.Tests
{
    public class UniqueIdGeneratorFeatureTests
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
        public void MultipleAttributes_OnSameLine_GenerateUniqueIds()
        {
            // Arrange
            var generator = new UniqueIdGenerator();
            var source = @"
using UniqueIdGenerator;

namespace MyNamespace
{
    public partial class MyClass
    {
        // Two attributes on the same line
        public string GetId1([UniqueId] string id = null) => id ?? GetId1_id_Id; public string GetId2([UniqueId] string id = null) => id ?? GetId2_id_Id;
    }
}";

            // Act
            var result = SourceGeneratorVerifier.RunGenerator(generator, source, DefaultReferences);

            // Assert
            Assert.Empty(result.Diagnostics);
            
            // Find the implementation file using the hint name pattern
            var generatedSource = result.GeneratedSources.FirstOrDefault(
                s => s.HintName.Contains("MyClass_UniqueIds.g.cs"));
            
            Assert.NotNull(generatedSource);
            
            var generatedCode = generatedSource.Source;
            
            // Extract both generated IDs
            var matches = Regex.Matches(generatedCode, @"public const string (\w+) = ""([a-f0-9]+)"";");
            Assert.Equal(2, matches.Count);
            
            // Get the IDs
            var id1 = matches[0].Groups[2].Value;
            var id2 = matches[1].Groups[2].Value;
            
            // Ensure they're different even though they're on the same line
            Assert.NotEqual(id1, id2);
        }
        
        [Fact]
        public void MultipleParameters_InSameMethod_GenerateUniqueIds()
        {
            // Arrange
            var generator = new UniqueIdGenerator();
            var source = @"
using UniqueIdGenerator;

namespace MyNamespace
{
    public partial class MyClass
    {
        // Method with multiple UniqueId parameters
        public string GetIds([UniqueId] string id1 = null, [UniqueId] string id2 = null) 
        {
            id1 ??= GetIds_id1_Id;
            id2 ??= GetIds_id2_Id;
            return id1 + id2;
        }
    }
}";

            // Act
            var result = SourceGeneratorVerifier.RunGenerator(generator, source, DefaultReferences);

            // Assert
            Assert.Empty(result.Diagnostics);
            
            // Find the implementation file using the hint name pattern
            var generatedSource = result.GeneratedSources.FirstOrDefault(
                s => s.HintName.Contains("MyClass_UniqueIds.g.cs"));
            
            Assert.NotNull(generatedSource);
            
            var generatedCode = generatedSource.Source;
            
            // Extract both generated IDs
            var matches = Regex.Matches(generatedCode, @"public const string (\w+) = ""([a-f0-9]+)"";");
            Assert.Equal(2, matches.Count);
            
            // Get the IDs
            var id1 = matches.First(m => m.Groups[1].Value == "GetIds_id1_Id").Groups[2].Value;
            var id2 = matches.First(m => m.Groups[1].Value == "GetIds_id2_Id").Groups[2].Value;
            
            // Ensure they're different
            Assert.NotEqual(id1, id2);
        }
        
        [Fact]
        public void DifferentClasses_GenerateUniqueIds()
        {
            // Arrange
            var generator = new UniqueIdGenerator();
            var source = @"
using UniqueIdGenerator;

namespace MyNamespace
{
    public partial class Class1
    {
        public string GetId([UniqueId] string id = null) => id ?? GetId_id_Id;
    }
    
    public partial class Class2
    {
        public string GetId([UniqueId] string id = null) => id ?? GetId_id_Id;
    }
}";

            // Act
            var result = SourceGeneratorVerifier.RunGenerator(generator, source, DefaultReferences);

            // Assert
            Assert.Empty(result.Diagnostics);
            
            // We should have multiple generated source files
            Assert.True(result.GeneratedSources.Length >= 3);
            
            // Find files for both classes using hint names
            var class1Source = result.GeneratedSources.FirstOrDefault(
                s => s.HintName.Contains("Class1_UniqueIds.g.cs"));
            var class2Source = result.GeneratedSources.FirstOrDefault(
                s => s.HintName.Contains("Class2_UniqueIds.g.cs"));
            
            Assert.NotNull(class1Source);
            Assert.NotNull(class2Source);
            
            // Extract IDs from both classes
            var match1 = Regex.Match(class1Source.Source, @"public const string GetId_id_Id = ""([a-f0-9]+)"";");
            var match2 = Regex.Match(class2Source.Source, @"public const string GetId_id_Id = ""([a-f0-9]+)"";");
            
            Assert.True(match1.Success);
            Assert.True(match2.Success);
            
            var id1 = match1.Groups[1].Value;
            var id2 = match2.Groups[1].Value;
            
            // Ensure IDs in different classes with same method name are different
            Assert.NotEqual(id1, id2);
        }
        
        [Fact]
        public void HtmlIdFormat_GeneratesValidIdStartingWithLetter()
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
            
            // Find the implementation file using the hint name pattern
            var generatedSource = result.GeneratedSources.FirstOrDefault(
                s => s.HintName.Contains("MyClass_UniqueIds.g.cs"));
            
            Assert.NotNull(generatedSource);
            
            var generatedCode = generatedSource.Source;
            
            // Extract the generated ID
            var match = Regex.Match(generatedCode, @"public const string GetId_id_Id = ""([a-zA-Z0-9\-_:\.]+)"";");
            Assert.True(match.Success);
            
            var generatedId = match.Groups[1].Value;
            
            // Verify HTML ID format requirements:
            // First character must be a letter (HTML ID requirement)
            Assert.True(char.IsLetter(generatedId[0]));
            
            // Only contains valid HTML ID characters
            Assert.Matches("^[a-zA-Z][a-zA-Z0-9\\-_:]*$", generatedId);
        }
        
        [Fact]
        public void SingleSource_MultipleIds_AllUnique()
        {
            // Arrange
            var generator = new UniqueIdGenerator();
            var source = @"
using UniqueIdGenerator;

namespace MyNamespace
{
    public partial class MyClass
    {
        public string GetId1([UniqueId] string id = null) => id ?? GetId1_id_Id;
        public string GetId2([UniqueId] string id = null) => id ?? GetId2_id_Id;
        public string GetId3([UniqueId] string id = null) => id ?? GetId3_id_Id;
        public string GetId4([UniqueId] string id = null) => id ?? GetId4_id_Id;
        public string GetId5([UniqueId] string id = null) => id ?? GetId5_id_Id;
        public string GetId6([UniqueId] string id = null) => id ?? GetId6_id_Id;
        public string GetId7([UniqueId] string id = null) => id ?? GetId7_id_Id;
        public string GetId8([UniqueId] string id = null) => id ?? GetId8_id_Id;
        public string GetId9([UniqueId] string id = null) => id ?? GetId9_id_Id;
        public string GetId10([UniqueId] string id = null) => id ?? GetId10_id_Id;
    }
}";

            // Act
            var result = SourceGeneratorVerifier.RunGenerator(generator, source, DefaultReferences);

            // Assert
            Assert.Empty(result.Diagnostics);
            
            // Find the implementation file using the hint name pattern
            var generatedSource = result.GeneratedSources.FirstOrDefault(
                s => s.HintName.Contains("MyClass_UniqueIds.g.cs"));
            
            Assert.NotNull(generatedSource);
            
            var generatedCode = generatedSource.Source;
            
            // Extract all generated IDs
            var matches = Regex.Matches(generatedCode, @"public const string (\w+) = ""([a-f0-9]+)"";");
            Assert.Equal(10, matches.Count);
            
            // Get all IDs and verify they're all unique
            var ids = matches.Select(m => m.Groups[2].Value).ToList();
            var uniqueIds = ids.Distinct().ToList();
            
            // All IDs should be unique
            Assert.Equal(ids.Count, uniqueIds.Count);
        }
    }
}
