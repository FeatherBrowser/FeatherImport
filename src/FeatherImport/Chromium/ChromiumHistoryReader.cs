using System.Diagnostics;
using Microsoft.Data.Sqlite;

namespace FeatherImport.Chromium;

public static class ChromiumHistoryReader
{
    public static List<ImportedHistoryEntry> Read(string profileDirectory, int limit = 5_000)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(profileDirectory);
        limit = Math.Clamp(limit, 1, 100_000);

        string source = Path.Join(profileDirectory, "History");
        if (!File.Exists(source))
            return [];

        string temporary = Path.Join(
            Path.GetTempPath(),
            $"feather-import-history-{Guid.NewGuid():N}.db");

        try
        {
            File.Copy(source, temporary, overwrite: true);

            using var connection = new SqliteConnection(
                $"Data Source={temporary};Mode=ReadOnly");
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = """
                SELECT url, title, last_visit_time, visit_count
                FROM urls
                WHERE (url LIKE 'http://%' OR url LIKE 'https://%')
                ORDER BY last_visit_time DESC
                LIMIT $limit;
                """;
            command.Parameters.AddWithValue("$limit", limit);

            using SqliteDataReader reader = command.ExecuteReader();
            var output = new List<ImportedHistoryEntry>();

            while (reader.Read())
            {
                output.Add(new ImportedHistoryEntry
                {
                    Url = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                    Title = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    LastVisited = ChromiumTime.FromMicrosecondsSince1601(
                        reader.IsDBNull(2) ? 0 : reader.GetInt64(2)),
                    VisitCount = reader.IsDBNull(3) ? 1 : Math.Max(1, reader.GetInt32(3))
                });
            }

            return output;
        }
        finally
        {
            TryDelete(temporary);
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (IOException ex)
        {
            Trace.TraceWarning($"Could not delete temporary Chromium history database: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            Trace.TraceWarning($"Could not delete temporary Chromium history database: {ex.Message}");
        }
    }
}
