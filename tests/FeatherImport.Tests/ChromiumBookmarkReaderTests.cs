using FeatherImport.Chromium;

namespace FeatherImport.Tests;

public sealed class ChromiumBookmarkReaderTests : IDisposable
{
    private readonly string _directory = Path.Join(
        Path.GetTempPath(),
        $"feather-import-tests-{Guid.NewGuid():N}");

    public ChromiumBookmarkReaderTests()
    {
        Directory.CreateDirectory(_directory);
    }

    [Fact]
    public void ReadsNestedChromiumBookmarks()
    {
        File.WriteAllText(Path.Join(_directory, "Bookmarks"), """
        {
          "roots": {
            "bookmark_bar": {
              "type": "folder",
              "name": "Bookmarks bar",
              "children": [
                {
                  "type": "folder",
                  "name": "Dev",
                  "children": [
                    {
                      "type": "url",
                      "name": "GitHub",
                      "url": "https://github.com/"
                    }
                  ]
                }
              ]
            }
          }
        }
        """);

        List<ImportedBookmark> bookmarks = ChromiumBookmarkReader.Read(_directory);

        ImportedBookmark bookmark = Assert.Single(bookmarks);
        Assert.Equal("GitHub", bookmark.Title);
        Assert.Equal("https://github.com/", bookmark.Url);
        Assert.Contains("Dev", bookmark.Folder);
    }

    public void Dispose()
    {
        Directory.Delete(_directory, recursive: true);
    }
}
