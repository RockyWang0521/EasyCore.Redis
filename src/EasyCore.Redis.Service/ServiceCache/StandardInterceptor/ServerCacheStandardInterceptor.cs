using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Castle.DynamicProxy;
using EasyCore.Redis.Distributed;
using EasyCore.Redis.Service.Attribute;
using Newtonsoft.Json;

namespace EasyCore.Redis.Service.Interceptor;

/// <summary>
/// Cache-aside interceptor. Resolves <see cref="ServerCacheAttribute"/> from interface / class / method.
/// Cache keys include declaring type, method name, and argument values.
/// </summary>
public sealed class ServerCacheStandardInterceptor : AsyncInterceptorBase
{
    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };

    private readonly IDistributedCache _cache;

    /// <summary>
    /// Pipeline order when multiple <see cref="IAsyncInterceptor"/> are stacked (lower = outer).
    /// Default <c>100</c> — typically inside resilience wrappers.
    /// </summary>
    public int Order { get; set; } = 100;

    /// <summary>
    /// Creates an interceptor that stores results in <see cref="IDistributedCache"/>.
    /// </summary>
    public ServerCacheStandardInterceptor(IDistributedCache cache)
        => _cache = cache ?? throw new ArgumentNullException(nameof(cache));

    /// <inheritdoc />
    protected override Task InterceptAsync(
        IInvocation invocation,
        IInvocationProceedInfo proceedInfo,
        Func<IInvocation, IInvocationProceedInfo, Task> proceed)
        => proceed(invocation, proceedInfo);

    /// <inheritdoc />
    protected override async Task<TResult> InterceptAsync<TResult>(
        IInvocation invocation,
        IInvocationProceedInfo proceedInfo,
        Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
    {
        var method = invocation.MethodInvocationTarget ?? invocation.Method;
        var targetType = invocation.TargetType ?? method.DeclaringType ?? typeof(object);
        var attribute = ServerCacheAttributeLocator.Find(targetType, method, invocation.Method);

        if (attribute is null)
        {
            return await proceed(invocation, proceedInfo).ConfigureAwait(false);
        }

        var cacheKey = BuildCacheKey(method, invocation.Arguments);

        if (await _cache.KeyExistsAsync(cacheKey).ConfigureAwait(false))
        {
            var cached = await _cache.GetAsync(cacheKey).ConfigureAwait(false);
            if (cached is not null)
            {
                var hit = JsonConvert.DeserializeObject<TResult>(cached, JsonSettings);
                if (hit is not null || typeof(TResult).IsClass)
                {
                    return hit!;
                }
            }
        }

        var result = await proceed(invocation, proceedInfo).ConfigureAwait(false);

        if (result is not null || attribute.CacheNullValues)
        {
            var payload = JsonConvert.SerializeObject(result, JsonSettings);
            await _cache.SetAsync(cacheKey, payload, attribute.CacheSeconds).ConfigureAwait(false);
        }

        return result!;
    }

    /// <summary>
    /// Builds a stable cache key from method identity and serialized arguments.
    /// </summary>
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
