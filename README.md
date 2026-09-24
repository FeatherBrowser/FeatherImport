# FeatherImport

Standalone Chromium profile import library extracted from Feather Browser 2.0.

The library does not know about `BrowserDataStore` or Feather Browser models. It returns plain import records that a browser host can merge into its own persistence layer.

## Current support

- Microsoft Edge profile discovery.
- Chromium bookmarks.
- Chromium history from the `History` SQLite database.
- Best-effort recovery of HTTP/HTTPS URLs from Chromium session files.
- Multiple Edge profiles (`Default`, `Profile *`).

## Usage

```csharp
using FeatherImport.Edge;

var importer = new EdgeImporter();
BrowserImportResult result = importer.Import();

foreach (ImportedBookmark bookmark in result.Bookmarks)
{
    // Merge into your browser's bookmark repository.
}

foreach (ImportedHistoryEntry entry in result.History)
{
    // Merge into your browser's history repository.
}
```

You can inject a custom Edge user-data directory for testing:

```csharp
var importer = new EdgeImporter(@"D:\Profiles\Edge User Data");
```

## Building

```bash
dotnet build src/FeatherImport/FeatherImport.csproj
dotnet test tests/FeatherImport.Tests/FeatherImport.Tests.csproj
```

## License

MIT.
