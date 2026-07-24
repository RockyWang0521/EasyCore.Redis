using EasyCore.Polly;
using EasyCore.Redis.Service.Attribute;
using Web.EasyCore.Cache.Attributes;

namespace Web.EasyCore.Cache.Services.Combo;

/// <summary>
/// Cross-package stack: [Trace] + [PollyConfig] + [ServerCache] on implementation.
/// </summary>
[Trace]
public sealed class ComboStackService : IComboStackService
{
    private int _retryAttempts;
    private int _cacheBodyHits;
    private int _stackedBodyHits;

    [PollyConfig(MaxRetry = 3, RetryIntervalSeconds = 0)]
    public Task<string> GetWithRetryAsync()
    {
        _retryAttempts++;
        Console.WriteLine($"[Combo] GetWithRetryAsync body attempt #{_retryAttempts}");
        if (_retryAttempts < 3)
        {
            throw new InvalidOperationException($"transient #{_retryAttempts}");
        }

        return Task.FromResult($"retry-ok after {_retryAttempts} attempts");
    }

    [ServerCache(CacheSeconds = 60)]
    public Task<string> GetCachedAsync(string key)
    {
        _cacheBodyHits++;
        Console.WriteLine($"[Combo] GetCachedAsync body hit #{_cacheBodyHits} key={key}");
        return Task.FromResult($"cached-value:{key}@{_cacheBodyHits}");
    }

    [PollyConfig(MaxRetry = 2, RetryIntervalSeconds = 0)]
    [ServerCache(CacheSeconds = 30)]
    public Task<string> GetStackedAsync(string key)
    {
        _stackedBodyHits++;
        Console.WriteLine($"[Combo] GetStackedAsync body hit #{_stackedBodyHits} key={key}");
        return Task.FromResult($"stacked:{key}@{_stackedBodyHits}");
    }
}
