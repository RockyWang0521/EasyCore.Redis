using Microsoft.AspNetCore.Mvc;
using Web.EasyCore.Cache.Services.Combo;

namespace Web.EasyCore.Cache.Controllers;

/// <summary>
/// Cross-stack demo: Redis ServerCache + Polly + Invocation.
/// </summary>
[ApiController]
[Route("api/combo")]
[Tags("Cross-stack (Redis + Polly + Invocation)")]
public sealed class ComboStackController : ControllerBase
{
    private readonly IComboStackService _combo;

    public ComboStackController(IComboStackService combo) => _combo = combo;

    [HttpGet("retry")]
    public Task<string> Retry() => _combo.GetWithRetryAsync();

    [HttpGet("cache")]
    public Task<string> Cache([FromQuery] string key = "demo") => _combo.GetCachedAsync(key);

    [HttpGet("stacked")]
    public Task<string> Stacked([FromQuery] string key = "demo") => _combo.GetStackedAsync(key);

    [HttpGet]
    public IActionResult Guide() => Ok(new
    {
        title = "Cross-stack combo",
        tip = "Console: [Trace/Invocation] + [Combo] body. Same Redis as ServerCache demos.",
        routes = new[]
        {
            "GET /api/combo/retry",
            "GET /api/combo/cache?key=demo (call twice)",
            "GET /api/combo/stacked?key=demo"
        }
    });
}
