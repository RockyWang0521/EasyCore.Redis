namespace Web.EasyCore.Cache.Services.Combo;

public sealed class ComboStackService : IComboStackService
{
    private int _retryAttempts;
    private int _cacheBodyHits;
    private int _stackedBodyHits;

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

    public Task<string> GetCachedAsync(string key)
    {
        _cacheBodyHits++;
        Console.WriteLine($"[Combo] GetCachedAsync body hit #{_cacheBodyHits} key={key}");
        return Task.FromResult($"cached-value:{key}@{_cacheBodyHits}");
    }

    public Task<string> GetStackedAsync(string key)
    {
        _stackedBodyHits++;
        Console.WriteLine($"[Combo] GetStackedAsync body hit #{_stackedBodyHits} key={key}");
        return Task.FromResult($"stacked:{key}@{_stackedBodyHits}");
    }
}
