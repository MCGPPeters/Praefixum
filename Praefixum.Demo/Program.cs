#nullable enable
using System;
using Praefixum;
using static Html;

namespace Praefixum.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Praefixum Source Generator Demo ===");

            // Test basic HTML generation with single UniqueId parameters
            Console.WriteLine("\n--- Single Parameter Examples ---");
            var h1Result = H1("Welcome to Praefixum");
            Console.WriteLine($"H1: {h1Result}");

            var divResult = Div("This div has an auto-generated ID");
            Console.WriteLine($"Div: {divResult}");

            var buttonResult = Button("Click Me");
            Console.WriteLine($"Button: {buttonResult}");

            // Test with explicit IDs
            Console.WriteLine("\n--- Explicit ID Examples ---");
            var explicitH1 = H1("Explicit Content", "my-explicit-h1");
            Console.WriteLine($"Explicit H1: {explicitH1}");

            var explicitDiv = Div("Explicit Div Content", "my-div-id");
            Console.WriteLine($"Explicit Div: {explicitDiv}");

            // NEW: Test multiple UniqueId parameters
            Console.WriteLine("\n--- Multiple Parameter Examples ---");
            var form = CreateForm();
            Console.WriteLine($"Form with multiple auto IDs:\n{form}");

            var formMixed = CreateForm(formId: "contact-form", emailInputId: "user-email");
            Console.WriteLine($"\nForm with mixed IDs:\n{formMixed}");

            var card = CreateCard(title: "Demo Card", content: "This demonstrates multiple UniqueId parameters.");
            Console.WriteLine($"\nCard with multiple formats:\n{card}");

            var widget = CreateWidget();
            Console.WriteLine($"\nWidget with auto IDs:\n{widget}");

            // Test return type preservation
            Console.WriteLine("\n--- Return Type Examples ---");
            var count1 = GetElementCount("div");
            var count2 = GetElementCount("span");
            Console.WriteLine($"Element counts - Div: {count1}, Span: {count2}");

            Console.WriteLine("\n=== Demo Complete ===");
        }
    }
}
