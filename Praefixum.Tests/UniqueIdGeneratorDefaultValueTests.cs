using Xunit;

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

    [Fact]
    public void BoolDefaultFalse_CompilesAndRunsCorrectly()
    {
        // If the generator emits "False" instead of "false", this won't compile at all.
        var result = TestHelpers.CreateInputText("Name");
        
        Assert.Contains("id=\"", result);
        Assert.Contains("placeholder=\"Name\"", result);
        Assert.DoesNotContain("required", result); // isRequired defaults to false
    }

    [Fact]
    public void BoolDefaultFalse_WithExplicitTrue_Works()
    {
        var result = TestHelpers.CreateInputText("Name", isRequired: true);
        
        Assert.Contains("id=\"", result);
        Assert.Contains("required", result);
    }

    [Fact]
    public void BoolDefaultTrue_CompilesAndRunsCorrectly()
    {
        // If the generator emits "True" instead of "true", this won't compile at all.
        var result = TestHelpers.CreateCheckbox("Remember me");
        
        Assert.Contains("id=\"", result);
        Assert.Contains("checked", result); // isChecked defaults to true
    }

    [Fact]
    public void BoolDefaultTrue_WithExplicitFalse_Works()
    {
        var result = TestHelpers.CreateCheckbox("Remember me", isChecked: false);
        
        Assert.Contains("id=\"", result);
        Assert.DoesNotContain("checked", result);
    }

    // ==========================================
    // char defaults
    // ==========================================

    [Fact]
    public void CharDefault_CompilesAndRunsCorrectly()
    {
        var result = TestHelpers.CreateElementWithSeparator("hello");
        
        Assert.Contains("id=\"", result);
        Assert.Contains("hello-hello", result); // separator defaults to '-'
    }

    [Fact]
    public void CharDefault_WithExplicitValue_Works()
    {
        var result = TestHelpers.CreateElementWithSeparator("hello", separator: '|');
        
        Assert.Contains("hello|hello", result);
    }

    // ==========================================
    // float defaults
    // ==========================================

    [Fact]
    public void FloatDefault_CompilesAndRunsCorrectly()
    {
        var result = TestHelpers.CreateProgressBar();
        
        Assert.Contains("id=\"", result);
        Assert.Contains("value=\"0\"", result); // progress defaults to 0.0f
    }

    [Fact]
    public void FloatDefault_WithExplicitValue_Works()
    {
        var result = TestHelpers.CreateProgressBar(progress: 0.75f);
        
        Assert.Contains("id=\"", result);
        // Float formatting is locale-dependent at runtime; just verify the value is present
        Assert.Contains(0.75f.ToString(), result);
    }

    // ==========================================
    // double defaults
    // ==========================================

    [Fact]
    public void DoubleDefault_CompilesAndRunsCorrectly()
    {
        var result = TestHelpers.CreateMeter();
        
        Assert.Contains("id=\"", result);
        Assert.Contains("value=\"0\"", result); // value defaults to 0.0d
    }

    [Fact]
    public void DoubleDefault_WithExplicitValue_Works()
    {
        var result = TestHelpers.CreateMeter(value: 42.5, max: 200.0);
        
        Assert.Contains("id=\"", result);
        // Double formatting is locale-dependent at runtime; just verify the values are present
        Assert.Contains(42.5d.ToString(), result);
        Assert.Contains(200d.ToString(), result);
    }

    // ==========================================
    // long defaults
    // ==========================================

    [Fact]
    public void LongDefault_CompilesAndRunsCorrectly()
    {
        var result = TestHelpers.CreateDataElement();
        
        Assert.Contains("id=\"", result);
        Assert.Contains("value=\"0\"", result); // dataValue defaults to 0L
    }

    [Fact]
    public void LongDefault_WithExplicitValue_Works()
    {
        var result = TestHelpers.CreateDataElement(dataValue: 9876543210L);
        
        Assert.Contains("id=\"", result);
        Assert.Contains("9876543210", result);
    }

    // ==========================================
    // int defaults (sanity check — should have worked before)
    // ==========================================

    [Fact]
    public void IntDefault_CompilesAndRunsCorrectly()
    {
        var result = TestHelpers.CreateOrderedItem("Item");
        
        Assert.Contains("id=\"", result);
        Assert.Contains("data-order=\"0\"", result); // order defaults to 0
    }

    [Fact]
    public void IntDefault_WithExplicitValue_Works()
    {
        var result = TestHelpers.CreateOrderedItem("Item", order: 5);
        
        Assert.Contains("id=\"", result);
        Assert.Contains("data-order=\"5\"", result);
    }

    // ==========================================
    // Determinism checks — IDs are still deterministic per call site
    // ==========================================

    [Fact]
    public void BoolDefaultMethod_GeneratesDeterministicId()
    {
        // Same call site (same source line) should produce the same deterministic ID
        var results = Enumerable.Range(0, 5).Select(_ => TestHelpers.CreateInputText("Name")).ToList();
        
        Assert.True(results.All(r => r == results[0]),
            "Multiple invocations from the same call site should produce identical results");
    }

    [Fact]
    public void DifferentDefaultTypeMethods_GenerateUniqueIds()
    {
        var input = TestHelpers.CreateInputText("Name");
        var checkbox = TestHelpers.CreateCheckbox("Check");
        var progress = TestHelpers.CreateProgressBar();
        
        var inputId = ExtractId(input);
        var checkboxId = ExtractId(checkbox);
        var progressId = ExtractId(progress);
        
        Assert.NotNull(inputId);
        Assert.NotNull(checkboxId);
        Assert.NotNull(progressId);
        
        // Different call sites → different IDs
        Assert.NotEqual(inputId, checkboxId);
        Assert.NotEqual(inputId, progressId);
        Assert.NotEqual(checkboxId, progressId);
    }

    private static string? ExtractId(string html)
    {
        var match = System.Text.RegularExpressions.Regex.Match(html, @"id=""([^""]+)""");
        return match.Success ? match.Groups[1].Value : null;
    }
}
