namespace Praefixum;

/// <summary>
/// Marks a parameter for unique ID generation by the Praefixum source generator.
/// </summary>
[System.AttributeUsage(System.AttributeTargets.Parameter)]
public sealed class UniqueIdAttribute : System.Attribute
{
    public UniqueIdFormat Format { get; }
    public string? Prefix { get; }

    public UniqueIdAttribute(UniqueIdFormat format = UniqueIdFormat.Guid, string? prefix = null)
    {
        Format = format;
        Prefix = prefix;
    }
}

public enum UniqueIdFormat
{
    Guid,
    HtmlId,
    Timestamp,
    ShortHash,
    /// <summary>
    /// Generates a deterministic 6-digit sequential number derived from the call site.
    /// </summary>
    Sequential,
    /// <summary>
    /// Generates a human-readable ID combining a kebab-case method name fragment with a short hash (e.g., "create-button-a3f2").
    /// </summary>
    Semantic
}
