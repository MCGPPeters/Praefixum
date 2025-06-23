#nullable enable
using Praefixum;

public static class Html
{
    public static string H1(string content, [UniqueId(UniqueIdFormat.HtmlId)] string? id = null) =>
        $"<h1 id=\"{id}\">{content}</h1>";

    public static string Div(string content, [UniqueId(UniqueIdFormat.ShortHash)] string? id = null) =>
        $"<div id=\"{id}\">{content}</div>";

    public static string Button(string content, [UniqueId(UniqueIdFormat.Guid)] string? id = null) =>
        $"<button id=\"{id}\">{content}</button>";

    // Test method with different return type
    public static int GetElementCount(string selector, [UniqueId(UniqueIdFormat.HtmlId)] string? id = null) =>
        selector.Length + (id?.Length ?? 0);

    // NEW: Multiple UniqueId parameters demonstration
    public static string CreateForm(
        [UniqueId(UniqueIdFormat.HtmlId, prefix: "form-")] string? formId = null,
        [UniqueId(UniqueIdFormat.HtmlId)] string? nameInputId = null,
        [UniqueId(UniqueIdFormat.HtmlId)] string? emailInputId = null,
        [UniqueId(UniqueIdFormat.Guid)] string? submitButtonId = null) =>
        $@"<form id=""{formId}"">
    <input id=""{nameInputId}"" name=""name"" type=""text"" placeholder=""Name"" />
    <input id=""{emailInputId}"" name=""email"" type=""email"" placeholder=""Email"" />
    <button id=""{submitButtonId}"" type=""submit"">Submit</button>
</form>";

    public static string CreateCard(
        [UniqueId(UniqueIdFormat.HtmlId, prefix: "card-")] string? cardId = null,
        [UniqueId(UniqueIdFormat.HtmlId, prefix: "title-")] string? titleId = null,
        [UniqueId(UniqueIdFormat.Timestamp)] string? timestampId = null,
        string title = "Card Title",
        string content = "Card content") =>
        $@"<div id=""{cardId}"" class=""card"" data-timestamp=""{timestampId}"">
    <h2 id=""{titleId}"">{title}</h2>
    <p>{content}</p>
</div>";

    public static string CreateWidget(
        [UniqueId(UniqueIdFormat.ShortHash, prefix: "widget-")] string? widgetId = null,
        [UniqueId(UniqueIdFormat.HtmlId)] string? contentId = null) =>
        $@"<div id=""{widgetId}"" class=""widget"">
    <div id=""{contentId}"" class=""widget-content"">
        Widget Content Area
    </div>
</div>";
}
