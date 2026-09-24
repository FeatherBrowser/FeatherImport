namespace FeatherImport.Chromium;

public static class ChromiumProfileLocator
{
    public static IReadOnlyList<string> FindProfiles(string userDataDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userDataDirectory);

        if (!Directory.Exists(userDataDirectory))
            return [];

        return Directory.EnumerateDirectories(userDataDirectory)
            .Where(IsProfileDirectory)
            .OrderBy(static path => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static bool IsProfileDirectory(string path)
    {
        string name = Path.GetFileName(path);
        return name.Equals("Default", StringComparison.OrdinalIgnoreCase) ||
               name.StartsWith("Profile ", StringComparison.OrdinalIgnoreCase);
    }
}
