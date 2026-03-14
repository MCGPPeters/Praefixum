using System.Collections.Concurrent;

namespace Praefixum.Tests;

/// <summary>
/// Tests for different UniqueId formats and their characteristics
/// </summary>
public class UniqueIdFormatTests
{
    [Test]
    public async Task GuidFormat_GeneratesValidGuid()
    {
        // Arrange & Act
        var result = TestHelpers.CreateButton(null, "Test Button");
        var id = TestHelpers.ExtractId(result);
          // Assert
        await Assert.That(id).IsNotNull();
        await Assert.That(id!.Length).IsEqualTo(32); // GUID without dashes
        await Assert.That(id.All(c => char.IsLetterOrDigit(c))).IsTrue();
        await Assert.That(id.Any(c => char.IsLetter(c))).IsTrue(); // Should contain letters
        await Assert.That(id.Any(c => char.IsDigit(c))).IsTrue(); // Should contain digits
    }

    [Test]
    public async Task HtmlIdFormat_GeneratesShortReadableId()
    {
        // Arrange & Act
        var result = TestHelpers.CreateInput(null, "email");
        var id = TestHelpers.ExtractId(result);
        
        // Assert
        await Assert.That(id).IsNotNull();
        await Assert.That(id!.Length >= 6 && id.Length <= 12).IsTrue();
        await Assert.That(id.All(c => char.IsLetterOrDigit(c))).IsTrue();
        await Assert.That(TestHelpers.IsValidHtmlId(id)).IsTrue();
    }

    [Test]
    [Arguments(10)]
    [Arguments(50)]
    [Arguments(100)]
    public async Task MultipleGuidGeneration_IsDeterministicPerCallSite(int count)
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
        await Assert.That(ids.Count).IsEqualTo(count);
        await Assert.That(ids.Distinct()).HasSingleItem();
    }

    [Test]
    [Arguments(10)]
    [Arguments(50)]
    [Arguments(100)]
    public async Task MultipleHtmlIdGeneration_IsDeterministicPerCallSite(int count)
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
        await Assert.That(ids.Count).IsEqualTo(count);
        await Assert.That(ids.Distinct()).HasSingleItem();
    }

    [Test]
    public async Task PrefixedIds_ContainSpecifiedPrefix()
    {
        // Arrange & Act
        var result = TestHelpers.CreatePrefixedDiv(null, "Content");
        var id = TestHelpers.ExtractId(result);
        
        // Assert
        await Assert.That(id).IsNotNull();
        await Assert.That(id!).StartsWith("div-");
        await Assert.That(id.Length > 4).IsTrue(); // Should have content after prefix
    }

    [Test]
    public async Task MultiplePrefixedIds_AreDeterministicPerCallSite()
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
        await Assert.That(ids.Count).IsEqualTo(10);
        await Assert.That(ids.All(id => id.StartsWith("div-"))).IsTrue();
        await Assert.That(ids.Distinct()).HasSingleItem();
    }

    [Test]
    public async Task FormElements_GenerateUniqueIdsWithCorrectPrefixes()
    {
        // Arrange & Act
        var form = TestHelpers.CreateForm();
        var idMatches = System.Text.RegularExpressions.Regex.Matches(form, @"id=""([^""]+)""");
        var ids = idMatches.Cast<System.Text.RegularExpressions.Match>()
            .Select(m => m.Groups[1].Value)
            .ToList();
        
        // Assert
        await Assert.That(ids.Count >= 3).IsTrue();
        await Assert.That(ids.Count).IsEqualTo(ids.Distinct().Count()); // All should be unique
        await Assert.That(ids.All(id => TestHelpers.IsValidHtmlId(id))).IsTrue();
    }

    [Test]
    public async Task ConcurrentIdGeneration_IsDeterministicPerCallSite()
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
        await Assert.That(ids.Count).IsEqualTo(100);
        await Assert.That(ids.Distinct()).HasSingleItem();
    }

    [Test]
    public async Task CrossFormat_IdGeneration_ProducesStableIds()
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
        await Assert.That(guidIds.Count).IsEqualTo(20);
        await Assert.That(htmlIds.Count).IsEqualTo(20);
        await Assert.That(prefixedIds.Count).IsEqualTo(20);
        
        await Assert.That(guidIds.Distinct()).HasSingleItem();
        await Assert.That(htmlIds.Distinct()).HasSingleItem();
        await Assert.That(prefixedIds.Distinct()).HasSingleItem();

        var allIds = guidIds.Concat(htmlIds).Concat(prefixedIds).ToList();
        await Assert.That(allIds.Distinct().Count()).IsEqualTo(3);
    }

    [Test]
    public async Task IdFormat_Validation_Characteristics()
    {
        // Test GUID format characteristics
        var guidResult = TestHelpers.CreateButton(null, "Test");
        var guidId = TestHelpers.ExtractId(guidResult);
        
        await Assert.That(guidId).IsNotNull();
        await Assert.That(guidId!.Length).IsEqualTo(32); // GUID without dashes
        await Assert.That(guidId.All(c => char.IsLetterOrDigit(c))).IsTrue();
        await Assert.That(guidId.Any(c => char.IsLetter(c)) && guidId.Any(c => char.IsDigit(c))).IsTrue();
        
        // Test HTML ID format characteristics
        var htmlResult = TestHelpers.CreateInput(null, "text");
        var htmlId = TestHelpers.ExtractId(htmlResult);
        
        await Assert.That(htmlId).IsNotNull();
        await Assert.That(htmlId!.Length >= 6 && htmlId.Length <= 12).IsTrue();
        await Assert.That(htmlId.All(c => char.IsLetterOrDigit(c))).IsTrue();
        await Assert.That(TestHelpers.IsValidHtmlId(htmlId)).IsTrue();
        
        // Test prefixed ID format characteristics
        var prefixedResult = TestHelpers.CreatePrefixedDiv(null, "Content");
        var prefixedId = TestHelpers.ExtractId(prefixedResult);
        
        await Assert.That(prefixedId).IsNotNull();
        await Assert.That(prefixedId!).StartsWith("div-");
        await Assert.That(prefixedId.Length > 4).IsTrue();
        await Assert.That(TestHelpers.IsValidHtmlId(prefixedId)).IsTrue();
    }

    [Test]
    [Arguments(1000)]
    [Arguments(5000)]
    public async Task HighVolume_IdGeneration_IsDeterministic(int count)
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
        await Assert.That(duplicateCount).IsEqualTo(count - 1);
        await Assert.That(ids).HasSingleItem();
    }

    [Test]
    public async Task NestedHtml_WithMultipleIds_AllUnique()
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
        await Assert.That(ids.Count).IsEqualTo(4);
        await Assert.That(ids.Count).IsEqualTo(ids.Distinct().Count());
        foreach (var id in ids)
        {
            await Assert.That(TestHelpers.IsValidHtmlId(id)).IsTrue();
        }
        
        // Verify format-specific requirements
        var prefixedIds = ids.Where(id => id.StartsWith("div-")).ToList();
        await Assert.That(prefixedIds).HasSingleItem(); // Only one should have div- prefix
    }

    [Test]
    public async Task EmptyString_NullString_HandledCorrectly()
    {
        // Test handling of empty strings and null values
        var emptyResult = TestHelpers.CreateHtmlElement("span", "");
        var nullResult = TestHelpers.CreateHtmlElement("span", null);
        var whitespaceResult = TestHelpers.CreateHtmlElement("span", "   ");
        
        var emptyId = TestHelpers.ExtractId(emptyResult);
        var nullId = TestHelpers.ExtractId(nullResult);
        var whitespaceId = TestHelpers.ExtractId(whitespaceResult);
        
        // All should generate IDs since empty/null/whitespace should trigger generation
        await Assert.That(emptyId).IsNotNull();
        await Assert.That(nullId).IsNotNull();
        await Assert.That(whitespaceId).IsNotNull();
        
        await Assert.That(emptyId!.Length > 0).IsTrue();
        await Assert.That(nullId!.Length > 0).IsTrue();
        await Assert.That(whitespaceId!.Length > 0).IsTrue();
        
        await Assert.That(emptyId).IsNotEqualTo(nullId);
        await Assert.That(nullId).IsNotEqualTo(whitespaceId);
        await Assert.That(emptyId).IsNotEqualTo(whitespaceId);
    }

    [Test]
    [Arguments("custom-id")]
    [Arguments("very-long-custom-identifier-name")]
    [Arguments("id123")]
    [Arguments("_underscore")]
    [Arguments("CamelCase")]
    public async Task ExplicitIds_ArePreserved(string explicitId)
    {
        // Act
        var result = TestHelpers.CreateHtmlElement("div", explicitId);
        var extractedId = TestHelpers.ExtractId(result);
        
        // Assert
        await Assert.That(extractedId).IsEqualTo(explicitId);
    }

    [Test]
    public async Task IdFormat_DoesNotCollideWithCommonHtmlIds()
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
        await Assert.That(collisions).IsEmpty();
    }

    [Test]
    public async Task SpecialCharacters_InPrefix_HandledCorrectly()
    {
        // Note: This tests the helper method behavior, real generator would need to handle this
        var specialPrefixResult = TestHelpers.CreatePrefixedDiv(null, "Content with special chars: @#$%");
        var id = TestHelpers.ExtractId(specialPrefixResult);
        
        await Assert.That(id).IsNotNull();
        await Assert.That(id!).StartsWith("div-");
        await Assert.That(TestHelpers.IsValidHtmlId(id)).IsTrue();
    }

    [Test]
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
        await Assert.That(idList.Count).IsEqualTo(threadCount * idsPerThread);
        await Assert.That(idList.Distinct()).HasSingleItem();
    }

    // ==========================================
    // TESTS USING ACTUAL [UniqueId] ATTRIBUTES WITH DIFFERENT FORMATS
    // These tests verify the source generator handles different formats correctly
    // ==========================================

    [Test]
    public async Task ActualGuidFormat_GeneratesValidGuidIds()
    {
        // Act - using actual [UniqueId(UniqueIdFormat.Guid)]
        var button1 = TestHelpers.CreateButtonWithGuid();
        var button2 = TestHelpers.CreateButtonWithGuid();

        // Assert
        var id1 = TestHelpers.ExtractId(button1);
        var id2 = TestHelpers.ExtractId(button2);

        await Assert.That(id1).IsNotNull();
        await Assert.That(id2).IsNotNull();
        await Assert.That(id1).IsNotEqualTo(id2);
        await Assert.That(id1!.Length).IsEqualTo(32); // GUID without dashes
        await Assert.That(id2!.Length).IsEqualTo(32);
        await Assert.That(id1.All(c => char.IsLetterOrDigit(c))).IsTrue();
        await Assert.That(id2.All(c => char.IsLetterOrDigit(c))).IsTrue();
    }

    [Test]
    public async Task ActualHtmlIdFormat_GeneratesValidHtmlIds()
    {
        // Act - using actual [UniqueId(UniqueIdFormat.HtmlId)]
        var input1 = TestHelpers.CreateInputWithHtmlId();
        var input2 = TestHelpers.CreateInputWithHtmlId();

        // Assert
        var id1 = TestHelpers.ExtractId(input1);
        var id2 = TestHelpers.ExtractId(input2);

        await Assert.That(id1).IsNotNull();
        await Assert.That(id2).IsNotNull();
        await Assert.That(id1).IsNotEqualTo(id2);
        await Assert.That(id1!.Length >= 4).IsTrue(); // HTML IDs should be reasonable length
        await Assert.That(id2!.Length >= 4).IsTrue();
        await Assert.That(TestHelpers.IsValidHtmlId(id1)).IsTrue();
        await Assert.That(TestHelpers.IsValidHtmlId(id2)).IsTrue();
    }

    [Test]
    public async Task ActualPrefixFormat_GeneratesValidPrefixedIds()
    {
        // Act - using actual [UniqueId(prefix: "span-")]
        var span1 = TestHelpers.CreateSpanWithPrefix();
        var span2 = TestHelpers.CreateSpanWithPrefix();

        // Assert
        var id1 = TestHelpers.ExtractId(span1);
        var id2 = TestHelpers.ExtractId(span2);

        await Assert.That(id1).IsNotNull();
        await Assert.That(id2).IsNotNull();
        await Assert.That(id1).IsNotEqualTo(id2);
        await Assert.That(id1!).StartsWith("span-");
        await Assert.That(id2!).StartsWith("span-");
        await Assert.That(id1.Length > 5).IsTrue(); // Should have content after prefix
        await Assert.That(id2.Length > 5).IsTrue();
    }

    [Test]
    public async Task ActualTimestampFormat_GeneratesTimestampBasedIds()
    {
        // Act - using actual [UniqueId(UniqueIdFormat.Timestamp)]
        var section1 = TestHelpers.CreateSectionWithTimestamp();
        var section2 = TestHelpers.CreateSectionWithTimestamp();

        // Assert
        var id1 = TestHelpers.ExtractId(section1);
        var id2 = TestHelpers.ExtractId(section2);

        await Assert.That(id1).IsNotNull();
        await Assert.That(id2).IsNotNull();
        await Assert.That(id1).IsNotEqualTo(id2);
    }

    [Test]
    public async Task ActualShortHashFormat_GeneratesShortHashIds()
    {
        // Act - using actual [UniqueId(UniqueIdFormat.ShortHash)]
        var article1 = TestHelpers.CreateArticleWithShortHash();
        var article2 = TestHelpers.CreateArticleWithShortHash();

        // Assert
        var id1 = TestHelpers.ExtractId(article1);
        var id2 = TestHelpers.ExtractId(article2);

        await Assert.That(id1).IsNotNull();
        await Assert.That(id2).IsNotNull();
        await Assert.That(id1).IsNotEqualTo(id2);
        await Assert.That(id1!.Length <= 16).IsTrue(); // Short hash should be reasonably short
        await Assert.That(id2!.Length <= 16).IsTrue();
    }

    [Test]
    [Arguments(25)]
    [Arguments(50)]
    public async Task ActualMultipleFormats_ProduceStableIds(int count)
    {
        // Act - using all different actual [UniqueId] formats
        var allIds = new List<string>();

        for (int i = 0; i < count; i++)
        {
            var guidButton = TestHelpers.CreateButtonWithGuid();
            var htmlInput = TestHelpers.CreateInputWithHtmlId();
            var prefixedSpan = TestHelpers.CreateSpanWithPrefix();
            var timestampSection = TestHelpers.CreateSectionWithTimestamp();
            var shortHashArticle = TestHelpers.CreateArticleWithShortHash();

            var guidId = TestHelpers.ExtractId(guidButton);
            var htmlId = TestHelpers.ExtractId(htmlInput);
            var prefixedId = TestHelpers.ExtractId(prefixedSpan);
            var timestampId = TestHelpers.ExtractId(timestampSection);
            var shortHashId = TestHelpers.ExtractId(shortHashArticle);

            // Only add non-null IDs to avoid skewing the count
            if (guidId != null) allIds.Add(guidId);
            if (htmlId != null) allIds.Add(htmlId);
            if (prefixedId != null) allIds.Add(prefixedId);
            if (timestampId != null) allIds.Add(timestampId);
            if (shortHashId != null) allIds.Add(shortHashId);
        }

        // Assert - We should have gotten IDs from all method calls
        await Assert.That(allIds.Count).IsEqualTo(count * 5);
        await Assert.That(allIds.Distinct().Count()).IsEqualTo(5);
    }

    [Test]
    public async Task ActualFormWithMultipleFormats_GeneratesCorrectFormatIds()
    {
        // Act - using method with multiple [UniqueId] parameters with different formats
        var form = TestHelpers.CreateFormWithMultipleIds();

        // Assert
        var formIdMatch = System.Text.RegularExpressions.Regex.Match(form, @"<form id=""([^""]+)""");
        var nameInputIdMatch = System.Text.RegularExpressions.Regex.Match(form, @"<input id=""([^""]+)"" type=""text""");
        var emailInputIdMatch = System.Text.RegularExpressions.Regex.Match(form, @"<input id=""([^""]+)"" type=""email""");
        var buttonIdMatch = System.Text.RegularExpressions.Regex.Match(form, @"<button id=""([^""]+)""");

        await Assert.That(formIdMatch.Success).IsTrue();
        await Assert.That(nameInputIdMatch.Success).IsTrue();
        await Assert.That(emailInputIdMatch.Success).IsTrue();
        await Assert.That(buttonIdMatch.Success).IsTrue();

        var formId = formIdMatch.Groups[1].Value;
        var nameInputId = nameInputIdMatch.Groups[1].Value;
        var emailInputId = emailInputIdMatch.Groups[1].Value;
        var buttonId = buttonIdMatch.Groups[1].Value;

        // Form ID should have prefix (prefix format)
        await Assert.That(formId).StartsWith("form-");

        // Name and email inputs should be HTML ID format (shorter)
        await Assert.That(nameInputId.Length <= 20).IsTrue();
        await Assert.That(emailInputId.Length <= 20).IsTrue();
        await Assert.That(TestHelpers.IsValidHtmlId(nameInputId)).IsTrue();
        await Assert.That(TestHelpers.IsValidHtmlId(emailInputId)).IsTrue();

        // Button should be GUID format (32 characters)
        await Assert.That(buttonId.Length).IsEqualTo(32);
        await Assert.That(buttonId.All(c => char.IsLetterOrDigit(c))).IsTrue();

        // All IDs should be unique
        var allIds = new[] { formId, nameInputId, emailInputId, buttonId };
        await Assert.That(allIds.Distinct().Count()).IsEqualTo(4);
    }

    [Test]
    public async Task ActualAsyncMethod_GeneratesDeterministicIds()
    {
        // Act - using actual async method with [UniqueId]
        var element1 = await TestHelpers.CreateElementAsync();
        var element2 = await TestHelpers.CreateElementAsync();

        // Assert
        var id1 = TestHelpers.ExtractId(element1);
        var id2 = TestHelpers.ExtractId(element2);

        await Assert.That(id1).IsNotNull();
        await Assert.That(id2).IsNotNull();
        await Assert.That(id1).IsNotEqualTo(id2);
        await Assert.That(TestHelpers.IsValidHtmlId(id1!)).IsTrue();
        await Assert.That(TestHelpers.IsValidHtmlId(id2!)).IsTrue();
    }

    [Test]
    public async Task Diagnostic_CheckGeneratedHtml()
    {
        // Check what HTML is being generated by each method
        var guidButton = TestHelpers.CreateButtonWithGuid();
        var htmlInput = TestHelpers.CreateInputWithHtmlId();
        var prefixedSpan = TestHelpers.CreateSpanWithPrefix();
        var timestampSection = TestHelpers.CreateSectionWithTimestamp();
        var shortHashArticle = TestHelpers.CreateArticleWithShortHash();

        // Check if we can extract IDs
        var guidId = TestHelpers.ExtractId(guidButton);
        var htmlId = TestHelpers.ExtractId(htmlInput);
        var prefixedId = TestHelpers.ExtractId(prefixedSpan);
        var timestampId = TestHelpers.ExtractId(timestampSection);
        var shortHashId = TestHelpers.ExtractId(shortHashArticle);

        // Count how many are null
        var nullCount = 0;
        if (guidId == null) nullCount++;
        if (htmlId == null) nullCount++;
        if (prefixedId == null) nullCount++;
        if (timestampId == null) nullCount++;
        if (shortHashId == null) nullCount++;

        // Fail with diagnostic info if any are null
        if (nullCount > 0)
        {
            Assert.Fail( 
                $"Found {nullCount} null IDs out of 5. " +
                $"GUID: '{guidId}' from '{guidButton}', " +
                $"HTML: '{htmlId}' from '{htmlInput}', " +
                $"Prefixed: '{prefixedId}' from '{prefixedSpan}', " +
                $"Timestamp: '{timestampId}' from '{timestampSection}', " +
                $"ShortHash: '{shortHashId}' from '{shortHashArticle}'");
        }

        // If we reached here without Assert.Fail, all methods generated valid HTML with IDs
    }

}
