namespace FeatherImport;

public sealed record ImportedHistoryEntry
{
    public string Title { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public DateTimeOffset LastVisited { get; init; } = DateTimeOffset.Now;
    public int VisitCount { get; init; } = 1;
}
