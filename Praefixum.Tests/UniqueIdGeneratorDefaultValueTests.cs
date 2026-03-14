namespace Praefixum.Tests;

/// <summary>
/// Tests that the source generator correctly emits C# default value literals
/// for non-string parameter types alongside [UniqueId] parameters.
/// See: https://github.com/MCGPPeters/Praefixum/issues/6
/// </summary>
public class UniqueIdGeneratorDefaultValueTests
{
    // ==========================================
    // bool defaults — the primary reported bug
    // ==========================================

    [Test]
    public async Task BoolDefaultFalse_CompilesAndRunsCorrectly()
    {
        // If the generator emits "False" instead of "false", this won't compile at all.
        var result = TestHelpers.CreateInputText("Name");
        
        await Assert.That(result).Contains("id=\"");
        await Assert.That(result).Contains("placeholder=\"Name\"");
        await Assert.That(result).DoesNotContain("required"); // isRequired defaults to false
    }

    [Test]
    public async Task BoolDefaultFalse_WithExplicitTrue_Works()
    {
        var result = TestHelpers.CreateInputText("Name", isRequired: true);
        
        await Assert.That(result).Contains("id=\"");
        await Assert.That(result).Contains("required");
    }

    [Test]
    public async Task BoolDefaultTrue_CompilesAndRunsCorrectly()
    {
        // If the generator emits "True" instead of "true", this won't compile at all.
        var result = TestHelpers.CreateCheckbox("Remember me");
        
        await Assert.That(result).Contains("id=\"");
        await Assert.That(result).Contains("checked"); // isChecked defaults to true
    }

    [Test]
    public async Task BoolDefaultTrue_WithExplicitFalse_Works()
    {
        var result = TestHelpers.CreateCheckbox("Remember me", isChecked: false);
        
        await Assert.That(result).Contains("id=\"");
        await Assert.That(result).DoesNotContain("checked");
    }

    // ==========================================
    // char defaults
    // ==========================================

    [Test]
    public async Task CharDefault_CompilesAndRunsCorrectly()
    {
        var result = TestHelpers.CreateElementWithSeparator("hello");
        
        await Assert.That(result).Contains("id=\"");
        await Assert.That(result).Contains("hello-hello"); // separator defaults to '-'
    }

    [Test]
    public async Task CharDefault_WithExplicitValue_Works()
    {
        var result = TestHelpers.CreateElementWithSeparator("hello", separator: '|');
        
        await Assert.That(result).Contains("hello|hello");
    }

    // ==========================================
    // float defaults
    // ==========================================

    [Test]
    public async Task FloatDefault_CompilesAndRunsCorrectly()
    {
        var result = TestHelpers.CreateProgressBar();
        
        await Assert.That(result).Contains("id=\"");
        await Assert.That(result).Contains("value=\"0\""); // progress defaults to 0.0f
    }

    [Test]
    public async Task FloatDefault_WithExplicitValue_Works()
    {
        var result = TestHelpers.CreateProgressBar(progress: 0.75f);
        
        await Assert.That(result).Contains("id=\"");
        // Float formatting is locale-dependent at runtime; just verify the value is present
        await Assert.That(result).Contains(0.75f.ToString());
    }

    // ==========================================
    // double defaults
    // ==========================================

    [Test]
    public async Task DoubleDefault_CompilesAndRunsCorrectly()
    {
        var result = TestHelpers.CreateMeter();
        
        await Assert.That(result).Contains("id=\"");
        await Assert.That(result).Contains("value=\"0\""); // value defaults to 0.0d
    }

    [Test]
    public async Task DoubleDefault_WithExplicitValue_Works()
    {
        var result = TestHelpers.CreateMeter(value: 42.5, max: 200.0);
        
        await Assert.That(result).Contains("id=\"");
        // Double formatting is locale-dependent at runtime; just verify the values are present
        await Assert.That(result).Contains(42.5d.ToString());
        await Assert.That(result).Contains(200d.ToString());
    }

    // ==========================================
    // long defaults
    // ==========================================

    [Test]
    public async Task LongDefault_CompilesAndRunsCorrectly()
    {
        var result = TestHelpers.CreateDataElement();
        
        await Assert.That(result).Contains("id=\"");
        await Assert.That(result).Contains("value=\"0\""); // dataValue defaults to 0L
    }

    [Test]
    public async Task LongDefault_WithExplicitValue_Works()
    {
        var result = TestHelpers.CreateDataElement(dataValue: 9876543210L);
        
        await Assert.That(result).Contains("id=\"");
        await Assert.That(result).Contains("9876543210");
    }

    // ==========================================
    // int defaults (sanity check — should have worked before)
    // ==========================================

    [Test]
    public async Task IntDefault_CompilesAndRunsCorrectly()
    {
        var result = TestHelpers.CreateOrderedItem("Item");
        
        await Assert.That(result).Contains("id=\"");
        await Assert.That(result).Contains("data-order=\"0\""); // order defaults to 0
    }

    [Test]
    public async Task IntDefault_WithExplicitValue_Works()
    {
        var result = TestHelpers.CreateOrderedItem("Item", order: 5);
        
        await Assert.That(result).Contains("id=\"");
        await Assert.That(result).Contains("data-order=\"5\"");
    }

    // ==========================================
    // Determinism checks — IDs are still deterministic per call site
    // ==========================================

    [Test]
    public async Task BoolDefaultMethod_GeneratesDeterministicId()
    {
        // Same call site (same source line) should produce the same deterministic ID
        var results = Enumerable.Range(0, 5).Select(_ => TestHelpers.CreateInputText("Name")).ToList();
        
        await Assert.That(results.All(r => r == results[0])).IsTrue()
            .Because("Multiple invocations from the same call site should produce identical results");
    }

    [Test]
    public async Task DifferentDefaultTypeMethods_GenerateUniqueIds()
    {
        var input = TestHelpers.CreateInputText("Name");
        var checkbox = TestHelpers.CreateCheckbox("Check");
        var progress = TestHelpers.CreateProgressBar();
        
        var inputId = TestHelpers.ExtractId(input);
        var checkboxId = TestHelpers.ExtractId(checkbox);
        var progressId = TestHelpers.ExtractId(progress);
        
        await Assert.That(inputId).IsNotNull();
        await Assert.That(checkboxId).IsNotNull();
        await Assert.That(progressId).IsNotNull();
        
        // Different call sites -> different IDs
        await Assert.That(inputId).IsNotEqualTo(checkboxId);
        await Assert.That(inputId).IsNotEqualTo(progressId);
        await Assert.That(checkboxId).IsNotEqualTo(progressId);
    }

    // ==========================================
    // ulong defaults
    // ==========================================

    [Test]
    public async Task UlongDefault_CompilesAndRunsCorrectly()
    {
        var result = TestHelpers.CreateUlongElement();
        
        await Assert.That(result).Contains("id=\"");
        await Assert.That(result).Contains("data-counter=\"0\""); // counter defaults to 0UL
    }

    [Test]
    public async Task UlongDefault_WithExplicitValue_Works()
    {
        var result = TestHelpers.CreateUlongElement(counter: 18446744073709551615UL);
        
        await Assert.That(result).Contains("id=\"");
        await Assert.That(result).Contains("18446744073709551615");
    }

    // ==========================================
    // decimal defaults
    // ==========================================

    [Test]
    public async Task DecimalDefault_CompilesAndRunsCorrectly()
    {
        var result = TestHelpers.CreatePriceElement();
        
        await Assert.That(result).Contains("id=\"");
        await Assert.That(result).Contains("data-price=\"0.0\""); // price defaults to 0.0m
    }

    [Test]
    public async Task DecimalDefault_WithExplicitValue_Works()
    {
        var result = TestHelpers.CreatePriceElement(price: 99.99m);
        
        await Assert.That(result).Contains("id=\"");
        await Assert.That(result).Contains("99.99");
    }

    // ==========================================
    // enum defaults
    // ==========================================

    [Test]
    public async Task EnumDefault_CompilesAndRunsCorrectly()
    {
        var result = TestHelpers.CreateAlignedElement("text");
        
        await Assert.That(result).Contains("id=\"");
        await Assert.That(result).Contains("data-comparison=\"Ordinal\""); // defaults to StringComparison.Ordinal
    }

    [Test]
    public async Task EnumDefault_WithExplicitValue_Works()
    {
        var result = TestHelpers.CreateAlignedElement("text", comparison: System.StringComparison.OrdinalIgnoreCase);
        
        await Assert.That(result).Contains("id=\"");
        await Assert.That(result).Contains("OrdinalIgnoreCase");
    }

}
