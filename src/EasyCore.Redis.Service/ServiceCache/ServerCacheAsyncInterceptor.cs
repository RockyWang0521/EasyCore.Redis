using Castle.DynamicProxy;
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
/// Castle DynamicProxy interceptor that applies <see cref="ServerCacheAttribute"/> cache-aside behavior.
/// </summary>
public sealed class ServerCacheAsyncInterceptor : IAsyncInterceptor
{
    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };

    private readonly IServiceProvider _services;

    public ServerCacheAsyncInterceptor(IServiceProvider services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    public void InterceptSynchronous(IInvocation invocation)
    {
        var returnType = (invocation.MethodInvocationTarget ?? invocation.Method).ReturnType;
        if (returnType == typeof(ValueTask))
        {
            invocation.ReturnValue = new ValueTask(InterceptAsync(invocation));
            return;
        }

        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
        {
            var resultType = returnType.GenericTypeArguments[0];
            var generic = typeof(ServerCacheAsyncInterceptor)
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .First(m => m.Name == nameof(InterceptAsync) && m.IsGenericMethodDefinition)
                .MakeGenericMethod(resultType);
            var task = generic.Invoke(this, new object[] { invocation })!;
            invocation.ReturnValue = Activator.CreateInstance(returnType, task);
            return;
        }

        if (ShouldSkip(invocation))
        {
            invocation.Proceed();
            return;
        }

        var attribute = FindAttribute(invocation);
        if (attribute is null || returnType == typeof(void))
        {
            invocation.Proceed();
            return;
        }

        // Sync T: reuse async cache path (Redis APIs are async) via GetResult.
        var syncMethod = typeof(ServerCacheAsyncInterceptor)
            .GetMethod(nameof(InterceptSync), BindingFlags.Instance | BindingFlags.NonPublic)!
            .MakeGenericMethod(returnType);
        invocation.ReturnValue = syncMethod.Invoke(this, new object[] { invocation });
    }

    private TResult InterceptSync<TResult>(IInvocation invocation)
        => InterceptAsync<TResult>(invocation).ConfigureAwait(false).GetAwaiter().GetResult();

    public void InterceptAsynchronous(IInvocation invocation)
    {
        invocation.ReturnValue = InterceptAsync(invocation);
    }

    public void InterceptAsynchronous<TResult>(IInvocation invocation)
    {
        invocation.ReturnValue = InterceptAsync<TResult>(invocation);
    }

    private async Task InterceptAsync(IInvocation invocation)
    {
        await InterceptAsync<object>(invocation).ConfigureAwait(false);
    }

    private async Task<TResult> InterceptAsync<TResult>(IInvocation invocation)
    {
        if (ShouldSkip(invocation))
        {
            invocation.Proceed();
            return await UnpackAsync<TResult>(invocation.ReturnValue).ConfigureAwait(false);
        }

        var attribute = FindAttribute(invocation);
        if (attribute is null)
        {
            invocation.Proceed();
            return await UnpackAsync<TResult>(invocation.ReturnValue).ConfigureAwait(false);
        }

        var method = invocation.MethodInvocationTarget ?? invocation.Method;
        var cache = _services.GetRequiredService<IDistributedCache>();
        var cacheKey = BuildCacheKey(method, invocation.Arguments);

        if (await cache.KeyExistsAsync(cacheKey).ConfigureAwait(false))
        {
            var cached = await cache.GetAsync(cacheKey).ConfigureAwait(false);
            if (cached is not null)
            {
                var hit = JsonConvert.DeserializeObject<TResult>(cached, JsonSettings);
                if (hit is not null || typeof(TResult).IsClass)
                    return hit!;
            }
        }

        invocation.Proceed();
        var result = await UnpackAsync<TResult>(invocation.ReturnValue).ConfigureAwait(false);

        if (result is not null || attribute.CacheNullValues)
        {
            var payload = JsonConvert.SerializeObject(result, JsonSettings);
            await cache.SetAsync(cacheKey, payload, attribute.CacheSeconds).ConfigureAwait(false);
        }

        return result!;
    }

    private static bool ShouldSkip(IInvocation invocation)
    {
        var target = invocation.InvocationTarget ?? invocation.Proxy;
        if (target is ControllerBase)
            return true;

        var declaring = (invocation.MethodInvocationTarget ?? invocation.Method).DeclaringType;
        return declaring is not null && typeof(ControllerBase).IsAssignableFrom(declaring);
    }

    private static ServerCacheAttribute? FindAttribute(IInvocation invocation)
    {
        var targetType = invocation.TargetType
                         ?? invocation.InvocationTarget?.GetType()
                         ?? invocation.Proxy.GetType();
        var method = invocation.MethodInvocationTarget ?? invocation.Method;
        return ServerCacheAttributeLocator.Find(targetType, method, invocation.Method);
    }

    private static async Task<TResult> UnpackAsync<TResult>(object? invoked)
    {
        if (invoked is Task<TResult> typed)
            return await typed.ConfigureAwait(false);

        if (invoked is Task task)
        {
            await task.ConfigureAwait(false);
            return default!;
        }

        if (invoked is ValueTask<TResult> valueTaskTyped)
            return await valueTaskTyped.ConfigureAwait(false);

        if (invoked is ValueTask valueTask)
        {
            await valueTask.ConfigureAwait(false);
            return default!;
        }

        return invoked is TResult direct ? direct : default!;
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
