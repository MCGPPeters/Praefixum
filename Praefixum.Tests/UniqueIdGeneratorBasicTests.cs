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
}
