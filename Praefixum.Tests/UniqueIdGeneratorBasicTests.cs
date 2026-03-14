namespace Praefixum.Tests;

/// <summary>
/// Basic functionality tests for UniqueIdGenerator source generator
/// </summary>
public class UniqueIdGeneratorBasicTests
{
    [Test]
    public async Task GenerateId_WithoutAttribute_ReturnsOriginalValue()
    {
        // Arrange & Act
        var result = TestHelpers.CreateHtmlElement("div", "explicit-id");
        
        // Assert
        await Assert.That(result).IsEqualTo("<div id=\"explicit-id\"></div>");
    }

    [Test]
    public async Task GenerateId_WithUniqueIdAttribute_GeneratesDifferentIdsForDifferentCallSites()
    {
        // Arrange & Act
        var result1 = TestHelpers.CreateHtmlElement("div", null);
        var result2 = TestHelpers.CreateHtmlElement("div", null);
        
        // Assert
        await Assert.That(result1).IsNotEqualTo(result2);
        await Assert.That(result1).Contains("id=\"");
        await Assert.That(result2).Contains("id=\"");
    }

    [Test]
    public async Task GenerateId_WithPrefix_IncludesPrefix()
    {
        // Arrange & Act
        var result = TestHelpers.CreatePrefixedDiv(null, "content");
        
        // Assert
        await Assert.That(result).Contains("id=\"div-");
    }

    [Test]
    public async Task GenerateId_WithGuidFormat_GeneratesGuidLikeId()
    {
        // Arrange & Act
        var result = TestHelpers.CreateButton(null, "Test");
        
        // Assert
        var idMatch = System.Text.RegularExpressions.Regex.Match(result, @"id=""([^""]+)""");
        await Assert.That(idMatch.Success).IsTrue();
        
        var id = idMatch.Groups[1].Value;
        await Assert.That(id.Length).IsEqualTo(32); // GUID without dashes
        await Assert.That(id.All(c => char.IsLetterOrDigit(c))).IsTrue();
    }

    [Test]
    public async Task GenerateId_WithHtmlIdFormat_GeneratesShortId()
    {
        // Arrange & Act
        var result = TestHelpers.CreateInput(null, "text");
        
        // Assert
        var idMatch = System.Text.RegularExpressions.Regex.Match(result, @"id=""([^""]+)""");
        await Assert.That(idMatch.Success).IsTrue();
        
        var id = idMatch.Groups[1].Value;
        await Assert.That(id.Length >= 6 && id.Length <= 12).IsTrue();
        await Assert.That(id.All(c => char.IsLetterOrDigit(c))).IsTrue();
    }

    [Test]
    public async Task GenerateId_MultipleCallsSameSite_GeneratesSameId()
    {
        // Arrange & Act
        var ids = TestHelpers.CreateMultipleElements(5);
        
        // Assert
        await Assert.That(ids.Count).IsEqualTo(5);
        await Assert.That(ids.Distinct()).HasSingleItem();
    }

    [Test]
    public async Task GenerateId_ValidatesHtmlIdFormat()
    {
        // Arrange & Act
        var result = TestHelpers.CreateHtmlElement("span");
        var id = TestHelpers.ExtractId(result);
        
        // Assert
        await Assert.That(id).IsNotNull();
        await Assert.That(TestHelpers.IsValidHtmlId(id!)).IsTrue();
    }

    [Test]
    public async Task CreateForm_GeneratesUniqueIdsForAllElements()
    {
        // Arrange & Act
        var form = TestHelpers.CreateForm();
        
        // Assert
        var idMatches = System.Text.RegularExpressions.Regex.Matches(form, @"id=""([^""]+)""");
        await Assert.That(idMatches.Count >= 3).IsTrue(); // Should have at least 3 elements with IDs
        
        var ids = idMatches.Cast<System.Text.RegularExpressions.Match>()
            .Select(m => m.Groups[1].Value)
            .ToList();
            
        await Assert.That(ids.Count).IsEqualTo(ids.Distinct().Count()); // All IDs should be unique
    }

    // ==========================================
    // TESTS USING ACTUAL [UniqueId] ATTRIBUTES
    // These tests verify the source generator and interceptors work correctly
    // ==========================================

    [Test]
    public async Task ActualUniqueIdAttribute_GeneratesDifferentIdsForDifferentCallSites()
    {
        // Act - using actual [UniqueId] attribute
        var div1 = TestHelpers.CreateDiv();
        var div2 = TestHelpers.CreateDiv();
        
        // Assert
        await Assert.That(div1).IsNotEqualTo(div2);
        await Assert.That(div1).Contains("id=\"");
        await Assert.That(div2).Contains("id=\"");
        
        var id1 = TestHelpers.ExtractId(div1);
        var id2 = TestHelpers.ExtractId(div2);
        await Assert.That(id1).IsNotNull();
        await Assert.That(id2).IsNotNull();
        await Assert.That(id1).IsNotEqualTo(id2);
    }

    [Test]
    public async Task ActualUniqueIdAttribute_WithExplicitValue_UsesProvidedValue()
    {
        // Act - using actual [UniqueId] attribute with explicit value
        var result = TestHelpers.CreateDiv("my-explicit-id", "test content");
        
        // Assert
        await Assert.That(result).IsEqualTo("<div id=\"my-explicit-id\">test content</div>");
    }

    [Test]
    public async Task ActualUniqueIdAttribute_DifferentFormats_GenerateDifferentStyleIds()
    {
        // Act - using different UniqueId formats
        var button = TestHelpers.CreateButtonWithGuid();
        var input = TestHelpers.CreateInputWithHtmlId();
        var span = TestHelpers.CreateSpanWithPrefix();
        
        // Assert
        var buttonId = TestHelpers.ExtractId(button);
        var inputId = TestHelpers.ExtractId(input);
        var spanId = TestHelpers.ExtractId(span);
        
        await Assert.That(buttonId).IsNotNull();
        await Assert.That(inputId).IsNotNull();
        await Assert.That(spanId).IsNotNull();
        
        // GUID format should be 32 characters
        await Assert.That(buttonId!.Length).IsEqualTo(32);
        
        // HTML ID should be shorter
        await Assert.That(inputId!.Length < buttonId.Length).IsTrue();
        
        // Span should have prefix
        await Assert.That(spanId!).StartsWith("span-");
    }

    [Test]
    public async Task ActualUniqueIdAttribute_MultipleParameters_GeneratesUniqueIds()
    {
        // Act - using method with multiple [UniqueId] parameters
        var form = TestHelpers.CreateFormWithMultipleIds();
        
        // Assert
        var idMatches = System.Text.RegularExpressions.Regex.Matches(form, @"id=""([^""]+)""");
        await Assert.That(idMatches.Count).IsEqualTo(4); // Form, name input, email input, submit button
        
        var ids = idMatches.Cast<System.Text.RegularExpressions.Match>()
            .Select(m => m.Groups[1].Value)
            .ToList();
            
        await Assert.That(ids.Distinct().Count()).IsEqualTo(4); // All IDs should be unique
    }

    [Test]
    public async Task ActualUniqueIdAttribute_ComplexMethod_WorksCorrectly()
    {
        // Act - using method with mixed parameters (only one with [UniqueId])
        var result = TestHelpers.CreateComplexElement("section", className: "highlight", content: "Important content");
        
        // Assert
        await Assert.That(result).StartsWith("<section");
        await Assert.That(result).Contains("class=\"highlight\"");
        await Assert.That(result).Contains("Important content");
        await Assert.That(result).Contains("id=\"");
        
        var id = TestHelpers.ExtractId(result);
        await Assert.That(id).IsNotNull();
        await Assert.That(id!).IsNotEmpty();
    }

}
