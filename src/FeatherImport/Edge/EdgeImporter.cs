using FeatherImport.Chromium;

namespace FeatherImport.Edge;

public sealed class EdgeImporter
{
    private readonly string _userDataDirectory;
    private readonly ChromiumImportOptions _options;

    public EdgeImporter(
        string? userDataDirectory = null,
        ChromiumImportOptions? options = null)
    {
        _userDataDirectory = userDataDirectory ?? GetDefaultUserDataDirectory();
        _options = options ?? new ChromiumImportOptions();
    }

    public BrowserImportResult Import()
    {
        if (!Directory.Exists(_userDataDirectory))
        {
            throw new DirectoryNotFoundException(
                $"Microsoft Edge profile data was not found: {_userDataDirectory}");
        }

        var result = new BrowserImportResult();
        var sessionUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (string profile in ChromiumProfileLocator.FindProfiles(_userDataDirectory))
        {
            result.ProfilesScanned++;
            ImportProfile(profile, result, sessionUrls);
        }

        return result;
    }

    public static string GetDefaultUserDataDirectory() =>
        Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Microsoft",
            "Edge",
            "User Data");

    private void ImportProfile(
        string profile,
        BrowserImportResult result,
        ISet<string> sessionUrls)
    {
        string profileName = Path.GetFileName(profile);

        try
        {
            result.Bookmarks.AddRange(ChromiumBookmarkReader.Read(profile));
        }
        catch (Exception ex) when (IsProfileReadFailure(ex))
        {
            result.Warnings.Add($"{profileName} favorites: {ex.Message}");
        }

        try
        {
            result.History.AddRange(
                ChromiumHistoryReader.Read(profile, _options.HistoryLimitPerProfile));
        }
        catch (Exception ex) when (IsProfileReadFailure(ex))
        {
            result.Warnings.Add($"{profileName} history: {ex.Message}");
        }

        try
        {
            foreach (string url in ChromiumSessionReader.Read(
                         profile,
                         _options.SessionUrlLimit,
                         _options.SessionFileLimit))
            {
                if (result.SessionUrls.Count >= _options.SessionUrlLimit)
                    break;

                if (sessionUrls.Add(url))
                    result.SessionUrls.Add(url);
            }
        }
        catch (Exception ex) when (IsProfileReadFailure(ex))
        {
            result.Warnings.Add($"{profileName} session tabs: {ex.Message}");
        }
    }

    private static bool IsProfileReadFailure(Exception exception) =>
        exception is IOException
            or UnauthorizedAccessException
            or System.Text.Json.JsonException
            or Microsoft.Data.Sqlite.SqliteException
            or InvalidDataException;
}
