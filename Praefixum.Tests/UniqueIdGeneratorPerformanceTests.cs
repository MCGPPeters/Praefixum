using System;

namespace Praefixum.Tests;

/// <summary>
/// Performance and scalability tests for the UniqueIdGenerator
/// </summary>
public class UniqueIdGeneratorPerformanceTests
{
    [Test]
    public async Task GenerateId_Performance_CompletesQuickly()
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
        await Assert.That(stopwatch.ElapsedMilliseconds < 1000).IsTrue()
            .Because($"Performance test took {stopwatch.ElapsedMilliseconds}ms for {iterations} iterations");
    }

    [Test]
    [Arguments(100)]
    [Arguments(500)]
    [Arguments(1000)]
    public async Task GenerateId_ScalabilityTest_HandlesLargeVolumes(int count)
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
        await Assert.That(ids.Count).IsEqualTo(count);
        await Assert.That(ids.Distinct()).HasSingleItem();
        
        // Performance should scale linearly (generous bounds)
        var maxExpectedMs = count * 2; // 2ms per operation is very generous
        await Assert.That(stopwatch.ElapsedMilliseconds < maxExpectedMs).IsTrue()
            .Because($"Scalability test for {count} items took {stopwatch.ElapsedMilliseconds}ms (expected < {maxExpectedMs}ms)");
    }

    [Test]
    public async Task GenerateId_ConcurrentPerformance_HandlesParallelLoad()
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
        
        await Assert.That(allIds.Count).IsEqualTo(expectedCount);
        await Assert.That(allIds.Distinct()).HasSingleItem();
        
        // Should complete concurrent operations quickly
        await Assert.That(stopwatch.ElapsedMilliseconds < 5000).IsTrue()
            .Because($"Concurrent performance test took {stopwatch.ElapsedMilliseconds}ms for {expectedCount} operations");
    }

    [Test]
    public async Task GenerateId_MemoryUsage_RemainsConstant()
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
        
        await Assert.That(memoryGrowth < maxAllowedGrowth).IsTrue()
            .Because($"Memory grew by {memoryGrowth / 1024.0 / 1024.0:F2} MB over 1000 operations");
    }

    [Test]
    public async Task GenerateId_GuidFormat_PerformanceComparison()
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
        await Assert.That(guidIds.Count).IsEqualTo(iterations);
        await Assert.That(htmlIds.Count).IsEqualTo(iterations);
        await Assert.That(guidIds.Distinct()).HasSingleItem();
        await Assert.That(htmlIds.Distinct()).HasSingleItem();
        
        // Both should be reasonably fast (under 500ms for 1000 operations)
        await Assert.That(guidTime < 500).IsTrue().Because($"GUID format took {guidTime}ms");
        await Assert.That(htmlTime < 500).IsTrue().Because($"HtmlId format took {htmlTime}ms");
    }

    [Test]
    public async Task GenerateId_PrefixHandling_DoesNotDegradePerformance()
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
        var noPrefixTicks = stopwatch.ElapsedTicks;
        
        // Act - Test with prefix
        stopwatch.Restart();
        for (int i = 0; i < iterations; i++)
        {
            var result = TestHelpers.CreatePrefixedDiv(null, "content");
            _ = TestHelpers.ExtractId(result);
        }
        var prefixTicks = stopwatch.ElapsedTicks;
        stopwatch.Stop();
        // Assert - Prefix handling should not add excessive overhead (allow up to 10x)
        var overheadRatio = (double)prefixTicks / Math.Max(1, noPrefixTicks);
        await Assert.That(overheadRatio < 10.0).IsTrue()
            .Because($"Prefix handling adds {overheadRatio:F2}x overhead ({prefixTicks} ticks vs {noPrefixTicks} ticks)");
    }
}
