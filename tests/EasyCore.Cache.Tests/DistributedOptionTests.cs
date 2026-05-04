using EasyCore.Redis.Distributed;
using Xunit;

namespace EasyCore.Redis.Tests;

public class DistributedOptionTests
{
    [Fact]
    public void GetPrefixedKey_UsesDistributedName()
    {
        var option = new DistributedOption { DistributedName = "App" };
        Assert.Equal("App:user:1", option.GetPrefixedKey("user:1"));
    }

    [Fact]
    public void GetPrefixedKey_ThrowsOnEmptyKey()
    {
        var option = new DistributedOption();
        Assert.Throws<ArgumentException>(() => option.GetPrefixedKey(" "));
    }

    [Fact]
    public void ToConfigurationOptions_MapsTimeoutsAsMilliseconds()
    {
        var option = new DistributedOption
        {
            EndPoints = { "127.0.0.1:6379" },
            ConnectTimeout = TimeSpan.FromSeconds(3),
            SyncTimeout = TimeSpan.FromMilliseconds(1500),
            DefaultDatabase = 2,
            AbortOnConnectFail = true
        };

        var config = option.ToConfigurationOptions();

        Assert.Equal(3000, config.ConnectTimeout);
        Assert.Equal(1500, config.SyncTimeout);
        Assert.Equal(2, config.DefaultDatabase);
        Assert.True(config.AbortOnConnectFail);
        Assert.Single(config.EndPoints);
    }

    [Fact]
    public void ToConfigurationOptions_ThrowsWhenNoEndpoints()
    {
        var option = new DistributedOption();
        Assert.Throws<InvalidOperationException>(() => option.ToConfigurationOptions());
    }
}
