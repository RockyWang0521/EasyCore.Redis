using Castle.DynamicProxy;
using EasyCore.DistributedCache.Cache;
using EasyCore.ServiceCache.ServiceCache.CacheAttribute;
using Newtonsoft.Json;
using System.Reflection;

namespace EasyCore.ServiceCache.ServiceCache.StandardInterceptor
{
    public class ServerCacheStandardInterceptor : AsyncInterceptorBase
    {
        private readonly IDistributedCache _cache;

        public ServerCacheStandardInterceptor(IDistributedCache cache) =>
            _cache = cache;

        protected async override Task InterceptAsync(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task> proceed)
        {
            await proceed.Invoke(invocation, proceedInfo);
        }

        protected async override Task<TResult> InterceptAsync<TResult>(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
        {
            TResult? ret = default;

            var method = invocation.MethodInvocationTarget ?? invocation.Method;

            if (method.IsDefined(typeof(ServerCacheAttribute), true))
            {
                var attribute = method.GetCustomAttribute<ServerCacheAttribute>();

                var seconds = attribute!.CacheSeconds;

                var methodName = method.Name;

                var parameterNames = method.GetParameters().Select(p => $"{p.ParameterType.Name}-{p.Name}");

                var parameterNamesInfo = string.Join(", ", parameterNames);

                var key = $"{methodName}-{parameterNamesInfo}";

                if (await _cache.KeyExistsAsync(key))
                {
                    var value = await _cache.GetAsync(key);

                    ret = (TResult)JsonConvert.DeserializeObject(value!, typeof(TResult))!;

                    return ret!;
                }

                ret = await proceed.Invoke(invocation, proceedInfo);

                if (ret != null)
                {
                    var cacheret = JsonConvert.SerializeObject(ret);

                    await _cache.SetAsync(key, cacheret, seconds);
                }
            }
            else ret = await proceed.Invoke(invocation, proceedInfo);

            return ret!;
        }
    }
}
