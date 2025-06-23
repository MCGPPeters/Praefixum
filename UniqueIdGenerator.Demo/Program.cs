using UniqueIdGenerator;

namespace UniqueIdGenerator.Demo;

public partial class Program
{
    public static void Main()
    {
        Console.WriteLine("UniqueIdGenerator Demo");
        Console.WriteLine("=====================");
        Console.WriteLine();

        // Basic usage demo
        Console.WriteLine("Basic Usage:");
        Console.WriteLine("------------");
        var id1 = GenerateId();
        var id2 = GenerateId();
        var id3 = GenerateAnotherUniqueId();
        
        Console.WriteLine($"ID1 (Hex16): {id1}");
        Console.WriteLine($"ID2 (Hex16): {id2}"); // Same as ID1 because it's the same method call
        Console.WriteLine($"ID3 (Hex16): {id3}"); // Different method, different ID
        
        // Demonstrate ID generation in a different class
        var helper = new Helper();
        var id4 = helper.GetUniqueId();
        
        Console.WriteLine($"ID4 (Hex16): {id4}"); // Different class/method, different ID
        Console.WriteLine();

        // Different formats demo
        Console.WriteLine("Different Format Options:");
        Console.WriteLine("------------------------");
        var id5 = GenerateIdWithHex32Format();
        var id6 = GenerateIdWithGuidFormat();
        var id7 = GenerateIdWithHex8Format();

        Console.WriteLine($"ID5 (Hex32): {id5}");
        Console.WriteLine($"ID6 (Guid): {id6}");
        Console.WriteLine($"ID7 (Hex8): {id7}");
        Console.WriteLine();

        // Contextual example
        Console.WriteLine("Practical Example:");
        Console.WriteLine("------------------");
        var userId = CreateUserId();
        var sessionId = CreateSessionId();
        var transactionId = CreateTransactionId();        Console.WriteLine($"User ID: {userId}");
        Console.WriteLine($"Session ID: {sessionId}");
        Console.WriteLine($"Transaction ID: {transactionId}");

        // Test case for multiple attributes on the same line
        TestMultipleAttributesOnSameLine();
        
        // Test HTML ID generation
        TestHtmlIdGeneration();
    }
    
    // This method generates a unique ID with the default format (Hex16)
    public static string GenerateId([UniqueId] string id = null)
    {
        return id ?? GenerateId_id_Id;
    }
    
    // Another method that uses the unique ID generator
    public static string GenerateAnotherUniqueId([UniqueId] string uniqueId = null)
    {
        return uniqueId ?? GenerateAnotherUniqueId_uniqueId_Id;
    }

    // Method that generates a 32-character hex ID
    public static string GenerateIdWithHex32Format([UniqueId(UniqueIdFormat.Hex32)] string id = null)
    {
        return id ?? GenerateIdWithHex32Format_id_Id;
    }

    // Method that generates a GUID-formatted ID
    public static string GenerateIdWithGuidFormat([UniqueId(UniqueIdFormat.Guid)] string id = null)
    {
        return id ?? GenerateIdWithGuidFormat_id_Id;
    }

    // Method that generates a short 8-character hex ID
    public static string GenerateIdWithHex8Format([UniqueId(UniqueIdFormat.Hex8)] string id = null)
    {
        return id ?? GenerateIdWithHex8Format_id_Id;
    }

    // Practical example methods
    public static string CreateUserId([UniqueId] string id = null)
    {
        return "usr_" + (id ?? CreateUserId_id_Id);
    }

    public static string CreateSessionId([UniqueId(UniqueIdFormat.Hex8)] string id = null)
    {
        return "sess_" + (id ?? CreateSessionId_id_Id);
    }

    public static string CreateTransactionId([UniqueId(UniqueIdFormat.Guid)] string id = null)
    {
        return "tx_" + (id ?? CreateTransactionId_id_Id);
    }

    // Test case for multiple attributes on the same line
    public static void TestMultipleAttributesOnSameLine()
    {
        // Generate multiple IDs on the same line
        var sameLineId1 = GenerateSameLineId1(); var sameLineId2 = GenerateSameLineId2();
        
        Console.WriteLine("\nMultiple Attributes on Same Line Test:");
        Console.WriteLine("---------------------------------------");
        Console.WriteLine($"Same Line ID1: {sameLineId1}");
        Console.WriteLine($"Same Line ID2: {sameLineId2}");
        
        // Check if they're unique
        Console.WriteLine($"Are the IDs unique? {sameLineId1 != sameLineId2}");
    }
    
    // Methods that will be on the same line in the source code
    public static string GenerateSameLineId1([UniqueId] string id = null) => id ?? GenerateSameLineId1_id_Id;
    public static string GenerateSameLineId2([UniqueId] string id = null) => id ?? GenerateSameLineId2_id_Id;    // HTML ID generation examples    
    public static void TestHtmlIdGeneration()
    {        Console.WriteLine("\nHTML5-Compliant Element ID Generation:");
        Console.WriteLine("-----------------------------------");
        
        // Generate multiple HTML element IDs
        var buttonId = GenerateHtmlButtonId();
        var inputId = GenerateHtmlInputId();
        var divId = GenerateHtmlDivId();
        var formId = GenerateHtmlFormId();
          Console.WriteLine($"Button ID: \"{buttonId}\" (6 chars, HTML5 compliant)");
        Console.WriteLine($"Input ID:  \"{inputId}\" (6 chars, HTML5 compliant)");
        Console.WriteLine($"Div ID:    \"{divId}\" (6 chars, HTML5 compliant)");
        Console.WriteLine($"Form ID:   \"{formId}\" (6 chars, HTML5 compliant)");
        
        // Sample HTML with the generated IDs
        Console.WriteLine("\nSample HTML with Compact IDs:");
        Console.WriteLine("---------------------------");
        Console.WriteLine($"<form id=\"{formId}\">");
        Console.WriteLine($"  <div id=\"{divId}\">");
        Console.WriteLine($"    <input id=\"{inputId}\" type=\"text\" />");
        Console.WriteLine($"    <button id=\"{buttonId}\">Submit</button>");
        Console.WriteLine($"  </div>");
        Console.WriteLine($"</form>");
    }
    // Methods for generating HTML5-compliant element IDs using the HtmlId format
    // These IDs start with a letter and can contain letters, digits, hyphens (-), and underscores (_)
    public static string GenerateHtmlButtonId([UniqueId(UniqueIdFormat.HtmlId)] string id = null) => id ?? GenerateHtmlButtonId_id_Id;
    public static string GenerateHtmlInputId([UniqueId(UniqueIdFormat.HtmlId)] string id = null) => id ?? GenerateHtmlInputId_id_Id;
    public static string GenerateHtmlDivId([UniqueId(UniqueIdFormat.HtmlId)] string id = null) => id ?? GenerateHtmlDivId_id_Id;
    public static string GenerateHtmlFormId([UniqueId(UniqueIdFormat.HtmlId)] string id = null) => id ?? GenerateHtmlFormId_id_Id;
}

public partial class Helper
{
    // Method in a different class that generates another unique ID
    public string GetUniqueId([UniqueId] string id = null)
    {
        return id ?? GetUniqueId_id_Id;
    }
}
