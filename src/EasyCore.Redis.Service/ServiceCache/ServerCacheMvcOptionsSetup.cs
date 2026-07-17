using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EasyCore.Redis.Service;

/// <summary>
/// Registers the MVC convention that merges interface <see cref="Attribute.ServerCacheAttribute"/> onto actions.
/// </summary>
internal sealed class ServerCacheMvcOptionsSetup : IConfigureOptions<MvcOptions>
{
    public void Configure(MvcOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Conventions.Add(new ServerCacheInterfaceAttributeConvention());
    }
}
