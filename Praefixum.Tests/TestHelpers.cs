using System.Text;
using Praefixum;

namespace Praefixum.Tests;

/// <summary>
/// Helper methods for testing the UniqueIdGenerator functionality
/// These methods simulate code that would use the UniqueId attribute
/// </summary>
public static class TestHelpers
{    /// <summary>
    /// Creates an HTML element with the specified tag and id
    /// This simulates a method that would have [UniqueId] on the id parameter
    /// </summary>
    public static string CreateHtmlElement(string tag, string? id = null)
    {
        // If id is null or empty, simulate what the generator would do
        if (string.IsNullOrEmpty(id))
        {
            id = GenerateUniqueId();
        }
        return $"<{tag} id=\"{id}\"></{tag}>";
    }    /// <summary>
    /// Creates an HTML element with custom attributes
    /// This simulates a method that would have [UniqueId] on the id parameter
    /// </summary>
    public static string CreateHtmlElementWithAttributes(string tag, string? id = null, string? className = null)
    {
        var sb = new StringBuilder();
        sb.Append($"<{tag}");
        
        if (string.IsNullOrEmpty(id))
        {
            id = GenerateUniqueId();
        }
        
        if (id != null)
            sb.Append($" id=\"{id}\"");
            
        if (className != null)
            sb.Append($" class=\"{className}\"");
            
        sb.Append($"></{tag}>");
        return sb.ToString();
    }    /// <summary>
    /// Creates a button with unique ID and specified text
    /// This simulates a method that would have [UniqueId(UniqueIdFormat.Guid)] on the id parameter
    /// </summary>
    public static string CreateButton(string? id = null, string text = "Click me")
    {
        if (string.IsNullOrEmpty(id))
        {
            id = GenerateGuidId();
        }
        return $"<button id=\"{id}\">{text}</button>";
    }

    /// <summary>
    /// Creates an input with unique ID and specified type
    /// This simulates a method that would have [UniqueId(UniqueIdFormat.HtmlId)] on the id parameter
    /// </summary>
    public static string CreateInput(string? id = null, string type = "text")
    {
        if (string.IsNullOrEmpty(id))
        {
            id = GenerateHtmlId();
        }
        return $"<input id=\"{id}\" type=\"{type}\" />";
    }

    /// <summary>
    /// Creates a div with prefix-based unique ID
    /// This simulates a method that would have [UniqueId(prefix: "div-")] on the id parameter
    /// </summary>
    public static string CreatePrefixedDiv(string? id = null, string content = "")
    {
        if (string.IsNullOrEmpty(id))
        {
            id = "div-" + GenerateUniqueId();
        }
        return $"<div id=\"{id}\">{content}</div>";
    }

    /// <summary>
    /// Generates a GUID-style unique ID (32 character hex string)
    /// </summary>
    private static string GenerateGuidId()
    {
        return Guid.NewGuid().ToString("N");
    }

    /// <summary>
    /// Generates an HTML-safe ID (6-12 characters, alphanumeric)
    /// </summary>
    private static string GenerateHtmlId()
    {
        var random = new Random();
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        var length = random.Next(6, 13); // 6-12 characters
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    /// <summary>
    /// Generates a basic unique ID
    /// </summary>
    private static string GenerateUniqueId()
    {
        return GenerateHtmlId();
    }

    /// <summary>
    /// Creates multiple elements and returns their IDs for uniqueness testing
    /// </summary>
    public static List<string> CreateMultipleElements(int count)
    {
        var ids = new List<string>();
        for (int i = 0; i < count; i++)
        {
            var element = CreateHtmlElement("div", null);
            var idMatch = System.Text.RegularExpressions.Regex.Match(element, @"id=""([^""]+)""");
            if (idMatch.Success)
            {
                ids.Add(idMatch.Groups[1].Value);
            }
        }
        return ids;
    }

    /// <summary>
    /// Extracts ID from HTML element string
    /// </summary>
    public static string? ExtractId(string htmlElement)
    {
        var match = System.Text.RegularExpressions.Regex.Match(htmlElement, @"id=""([^""]+)""");
        return match.Success ? match.Groups[1].Value : null;
    }

    /// <summary>
    /// Validates that an ID follows HTML5 ID rules
    /// </summary>
    public static bool IsValidHtmlId(string id)
    {
        if (string.IsNullOrEmpty(id))
            return false;
            
        // HTML5 IDs must not contain spaces and should be unique
        return !id.Contains(' ') && id.Length > 0;
    }    /// <summary>
    /// Creates a form with multiple inputs having unique IDs
    /// </summary>
    public static string CreateForm()
    {
        var nameInput = CreateInput(null, "text");
        var emailInput = CreateInput(null, "email");
        var submitButton = CreateButton(null, "Submit");
        
        return $"<form>{nameInput}{emailInput}{submitButton}</form>";
    }

    // ==========================================
    // METHODS FOR TESTING RETURN TYPE PRESERVATION
    // These methods have [UniqueId] parameters and different return types
    // to test that the source generator preserves return types correctly
    // ==========================================

    /// <summary>
    /// Method with string return type and UniqueId parameter
    /// Tests that the generator preserves string return type
    /// </summary>
    public static string GetStringWithUniqueId([UniqueId] string? id = null)
    {
        return id ?? GenerateUniqueId();
    }

    /// <summary>
    /// Method with int return type and UniqueId parameter
    /// Tests that the generator preserves int return type
    /// </summary>
    public static int GetIntWithUniqueId([UniqueId] string? id = null)
    {
        if (int.TryParse(id, out var result))
            return result;
        return id?.GetHashCode() ?? new Random().Next(1000, 9999);
    }

    /// <summary>
    /// Method with bool return type and UniqueId parameter
    /// Tests that the generator preserves bool return type
    /// </summary>
    public static bool GetBoolWithUniqueId([UniqueId] string? id = null)
    {
        if (bool.TryParse(id, out var result))
            return result;
        return (id?.GetHashCode() ?? 0) % 2 == 0;
    }

    /// <summary>
    /// Method with void return type and UniqueId parameter
    /// Tests that the generator preserves void return type
    /// </summary>
    public static void DoSomethingWithUniqueId([UniqueId] string? id = null)
    {
        // Store the ID for later verification
        LastGeneratedId = id ?? GenerateUniqueId();
    }

    /// <summary>
    /// Method with double return type and UniqueId parameter
    /// Tests that the generator preserves double return type
    /// </summary>
    public static double GetDoubleWithUniqueId([UniqueId] string? id = null)
    {
        if (double.TryParse(id, out var result))
            return result;
        return (id?.GetHashCode() ?? 0) * 0.001;
    }

    /// <summary>
    /// Method with nullable string return type and UniqueId parameter
    /// Tests that the generator preserves nullable return types
    /// </summary>
    public static string? GetNullableStringWithUniqueId([UniqueId] string? id = null)
    {
        return string.IsNullOrEmpty(id) ? null : id;
    }

    /// <summary>
    /// Method with custom class return type and UniqueId parameter
    /// Tests that the generator preserves custom return types
    /// </summary>
    public static TestResult GetCustomTypeWithUniqueId([UniqueId] string? id = null)
    {
        return new TestResult { Value = id ?? GenerateUniqueId() };
    }

    /// <summary>
    /// Method with generic List return type and UniqueId parameter
    /// Tests that the generator preserves generic return types
    /// </summary>
    public static List<string> GetListWithUniqueId([UniqueId] string? id = null)
    {
        return new List<string> { id ?? GenerateUniqueId() };
    }

    /// <summary>
    /// Method with complex generic return type and UniqueId parameter
    /// Tests that the generator preserves complex generic return types
    /// </summary>
    public static Dictionary<string, int> GetDictionaryWithUniqueId([UniqueId] string? id = null)
    {
        var key = id ?? GenerateUniqueId();
        return new Dictionary<string, int> { { key, key.GetHashCode() } };
    }

    /// <summary>
    /// Method with async Task return type and UniqueId parameter
    /// Tests that the generator preserves async return types
    /// </summary>
    public static async Task<string> GetStringAsyncWithUniqueId([UniqueId] string? id = null)
    {
        await Task.Delay(1); // Simulate async work
        return id ?? GenerateUniqueId();
    }

    /// <summary>
    /// Method with ValueTask return type and UniqueId parameter
    /// Tests that the generator preserves ValueTask return types
    /// </summary>
    public static async ValueTask<int> GetIntValueTaskWithUniqueId([UniqueId] string? id = null)
    {
        await Task.Delay(1); // Simulate async work
        if (int.TryParse(id, out var result))
            return result;
        return id?.GetHashCode() ?? new Random().Next(1000, 9999);
    }

    /// <summary>
    /// Method with nullable reference return type and UniqueId parameter
    /// Tests that the generator preserves nullable reference return types
    /// </summary>
    public static TestResult? GetNullableCustomTypeWithUniqueId([UniqueId] string? id = null)
    {
        return string.IsNullOrEmpty(id) ? null : new TestResult { Value = id };
    }

    // ==========================================
    // HELPER PROPERTIES AND CLASSES FOR TESTING
    // ==========================================

    /// <summary>
    /// Property to store the last generated ID for void method testing
    /// </summary>
    public static string? LastGeneratedId { get; private set; }

    /// <summary>
    /// Test result class for testing custom return types
    /// </summary>
    public class TestResult
    {
        public string Value { get; set; } = "";
        
        public override bool Equals(object? obj)
        {
            return obj is TestResult other && Value == other.Value;
        }
        
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
