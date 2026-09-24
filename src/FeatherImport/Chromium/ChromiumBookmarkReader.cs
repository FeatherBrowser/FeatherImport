using System.Text.Json;

namespace FeatherImport.Chromium;

public static class ChromiumBookmarkReader
{
    public static List<ImportedBookmark> Read(string profileDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(profileDirectory);
        string path = Path.Join(profileDirectory, "Bookmarks");
        if (!File.Exists(path))
            return [];

        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
        var output = new List<ImportedBookmark>();

        if (!document.RootElement.TryGetProperty("roots", out JsonElement roots))
            return output;

        foreach (JsonProperty root in roots.EnumerateObject())
        {
            string rootName = root.Name switch
            {
                "bookmark_bar" => "Favorites bar",
                "other" => "Other favorites",
                "synced" => "Mobile favorites",
                _ => root.Name
            };

            ReadNode(root.Value, rootName, output);
        }

        return output;
    }

    private static void ReadNode(
        JsonElement node,
        string folder,
        ICollection<ImportedBookmark> output)
    {
        if (node.TryGetProperty("type", out JsonElement type) &&
            type.GetString() == "url")
        {
            string url = node.TryGetProperty("url", out JsonElement urlElement)
                ? urlElement.GetString() ?? string.Empty
                : string.Empty;

            if (!IsHttpUrl(url))
                return;

            string title = node.TryGetProperty("name", out JsonElement nameElement)
                ? nameElement.GetString() ?? url
                : url;

            output.Add(new ImportedBookmark
            {
                Title = title,
                Url = url,
                Folder = string.IsNullOrWhiteSpace(folder) ? "Imported" : folder,
                CreatedAt = DateTimeOffset.Now
            });
            return;
        }

        string currentFolder = folder;
        if (node.TryGetProperty("name", out JsonElement folderName))
        {
            string? name = folderName.GetString();
            if (!string.IsNullOrWhiteSpace(name) &&
                !name.Equals("Bookmarks bar", StringComparison.OrdinalIgnoreCase))
            {
                currentFolder = string.IsNullOrWhiteSpace(folder)
                    ? name
                    : $"{folder} / {name}";
            }
        }

        if (!node.TryGetProperty("children", out JsonElement children) ||
            children.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (JsonElement child in children.EnumerateArray())
            ReadNode(child, currentFolder, output);
    }

    private static bool IsHttpUrl(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out Uri? uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
