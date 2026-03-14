namespace Praefixum.Tests;

/// <summary>
/// Edge case and robustness tests for the UniqueIdGenerator
/// </summary>
public class UniqueIdGeneratorEdgeCaseTests
{
    [Test]
    public async Task GenerateId_WithNullParameter_GeneratesId()
    {
        // Arrange & Act
        var result = TestHelpers.CreateHtmlElement("div", null);
        var id = TestHelpers.ExtractId(result);
        
        // Assert
        await Assert.That(id).IsNotNull();
        await Assert.That(id!.Length > 0).IsTrue();
    }

    [Test]
    public async Task GenerateId_WithExplicitId_ReturnsExplicitId()
    {
        // Arrange & Act
        var result = TestHelpers.CreateHtmlElement("div", "my-explicit-id");
        
        // Assert
        await Assert.That(result).IsEqualTo("<div id=\"my-explicit-id\"></div>");
    }

    [Test]
    public async Task GenerateId_WithEmptyString_GeneratesId()
    {
        // Arrange & Act
        var result = TestHelpers.CreateHtmlElement("div", "");
        var id = TestHelpers.ExtractId(result);
        
        // Assert
        // Empty string should be treated as null and generate an ID
        await Assert.That(id).IsNotNull();
        await Assert.That(id!.Length > 0).IsTrue();
    }

    [Test]
    [Arguments("div")]
    [Arguments("span")]
    [Arguments("button")]
    [Arguments("input")]
    [Arguments("section")]
    [Arguments("article")]
    [Arguments("header")]
    [Arguments("footer")]
    public async Task GenerateId_WithVariousHtmlTags_WorksCorrectly(string tagName)
    {
        // Arrange & Act
        var result = TestHelpers.CreateHtmlElement(tagName, null);
        var id = TestHelpers.ExtractId(result);
        
        // Assert
        await Assert.That(id).IsNotNull();
        await Assert.That(TestHelpers.IsValidHtmlId(id!)).IsTrue();
        await Assert.That(result).Contains($"<{tagName} id=\"{id}\"></{tagName}>");
    }

    [Test]
    public async Task GenerateId_LargeNumberOfCalls_IsDeterministicPerCallSite()
    {
        // Arrange
        const int largeCount = 1000;
        
        // Act
        var ids = TestHelpers.CreateMultipleElements(largeCount);
        
        // Assert
        await Assert.That(ids.Count).IsEqualTo(largeCount);
        await Assert.That(ids.Distinct()).HasSingleItem();
        await Assert.That(ids.All(id => TestHelpers.IsValidHtmlId(id))).IsTrue();
    }

    [Test]
    public async Task GenerateId_MultipleAttributeUsages_WorksCorrectly()
    {
        // Arrange & Act
        var form = TestHelpers.CreateForm();
        var idMatches = System.Text.RegularExpressions.Regex.Matches(form, @"id=""([^""]+)""");
        
        // Assert
        await Assert.That(idMatches.Count >= 3).IsTrue(); // Form should have multiple elements
        
        var ids = idMatches.Cast<System.Text.RegularExpressions.Match>()
            .Select(m => m.Groups[1].Value)
            .ToList();
            
        await Assert.That(ids.Count).IsEqualTo(ids.Distinct().Count()); // All should be unique
    }

    [Test]
    public async Task GenerateId_WithSpecialCharacterContent_ProducesSafeIds()
    {
        // Arrange & Act
        var result1 = TestHelpers.CreatePrefixedDiv(null, "Content with <special> & characters!");
        var result2 = TestHelpers.CreateHtmlElementWithAttributes("span", null, "class-with-special-chars");
        
        var id1 = TestHelpers.ExtractId(result1);
        var id2 = TestHelpers.ExtractId(result2);
        
        // Assert
        await Assert.That(id1).IsNotNull();
        await Assert.That(id2).IsNotNull();
        await Assert.That(TestHelpers.IsValidHtmlId(id1!)).IsTrue();
        await Assert.That(TestHelpers.IsValidHtmlId(id2!)).IsTrue();
        
        // IDs should not contain problematic characters
        await Assert.That(id1!).DoesNotContain("<");
        await Assert.That(id1).DoesNotContain(">");
        await Assert.That(id1).DoesNotContain("&");
        await Assert.That(id1).DoesNotContain(" ");
    }

    [Test]
    public async Task GenerateId_MemoryEfficiency_DoesNotLeakMemory()
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
        await Assert.That(memoryIncrease < 10 * 1024 * 1024).IsTrue()
            .Because($"Memory increase: {memoryIncrease / 1024.0 / 1024.0:F2} MB");
    }

    [Test]
    public async Task GenerateId_ThreadSafety_ProducesDeterministicIdsAcrossThreads()
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

        await Task.WhenAll(tasks);

        // Assert
        var expectedCount = threadsCount * idsPerThread;
        await Assert.That(allIds.Count).IsEqualTo(expectedCount);
        await Assert.That(allIds.Distinct()).HasSingleItem();
    }
}
