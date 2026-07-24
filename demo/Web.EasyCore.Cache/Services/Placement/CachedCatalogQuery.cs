using EasyCore.Redis.Service.Attribute;

namespace Web.EasyCore.Cache.Services.Placement;

/// <summary>E — multi-interface; cache only on GetItemAsync.</summary>
public interface ICachedCatalog
{
    Task<string> GetItemAsync(string sku);
}

/// <summary>E — plain side, no ServerCache.</summary>
public interface IPlainCatalogQuery
{
    Task<string> ListAsync();
}

public sealed class CachedCatalogQuery : ICachedCatalog, IPlainCatalogQuery
{
    private int _itemHits;
    private int _listHits;

    [ServerCache(CacheSeconds = 60)]
    public Task<string> GetItemAsync(string sku)
    {
        var n = Interlocked.Increment(ref _itemHits);
        Console.WriteLine($"  [CachedCatalogQuery] GetItemAsync body #{n} sku={sku}");
        return Task.FromResult($"item:{sku}@{n}");
    }

    public Task<string> ListAsync()
    {
        var n = Interlocked.Increment(ref _listHits);
        Console.WriteLine($"  [CachedCatalogQuery] ListAsync body #{n} (plain — no ServerCache expected)");
        return Task.FromResult($"list@{n}");
    }
}
