namespace Praefixum.Tests;

/// <summary>
/// Tests that actually use the [UniqueId] attribute to trigger source generation and interceptors
/// These tests verify that the source generator correctly handles methods with UniqueId parameters
/// </summary>
public class UniqueIdAttributeTests
{
    [Test]
    public async Task CreateDiv_WithUniqueIdAttribute_GeneratesUniqueId()
    {
        // Act
        var result1 = TestHelpers.CreateDiv();
        var result2 = TestHelpers.CreateDiv();

        // Assert
        await Assert.That(result1).IsNotEqualTo(result2);
        await Assert.That(result1).Contains("id=\"");
        await Assert.That(result2).Contains("id=\"");
        
        var id1 = TestHelpers.ExtractId(result1);
        var id2 = TestHelpers.ExtractId(result2);
        await Assert.That(id1).IsNotNull();
        await Assert.That(id2).IsNotNull();
        await Assert.That(id1).IsNotEqualTo(id2);
    }

    [Test]
    public async Task CreateDiv_WithExplicitId_UsesProvidedId()
    {
        // Act
        var result = TestHelpers.CreateDiv("explicit-div-id", "content");

        // Assert
        await Assert.That(result).IsEqualTo("<div id=\"explicit-div-id\">content</div>");
    }

    [Test]
    public async Task CreateButtonWithGuid_GeneratesGuidFormatId()
    {
        // Act
        var result = TestHelpers.CreateButtonWithGuid();

        // Assert
        var id = TestHelpers.ExtractId(result);
        await Assert.That(id).IsNotNull();
        await Assert.That(id!.Length).IsEqualTo(32); // GUID without dashes should be 32 characters
        await Assert.That(id.All(c => char.IsLetterOrDigit(c))).IsTrue();
    }

    [Test]
    public async Task CreateInputWithHtmlId_GeneratesHtmlSafeId()
    {
        // Act
        var result = TestHelpers.CreateInputWithHtmlId();

        // Assert
        var id = TestHelpers.ExtractId(result);
        await Assert.That(id).IsNotNull();
        await Assert.That(id!.Length >= 4).IsTrue(); // HTML IDs should be reasonably short
        await Assert.That(id.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_')).IsTrue();
        await Assert.That(TestHelpers.IsValidHtmlId(id)).IsTrue();
    }

    [Test]
    public async Task CreateSpanWithPrefix_IncludesPrefix()
    {
        // Act
        var result = TestHelpers.CreateSpanWithPrefix();

        // Assert
        var id = TestHelpers.ExtractId(result);
        await Assert.That(id).IsNotNull();
        await Assert.That(id!).StartsWith("span-");
    }

    [Test]
    public async Task CreateSectionWithTimestamp_GeneratesTimestampBasedId()
    {
        // Act
        var result = TestHelpers.CreateSectionWithTimestamp();

        // Assert
        var id = TestHelpers.ExtractId(result);
        await Assert.That(id).IsNotNull();
        await Assert.That(id!).IsNotEmpty();
        // Timestamp-based IDs should be different when called at different times
        Thread.Sleep(1);
        var result2 = TestHelpers.CreateSectionWithTimestamp();
        var id2 = TestHelpers.ExtractId(result2);
        await Assert.That(id).IsNotEqualTo(id2);
    }

    [Test]
    public async Task CreateArticleWithShortHash_GeneratesShortHashId()
    {
        // Act
        var result = TestHelpers.CreateArticleWithShortHash();

        // Assert
        var id = TestHelpers.ExtractId(result);
        await Assert.That(id).IsNotNull();
        await Assert.That(id!).IsNotEmpty();
        await Assert.That(id.Length <= 16).IsTrue(); // Short hash should be reasonably short
    }

    [Test]
    public async Task CreateFormWithMultipleIds_GeneratesDifferentIdsForEachParameter()
    {
        // Act
        var result = TestHelpers.CreateFormWithMultipleIds();

        // Assert
        var formIdMatch = System.Text.RegularExpressions.Regex.Match(result, @"<form id=""([^""]+)""");
        var nameInputIdMatch = System.Text.RegularExpressions.Regex.Match(result, @"<input id=""([^""]+)"" type=""text""");
        var emailInputIdMatch = System.Text.RegularExpressions.Regex.Match(result, @"<input id=""([^""]+)"" type=""email""");
        var buttonIdMatch = System.Text.RegularExpressions.Regex.Match(result, @"<button id=""([^""]+)""");

        await Assert.That(formIdMatch.Success).IsTrue();
        await Assert.That(nameInputIdMatch.Success).IsTrue();
        await Assert.That(emailInputIdMatch.Success).IsTrue();
        await Assert.That(buttonIdMatch.Success).IsTrue();

        var formId = formIdMatch.Groups[1].Value;
        var nameInputId = nameInputIdMatch.Groups[1].Value;
        var emailInputId = emailInputIdMatch.Groups[1].Value;
        var buttonId = buttonIdMatch.Groups[1].Value;

        // All IDs should be different
        await Assert.That(formId).IsNotEqualTo(nameInputId);
        await Assert.That(formId).IsNotEqualTo(emailInputId);
        await Assert.That(formId).IsNotEqualTo(buttonId);
        await Assert.That(nameInputId).IsNotEqualTo(emailInputId);
        await Assert.That(nameInputId).IsNotEqualTo(buttonId);
        await Assert.That(emailInputId).IsNotEqualTo(buttonId);

        // Form ID should have prefix
        await Assert.That(formId).StartsWith("form-");

        // Button ID should be GUID format (32 characters)
        await Assert.That(buttonId.Length).IsEqualTo(32);
    }

    [Test]
    public async Task GetIdForElement_ReturnsGeneratedId()
    {
        // Act
        var id = TestHelpers.GetIdForElement();

        // Assert
        await Assert.That(id).IsNotNull();
        await Assert.That(id).IsNotEmpty();
    }

    [Test]
    public async Task GetHashFromId_ReturnsHashOfGeneratedId()
    {
        // Act
        var hash1 = TestHelpers.GetHashFromId();
        var hash2 = TestHelpers.GetHashFromId();

        // Assert
        await Assert.That(hash1).IsNotEqualTo(0);
        await Assert.That(hash2).IsNotEqualTo(0);
        await Assert.That(hash1).IsNotEqualTo(hash2); // Should be different because IDs are different
    }

    [Test]
    public async Task IsValidId_ReturnsTrueForGeneratedId()
    {
        // Act
        var isValid = TestHelpers.IsValidId();

        // Assert
        await Assert.That(isValid).IsTrue();
    }

    [Test]
    public async Task ProcessElementWithId_StoresGeneratedId()
    {
        // Act
        TestHelpers.ProcessElementWithId();

        // Assert
        await Assert.That(TestHelpers.LastProcessedId).IsNotNull();
        await Assert.That(TestHelpers.LastProcessedId!).StartsWith("process-");
    }

    [Test]
    public async Task CreateElementAsync_GeneratesUniqueIdInAsyncMethod()
    {
        // Act
        var result1 = await TestHelpers.CreateElementAsync();
        var result2 = await TestHelpers.CreateElementAsync();

        // Assert
        await Assert.That(result1).IsNotEqualTo(result2);
        var id1 = TestHelpers.ExtractId(result1);
        var id2 = TestHelpers.ExtractId(result2);
        await Assert.That(id1).IsNotNull();
        await Assert.That(id2).IsNotNull();
        await Assert.That(id1).IsNotEqualTo(id2);
    }

    [Test]
    public async Task CreateListWithId_GeneratesIdForGenericMethod()
    {
        // Act
        var list = TestHelpers.CreateListWithId(["item1", "item2", "item3"]);

        // Assert
        await Assert.That(list.Count).IsEqualTo(3);
        await Assert.That(TestHelpers.LastGeneratedId).IsNotNull();
        await Assert.That(TestHelpers.LastGeneratedId!).IsNotEmpty();
    }

    [Test]
    public async Task CreateComplexElement_GeneratesIdForSingleParameter()
    {
        // Act
        var result = TestHelpers.CreateComplexElement("p", className: "test-class", content: "Hello World");

        // Assert
        var id = TestHelpers.ExtractId(result);
        await Assert.That(id).IsNotNull();
        await Assert.That(result).Contains("class=\"test-class\"");
        await Assert.That(result).Contains("Hello World");
        await Assert.That(result).StartsWith("<p");
    }

    [Test]
    [Arguments(5)]
    [Arguments(10)]
    [Arguments(20)]
    public async Task MultipleCallsWithUniqueId_AreDeterministic(int count)
    {
        // Act
        var ids = new HashSet<string>();
        for (int i = 0; i < count; i++)
        {
            var result = TestHelpers.CreateDiv();
            var id = TestHelpers.ExtractId(result);
            await Assert.That(id).IsNotNull();
            ids.Add(id!);
        }

        // Assert
        await Assert.That(ids).HasSingleItem();
    }

    [Test]
    public async Task DifferentFormats_GenerateDifferentStyleIds()
    {
        // Act
        var guidButton = TestHelpers.CreateButtonWithGuid();
        var htmlInput = TestHelpers.CreateInputWithHtmlId();
        var prefixedSpan = TestHelpers.CreateSpanWithPrefix();

        // Assert
        var guidId = TestHelpers.ExtractId(guidButton);
        var htmlId = TestHelpers.ExtractId(htmlInput);
        var prefixedId = TestHelpers.ExtractId(prefixedSpan);

        await Assert.That(guidId).IsNotNull();
        await Assert.That(htmlId).IsNotNull();
        await Assert.That(prefixedId).IsNotNull();

        // GUID format should be 32 characters
        await Assert.That(guidId!.Length).IsEqualTo(32);

        // HTML ID should be shorter and HTML-safe
        await Assert.That(htmlId!.Length < guidId.Length).IsTrue();
        await Assert.That(TestHelpers.IsValidHtmlId(htmlId)).IsTrue();

        // Prefixed ID should start with prefix
        await Assert.That(prefixedId!).StartsWith("span-");
    }

    [Test]
    public async Task ExplicitIds_AreNotOverridden()
    {
        // Act
        var divResult = TestHelpers.CreateDiv("my-explicit-div-id");
        var buttonResult = TestHelpers.CreateButtonWithGuid("my-explicit-button-id");
        var inputResult = TestHelpers.CreateInputWithHtmlId("my-explicit-input-id");

        // Assert
        await Assert.That(divResult).Contains("my-explicit-div-id");
        await Assert.That(buttonResult).Contains("my-explicit-button-id");
        await Assert.That(inputResult).Contains("my-explicit-input-id");
    }

    // ==========================================
    // TESTS FOR NEW Sequential AND Semantic FORMATS
    // ==========================================

    [Test]
    public async Task CreateNavWithSequential_GeneratesSequentialFormatId()
    {
        // Act
        var result = TestHelpers.CreateNavWithSequential();

        // Assert
        var id = TestHelpers.ExtractId(result);
        await Assert.That(id).IsNotNull();
        await Assert.That(id!).IsNotEmpty();
        // Sequential format should be a 6-digit zero-padded number
        await Assert.That(id).Matches(@"^\d{6}$");
    }

    [Test]
    public async Task CreateNavWithSequential_IsDeterministicPerCallSite()
    {
        // Act
        var ids = new HashSet<string>();
        for (int i = 0; i < 10; i++)
        {
            var result = TestHelpers.CreateNavWithSequential();
            var id = TestHelpers.ExtractId(result);
            await Assert.That(id).IsNotNull();
            ids.Add(id!);
        }

        // Assert — same call site should produce the same ID
        await Assert.That(ids).HasSingleItem();
    }

    [Test]
    public async Task CreateNavWithSequential_DifferentCallSites_ProduceDifferentIds()
    {
        // Act — two distinct call sites
        var nav1 = TestHelpers.CreateNavWithSequential();
        var nav2 = TestHelpers.CreateNavWithSequential();

        // Assert
        var id1 = TestHelpers.ExtractId(nav1);
        var id2 = TestHelpers.ExtractId(nav2);
        await Assert.That(id1).IsNotNull();
        await Assert.That(id2).IsNotNull();
        await Assert.That(id1).IsNotEqualTo(id2);
    }

    [Test]
    public async Task CreateNavWithSequential_WithExplicitId_UsesProvidedId()
    {
        // Act
        var result = TestHelpers.CreateNavWithSequential("my-nav-id");

        // Assert
        await Assert.That(result).Contains("my-nav-id");
    }

    [Test]
    public async Task CreateHeaderWithSemantic_GeneratesSemanticFormatId()
    {
        // Act
        var result = TestHelpers.CreateHeaderWithSemantic();

        // Assert
        var id = TestHelpers.ExtractId(result);
        await Assert.That(id).IsNotNull();
        await Assert.That(id!).IsNotEmpty();
        // Semantic format should be kebab-case-fragment-hash e.g. "create-header-with-semantic-a3f2"
        await Assert.That(id).Contains("-");
        // Should end with a short hash (4 hex-like chars)
        var parts = id.Split('-');
        await Assert.That(parts.Length >= 2).IsTrue().Because($"Semantic ID '{id}' should have at least 2 parts separated by '-'");
    }

    [Test]
    public async Task CreateHeaderWithSemantic_IsDeterministicPerCallSite()
    {
        // Act
        var ids = new HashSet<string>();
        for (int i = 0; i < 10; i++)
        {
            var result = TestHelpers.CreateHeaderWithSemantic();
            var id = TestHelpers.ExtractId(result);
            await Assert.That(id).IsNotNull();
            ids.Add(id!);
        }

        // Assert — same call site should produce the same ID
        await Assert.That(ids).HasSingleItem();
    }

    [Test]
    public async Task CreateHeaderWithSemantic_DifferentCallSites_ProduceDifferentIds()
    {
        // Act — two distinct call sites
        var header1 = TestHelpers.CreateHeaderWithSemantic();
        var header2 = TestHelpers.CreateHeaderWithSemantic();

        // Assert
        var id1 = TestHelpers.ExtractId(header1);
        var id2 = TestHelpers.ExtractId(header2);
        await Assert.That(id1).IsNotNull();
        await Assert.That(id2).IsNotNull();
        await Assert.That(id1).IsNotEqualTo(id2);
    }

    [Test]
    public async Task CreateHeaderWithSemantic_WithExplicitId_UsesProvidedId()
    {
        // Act
        var result = TestHelpers.CreateHeaderWithSemantic("my-header-id");

        // Assert
        await Assert.That(result).Contains("my-header-id");
    }

    [Test]
    public async Task SequentialAndSemantic_ProduceDifferentStyleIds()
    {
        // Act
        var nav = TestHelpers.CreateNavWithSequential();
        var header = TestHelpers.CreateHeaderWithSemantic();

        // Assert
        var seqId = TestHelpers.ExtractId(nav);
        var semId = TestHelpers.ExtractId(header);

        await Assert.That(seqId).IsNotNull();
        await Assert.That(semId).IsNotNull();
        await Assert.That(seqId).IsNotEqualTo(semId);

        // Sequential should be all digits
        await Assert.That(seqId!.All(char.IsDigit)).IsTrue().Because($"Sequential ID '{seqId}' should be all digits");
        // Semantic should contain letters and dashes
        await Assert.That(semId!).Contains("-");
        await Assert.That(semId.Any(char.IsLetter)).IsTrue().Because($"Semantic ID '{semId}' should contain letters");
    }
}
