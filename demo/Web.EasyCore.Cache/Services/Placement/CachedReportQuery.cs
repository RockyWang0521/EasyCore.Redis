using EasyCore.Redis.Service.Attribute;

namespace Web.EasyCore.Cache.Services.Placement;

/// <summary>C — no type attrs; only some impl methods.</summary>
public interface ICachedReportQuery
{
    Task<string> GenerateAsync(string name);

    Task<string> PreviewAsync(string name);
}

public sealed class CachedReportQuery : ICachedReportQuery
{
    private int _generateHits;
    private int _previewHits;

    [ServerCache(CacheSeconds = 60)]
    public Task<string> GenerateAsync(string name)
    {
        var n = Interlocked.Increment(ref _generateHits);
        Console.WriteLine($"  [CachedReportQuery] GenerateAsync body #{n} name={name}");
        return Task.FromResult($"report:{name}@{n}");
    }

    public Task<string> PreviewAsync(string name)
    {
        var n = Interlocked.Increment(ref _previewHits);
        Console.WriteLine($"  [CachedReportQuery] PreviewAsync body #{n} (no ServerCache expected)");
        return Task.FromResult($"preview:{name}@{n}");
    }
}
