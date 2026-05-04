using System.Security.Cryptography;
using System.Text;
using Castle.DynamicProxy;
using EasyCore.Redis.Distributed;
using EasyCore.Redis.Service.Attribute;
using Newtonsoft.Json;
using System.Reflection;

namespace EasyCore.Redis.Service.Interceptor
{
    /// <summary>
    /// Cache-aside interceptor. Cache keys include declaring type, method name, and argument values.
    /// Methods without <see cref="ServerCacheAttribute"/> proceed without caching.
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
        /// Creates an interceptor that stores results in <see cref="IDistributedCache"/>.
        /// </summary>
        /// <param name="cache">Distributed cache used for cache-aside reads/writes.</param>
        public ServerCacheStandardInterceptor(IDistributedCache cache)
            => _cache = cache ?? throw new ArgumentNullException(nameof(cache));

        /// <summary>
        /// Non-generic async methods are not cached; the call proceeds normally.
        /// </summary>
        /// <param name="invocation">Proxy invocation.</param>
        /// <param name="proceedInfo">Proceed metadata.</param>
        /// <param name="proceed">Continuation that invokes the target.</param>
        /// <returns>A task that completes when the target method completes.</returns>
        protected override Task InterceptAsync(
            IInvocation invocation,
            IInvocationProceedInfo proceedInfo,
            Func<IInvocation, IInvocationProceedInfo, Task> proceed)
            => proceed(invocation, proceedInfo);

        /// <summary>
        /// Cache-aside for methods returning <typeparamref name="TResult"/> and marked with <see cref="ServerCacheAttribute"/>.
        /// </summary>
        /// <typeparam name="TResult">Method return type.</typeparam>
        /// <param name="invocation">Proxy invocation.</param>
        /// <param name="proceedInfo">Proceed metadata.</param>
        /// <param name="proceed">Continuation that invokes the target.</param>
        /// <returns>Cached or freshly computed result.</returns>
        protected override async Task<TResult> InterceptAsync<TResult>(
            IInvocation invocation,
            IInvocationProceedInfo proceedInfo,
            Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
        {
            var method = invocation.MethodInvocationTarget ?? invocation.Method;
            var attribute = method.GetCustomAttribute<ServerCacheAttribute>(inherit: true);
            if (attribute is null)
                return await proceed(invocation, proceedInfo).ConfigureAwait(false);

            var cacheKey = BuildCacheKey(method, invocation.Arguments);

            if (await _cache.KeyExistsAsync(cacheKey).ConfigureAwait(false))
            {
                var cached = await _cache.GetAsync(cacheKey).ConfigureAwait(false);
                if (cached is not null)
                {
                    var hit = JsonConvert.DeserializeObject<TResult>(cached, JsonSettings);
                    if (hit is not null || typeof(TResult).IsClass)
                        return hit!;
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
        /// <param name="method">Target method.</param>
        /// <param name="arguments">Invocation arguments.</param>
        /// <returns>Cache key in the form <c>svc:{methodName}:{sha256}</c>.</returns>
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
}
