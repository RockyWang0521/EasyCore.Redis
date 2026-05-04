using EasyCore.Redis.Service.Interceptor;
using Xunit;

namespace EasyCore.Redis.Tests;

public class ServerCacheKeyTests
{
    [Fact]
    public void BuildCacheKey_DiffersByArgumentValues()
    {
        var method = typeof(Sample).GetMethod(nameof(Sample.Echo))!;

        var keyA = ServerCacheStandardInterceptor.BuildCacheKey(method, new object?[] { "a" });
        var keyB = ServerCacheStandardInterceptor.BuildCacheKey(method, new object?[] { "b" });
        var keyA2 = ServerCacheStandardInterceptor.BuildCacheKey(method, new object?[] { "a" });

        Assert.NotEqual(keyA, keyB);
        Assert.Equal(keyA, keyA2);
        Assert.StartsWith("svc:Echo:", keyA);
    }

    [Fact]
    public void BuildCacheKey_IncludesNullArguments()
    {
        var method = typeof(Sample).GetMethod(nameof(Sample.Echo))!;
        var key = ServerCacheStandardInterceptor.BuildCacheKey(method, new object?[] { null });
        Assert.False(string.IsNullOrWhiteSpace(key));
    }

    private sealed class Sample
    {
        public string Echo(string? input) => input ?? string.Empty;
    }
}
