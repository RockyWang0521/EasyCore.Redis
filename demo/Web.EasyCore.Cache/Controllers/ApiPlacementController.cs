using EasyCore.Redis.Service.Attribute;
using Microsoft.AspNetCore.Mvc;

namespace Web.EasyCore.Cache.Controllers;

/// <summary>F — [ServerCache] on Controller class (IFilterFactory).</summary>
[ApiController]
[Route("api/apiplacement")]
[Tags("F. API controller")]
[ServerCache(CacheSeconds = 60)]
public sealed class ApiPlacementController : ControllerBase
{
    private static int _pingHits;

    [HttpGet("ping")]
    public Task<string> Ping()
    {
        var n = Interlocked.Increment(ref _pingHits);
        Console.WriteLine($"  [ApiPlacementController] Ping body #{n}");
        return Task.FromResult($"api-ping@{n}");
    }

    [HttpGet("echo")]
    public Task<string> Echo([FromQuery] string text = "hello")
    {
        Console.WriteLine($"  [ApiPlacementController] Echo text={text}");
        return Task.FromResult($"echo:{text}");
    }
}

/// <summary>F2 — action-level only.</summary>
[ApiController]
[Route("api/apimethodonly")]
[Tags("F. API controller")]
public sealed class ApiMethodOnlyController : ControllerBase
{
    private static int _helloHits;
    private static int _plainHits;

    [HttpGet("hello")]
    [ServerCache(CacheSeconds = 60)]
    public Task<string> Hello()
    {
        var n = Interlocked.Increment(ref _helloHits);
        Console.WriteLine($"  [ApiMethodOnlyController] Hello body #{n}");
        return Task.FromResult($"hello@{n}");
    }

    [HttpGet("plain")]
    public Task<string> Plain()
    {
        var n = Interlocked.Increment(ref _plainHits);
        Console.WriteLine($"  [ApiMethodOnlyController] Plain body #{n} (no ServerCache expected)");
        return Task.FromResult($"plain@{n}");
    }
}
