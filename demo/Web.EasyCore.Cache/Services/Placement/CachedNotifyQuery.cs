using EasyCore.Redis.Service.Attribute;

namespace Web.EasyCore.Cache.Services.Placement;

/// <summary>D — interface method only.</summary>
public interface ICachedNotifyQuery
{
    [ServerCache(CacheSeconds = 60)]
    Task<string> GetTemplateAsync(string key);

    Task<string> PingAsync();
}

public sealed class CachedNotifyQuery : ICachedNotifyQuery
{
    private int _templateHits;
    private int _pingHits;

    public Task<string> GetTemplateAsync(string key)
    {
        var n = Interlocked.Increment(ref _templateHits);
        Console.WriteLine($"  [CachedNotifyQuery] GetTemplateAsync body #{n} key={key}");
        return Task.FromResult($"tpl:{key}@{n}");
    }

    public Task<string> PingAsync()
    {
        var n = Interlocked.Increment(ref _pingHits);
        Console.WriteLine($"  [CachedNotifyQuery] PingAsync body #{n} (no ServerCache expected)");
        return Task.FromResult($"pong@{n}");
    }
}
