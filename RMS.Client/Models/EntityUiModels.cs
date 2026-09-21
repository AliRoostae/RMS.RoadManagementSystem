namespace RMS.Client.Models;

public sealed record EntityColumn(string Key, string Title, string? CssClass = null);

public sealed record LookupOption(string Value, string Label);

public sealed record EntityPageRequest(
    int Skip,
    int Take,
    string SearchTerm,
    CancellationToken CancellationToken);

public sealed class EntityRow
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Dictionary<string, string> Values { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public string Status { get; set; } = "فعال";
    public string StatusTone { get; set; } = "success";
}
