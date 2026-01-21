using Xunit;

namespace Praefixum.Tests;

/// <summary>
/// Tests that actually use the [UniqueId] attribute to trigger source generation and interceptors
/// These tests verify that the source generator correctly handles methods with UniqueId parameters
/// </summary>
public class UniqueIdAttributeTests
{
    [Fact]
    public void CreateDiv_WithUniqueIdAttribute_GeneratesUniqueId()
    {
        // Act
        var result1 = TestHelpers.CreateDiv();
        var result2 = TestHelpers.CreateDiv();

        // Assert
        Assert.NotEqual(result1, result2);
        Assert.Contains("id=\"", result1);
        Assert.Contains("id=\"", result2);
        
        var id1 = ExtractId(result1);
        var id2 = ExtractId(result2);
        Assert.NotNull(id1);
        Assert.NotNull(id2);
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void CreateDiv_WithExplicitId_UsesProvidedId()
    {
        // Act
        var result = TestHelpers.CreateDiv("explicit-div-id", "content");

        // Assert
        Assert.Equal("<div id=\"explicit-div-id\">content</div>", result);
    }

    [Fact]
    public void CreateButtonWithGuid_GeneratesGuidFormatId()
    {
        // Act
        var result = TestHelpers.CreateButtonWithGuid();

        // Assert
        var id = ExtractId(result);
        Assert.NotNull(id);
        Assert.Equal(32, id.Length); // GUID without dashes should be 32 characters
        Assert.True(id.All(c => char.IsLetterOrDigit(c)));
    }

    [Fact]
    public void CreateInputWithHtmlId_GeneratesHtmlSafeId()
    {
        // Act
        var result = TestHelpers.CreateInputWithHtmlId();

        // Assert
        var id = ExtractId(result);
        Assert.NotNull(id);
        Assert.True(id.Length >= 4); // HTML IDs should be reasonably short
        Assert.True(id.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_'));
        Assert.True(TestHelpers.IsValidHtmlId(id));
    }

    [Fact]
    public void CreateSpanWithPrefix_IncludesPrefix()
    {
        // Act
        var result = TestHelpers.CreateSpanWithPrefix();

        // Assert
        var id = ExtractId(result);
        Assert.NotNull(id);
        Assert.StartsWith("span-", id);
    }

    [Fact]
    public void CreateSectionWithTimestamp_GeneratesTimestampBasedId()
    {
        // Act
        var result = TestHelpers.CreateSectionWithTimestamp();

        // Assert
        var id = ExtractId(result);
        Assert.NotNull(id);
        Assert.NotEmpty(id);
        // Timestamp-based IDs should be different when called at different times
        Thread.Sleep(1);
        var result2 = TestHelpers.CreateSectionWithTimestamp();
        var id2 = ExtractId(result2);
        Assert.NotEqual(id, id2);
    }

    [Fact]
    public void CreateArticleWithShortHash_GeneratesShortHashId()
    {
        // Act
        var result = TestHelpers.CreateArticleWithShortHash();

        // Assert
        var id = ExtractId(result);
        Assert.NotNull(id);
        Assert.NotEmpty(id);
        Assert.True(id.Length <= 16); // Short hash should be reasonably short
    }

    [Fact]
    public void CreateFormWithMultipleIds_GeneratesDifferentIdsForEachParameter()
    {
        // Act
        var result = TestHelpers.CreateFormWithMultipleIds();

        // Assert
        var formIdMatch = System.Text.RegularExpressions.Regex.Match(result, @"<form id=""([^""]+)""");
        var nameInputIdMatch = System.Text.RegularExpressions.Regex.Match(result, @"<input id=""([^""]+)"" type=""text""");
        var emailInputIdMatch = System.Text.RegularExpressions.Regex.Match(result, @"<input id=""([^""]+)"" type=""email""");
        var buttonIdMatch = System.Text.RegularExpressions.Regex.Match(result, @"<button id=""([^""]+)""");

        Assert.True(formIdMatch.Success);
        Assert.True(nameInputIdMatch.Success);
        Assert.True(emailInputIdMatch.Success);
        Assert.True(buttonIdMatch.Success);

        var formId = formIdMatch.Groups[1].Value;
        var nameInputId = nameInputIdMatch.Groups[1].Value;
        var emailInputId = emailInputIdMatch.Groups[1].Value;
        var buttonId = buttonIdMatch.Groups[1].Value;

        // All IDs should be different
        Assert.NotEqual(formId, nameInputId);
        Assert.NotEqual(formId, emailInputId);
        Assert.NotEqual(formId, buttonId);
        Assert.NotEqual(nameInputId, emailInputId);
        Assert.NotEqual(nameInputId, buttonId);
        Assert.NotEqual(emailInputId, buttonId);

        // Form ID should have prefix
        Assert.StartsWith("form-", formId);

        // Button ID should be GUID format (32 characters)
        Assert.Equal(32, buttonId.Length);
    }

    [Fact]
    public void GetIdForElement_ReturnsGeneratedId()
    {
        // Act
        var id = TestHelpers.GetIdForElement();

        // Assert
        Assert.NotNull(id);
        Assert.NotEmpty(id);
    }

    [Fact]
    public void GetHashFromId_ReturnsHashOfGeneratedId()
    {
        // Act
        var hash1 = TestHelpers.GetHashFromId();
        var hash2 = TestHelpers.GetHashFromId();

        // Assert
        Assert.NotEqual(0, hash1);
        Assert.NotEqual(0, hash2);
        Assert.NotEqual(hash1, hash2); // Should be different because IDs are different
    }

    [Fact]
    public void IsValidId_ReturnsTrueForGeneratedId()
    {
        // Act
        var isValid = TestHelpers.IsValidId();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void ProcessElementWithId_StoresGeneratedId()
    {
        // Act
        TestHelpers.ProcessElementWithId();

        // Assert
        Assert.NotNull(TestHelpers.LastProcessedId);
        Assert.StartsWith("process-", TestHelpers.LastProcessedId);
    }

    [Fact]
    public async Task CreateElementAsync_GeneratesUniqueIdInAsyncMethod()
    {
        // Act
        var result1 = await TestHelpers.CreateElementAsync();
        var result2 = await TestHelpers.CreateElementAsync();

        // Assert
        Assert.NotEqual(result1, result2);
        var id1 = ExtractId(result1);
        var id2 = ExtractId(result2);
        Assert.NotNull(id1);
        Assert.NotNull(id2);
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void CreateListWithId_GeneratesIdForGenericMethod()
    {
        // Act
        var list = TestHelpers.CreateListWithId(["item1", "item2", "item3"]);

        // Assert
        Assert.Equal(3, list.Count);
        Assert.NotNull(TestHelpers.LastGeneratedId);
        Assert.NotEmpty(TestHelpers.LastGeneratedId);
    }

    [Fact]
    public void CreateComplexElement_GeneratesIdForSingleParameter()
    {
        // Act
        var result = TestHelpers.CreateComplexElement("p", className: "test-class", content: "Hello World");

        // Assert
        var id = ExtractId(result);
        Assert.NotNull(id);
        Assert.Contains("class=\"test-class\"", result);
        Assert.Contains("Hello World", result);
        Assert.StartsWith("<p", result);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(20)]
    public void MultipleCallsWithUniqueId_AreDeterministic(int count)
    {
        // Act
        var ids = new HashSet<string>();
        for (int i = 0; i < count; i++)
        {
            var result = TestHelpers.CreateDiv();
            var id = ExtractId(result);
            Assert.NotNull(id);
            ids.Add(id);
        }

        // Assert
        Assert.Single(ids);
    }

    [Fact]
    public void DifferentFormats_GenerateDifferentStyleIds()
    {
        // Act
        var guidButton = TestHelpers.CreateButtonWithGuid();
        var htmlInput = TestHelpers.CreateInputWithHtmlId();
        var prefixedSpan = TestHelpers.CreateSpanWithPrefix();

        // Assert
        var guidId = ExtractId(guidButton);
        var htmlId = ExtractId(htmlInput);
        var prefixedId = ExtractId(prefixedSpan);

        Assert.NotNull(guidId);
        Assert.NotNull(htmlId);
        Assert.NotNull(prefixedId);

        // GUID format should be 32 characters
        Assert.Equal(32, guidId.Length);

        // HTML ID should be shorter and HTML-safe
        Assert.True(htmlId.Length < guidId.Length);
        Assert.True(TestHelpers.IsValidHtmlId(htmlId));

        // Prefixed ID should start with prefix
        Assert.StartsWith("span-", prefixedId);
    }

    [Fact]
    public void ExplicitIds_AreNotOverridden()
    {
        // Act
        var divResult = TestHelpers.CreateDiv("my-explicit-div-id");
        var buttonResult = TestHelpers.CreateButtonWithGuid("my-explicit-button-id");
        var inputResult = TestHelpers.CreateInputWithHtmlId("my-explicit-input-id");

        // Assert
        Assert.Contains("my-explicit-div-id", divResult);
        Assert.Contains("my-explicit-button-id", buttonResult);
        Assert.Contains("my-explicit-input-id", inputResult);
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
