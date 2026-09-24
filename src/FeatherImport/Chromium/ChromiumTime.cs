namespace FeatherImport.Chromium;

public static class ChromiumTime
{
    public static DateTimeOffset FromMicrosecondsSince1601(long microseconds)
    {
        try
        {
            long fileTime = checked(microseconds * 10);
            return DateTimeOffset.FromFileTime(fileTime);
        }
        catch (Exception ex) when (ex is OverflowException or ArgumentOutOfRangeException)
        {
            return DateTimeOffset.UnixEpoch;
        }
    }
}
