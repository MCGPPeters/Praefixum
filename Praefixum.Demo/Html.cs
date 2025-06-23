using Praefixum;
public static class Html
{
    public static string H1(string content, [UniqueId(UniqueIdFormat.HtmlId)] string? id = null) =>
        $"<h1 id=\"{id}\">{content}</h1>";

    public static string Div(string content, [UniqueId(UniqueIdFormat.ShortHash)] string? id = null) =>
        $"<div id=\"{id}\">{content}</div>";

    public static string Button(string content, [UniqueId(UniqueIdFormat.Guid)] string? id = null) =>
        $"<button id=\"{id}\">{content}</button>";
}
