using AspectInjector.Broker;
using EasyCore.Ambient;
using EasyCore.Redis.Distributed;
using EasyCore.Redis.Service.Attribute;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace EasyCore.Redis.Service;

/// <summary>
/// AspectInjector aspect that applies <see cref="ServerCacheAttribute"/> cache-aside behavior.
/// </summary>
[Aspect(Scope.Global)]
public sealed class ServerCacheAspect
{
    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };

    /// <summary>
    /// Wraps the target with cache get/set.
    /// </summary>
    [Advice(Kind.Around, Targets = Target.Method)]
    public object? Handle(
        [Argument(Source.Instance)] object instance,
        [Argument(Source.Metadata)] MethodBase method,
        [Argument(Source.Target)] Func<object[], object> target,
        [Argument(Source.Arguments)] object[] args,
        [Argument(Source.ReturnType)] Type returnType,
        [Argument(Source.Triggers)] System.Attribute[] triggers)
    {
        if (instance is ControllerBase
            || (method.DeclaringType is not null && typeof(ControllerBase).IsAssignableFrom(method.DeclaringType)))
        {
            return target(args);
        }

        var attribute = triggers.OfType<ServerCacheAttribute>().FirstOrDefault();
        if (attribute is null && method is MethodInfo methodInfo)
        {
            attribute = ServerCacheAttributeLocator.Find(instance.GetType(), methodInfo, methodInfo);
        }

        if (attribute is null)
            return target(args);

        if (!typeof(Task).IsAssignableFrom(returnType))
            return target(args); // sync caching not used historically; keep pass-through

        var resultType = returnType.IsGenericType
            ? returnType.GenericTypeArguments[0]
            : typeof(object);

        return typeof(ServerCacheAspect)
            .GetMethod(nameof(ExecuteAsyncTyped), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(resultType)
            .Invoke(null, new object[] { method, args, target, attribute })!;
    }

    private static async Task<T> ExecuteAsyncTyped<T>(
        MethodBase method,
        object[] args,
        Func<object[], object> target,
        ServerCacheAttribute attribute)
    {
        var sp = EasyCoreSharedAmbient.Current
                 ?? throw new InvalidOperationException(
                     "EasyCore ambient IServiceProvider is not set. Call AddEasyCoreRedisService() and ensure the host has started.");

        var cache = sp.GetRequiredService<IDistributedCache>();
        var methodInfo = (MethodInfo)method;
        var cacheKey = BuildCacheKey(methodInfo, args);

        if (await cache.KeyExistsAsync(cacheKey).ConfigureAwait(false))
        {
            var cached = await cache.GetAsync(cacheKey).ConfigureAwait(false);
            if (cached is not null)
            {
                var hit = JsonConvert.DeserializeObject<T>(cached, JsonSettings);
                if (hit is not null || typeof(T).IsClass)
                    return hit!;
            }
        }

        var result = await InvokeAsync<T>(target, args).ConfigureAwait(false);

        if (result is not null || attribute.CacheNullValues)
        {
            var payload = JsonConvert.SerializeObject(result, JsonSettings);
            await cache.SetAsync(cacheKey, payload, attribute.CacheSeconds).ConfigureAwait(false);
        }

        return result!;
    }

    private static async Task<T> InvokeAsync<T>(Func<object[], object> target, object[] args)
    {
        var invoked = target(args);
        if (invoked is Task<T> typed)
            return await typed.ConfigureAwait(false);

        if (invoked is Task task)
        {
            await task.ConfigureAwait(false);
            return default!;
        }

        return invoked is T direct ? direct : default!;
    }

    internal static string BuildCacheKey(MethodInfo method, object?[] arguments)
    {
        var declaringType = method.DeclaringType?.FullName ?? "UnknownType";
        var argJson = JsonConvert.SerializeObject(arguments ?? Array.Empty<object?>(), JsonSettings);
        var raw = $"{declaringType}:{method.Name}:{argJson}";

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        var hashText = Convert.ToHexString(hash).ToLowerInvariant();
        return $"svc:{method.Name}:{hashText}";
    }
}
