using EasyCore.Redis.Service.Attribute;

namespace Web.EasyCore.Cache.Services.Placement;

public interface ICachedOrderQuery
{
    Task<string> GetAsync(string orderId);

    Task<string> ListAsync();
}

/// <summary>B — class type: every method cached.</summary>
[ServerCache(CacheSeconds = 60)]
public sealed class CachedOrderQuery : ICachedOrderQuery
{
    private int _getHits;
    private int _listHits;

    public Task<string> GetAsync(string orderId)
    {
        var n = Interlocked.Increment(ref _getHits);
        Console.WriteLine($"  [CachedOrderQuery] GetAsync body #{n} orderId={orderId}");
        return Task.FromResult($"order-{orderId}@{n}");
    }

    public Task<string> ListAsync()
    {
        var n = Interlocked.Increment(ref _listHits);
        Console.WriteLine($"  [CachedOrderQuery] ListAsync body #{n}");
        return Task.FromResult($"orders@{n}");
    }
}
