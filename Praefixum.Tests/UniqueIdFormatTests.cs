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
    }

    [Fact]
    public void ConcurrentIdGeneration_ProducesUniqueIds()
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

        var ids = Task.WhenAll(tasks).Result
            .Where(id => id != null)
            .ToList();
        
        // Assert
        Assert.Equal(100, ids.Count);
        Assert.Equal(ids.Count, ids.Distinct().Count()); // All should be unique even when generated concurrently
    }
}
