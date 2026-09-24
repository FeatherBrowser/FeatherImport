using System.Text;
using System.Text.RegularExpressions;

namespace FeatherImport.Chromium;

public static partial class ChromiumSessionReader
{
    public static List<string> Read(
        string profileDirectory,
        int urlLimit = 40,
        int fileLimit = 6)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(profileDirectory);
        urlLimit = Math.Clamp(urlLimit, 1, 1_000);
        fileLimit = Math.Clamp(fileLimit, 1, 100);

        IEnumerable<string> files = EnumerateSessionFiles(profileDirectory, fileLimit);
        var output = new List<string>();
        var known = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (string path in files)
        {
            byte[] data = File.ReadAllBytes(path);
            ExtractUrls(Encoding.Latin1.GetString(data), output, known, urlLimit);
            ExtractUrls(Encoding.Unicode.GetString(data), output, known, urlLimit);

            if (output.Count >= urlLimit)
                break;
        }

        return output.Take(urlLimit).ToList();
    }

    private static IEnumerable<string> EnumerateSessionFiles(
        string profileDirectory,
        int fileLimit)
    {
        string sessionsDirectory = Path.Join(profileDirectory, "Sessions");

        IEnumerable<string> modern = Directory.Exists(sessionsDirectory)
            ? Directory.EnumerateFiles(sessionsDirectory)
                .Where(path =>
                {
                    string name = Path.GetFileName(path);
                    return name.StartsWith("Tabs_", StringComparison.OrdinalIgnoreCase) ||
                           name.StartsWith("Session_", StringComparison.OrdinalIgnoreCase);
                })
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .Take(fileLimit)
            : [];

        string[] legacyNames =
        [
            "Current Tabs",
            "Last Tabs",
            "Current Session",
            "Last Session"
        ];

        IEnumerable<string> legacy = legacyNames
            .Select(name => Path.Join(profileDirectory, name))
            .Where(File.Exists);

        return modern
            .Concat(legacy)
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private static void ExtractUrls(
        string text,
        ICollection<string> output,
        ISet<string> known,
        int limit)
    {
        foreach (Match match in UrlRegex().Matches(text))
        {
            string candidate = match.Value.TrimEnd('.', ',', ';', ':');
            if (!Uri.TryCreate(candidate, UriKind.Absolute, out Uri? uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                continue;
            }

            string normalized = uri.AbsoluteUri;
            if (known.Add(normalized))
                output.Add(normalized);

            if (output.Count >= limit)
                return;
        }
    }

    [GeneratedRegex(
        @"https?://[A-Za-z0-9\-._~:/?#\[\]@!$&'()*+,;=%]{3,2048}",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex UrlRegex();
}
