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

    [Test]
    public async Task InstanceMethod_GeneratesUniqueId()
    {
        // Arrange
        var builder = new HtmlBuilder("section");

        // Act
        var result = builder.CreateElement(content: "Hello");

        // Assert
        var id = TestHelpers.ExtractId(result);
        await Assert.That(id).IsNotNull();
        await Assert.That(id!).IsNotEmpty();
        await Assert.That(result).StartsWith("<section");
        await Assert.That(result).Contains("Hello");
    }

    [Test]
    public async Task InstanceMethod_DifferentCallSites_ProduceDifferentIds()
    {
        // Arrange
        var builder = new HtmlBuilder("div");

        // Act
        var result1 = builder.CreateElement(content: "First");
        var result2 = builder.CreateElement(content: "Second");

        // Assert
        var id1 = TestHelpers.ExtractId(result1);
        var id2 = TestHelpers.ExtractId(result2);
        await Assert.That(id1).IsNotNull();
        await Assert.That(id2).IsNotNull();
        await Assert.That(id1).IsNotEqualTo(id2);
    }

    [Test]
    public async Task InstanceMethod_SameCallSite_ProducesSameId()
    {
        var builder = new HtmlBuilder("div");
        var ids = new HashSet<string>();
        for (int i = 0; i < 10; i++)
        {
            var result = builder.CreateElement(content: $"Item {i}");
            var id = TestHelpers.ExtractId(result);
            await Assert.That(id).IsNotNull();
            ids.Add(id!);
        }

        // Same call site should produce the same deterministic ID
        await Assert.That(ids).HasSingleItem();
    }

    [Test]
    public async Task InstanceMethod_WithExplicitId_UsesProvidedId()
    {
        var builder = new HtmlBuilder("span");
        var result = builder.CreateElement("my-explicit-id", "text");

        await Assert.That(result).Contains("my-explicit-id");
    }

    [Test]
    public async Task InstanceMethod_GuidFormat_Generates32CharId()
    {
        var builder = new HtmlBuilder("div");
        var result = builder.CreateElementWithGuid();

        var id = TestHelpers.ExtractId(result);
        await Assert.That(id).IsNotNull();
        await Assert.That(id!.Length).IsEqualTo(32);
        await Assert.That(id.All(c => char.IsLetterOrDigit(c))).IsTrue();
    }

    [Test]
    public async Task InstanceMethod_WithPrefix_IncludesPrefix()
    {
        var builder = new HtmlBuilder("div");
        var result = builder.CreateElementWithPrefix();

        var id = TestHelpers.ExtractId(result);
        await Assert.That(id).IsNotNull();
        await Assert.That(id!).StartsWith("inst-");
    }

    [Test]
    public async Task InstanceMethod_SequentialFormat_GeneratesNumericId()
    {
        var builder = new HtmlBuilder("div");
        var result = builder.CreateElementWithSequential();

        var id = TestHelpers.ExtractId(result);
        await Assert.That(id).IsNotNull();
        await Assert.That(id!.All(char.IsDigit)).IsTrue().Because($"Sequential ID '{id}' should be all digits");
    }

    [Test]
    public async Task InstanceMethod_SemanticFormat_GeneratesReadableId()
    {
        var builder = new HtmlBuilder("div");
        var result = builder.CreateElementWithSemantic();

        var id = TestHelpers.ExtractId(result);
        await Assert.That(id).IsNotNull();
        await Assert.That(id!).Contains("-");
    }

    [Test]
    public async Task InstanceMethod_NonStringReturnType_WorksCorrectly()
    {
        var builder = new HtmlBuilder("div");
        var hash = builder.GetIdHash();

        // Should have been called with a generated ID, producing a non-zero hash
        await Assert.That(hash).IsNotEqualTo(0);
    }

    [Test]
    public async Task InstanceMethod_VoidReturnType_WorksCorrectly()
    {
        var builder = new HtmlBuilder("div");
        builder.SetId();

        await Assert.That(builder.LastSetId).IsNotNull();
        await Assert.That(builder.LastSetId!).IsNotEmpty();
    }

    [Test]
    public async Task InstanceMethod_AsyncMethod_WorksCorrectly()
    {
        var builder = new HtmlBuilder("div");
        var result = await builder.CreateElementAsync();

        var id = TestHelpers.ExtractId(result);
        await Assert.That(id).IsNotNull();
        await Assert.That(id!).IsNotEmpty();
    }

    [Test]
    public async Task InstanceMethod_DifferentInstances_DifferentCallSites_ProduceDifferentIds()
    {
        // The ID is determined by call site, not by instance
        var builder1 = new HtmlBuilder("div");
        var builder2 = new HtmlBuilder("span");

        var result1 = builder1.CreateElement(content: "from builder1");
        var result2 = builder2.CreateElement(content: "from builder2");

        var id1 = TestHelpers.ExtractId(result1);
        var id2 = TestHelpers.ExtractId(result2);

        // Different call sites (different lines), so IDs should differ
        await Assert.That(id1).IsNotNull();
        await Assert.That(id2).IsNotNull();
        await Assert.That(id1).IsNotEqualTo(id2);
    }
}
