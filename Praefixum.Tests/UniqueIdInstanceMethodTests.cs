using Xunit;

namespace Praefixum.Tests;

/// <summary>
/// Tests that the source generator correctly handles instance methods with [UniqueId] parameters.
/// Instance methods require the generator to emit extension-method-style interceptors with a 'this' receiver.
/// </summary>
public class UniqueIdInstanceMethodTests
{
    // ==========================================
    // INSTANCE HELPER CLASS
    // ==========================================

    /// <summary>
    /// A non-static class with instance methods decorated with [UniqueId].
    /// Used to verify that the source generator emits correct interceptors for instance methods.
    /// </summary>
    public class HtmlBuilder
    {
        public string Tag { get; }

        public HtmlBuilder(string tag = "div")
        {
            Tag = tag;
        }

        public string CreateElement([UniqueId(UniqueIdFormat.HtmlId)] string? id = null, string content = "")
        {
            return $"<{Tag} id=\"{id}\">{content}</{Tag}>";
        }

        public string CreateElementWithGuid([UniqueId(UniqueIdFormat.Guid)] string? id = null)
        {
            return $"<{Tag} id=\"{id}\"></{Tag}>";
        }

        public string CreateElementWithPrefix([UniqueId(UniqueIdFormat.HtmlId, prefix: "inst-")] string? id = null)
        {
            return $"<{Tag} id=\"{id}\"></{Tag}>";
        }

        public string CreateElementWithSequential([UniqueId(UniqueIdFormat.Sequential)] string? id = null)
        {
            return $"<{Tag} id=\"{id}\"></{Tag}>";
        }

        public string CreateElementWithSemantic([UniqueId(UniqueIdFormat.Semantic)] string? id = null)
        {
            return $"<{Tag} id=\"{id}\"></{Tag}>";
        }

        public int GetIdHash([UniqueId(UniqueIdFormat.HtmlId)] string? id = null)
        {
            return id?.GetHashCode() ?? 0;
        }

        public void SetId([UniqueId(UniqueIdFormat.HtmlId)] string? id = null)
        {
            LastSetId = id;
        }

        public string? LastSetId { get; private set; }

        public async Task<string> CreateElementAsync([UniqueId(UniqueIdFormat.HtmlId)] string? id = null)
        {
            await Task.Delay(1);
            return $"<{Tag} id=\"{id}\"></{Tag}>";
        }
    }

    // ==========================================
    // TESTS
    // ==========================================

    [Fact]
    public void InstanceMethod_GeneratesUniqueId()
    {
        // Arrange
        var builder = new HtmlBuilder("section");

        // Act
        var result = builder.CreateElement(content: "Hello");

        // Assert
        var id = TestHelpers.ExtractId(result);
        Assert.NotNull(id);
        Assert.NotEmpty(id);
        Assert.StartsWith("<section", result);
        Assert.Contains("Hello", result);
    }

    [Fact]
    public void InstanceMethod_DifferentCallSites_ProduceDifferentIds()
    {
        // Arrange
        var builder = new HtmlBuilder("div");

        // Act
        var result1 = builder.CreateElement(content: "First");
        var result2 = builder.CreateElement(content: "Second");

        // Assert
        var id1 = TestHelpers.ExtractId(result1);
        var id2 = TestHelpers.ExtractId(result2);
        Assert.NotNull(id1);
        Assert.NotNull(id2);
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void InstanceMethod_SameCallSite_ProducesSameId()
    {
        var builder = new HtmlBuilder("div");
        var ids = new HashSet<string>();
        for (int i = 0; i < 10; i++)
        {
            var result = builder.CreateElement(content: $"Item {i}");
            var id = TestHelpers.ExtractId(result);
            Assert.NotNull(id);
            ids.Add(id);
        }

        // Same call site should produce the same deterministic ID
        Assert.Single(ids);
    }

    [Fact]
    public void InstanceMethod_WithExplicitId_UsesProvidedId()
    {
        var builder = new HtmlBuilder("span");
        var result = builder.CreateElement("my-explicit-id", "text");

        Assert.Contains("my-explicit-id", result);
    }

    [Fact]
    public void InstanceMethod_GuidFormat_Generates32CharId()
    {
        var builder = new HtmlBuilder("div");
        var result = builder.CreateElementWithGuid();

        var id = TestHelpers.ExtractId(result);
        Assert.NotNull(id);
        Assert.Equal(32, id.Length);
        Assert.True(id.All(c => char.IsLetterOrDigit(c)));
    }

    [Fact]
    public void InstanceMethod_WithPrefix_IncludesPrefix()
    {
        var builder = new HtmlBuilder("div");
        var result = builder.CreateElementWithPrefix();

        var id = TestHelpers.ExtractId(result);
        Assert.NotNull(id);
        Assert.StartsWith("inst-", id);
    }

    [Fact]
    public void InstanceMethod_SequentialFormat_GeneratesNumericId()
    {
        var builder = new HtmlBuilder("div");
        var result = builder.CreateElementWithSequential();

        var id = TestHelpers.ExtractId(result);
        Assert.NotNull(id);
        Assert.True(id.All(char.IsDigit), $"Sequential ID '{id}' should be all digits");
    }

    [Fact]
    public void InstanceMethod_SemanticFormat_GeneratesReadableId()
    {
        var builder = new HtmlBuilder("div");
        var result = builder.CreateElementWithSemantic();

        var id = TestHelpers.ExtractId(result);
        Assert.NotNull(id);
        Assert.Contains("-", id);
    }

    [Fact]
    public void InstanceMethod_NonStringReturnType_WorksCorrectly()
    {
        var builder = new HtmlBuilder("div");
        var hash = builder.GetIdHash();

        // Should have been called with a generated ID, producing a non-zero hash
        Assert.NotEqual(0, hash);
    }

    [Fact]
    public void InstanceMethod_VoidReturnType_WorksCorrectly()
    {
        var builder = new HtmlBuilder("div");
        builder.SetId();

        Assert.NotNull(builder.LastSetId);
        Assert.NotEmpty(builder.LastSetId);
    }

    [Fact]
    public async Task InstanceMethod_AsyncMethod_WorksCorrectly()
    {
        var builder = new HtmlBuilder("div");
        var result = await builder.CreateElementAsync();

        var id = TestHelpers.ExtractId(result);
        Assert.NotNull(id);
        Assert.NotEmpty(id);
    }

    [Fact]
    public void InstanceMethod_DifferentInstances_DifferentCallSites_ProduceDifferentIds()
    {
        // The ID is determined by call site, not by instance
        var builder1 = new HtmlBuilder("div");
        var builder2 = new HtmlBuilder("span");

        var result1 = builder1.CreateElement(content: "from builder1");
        var result2 = builder2.CreateElement(content: "from builder2");

        var id1 = TestHelpers.ExtractId(result1);
        var id2 = TestHelpers.ExtractId(result2);

        // Different call sites (different lines), so IDs should differ
        Assert.NotNull(id1);
        Assert.NotNull(id2);
        Assert.NotEqual(id1, id2);
    }
}
