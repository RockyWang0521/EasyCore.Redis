using EasyCore.Polly;
using EasyCore.Redis.Service.Attribute;
using Web.EasyCore.Cache.Attributes;

namespace Web.EasyCore.Cache.Services.Combo;

/// <summary>
/// Cross-package stack: Invocation [Trace] + Polly [PollyConfig] + Redis [ServerCache].
/// </summary>
[Trace]
public interface IComboStackService
{
    [PollyConfig(MaxRetry = 3, RetryIntervalSeconds = 0)]
    Task<string> GetWithRetryAsync();

    [ServerCache(CacheSeconds = 60)]
    Task<string> GetCachedAsync(string key);

    [PollyConfig(MaxRetry = 2, RetryIntervalSeconds = 0)]
    [ServerCache(CacheSeconds = 30)]
    Task<string> GetStackedAsync(string key);
}
