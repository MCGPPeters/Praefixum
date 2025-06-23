using System.Collections.Concurrent;
using Xunit;

namespace Praefixum.Tests;

/// <summary>
/// Tests for different UniqueId formats and their characteristics
/// </summary>
public class UniqueIdFormatTests
{
    [Fact]
    public void GuidFormat_GeneratesValidGuid()
    {
        // Arrange & Act
        var result = TestHelpers.CreateButton(null, "Test Button");
        var id = TestHelpers.ExtractId(result);
          // Assert
        Assert.NotNull(id);
        Assert.Equal(32, id.Length); // GUID without dashes
        Assert.True(id.All(c => char.IsLetterOrDigit(c)));
        Assert.Contains(id, c => char.IsLetter(c)); // Should contain letters
        Assert.Contains(id, c => char.IsDigit(c)); // Should contain digits
    }

    [Fact]
    public void HtmlIdFormat_GeneratesShortReadableId()
    {
        // Arrange & Act
        var result = TestHelpers.CreateInput(null, "email");
        var id = TestHelpers.ExtractId(result);
        
        // Assert
        Assert.NotNull(id);
        Assert.True(id.Length >= 6 && id.Length <= 12);
        Assert.True(id.All(c => char.IsLetterOrDigit(c)));
        Assert.True(TestHelpers.IsValidHtmlId(id));
    }

    [Theory]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    public void MultipleGuidGeneration_ProducesUniqueIds(int count)
    {
        // Arrange & Act
        var ids = new List<string>();
        for (int i = 0; i < count; i++)
        {
            var result = TestHelpers.CreateButton(null, $"Button {i}");
            var id = TestHelpers.ExtractId(result);
            if (id != null) ids.Add(id);
        }
        
        // Assert
        Assert.Equal(count, ids.Count);
        Assert.Equal(ids.Count, ids.Distinct().Count()); // All should be unique
    }

    [Theory]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    public void MultipleHtmlIdGeneration_ProducesUniqueIds(int count)
    {
        // Arrange & Act
        var ids = new List<string>();
        for (int i = 0; i < count; i++)
        {
            var result = TestHelpers.CreateInput(null, "text");
            var id = TestHelpers.ExtractId(result);
            if (id != null) ids.Add(id);
        }
        
        // Assert
        Assert.Equal(count, ids.Count);
        Assert.Equal(ids.Count, ids.Distinct().Count()); // All should be unique
    }

    [Fact]
    public void PrefixedIds_ContainSpecifiedPrefix()
    {
        // Arrange & Act
        var result = TestHelpers.CreatePrefixedDiv(null, "Content");
        var id = TestHelpers.ExtractId(result);
        
        // Assert
        Assert.NotNull(id);
        Assert.StartsWith("div-", id);
        Assert.True(id.Length > 4); // Should have content after prefix
    }

    [Fact]
    public void MultiplePrefixedIds_AreUnique()
    {
        // Arrange & Act
        var ids = new List<string>();
        for (int i = 0; i < 10; i++)
        {
            var result = TestHelpers.CreatePrefixedDiv(null, $"Content {i}");
            var id = TestHelpers.ExtractId(result);
            if (id != null) ids.Add(id);
        }
        
        // Assert
        Assert.Equal(10, ids.Count);
        Assert.True(ids.All(id => id.StartsWith("div-")));
        Assert.Equal(ids.Count, ids.Distinct().Count()); // All should be unique
    }

    [Fact]
    public void FormElements_GenerateUniqueIdsWithCorrectPrefixes()
    {
        // Arrange & Act
        var form = TestHelpers.CreateForm();
        var idMatches = System.Text.RegularExpressions.Regex.Matches(form, @"id=""([^""]+)""");
        var ids = idMatches.Cast<System.Text.RegularExpressions.Match>()
            .Select(m => m.Groups[1].Value)
            .ToList();
        
        // Assert
        Assert.True(ids.Count >= 3);
        Assert.Equal(ids.Count, ids.Distinct().Count()); // All should be unique
        Assert.True(ids.All(id => TestHelpers.IsValidHtmlId(id)));
    }    [Fact]
    public async Task ConcurrentIdGeneration_ProducesUniqueIds()
    {
        // Arrange & Act
        var tasks = Enumerable.Range(0, 100)
            .Select(async i => 
            {
                await Task.Delay(1); // Small delay to simulate real usage
                var result = TestHelpers.CreateHtmlElement("div", null);
                return TestHelpers.ExtractId(result);
            })
            .ToArray();

        var ids = (await Task.WhenAll(tasks))
            .Where(id => id != null)
            .ToList();
        
        // Assert
        Assert.Equal(100, ids.Count);
        Assert.Equal(ids.Count, ids.Distinct().Count()); // All should be unique even when generated concurrently
    }

    [Fact]
    public void CrossFormat_IdGeneration_ProducesUniqueIds()
    {
        // Arrange & Act - Mix different format generators
        var guidIds = new List<string>();
        var htmlIds = new List<string>();
        var prefixedIds = new List<string>();
        
        for (int i = 0; i < 20; i++)
        {
            var guidResult = TestHelpers.CreateButton(null, $"Button {i}");
            var htmlResult = TestHelpers.CreateInput(null, "text");
            var prefixedResult = TestHelpers.CreatePrefixedDiv(null, $"Content {i}");
            
            var guidId = TestHelpers.ExtractId(guidResult);
            var htmlId = TestHelpers.ExtractId(htmlResult);
            var prefixedId = TestHelpers.ExtractId(prefixedResult);
            
            if (guidId != null) guidIds.Add(guidId);
            if (htmlId != null) htmlIds.Add(htmlId);
            if (prefixedId != null) prefixedIds.Add(prefixedId);
        }
        
        // Assert
        Assert.Equal(20, guidIds.Count);
        Assert.Equal(20, htmlIds.Count);
        Assert.Equal(20, prefixedIds.Count);
        
        // All IDs within each format should be unique
        Assert.Equal(guidIds.Count, guidIds.Distinct().Count());
        Assert.Equal(htmlIds.Count, htmlIds.Distinct().Count());
        Assert.Equal(prefixedIds.Count, prefixedIds.Distinct().Count());
        
        // Cross-format uniqueness (no overlap between different formats)
        var allIds = guidIds.Concat(htmlIds).Concat(prefixedIds).ToList();
        Assert.Equal(allIds.Count, allIds.Distinct().Count());
    }

    [Fact]
    public void IdFormat_Validation_Characteristics()
    {
        // Test GUID format characteristics
        var guidResult = TestHelpers.CreateButton(null, "Test");
        var guidId = TestHelpers.ExtractId(guidResult);
        
        Assert.NotNull(guidId);
        Assert.Equal(32, guidId!.Length); // GUID without dashes
        Assert.True(guidId.All(c => char.IsLetterOrDigit(c)));
        Assert.True(guidId.Any(c => char.IsLetter(c)) && guidId.Any(c => char.IsDigit(c)));
        
        // Test HTML ID format characteristics
        var htmlResult = TestHelpers.CreateInput(null, "text");
        var htmlId = TestHelpers.ExtractId(htmlResult);
        
        Assert.NotNull(htmlId);
        Assert.True(htmlId!.Length >= 6 && htmlId.Length <= 12);
        Assert.True(htmlId.All(c => char.IsLetterOrDigit(c)));
        Assert.True(TestHelpers.IsValidHtmlId(htmlId));
        
        // Test prefixed ID format characteristics
        var prefixedResult = TestHelpers.CreatePrefixedDiv(null, "Content");
        var prefixedId = TestHelpers.ExtractId(prefixedResult);
        
        Assert.NotNull(prefixedId);
        Assert.StartsWith("div-", prefixedId!);
        Assert.True(prefixedId.Length > 4);
        Assert.True(TestHelpers.IsValidHtmlId(prefixedId));
    }

    [Theory]
    [InlineData(1000)]
    [InlineData(5000)]
    public void HighVolume_IdGeneration_MaintainsUniqueness(int count)
    {
        // Arrange & Act
        var ids = new HashSet<string>();
        var duplicateCount = 0;
        
        for (int i = 0; i < count; i++)
        {
            var result = TestHelpers.CreateHtmlElement("div", null);
            var id = TestHelpers.ExtractId(result);
            
            if (id != null)
            {
                if (!ids.Add(id))
                {
                    duplicateCount++;
                }
            }
        }
        
        // Assert
        Assert.Equal(0, duplicateCount); // No duplicates should occur
        Assert.Equal(count, ids.Count);
    }

    [Fact]
    public void NestedHtml_WithMultipleIds_AllUnique()
    {
        // Arrange & Act - Create nested HTML with multiple ID-bearing elements
        var outerDiv = TestHelpers.CreateHtmlElementWithAttributes("div", null, "container");
        var innerButton = TestHelpers.CreateButton(null, "Submit");
        var innerInput = TestHelpers.CreateInput(null, "email");
        var anotherDiv = TestHelpers.CreatePrefixedDiv(null, "Footer content");
        
        var allHtml = $"{outerDiv}{innerButton}{innerInput}{anotherDiv}";
        
        // Extract all IDs
        var idMatches = System.Text.RegularExpressions.Regex.Matches(allHtml, @"id=""([^""]+)""");
        var ids = idMatches.Cast<System.Text.RegularExpressions.Match>()
            .Select(m => m.Groups[1].Value)
            .ToList();
        
        // Assert
        Assert.Equal(4, ids.Count);
        Assert.Equal(ids.Count, ids.Distinct().Count()); // All should be unique
        Assert.All(ids, id => Assert.True(TestHelpers.IsValidHtmlId(id)));
        
        // Verify format-specific requirements
        var prefixedIds = ids.Where(id => id.StartsWith("div-")).ToList();
        Assert.Single(prefixedIds); // Only one should have div- prefix
    }

    [Fact]
    public void EmptyString_NullString_HandledCorrectly()
    {
        // Test handling of empty strings and null values
        var emptyResult = TestHelpers.CreateHtmlElement("span", "");
        var nullResult = TestHelpers.CreateHtmlElement("span", null);
        var whitespaceResult = TestHelpers.CreateHtmlElement("span", "   ");
        
        var emptyId = TestHelpers.ExtractId(emptyResult);
        var nullId = TestHelpers.ExtractId(nullResult);
        var whitespaceId = TestHelpers.ExtractId(whitespaceResult);
        
        // All should generate IDs since empty/null/whitespace should trigger generation
        Assert.NotNull(emptyId);
        Assert.NotNull(nullId);
        Assert.NotNull(whitespaceId);
        
        Assert.True(emptyId!.Length > 0);
        Assert.True(nullId!.Length > 0);
        Assert.True(whitespaceId!.Length > 0);
        
        // They should all be different
        Assert.NotEqual(emptyId, nullId);
        Assert.NotEqual(nullId, whitespaceId);
        Assert.NotEqual(emptyId, whitespaceId);
    }

    [Theory]
    [InlineData("custom-id")]
    [InlineData("very-long-custom-identifier-name")]
    [InlineData("id123")]
    [InlineData("_underscore")]
    [InlineData("CamelCase")]
    public void ExplicitIds_ArePreserved(string explicitId)
    {
        // Act
        var result = TestHelpers.CreateHtmlElement("div", explicitId);
        var extractedId = TestHelpers.ExtractId(result);
        
        // Assert
        Assert.Equal(explicitId, extractedId);
    }

    [Fact]
    public void IdFormat_DoesNotCollideWithCommonHtmlIds()
    {
        // Generate a bunch of IDs and check they don't collide with common HTML IDs
        var commonIds = new[] { "header", "footer", "main", "nav", "content", "sidebar", "menu" };
        var generatedIds = new List<string>();
        
        for (int i = 0; i < 1000; i++)
        {
            var result = TestHelpers.CreateHtmlElement("div", null);
            var id = TestHelpers.ExtractId(result);
            if (id != null) generatedIds.Add(id);
        }
        
        // Assert no collisions with common IDs
        var collisions = generatedIds.Intersect(commonIds).ToList();
        Assert.Empty(collisions);
    }

    [Fact]
    public void SpecialCharacters_InPrefix_HandledCorrectly()
    {
        // Note: This tests the helper method behavior, real generator would need to handle this
        var specialPrefixResult = TestHelpers.CreatePrefixedDiv(null, "Content with special chars: @#$%");
        var id = TestHelpers.ExtractId(specialPrefixResult);
        
        Assert.NotNull(id);
        Assert.StartsWith("div-", id!);
        Assert.True(TestHelpers.IsValidHtmlId(id));
    }    [Fact]
    public async Task ThreadSafety_MultipleThreadsGeneratingIds()
    {
        // Test thread safety by generating IDs from multiple threads
        var threadCount = 10;
        var idsPerThread = 100;
        var allIds = new ConcurrentBag<string>();
        
        var tasks = Enumerable.Range(0, threadCount)
            .Select(threadId => Task.Run(() =>
            {
                for (int i = 0; i < idsPerThread; i++)
                {
                    var result = TestHelpers.CreateHtmlElement("div", null);
                    var id = TestHelpers.ExtractId(result);
                    if (id != null) allIds.Add(id);
                }
            }))
            .ToArray();
        
        await Task.WhenAll(tasks);
        
        var idList = allIds.ToList();
        Assert.Equal(threadCount * idsPerThread, idList.Count);
        Assert.Equal(idList.Count, idList.Distinct().Count()); // All should be unique
    }
}
