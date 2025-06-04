using System;
using Praefixum;

// =======================
// Program.cs
// =======================
Console.WriteLine(Html.H1("Welcome")); Console.WriteLine(Html.H1("Hello again"));
Console.WriteLine(Html.Div("Div one")); Console.WriteLine(Html.Div("Div two"));
Console.WriteLine(Html.Button("Click one")); Console.WriteLine(Html.Button("Click two"));

// Test method with int return type
var count1 = Html.GetElementCount("div");
var count2 = Html.GetElementCount("span");
Console.WriteLine($"Count 1: {count1}, Count 2: {count2}");