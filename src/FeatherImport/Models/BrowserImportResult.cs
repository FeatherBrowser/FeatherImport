namespace FeatherImport;

public sealed class BrowserImportResult
{
    public int ProfilesScanned { get; internal set; }
    public List<ImportedBookmark> Bookmarks { get; } = [];
    public List<ImportedHistoryEntry> History { get; } = [];
    public List<string> SessionUrls { get; } = [];
    public List<string> Warnings { get; } = [];
}
