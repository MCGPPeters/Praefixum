using Xunit;

namespace Praefixum.Tests;

/// <summary>
/// Edge case and robustness tests for the UniqueIdGenerator
/// </summary>
public class UniqueIdGeneratorEdgeCaseTests
{
    [Fact]
    public void GenerateId_WithNullParameter_GeneratesId()
    {
        // Arrange & Act
        var result = TestHelpers.CreateHtmlElement("div", null);
        var id = TestHelpers.ExtractId(result);
        
        // Assert
        Assert.NotNull(id);
        Assert.True(id.Length > 0);
    }

    [Fact]
    public void GenerateId_WithExplicitId_ReturnsExplicitId()
    {
        // Arrange & Act
        var result = TestHelpers.CreateHtmlElement("div", "my-explicit-id");
        
        // Assert
        Assert.Equal("<div id=\"my-explicit-id\"></div>", result);
    }

    [Fact]
    public void GenerateId_WithEmptyString_GeneratesId()
    {
        // Arrange & Act
        var result = TestHelpers.CreateHtmlElement("div", "");
        var id = TestHelpers.ExtractId(result);
        
        // Assert
        // Empty string should be treated as null and generate an ID
        Assert.NotNull(id);
        Assert.True(id.Length > 0);
    }

    [Theory]
    [InlineData("div")]
    [InlineData("span")]
    [InlineData("button")]
    [InlineData("input")]
    [InlineData("section")]
    [InlineData("article")]
    [InlineData("header")]
    [InlineData("footer")]
    public void GenerateId_WithVariousHtmlTags_WorksCorrectly(string tagName)
    {
        // Arrange & Act
        var result = TestHelpers.CreateHtmlElement(tagName, null);
        var id = TestHelpers.ExtractId(result);
        
        // Assert
        Assert.NotNull(id);
        Assert.True(TestHelpers.IsValidHtmlId(id));
        Assert.Contains($"<{tagName} id=\"{id}\"></{tagName}>", result);
    }

    [Fact]
    public void GenerateId_LargeNumberOfCalls_MaintainsUniqueness()
    {
        // Arrange
        const int largeCount = 1000;
        
        // Act
        var ids = TestHelpers.CreateMultipleElements(largeCount);
        
        // Assert
        Assert.Equal(largeCount, ids.Count);
        Assert.Equal(ids.Count, ids.Distinct().Count()); // All should be unique
        Assert.True(ids.All(id => TestHelpers.IsValidHtmlId(id)));
    }

    [Fact]
    public void GenerateId_MultipleAttributeUsages_WorksCorrectly()
    {
        // Arrange & Act
        var form = TestHelpers.CreateForm();
        var idMatches = System.Text.RegularExpressions.Regex.Matches(form, @"id=""([^""]+)""");
        
        // Assert
        Assert.True(idMatches.Count >= 3); // Form should have multiple elements
        
        var ids = idMatches.Cast<System.Text.RegularExpressions.Match>()
            .Select(m => m.Groups[1].Value)
            .ToList();
            
        Assert.Equal(ids.Count, ids.Distinct().Count()); // All should be unique
    }

    [Fact]
    public void GenerateId_WithSpecialCharacterContent_ProducesSafeIds()
    {
        // Arrange & Act
        var result1 = TestHelpers.CreatePrefixedDiv(null, "Content with <special> & characters!");
        var result2 = TestHelpers.CreateHtmlElementWithAttributes("span", null, "class-with-special-chars");
        
        var id1 = TestHelpers.ExtractId(result1);
        var id2 = TestHelpers.ExtractId(result2);
        
        // Assert
        Assert.NotNull(id1);
        Assert.NotNull(id2);
        Assert.True(TestHelpers.IsValidHtmlId(id1));
        Assert.True(TestHelpers.IsValidHtmlId(id2));
        
        // IDs should not contain problematic characters
        Assert.DoesNotContain("<", id1);
        Assert.DoesNotContain(">", id1);
        Assert.DoesNotContain("&", id1);
        Assert.DoesNotContain(" ", id1);
    }

    [Fact]
    public void GenerateId_MemoryEfficiency_DoesNotLeakMemory()
    {
        // Arrange
        var initialMemory = GC.GetTotalMemory(true);
        
        // Act - Generate many IDs
        for (int i = 0; i < 10000; i++)
        {
            var result = TestHelpers.CreateHtmlElement("div", null);
            // Deliberately not storing results to test for memory leaks
        }
        
        // Force garbage collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        var finalMemory = GC.GetTotalMemory(true);
        
        // Assert - Memory increase should be reasonable (less than 10MB for 10k elements)
        var memoryIncrease = finalMemory - initialMemory;
        Assert.True(memoryIncrease < 10 * 1024 * 1024, $"Memory increase: {memoryIncrease / 1024.0 / 1024.0:F2} MB");
    }

    [Fact]
    public void GenerateId_ThreadSafety_ProducesUniqueIdsAcrossThreads()
    {
        // Arrange
        const int threadsCount = 10;
        const int idsPerThread = 100;
        var allIds = new List<string>();
        var lockObject = new object();
        
        // Act
        var tasks = Enumerable.Range(0, threadsCount)
            .Select(threadId => Task.Run(() =>
            {
                var threadIds = new List<string>();
                for (int i = 0; i < idsPerThread; i++)
                {
                    var result = TestHelpers.CreateHtmlElement("div", null);
                    var id = TestHelpers.ExtractId(result);
                    if (id != null) threadIds.Add(id);
                }
                
                lock (lockObject)
                {
                    allIds.AddRange(threadIds);
                }
            }))
            .ToArray();
            
        Task.WaitAll(tasks);
        
        // Assert
        var expectedCount = threadsCount * idsPerThread;
        Assert.Equal(expectedCount, allIds.Count);
        Assert.Equal(allIds.Count, allIds.Distinct().Count()); // All should be unique across threads
    }
}
