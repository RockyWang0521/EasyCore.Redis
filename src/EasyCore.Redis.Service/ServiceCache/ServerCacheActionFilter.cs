using EasyCore.Redis.Distributed;
using EasyCore.Redis.Service.Attribute;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Reflection;

namespace EasyCore.Redis.Service;

/// <summary>
/// MVC action filter created by <see cref="ServerCacheAttribute"/> via <see cref="IFilterFactory"/>.
/// </summary>
internal sealed class ServerCacheActionFilter : IAsyncActionFilter
{
    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };

    private readonly ServerCacheAttribute _attribute;
    private readonly IServiceProvider _services;

    public ServerCacheActionFilter(ServerCacheAttribute attribute, IServiceProvider services)
    {
        _attribute = attribute ?? throw new ArgumentNullException(nameof(attribute));
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        if (context.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
        {
            await next().ConfigureAwait(false);
            return;
        }

        var method = actionDescriptor.MethodInfo;
        if (!TryGetResultType(method.ReturnType, out var resultType))
        {
            await next().ConfigureAwait(false);
            return;
        }

        var cache = _services.GetService<IDistributedCache>();
        if (cache is null)
        {
            await next().ConfigureAwait(false);
            return;
        }

        var arguments = BuildArguments(context, method.GetParameters());
        var cacheKey = ServerCacheAspect.BuildCacheKey(method, arguments);

        if (await cache.KeyExistsAsync(cacheKey).ConfigureAwait(false))
        {
            var cached = await cache.GetAsync(cacheKey).ConfigureAwait(false);
            if (cached is not null)
            {
                var hit = JsonConvert.DeserializeObject(cached, resultType, JsonSettings);
                context.Result = hit as IActionResult ?? new ObjectResult(hit);
                return;
            }
        }

        var executed = await next().ConfigureAwait(false);
        if (executed.Exception is not null && !executed.ExceptionHandled)
        {
            return;
        }

        var payload = ExtractPayload(executed.Result);
        if (payload is not null || _attribute.CacheNullValues)
        {
            var json = JsonConvert.SerializeObject(payload, JsonSettings);
            await cache.SetAsync(cacheKey, json, _attribute.CacheSeconds).ConfigureAwait(false);
        }
    }

    private static object? ExtractPayload(IActionResult? result)
        => result switch
        {
            // Dynamic API / AppService actions typically return Task<T> → ObjectResult.
            ObjectResult objectResult => objectResult.Value,
            JsonResult jsonResult => jsonResult.Value,
            ContentResult contentResult => contentResult.Content,
            _ => null
        };

    private static object?[] BuildArguments(ActionExecutingContext context, ParameterInfo[] parameters)
    {
        var arguments = new object?[parameters.Length];
        for (var i = 0; i < parameters.Length; i++)
        {
            var name = parameters[i].Name;
            if (name is not null && context.ActionArguments.TryGetValue(name, out var value))
            {
                arguments[i] = value;
            }
            else
            {
                arguments[i] = parameters[i].HasDefaultValue ? parameters[i].DefaultValue : null;
            }
        }

        return arguments;
    }

    private static bool TryGetResultType(Type returnType, out Type resultType)
    {
        resultType = returnType;

        if (returnType == typeof(void) || returnType == typeof(Task) || returnType == typeof(ValueTask))
        {
            return false;
        }

        if (returnType.IsGenericType)
        {
            var def = returnType.GetGenericTypeDefinition();
            if (def == typeof(Task<>) || def == typeof(ValueTask<>))
            {
                resultType = returnType.GetGenericArguments()[0];
                return true;
            }
        }

        if (typeof(Task).IsAssignableFrom(returnType) || typeof(ValueTask).IsAssignableFrom(returnType))
        {
            return false;
        }

        return true;
    }
}
