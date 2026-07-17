using Microsoft.AspNetCore.Mvc;
using Web.EasyCore.Cache.Services.Placement;

namespace Web.EasyCore.Cache.Controllers;

[ApiController]
[Route("api/demo")]
[Tags("0. Demo guide")]
public sealed class DemoController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        title = "EasyCore.Redis demos",
        tip =
            "Placement A–F use [ServerCache]. Call each cached route twice — 2nd should HIT (no body log). " +
            "Need Redis localhost:6379.",
        scenarios = new object[]
        {
            new
            {
                id = "A",
                name = "Service · interface type",
                rule = "[ServerCache] on ICachedUserQuery",
                routes = new[] { "GET /api/users/name?id=1 (×2)", "GET /api/users/status (×2)" }
            },
            new
            {
                id = "B",
                name = "Service · class type",
                rule = "[ServerCache] on CachedOrderQuery class",
                routes = new[] { "GET /api/orders?orderId=1 (×2)", "GET /api/orders/list (×2)" }
            },
            new
            {
                id = "C",
                name = "Service · method",
                rule = "Only GenerateAsync has [ServerCache]",
                routes = new[] { "GET /api/reports/generate?name=w (×2)", "GET /api/reports/preview?name=w (always body)" }
            },
            new
            {
                id = "D",
                name = "Service · interface method",
                rule = "[ServerCache] on ICachedNotifyQuery.GetTemplateAsync only",
                routes = new[] { "GET /api/notify/template?key=a (×2)", "GET /api/notify/ping (always body)" }
            },
            new
            {
                id = "E",
                name = "Service · multi-interface",
                rule = "[ServerCache] on ICachedCatalog only",
                routes = new[] { "GET /api/catalog/item?sku=a (×2)", "GET /api/catalog (plain)" }
            },
            new
            {
                id = "F",
                name = "Controller · class / action",
                rule = "[ServerCache] on Controller or Action",
                routes = new[] { "GET /api/apiplacement/ping (×2)", "GET /api/apimethodonly/hello (×2)", "GET /api/apimethodonly/plain" }
            },
            new
            {
                id = "S",
                name = "ServerCache args (legacy)",
                rule = "IServer overload demos",
                routes = new[] { "GET /api/ServiceCache/ServerCacheNoParameter", "…" }
            },
            new
            {
                id = "H",
                name = "Cross-stack · Redis + Polly + Invocation",
                rule = "[Trace]+[PollyConfig]+[ServerCache]",
                routes = new[] { "GET /api/combo", "GET /api/combo/retry", "GET /api/combo/cache?key=demo" }
            }
        }
    });
}

[ApiController]
[Route("api/users")]
[Tags("A. Interface type")]
public sealed class UsersController : ControllerBase
{
    private readonly ICachedUserQuery _users;
    public UsersController(ICachedUserQuery users) => _users = users;

    [HttpGet("name")]
    public Task<string> Name([FromQuery] string id = "1") => _users.GetNameAsync(id);

    [HttpGet("status")]
    public Task<string> Status() => _users.GetStatusAsync();
}

[ApiController]
[Route("api/orders")]
[Tags("B. Class type")]
public sealed class OrdersController : ControllerBase
{
    private readonly ICachedOrderQuery _orders;
    public OrdersController(ICachedOrderQuery orders) => _orders = orders;

    [HttpGet]
    public Task<string> Get([FromQuery] string orderId = "1") => _orders.GetAsync(orderId);

    [HttpGet("list")]
    public Task<string> List() => _orders.ListAsync();
}

[ApiController]
[Route("api/reports")]
[Tags("C. Method")]
public sealed class ReportsController : ControllerBase
{
    private readonly ICachedReportQuery _reports;
    public ReportsController(ICachedReportQuery reports) => _reports = reports;

    [HttpGet("generate")]
    public Task<string> Generate([FromQuery] string name = "weekly") => _reports.GenerateAsync(name);

    [HttpGet("preview")]
    public Task<string> Preview([FromQuery] string name = "weekly") => _reports.PreviewAsync(name);
}

[ApiController]
[Route("api/notify")]
[Tags("D. Interface method")]
public sealed class NotifyController : ControllerBase
{
    private readonly ICachedNotifyQuery _notify;
    public NotifyController(ICachedNotifyQuery notify) => _notify = notify;

    [HttpGet("template")]
    public Task<string> Template([FromQuery] string key = "a") => _notify.GetTemplateAsync(key);

    [HttpGet("ping")]
    public Task<string> Ping() => _notify.PingAsync();
}

[ApiController]
[Route("api/catalog")]
[Tags("E. Multi-interface")]
public sealed class CatalogController : ControllerBase
{
    private readonly ICachedCatalog _cached;
    private readonly IPlainCatalogQuery _plain;

    public CatalogController(ICachedCatalog cached, IPlainCatalogQuery plain)
    {
        _cached = cached;
        _plain = plain;
    }

    [HttpGet("item")]
    public Task<string> Item([FromQuery] string sku = "a") => _cached.GetItemAsync(sku);

    [HttpGet]
    public Task<string> List() => _plain.ListAsync();
}
