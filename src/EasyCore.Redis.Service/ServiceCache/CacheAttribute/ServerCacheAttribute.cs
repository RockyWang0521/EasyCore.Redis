using EasyCore.Redis.Service;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace EasyCore.Redis.Service.Attribute;

/// <summary>
/// Marks a class, interface, method, or MVC controller / action for result caching via Castle DynamicProxy
/// (services) or <see cref="IFilterFactory"/> (API). Placement mirrors EasyCore.Invocation style without
/// depending on that package. Composes with other <c>IAsyncInterceptor</c> registrations via DI.
/// </summary>
[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Interface,
    Inherited = true,
    AllowMultiple = false)]
public sealed class ServerCacheAttribute : System.Attribute, IFilterFactory, IOrderedFilter
{
    /// <summary>
    /// Cache TTL in seconds. Default: 300.
    /// </summary>
    public int CacheSeconds { get; set; } = 300;

    /// <summary>
    /// Whether to cache null results. Default: <c>false</c>.
    /// </summary>
    public bool CacheNullValues { get; set; }

    /// <summary>
    /// MVC filter order. Lower runs earlier. Default: <c>100</c> (typically inside resilience wrappers).
    /// </summary>
    public int Order { get; set; } = 100;

    /// <inheritdoc />
    bool IFilterFactory.IsReusable => false;

    /// <inheritdoc />
    int IOrderedFilter.Order => Order;

    /// <inheritdoc />
    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        return ActivatorUtilities.CreateInstance<ServerCacheActionFilter>(serviceProvider, this);
    }
}
