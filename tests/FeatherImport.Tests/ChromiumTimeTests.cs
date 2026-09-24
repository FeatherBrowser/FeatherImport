using FeatherImport.Chromium;

namespace FeatherImport.Tests;

public sealed class ChromiumTimeTests
{
    [Fact]
    public void ZeroMapsToWindowsEpoch()
    {
        DateTimeOffset value = ChromiumTime.FromMicrosecondsSince1601(0);
        Assert.Equal(1601, value.Year);
        Assert.Equal(1, value.Month);
        Assert.Equal(1, value.Day);
    }

    [Fact]
    public void InvalidValueFallsBackToUnixEpoch()
    {
        DateTimeOffset value = ChromiumTime.FromMicrosecondsSince1601(long.MaxValue);
        Assert.Equal(DateTimeOffset.UnixEpoch, value);
    }
}
