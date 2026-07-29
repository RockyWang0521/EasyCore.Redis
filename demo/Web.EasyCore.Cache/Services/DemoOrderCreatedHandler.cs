using EasyCore.EventBus.Event;
using EasyCore.Polly;
using Web.EasyCore.Cache.Attributes;

namespace Web.EasyCore.Cache.Services;

public sealed class DemoOrderCreatedEvent : IEvent
{
    public string OrderId { get; init; } = string.Empty;
}

/// <summary>
/// EventBus Handler + [PollyConfig] (+ optional [Trace]): dispatch goes through nested Castle proxies.
/// EventBus itself only CreateScope + GetServices — no Ambient / AOP.
/// </summary>
public sealed class DemoOrderCreatedHandler : ILocalEventHandler<DemoOrderCreatedEvent>
{
    [Trace]
    public Task HandleAsync(DemoOrderCreatedEvent eventMessage)
    {
        return Task.CompletedTask;
    }
}
