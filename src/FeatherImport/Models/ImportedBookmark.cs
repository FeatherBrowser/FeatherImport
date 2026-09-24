namespace FeatherImport;

public sealed record ImportedBookmark
{
    public string Title { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string Folder { get; init; } = "Favorites";
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.Now;
}
