using Xunit;

namespace Praefixum.Tests;

/// <summary>
/// Performance and scalability tests for the UniqueIdGenerator
/// </summary>
public class UniqueIdGeneratorPerformanceTests
{
    [Fact]
    public void GenerateId_Performance_CompletesQuickly()
    {
        // Arrange
        const int iterations = 1000;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        // Act
        for (int i = 0; i < iterations; i++)
        {
            var result = TestHelpers.CreateHtmlElement("div", null);
            // Force evaluation
            _ = TestHelpers.ExtractId(result);
        }
        
        stopwatch.Stop();
        
        // Assert - Should complete in under 1 second for 1000 iterations
        Assert.True(stopwatch.ElapsedMilliseconds < 1000, 
            $"Performance test took {stopwatch.ElapsedMilliseconds}ms for {iterations} iterations");
    }

    [Theory]
    [InlineData(100)]
    [InlineData(500)]
    [InlineData(1000)]
    public void GenerateId_ScalabilityTest_HandlesLargeVolumes(int count)
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var ids = new List<string>();
        
        // Act
        for (int i = 0; i < count; i++)
        {
            var result = TestHelpers.CreateHtmlElement("div", null);
            var id = TestHelpers.ExtractId(result);
            if (id != null) ids.Add(id);
        }
        
        stopwatch.Stop();
        
        // Assert
        Assert.Equal(count, ids.Count);
        Assert.Equal(ids.Count, ids.Distinct().Count()); // All unique
        
        // Performance should scale linearly (generous bounds)
        var maxExpectedMs = count * 2; // 2ms per operation is very generous
        Assert.True(stopwatch.ElapsedMilliseconds < maxExpectedMs,
            $"Scalability test for {count} items took {stopwatch.ElapsedMilliseconds}ms (expected < {maxExpectedMs}ms)");
    }

    [Fact]
    public async void GenerateId_ConcurrentPerformance_HandlesParallelLoad()
    {
        // Arrange
        const int concurrentTasks = 20;
        const int operationsPerTask = 50;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        // Act
        var tasks = Enumerable.Range(0, concurrentTasks)
            .Select(taskId => Task.Run(() =>
            {
                var taskIds = new List<string>();
                for (int i = 0; i < operationsPerTask; i++)
                {
                    var result = TestHelpers.CreateButton(null, $"Button {taskId}-{i}");
                    var id = TestHelpers.ExtractId(result);
                    if (id != null) taskIds.Add(id);
                }
                return taskIds;
            }))
            .ToArray();
            
        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();
        
        // Assert
        var allIds = results.SelectMany(ids => ids).ToList();
        var expectedCount = concurrentTasks * operationsPerTask;
        
        Assert.Equal(expectedCount, allIds.Count);
        Assert.Equal(allIds.Count, allIds.Distinct().Count()); // All unique
        
        // Should complete concurrent operations quickly
        Assert.True(stopwatch.ElapsedMilliseconds < 5000,
            $"Concurrent performance test took {stopwatch.ElapsedMilliseconds}ms for {expectedCount} operations");
    }

    [Fact]
    public void GenerateId_MemoryUsage_RemainsConstant()
    {
        // Arrange
        var initialMemory = GC.GetTotalMemory(true);
        
        // Act - Generate IDs in batches and measure memory
        var memoryMeasurements = new List<long>();
        
        for (int batch = 0; batch < 10; batch++)
        {
            for (int i = 0; i < 100; i++)
            {
                var result = TestHelpers.CreateInput(null, "text");
                // Don't store the result to avoid memory accumulation
            }
            
            GC.Collect();
            GC.WaitForPendingFinalizers();
            memoryMeasurements.Add(GC.GetTotalMemory(false));
        }
        
        // Assert - Memory should not grow significantly between batches
        var memoryGrowth = memoryMeasurements.Last() - memoryMeasurements.First();
        var maxAllowedGrowth = 5 * 1024 * 1024; // 5MB
        
        Assert.True(memoryGrowth < maxAllowedGrowth,
            $"Memory grew by {memoryGrowth / 1024.0 / 1024.0:F2} MB over 1000 operations");
    }

    [Fact]
    public void GenerateId_GuidFormat_PerformanceComparison()
    {
        // Arrange
        const int iterations = 1000;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        // Act - Test GUID format performance
        var guidIds = new List<string>();
        for (int i = 0; i < iterations; i++)
        {
            var result = TestHelpers.CreateButton(null, "Button");
            var id = TestHelpers.ExtractId(result);
            if (id != null) guidIds.Add(id);
        }
        
        var guidTime = stopwatch.ElapsedMilliseconds;
        stopwatch.Restart();
        
        // Act - Test HtmlId format performance
        var htmlIds = new List<string>();
        for (int i = 0; i < iterations; i++)
        {
            var result = TestHelpers.CreateInput(null, "text");
            var id = TestHelpers.ExtractId(result);
            if (id != null) htmlIds.Add(id);
        }
        
        var htmlTime = stopwatch.ElapsedMilliseconds;
        stopwatch.Stop();
        
        // Assert
        Assert.Equal(iterations, guidIds.Count);
        Assert.Equal(iterations, htmlIds.Count);
        Assert.Equal(guidIds.Count, guidIds.Distinct().Count());
        Assert.Equal(htmlIds.Count, htmlIds.Distinct().Count());
        
        // Both should be reasonably fast (under 500ms for 1000 operations)
        Assert.True(guidTime < 500, $"GUID format took {guidTime}ms");
        Assert.True(htmlTime < 500, $"HtmlId format took {htmlTime}ms");
    }

    [Fact]
    public void GenerateId_PrefixHandling_DoesNotDegradePerformance()
    {
        // Arrange
        const int iterations = 500;
        
        // Act - Test without prefix
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            var result = TestHelpers.CreateHtmlElement("div", null);
            _ = TestHelpers.ExtractId(result);
        }
        var noPrefixTime = stopwatch.ElapsedMilliseconds;
        
        // Act - Test with prefix
        stopwatch.Restart();
        for (int i = 0; i < iterations; i++)
        {
            var result = TestHelpers.CreatePrefixedDiv(null, "content");
            _ = TestHelpers.ExtractId(result);
        }
        var prefixTime = stopwatch.ElapsedMilliseconds;
        stopwatch.Stop();
        
        // Assert - Prefix handling should not add significant overhead
        var overheadRatio = (double)prefixTime / noPrefixTime;
        Assert.True(overheadRatio < 2.0, 
            $"Prefix handling adds {overheadRatio:F2}x overhead ({prefixTime}ms vs {noPrefixTime}ms)");
    }
}
