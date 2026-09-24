namespace FeatherImport.Chromium;

public sealed record ChromiumImportOptions
{
    public int HistoryLimitPerProfile { get; init; } = 5_000;
    public int SessionUrlLimit { get; init; } = 40;
    public int SessionFileLimit { get; init; } = 6;
}
