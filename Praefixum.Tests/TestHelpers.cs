using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Praefixum;

namespace Praefixum.Tests;

/// <summary>
/// Helper methods for testing the UniqueIdGenerator functionality
/// These methods simulate code that would use the UniqueId attribute
/// </summary>
public static class TestHelpers
{
    // ==========================================
    // ACTUAL METHODS WITH [UniqueId] ATTRIBUTES
    // These methods will trigger the source generator and interceptors
    // ==========================================

    /// <summary>
    /// Creates an HTML div with a unique ID using the default format
    /// This method uses [UniqueId] and will trigger source generation
    /// </summary>
    public static string CreateDiv([UniqueId] string? id = null, string content = "")
    {
        return $"<div id=\"{id}\">{content}</div>";
    }

    /// <summary>
    /// Creates an HTML button with a GUID-format unique ID
    /// This method uses [UniqueId] and will trigger source generation
    /// </summary>
    public static string CreateButtonWithGuid([UniqueId(UniqueIdFormat.Guid)] string? id = null, string text = "Click me")
    {
        return $"<button id=\"{id}\">{text}</button>";
    }

    /// <summary>
    /// Creates an HTML input with an HTML-safe unique ID
    /// This method uses [UniqueId] and will trigger source generation
    /// </summary>
    public static string CreateInputWithHtmlId([UniqueId(UniqueIdFormat.HtmlId)] string? id = null, string type = "text")
    {
        return $"<input id=\"{id}\" type=\"{type}\" />";
    }

    /// <summary>
    /// Creates an HTML span with a prefixed unique ID
    /// This method uses [UniqueId] and will trigger source generation
    /// </summary>
    public static string CreateSpanWithPrefix([UniqueId(prefix: "span-")] string? id = null, string content = "")
    {
        return $"<span id=\"{id}\">{content}</span>";
    }

    /// <summary>
    /// Creates an HTML section with a timestamp-based unique ID
    /// This method uses [UniqueId] and will trigger source generation
    /// </summary>
    public static string CreateSectionWithTimestamp([UniqueId(UniqueIdFormat.Timestamp)] string? id = null, string content = "")
    {
        return $"<section id=\"{id}\">{content}</section>";
    }

    /// <summary>
    /// Creates an HTML article with a short hash unique ID
    /// This method uses [UniqueId] and will trigger source generation
    /// </summary>
    public static string CreateArticleWithShortHash([UniqueId(UniqueIdFormat.ShortHash)] string? id = null, string content = "")
    {
        return $"<article id=\"{id}\">{content}</article>";
    }

    /// <summary>
    /// Creates an HTML form with multiple UniqueId parameters
    /// This method uses multiple [UniqueId] attributes and will trigger source generation
    /// </summary>
    public static string CreateFormWithMultipleIds(
        [UniqueId(prefix: "form-")] string? formId = null,
        [UniqueId(UniqueIdFormat.HtmlId)] string? nameInputId = null,
        [UniqueId(UniqueIdFormat.HtmlId)] string? emailInputId = null,
        [UniqueId(UniqueIdFormat.Guid)] string? submitButtonId = null)
    {
        return $"""
            <form id="{formId}">
                <input id="{nameInputId}" type="text" name="name" />
                <input id="{emailInputId}" type="email" name="email" />
                <button id="{submitButtonId}" type="submit">Submit</button>
            </form>
            """;
    }

    /// <summary>
    /// Method with different return type and UniqueId parameter
    /// Tests that the generator preserves return types correctly
    /// </summary>
    public static string GetIdForElement([UniqueId] string? id = null)
    {
        return id ?? throw new InvalidOperationException("ID should have been generated");
    }

    /// <summary>
    /// Method with int return type and UniqueId parameter
    /// Tests that the generator preserves return types correctly
    /// </summary>
    public static int GetHashFromId([UniqueId(UniqueIdFormat.HtmlId)] string? id = null)
    {
        return id?.GetHashCode() ?? 0;
    }

    /// <summary>
    /// Method with bool return type and UniqueId parameter
    /// Tests that the generator preserves return types correctly
    /// </summary>
    public static bool IsValidId([UniqueId(UniqueIdFormat.Guid)] string? id = null)
    {
        return !string.IsNullOrEmpty(id) && id.Length > 0;
    }

    /// <summary>
    /// Void method with UniqueId parameter
    /// Tests that the generator works with void methods
    /// </summary>
    public static void ProcessElementWithId([UniqueId(prefix: "process-")] string? id = null)
    {
        LastProcessedId = id;
    }

    /// <summary>
    /// Async method with UniqueId parameter
    /// Tests that the generator works with async methods
    /// </summary>
    public static async Task<string> CreateElementAsync([UniqueId(UniqueIdFormat.HtmlId)] string? id = null, string tag = "div")
    {
        await Task.Delay(1); // Simulate async work
        return $"<{tag} id=\"{id}\"></{tag}>";
    }

    /// <summary>
    /// Generic method with UniqueId parameter
    /// Tests that the generator works with generic methods
    /// </summary>
    public static List<T> CreateListWithId<T>(T[] items, [UniqueId] string? id = null)
    {
        // Store the ID for testing
        LastGeneratedId = id;
        return [.. items];
    }

    /// <summary>
    /// Method with multiple parameters where only one has UniqueId
    /// Tests selective parameter handling
    /// </summary>
    public static string CreateComplexElement(
        string tag,
        [UniqueId(UniqueIdFormat.HtmlId)] string? id = null,
        string? className = null,
        string content = "")
    {
        var classAttr = string.IsNullOrEmpty(className) ? "" : $" class=\"{className}\"";
        return $"<{tag} id=\"{id}\"{classAttr}>{content}</{tag}>";
    }

    /// <summary>
    /// Property to store the last processed ID for void method testing
    /// </summary>
    public static string? LastProcessedId { get; private set; }

    // ==========================================
    // LEGACY SIMULATION METHODS (keeping for backward compatibility)
    // ==========================================

    /// <summary>
    /// Creates an HTML element with the specified tag and id
    /// This simulates a method that would have [UniqueId] on the id parameter
    /// </summary>
    public static string CreateHtmlElement(
        string tag,
        string? id = null,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "")
    {
        // If id is null or empty, simulate what the generator would do
        if (string.IsNullOrEmpty(id))
        {
            id = GenerateDeterministicId(
                UniqueIdFormat.HtmlId,
                prefix: null,
                BuildDeterministicKey(filePath, lineNumber, memberName));
        }
        return $"<{tag} id=\"{id}\"></{tag}>";
    }

    /// <summary>
    /// Creates an HTML element with custom attributes
    /// This simulates a method that would have [UniqueId] on the id parameter
    /// </summary>
    public static string CreateHtmlElementWithAttributes(
        string tag,
        string? id = null,
        string? className = null,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "")
    {
        var sb = new StringBuilder();
        sb.Append($"<{tag}");
        
        if (string.IsNullOrEmpty(id))
        {
            id = GenerateDeterministicId(
                UniqueIdFormat.HtmlId,
                prefix: null,
                BuildDeterministicKey(filePath, lineNumber, memberName));
        }
        
        if (id != null)
            sb.Append($" id=\"{id}\"");
            
        if (className != null)
            sb.Append($" class=\"{className}\"");
            
        sb.Append($"></{tag}>");
        return sb.ToString();
    }

    /// <summary>
    /// Creates a button with unique ID and specified text
    /// This simulates a method that would have [UniqueId(UniqueIdFormat.Guid)] on the id parameter
    /// </summary>
    public static string CreateButton(
        string? id = null,
        string text = "Click me",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "")
    {
        if (string.IsNullOrEmpty(id))
        {
            id = GenerateDeterministicId(
                UniqueIdFormat.Guid,
                prefix: null,
                BuildDeterministicKey(filePath, lineNumber, memberName));
        }
        return $"<button id=\"{id}\">{text}</button>";
    }

    /// <summary>
    /// Creates an input with unique ID and specified type
    /// This simulates a method that would have [UniqueId(UniqueIdFormat.HtmlId)] on the id parameter
    /// </summary>
    public static string CreateInput(
        string? id = null,
        string type = "text",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "")
    {
        if (string.IsNullOrEmpty(id))
        {
            id = GenerateDeterministicId(
                UniqueIdFormat.HtmlId,
                prefix: null,
                BuildDeterministicKey(filePath, lineNumber, memberName));
        }
        return $"<input id=\"{id}\" type=\"{type}\" />";
    }

    /// <summary>
    /// Creates a div with prefix-based unique ID
    /// This simulates a method that would have [UniqueId(prefix: "div-")] on the id parameter
    /// </summary>
    public static string CreatePrefixedDiv(
        string? id = null,
        string content = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "")
    {
        if (string.IsNullOrEmpty(id))
        {
            id = GenerateDeterministicId(
                UniqueIdFormat.HtmlId,
                prefix: "div-",
                BuildDeterministicKey(filePath, lineNumber, memberName));
        }
        return $"<div id=\"{id}\">{content}</div>";
    }

    private static string BuildDeterministicKey(string filePath, int lineNumber, string memberName)
    {
        return $"{filePath}:{lineNumber}:{memberName}";
    }

    private static string GenerateDeterministicId(UniqueIdFormat format, string? prefix, string key)
    {
        var baseId = format switch
        {
            UniqueIdFormat.Guid => DeterministicGuid(key),
            UniqueIdFormat.Timestamp => DeterministicTimestamp(key),
            UniqueIdFormat.ShortHash => ShortHash(key),
            UniqueIdFormat.HtmlId => HtmlSafeId(ShortHash(key)),
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };

        return prefix is null ? baseId : $"{prefix}{baseId}";
    }

    private static string ShortHash(string key)
    {
        using var sha256 = SHA256.Create();
        var inputBytes = Encoding.UTF8.GetBytes(key);
        var hashBytes = sha256.ComputeHash(inputBytes);
        return Convert.ToBase64String(hashBytes)
            .Replace("+", "a").Replace("/", "b").Replace("=", string.Empty)
            .Substring(0, 8);
    }

    private static string DeterministicGuid(string key)
    {
        using var sha256 = SHA256.Create();
        var inputBytes = Encoding.UTF8.GetBytes($"{key}:guid");
        var hashBytes = sha256.ComputeHash(inputBytes);
        var guidBytes = new byte[16];
        Array.Copy(hashBytes, guidBytes, 16);
        var guid = new Guid(guidBytes);
        return guid.ToString("N");
    }

    private static string DeterministicTimestamp(string key)
    {
        using var sha256 = SHA256.Create();
        var inputBytes = Encoding.UTF8.GetBytes($"{key}:timestamp");
        var hashBytes = sha256.ComputeHash(inputBytes);
        var timestampLong = Math.Abs(BitConverter.ToInt64(hashBytes, 0));
        var baseTimestamp = 1700000000000L;
        var maxOffset = 100000000000L;
        var deterministicTimestamp = baseTimestamp + (timestampLong % maxOffset);
        return deterministicTimestamp.ToString();
    }

    private static string HtmlSafeId(string id)
    {
        if (id.Length > 0 && !char.IsLetter(id[0]))
            return "x" + id;
        return id;
    }

    private static string GenerateUniqueId(
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "")
    {
        return GenerateDeterministicId(
            UniqueIdFormat.HtmlId,
            prefix: null,
            BuildDeterministicKey(filePath, lineNumber, memberName));
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
    }

    /// <summary>
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
