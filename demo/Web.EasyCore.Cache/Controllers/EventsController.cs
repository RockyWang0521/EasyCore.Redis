using EasyCore.EventBus.Local;
using Microsoft.AspNetCore.Mvc;
using Web.EasyCore.Cache.Services;

namespace Web.EasyCore.Cache.Controllers;

[ApiController]
[Route("api/events")]
[Tags("J. EventBus + Polly")]
public sealed class EventsController : ControllerBase
{
    private readonly ILocalEventBus _bus;

    public EventsController(ILocalEventBus bus) => _bus = bus;

    [HttpPost("order-created")]
    public async Task<IActionResult> PublishOrderCreated([FromQuery] string orderId = "ORD-1")
    {
        await _bus.PublishAsync(new DemoOrderCreatedEvent { OrderId = orderId });
        return Ok(new { published = orderId, tip = "Watch console for Polly retry on HandleAsync" });
    }
}
