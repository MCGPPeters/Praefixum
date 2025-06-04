using System.Text;

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
}
