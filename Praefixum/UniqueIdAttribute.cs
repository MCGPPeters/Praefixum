namespace Praefixum;

[AttributeUsage(AttributeTargets.Parameter)]
public sealed class UniqueIdAttribute : Attribute
{
    public UniqueIdFormat Format { get; }
    public string? Prefix { get; }
    public bool Deterministic { get; }

    public UniqueIdAttribute(UniqueIdFormat format = UniqueIdFormat.Guid, string? prefix = null, bool deterministic = true)
    {
        Format = format;
        Prefix = prefix;
        Deterministic = deterministic;
    }
}

public enum UniqueIdFormat
{
    Guid,
    HtmlId,
    Timestamp,
    ShortHash
}