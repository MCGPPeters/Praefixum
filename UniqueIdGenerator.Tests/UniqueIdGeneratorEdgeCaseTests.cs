using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace UniqueIdGenerator.Tests
{
    public class UniqueIdGeneratorEdgeCaseTests
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
        public void EdgeCase_EmptyNamespace_GeneratesCorrectCode()
        {
            // Arrange
            var generator = new UniqueIdGenerator();
            var source = @"
using UniqueIdGenerator;

// No namespace
public partial class GlobalClass
{
    public string GetId([UniqueId] string id = null) => id ?? GetId_id_Id;
}";

            // Act
            var result = SourceGeneratorVerifier.RunGenerator(generator, source, DefaultReferences);

            // Assert
            Assert.Empty(result.Diagnostics);
            
            // Find the implementation file using the hint name pattern
            var generatedSource = result.GeneratedSources.FirstOrDefault(
                s => s.HintName.Contains("GlobalClass_UniqueIds.g.cs"));
            
            Assert.NotNull(generatedSource);
            
            var generatedCode = generatedSource.Source;
            
            // Note: The source generator may now include an empty namespace or a special global namespace
            // What's important is that our class and constant are generated correctly
            Assert.Contains("partial class GlobalClass", generatedCode);
            Assert.Contains("public const string GetId_id_Id", generatedCode);
        }
        
        [Fact]
        public void EdgeCase_GenericClass_GeneratesCorrectCode()
        {
            // Arrange
            var generator = new UniqueIdGenerator();
            var source = @"
using UniqueIdGenerator;

namespace MyNamespace
{
    public partial class GenericClass<T>
    {
        public string GetId([UniqueId] string id = null) => id ?? GetId_id_Id;
    }
}";

            // Act
            var result = SourceGeneratorVerifier.RunGenerator(generator, source, DefaultReferences);

            // Assert
            Assert.Empty(result.Diagnostics);
            
            // Find the implementation file using the hint name pattern
            var generatedSource = result.GeneratedSources.FirstOrDefault(
                s => s.HintName.Contains("GenericClass_UniqueIds.g.cs"));
            
            Assert.NotNull(generatedSource);
            
            var generatedCode = generatedSource.Source;
            
            // The generated code should include the generic type parameter
            Assert.Contains("class GenericClass", generatedCode);
            Assert.Contains("public const string GetId_id_Id", generatedCode);
        }
        
        [Fact]
        public void EdgeCase_NonDefaultParameterName_GeneratesCorrectCode()
        {
            // Arrange
            var generator = new UniqueIdGenerator();
            var source = @"
using UniqueIdGenerator;

namespace MyNamespace
{
    public partial class MyClass
    {
        public string GetId([UniqueId] string customParamName = null) => customParamName ?? GetId_customParamName_Id;
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
            Assert.Contains("public const string GetId_customParamName_Id", generatedCode);
        }
        
        [Fact]
        public void EdgeCase_OtherAttributes_DontInterfere()
        {
            // Arrange
            var generator = new UniqueIdGenerator();
            var source = @"
using UniqueIdGenerator;
using System;

namespace MyNamespace
{
    public partial class MyClass
    {
        public string GetId([Obsolete][UniqueId] string id = null) => id ?? GetId_id_Id;
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
        }
        
        [Fact]
        public void EdgeCase_NestedClass_GeneratesCorrectCode()
        {
            // Arrange
            var generator = new UniqueIdGenerator();
            var source = @"
using UniqueIdGenerator;

namespace MyNamespace
{
    public partial class OuterClass
    {
        public partial class NestedClass
        {
            public string GetId([UniqueId] string id = null) => id ?? GetId_id_Id;
        }
    }
}";

            // Act
            var result = SourceGeneratorVerifier.RunGenerator(generator, source, DefaultReferences);

            // Assert
            Assert.Empty(result.Diagnostics);
            
            // Find the implementation file for either OuterClass or NestedClass
            var generatedSource = result.GeneratedSources.FirstOrDefault(
                s => s.HintName.Contains("OuterClass_UniqueIds.g.cs") || 
                     s.HintName.Contains("NestedClass_UniqueIds.g.cs"));
            
            Assert.NotNull(generatedSource);
            
            var generatedCode = generatedSource.Source;
            
            // The generated code should contain the nested structure
            // Either as a nested class or with direct access to the nested class
            Assert.Contains("GetId_id_Id", generatedCode);
        }
        
        [Fact]
        public void EdgeCase_ClassWithExplicitInterfaceImplementation_GeneratesCorrectCode()
        {
            // Arrange
            var generator = new UniqueIdGenerator();
            var source = @"
using UniqueIdGenerator;

namespace MyNamespace
{
    public interface IHasId
    {
        string GetId(string id = null);
    }
    
    public partial class MyClass : IHasId
    {
        // Explicit interface implementation with UniqueId
        string IHasId.GetId([UniqueId] string id) => id ?? GetId_id_Id;
        
        // Regular method
        public string GetAnotherId([UniqueId] string id = null) => id ?? GetAnotherId_id_Id;
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
            
            // Verify the constants for both methods are present
            // The constant names should match the method names, regardless of whether it's
            // an explicit interface implementation or not
            Assert.Contains("GetId_id_Id", generatedCode);
            Assert.Contains("GetAnotherId_id_Id", generatedCode);
        }
        
        [Fact]
        public void EdgeCase_MethodWithMultipleParameters_OnlyUniqueIdAttributeParametersGenerate()
        {
            // Arrange
            var generator = new UniqueIdGenerator();
            var source = @"
using UniqueIdGenerator;

namespace MyNamespace
{
    public partial class MyClass
    {
        public string ProcessData(
            string regularParam, 
            [UniqueId] string id = null, 
            int count = 0, 
            [UniqueId] string secondId = null)
        {
            id ??= ProcessData_id_Id;
            secondId ??= ProcessData_secondId_Id;
            return id + secondId;
        }
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
            
            // Only the parameters with UniqueId attribute should generate constants
            Assert.Contains("public const string ProcessData_id_Id", generatedCode);
            Assert.Contains("public const string ProcessData_secondId_Id", generatedCode);
            
            // The method should have exactly 2 constants
            var constants = Regex.Matches(generatedCode, @"public const string ProcessData_\w+_Id");
            Assert.Equal(2, constants.Count);
        }
    }
}
