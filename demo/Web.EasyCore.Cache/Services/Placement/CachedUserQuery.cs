using EasyCore.Redis.Service.Attribute;

namespace Web.EasyCore.Cache.Services.Placement;

/// <summary>A — interface type: every method cached.</summary>
[ServerCache(CacheSeconds = 60)]
public interface ICachedUserQuery
{
    Task<string> GetNameAsync(string id);

    Task<string> GetStatusAsync();
}

public sealed class CachedUserQuery : ICachedUserQuery
{
    private int _nameHits;
    private int _statusHits;

    public Task<string> GetNameAsync(string id)
    {
        var n = Interlocked.Increment(ref _nameHits);
        Console.WriteLine($"  [CachedUserQuery] GetNameAsync body #{n} id={id}");
        return Task.FromResult($"user-{id}@{n}");
    }

    public Task<string> GetStatusAsync()
    {
        var n = Interlocked.Increment(ref _statusHits);
        Console.WriteLine($"  [CachedUserQuery] GetStatusAsync body #{n}");
        return Task.FromResult($"status@{n}");
    }
}
