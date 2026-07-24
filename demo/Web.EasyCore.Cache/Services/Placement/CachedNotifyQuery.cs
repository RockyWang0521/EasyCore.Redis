using EasyCore.Redis.Service.Attribute;

namespace Web.EasyCore.Cache.Services.Placement;

/// <summary>D — method-level [ServerCache] on implementation.</summary>
public interface ICachedNotifyQuery
{
    Task<string> GetTemplateAsync(string key);

    Task<string> PingAsync();
}

public sealed class CachedNotifyQuery : ICachedNotifyQuery
{
    private int _templateHits;
    private int _pingHits;

    [ServerCache(CacheSeconds = 60)]
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
