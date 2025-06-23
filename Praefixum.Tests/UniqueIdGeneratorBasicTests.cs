using Xunit;

namespace Praefixum.Tests;

/// <summary>
/// Basic functionality tests for UniqueIdGenerator source generator
/// </summary>
public class UniqueIdGeneratorBasicTests
{
    [Fact]
    public void GenerateId_WithoutAttribute_ReturnsOriginalValue()
    {
        // Arrange & Act
        var result = TestHelpers.CreateHtmlElement("div", "explicit-id");
        
        // Assert
        Assert.Equal("<div id=\"explicit-id\"></div>", result);
    }

    [Fact]
    public void GenerateId_WithUniqueIdAttribute_GeneratesUniqueValue()
    {
        // Arrange & Act
        var result1 = TestHelpers.CreateHtmlElement("div", null);
        var result2 = TestHelpers.CreateHtmlElement("div", null);
        
        // Assert
        Assert.NotEqual(result1, result2);
        Assert.Contains("id=\"", result1);
        Assert.Contains("id=\"", result2);
    }

    [Fact]
    public void GenerateId_WithPrefix_IncludesPrefix()
    {
        // Arrange & Act
        var result = TestHelpers.CreatePrefixedDiv(null, "content");
        
        // Assert
        Assert.Contains("id=\"div-", result);
    }

    [Fact]
    public void GenerateId_WithGuidFormat_GeneratesGuidLikeId()
    {
        // Arrange & Act
        var result = TestHelpers.CreateButton(null, "Test");
        
        // Assert
        var idMatch = System.Text.RegularExpressions.Regex.Match(result, @"id=""([^""]+)""");
        Assert.True(idMatch.Success);
        
        var id = idMatch.Groups[1].Value;
        Assert.Equal(32, id.Length); // GUID without dashes
        Assert.True(id.All(c => char.IsLetterOrDigit(c)));
    }

    [Fact]
    public void GenerateId_WithHtmlIdFormat_GeneratesShortId()
    {
        // Arrange & Act
        var result = TestHelpers.CreateInput(null, "text");
        
        // Assert
        var idMatch = System.Text.RegularExpressions.Regex.Match(result, @"id=""([^""]+)""");
        Assert.True(idMatch.Success);
        
        var id = idMatch.Groups[1].Value;
        Assert.True(id.Length >= 6 && id.Length <= 12);
        Assert.True(id.All(c => char.IsLetterOrDigit(c)));
    }

    [Fact]
    public void GenerateId_MultipleCallsSameSite_GeneratesDifferentIds()
    {
        // Arrange & Act
        var ids = TestHelpers.CreateMultipleElements(5);
        
        // Assert
        Assert.Equal(5, ids.Count);
        Assert.Equal(ids.Count, ids.Distinct().Count()); // All IDs should be unique
    }

    [Fact]
    public void GenerateId_ValidatesHtmlIdFormat()
    {
        // Arrange & Act
        var result = TestHelpers.CreateHtmlElement("span");
        var id = TestHelpers.ExtractId(result);
        
        // Assert
        Assert.NotNull(id);
        Assert.True(TestHelpers.IsValidHtmlId(id));
    }

    [Fact]
    public void CreateForm_GeneratesUniqueIdsForAllElements()
    {
        // Arrange & Act
        var form = TestHelpers.CreateForm();
        
        // Assert
        var idMatches = System.Text.RegularExpressions.Regex.Matches(form, @"id=""([^""]+)""");
        Assert.True(idMatches.Count >= 3); // Should have at least 3 elements with IDs
        
        var ids = idMatches.Cast<System.Text.RegularExpressions.Match>()
            .Select(m => m.Groups[1].Value)
            .ToList();
            
        Assert.Equal(ids.Count, ids.Distinct().Count()); // All IDs should be unique
    }

    // ==========================================
    // TESTS USING ACTUAL [UniqueId] ATTRIBUTES
    // These tests verify the source generator and interceptors work correctly
    // ==========================================

    [Fact]
    public void ActualUniqueIdAttribute_GeneratesValidIds()
    {
        // Act - using actual [UniqueId] attribute
        var div1 = TestHelpers.CreateDiv();
        var div2 = TestHelpers.CreateDiv();
        
        // Assert
        Assert.NotEqual(div1, div2);
        Assert.Contains("id=\"", div1);
        Assert.Contains("id=\"", div2);
        
        var id1 = ExtractId(div1);
        var id2 = ExtractId(div2);
        Assert.NotNull(id1);
        Assert.NotNull(id2);
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void ActualUniqueIdAttribute_WithExplicitValue_UsesProvidedValue()
    {
        // Act - using actual [UniqueId] attribute with explicit value
        var result = TestHelpers.CreateDiv("my-explicit-id", "test content");
        
        // Assert
        Assert.Equal("<div id=\"my-explicit-id\">test content</div>", result);
    }

    [Fact]
    public void ActualUniqueIdAttribute_DifferentFormats_GenerateDifferentStyleIds()
    {
        // Act - using different UniqueId formats
        var button = TestHelpers.CreateButtonWithGuid();
        var input = TestHelpers.CreateInputWithHtmlId();
        var span = TestHelpers.CreateSpanWithPrefix();
        
        // Assert
        var buttonId = ExtractId(button);
        var inputId = ExtractId(input);
        var spanId = ExtractId(span);
        
        Assert.NotNull(buttonId);
        Assert.NotNull(inputId);
        Assert.NotNull(spanId);
        
        // GUID format should be 32 characters
        Assert.Equal(32, buttonId.Length);
        
        // HTML ID should be shorter
        Assert.True(inputId.Length < buttonId.Length);
        
        // Span should have prefix
        Assert.StartsWith("span-", spanId);
    }

    [Fact]
    public void ActualUniqueIdAttribute_MultipleParameters_GeneratesUniqueIds()
    {
        // Act - using method with multiple [UniqueId] parameters
        var form = TestHelpers.CreateFormWithMultipleIds();
        
        // Assert
        var idMatches = System.Text.RegularExpressions.Regex.Matches(form, @"id=""([^""]+)""");
        Assert.Equal(4, idMatches.Count); // Form, name input, email input, submit button
        
        var ids = idMatches.Cast<System.Text.RegularExpressions.Match>()
            .Select(m => m.Groups[1].Value)
            .ToList();
            
        Assert.Equal(4, ids.Distinct().Count()); // All IDs should be unique
    }

    [Fact]
    public void ActualUniqueIdAttribute_ComplexMethod_WorksCorrectly()
    {
        // Act - using method with mixed parameters (only one with [UniqueId])
        var result = TestHelpers.CreateComplexElement("section", className: "highlight", content: "Important content");
        
        // Assert
        Assert.StartsWith("<section", result);
        Assert.Contains("class=\"highlight\"", result);
        Assert.Contains("Important content", result);
        Assert.Contains("id=\"", result);
        
        var id = ExtractId(result);
        Assert.NotNull(id);
        Assert.NotEmpty(id);
    }

    /// <summary>
    /// Helper method to extract ID from HTML element
    /// </summary>
    private static string? ExtractId(string htmlElement)
    {
        var match = System.Text.RegularExpressions.Regex.Match(htmlElement, @"id=""([^""]+)""");
        return match.Success ? match.Groups[1].Value : null;
    }
}
