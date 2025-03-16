using EasyCore.DistributedCache.Cache;
using EasyCore.DistributedCache.Transaction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EasyCore.DistributedCache
{
    public static class DistributedCacheExtend
    {
        public static void EasyCoreDistributedCache(this IServiceCollection service, Action<DistributedCacheOption> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            service.AddOptions();

            service.Configure(action);

            service.TryAddSingleton<IDistributedCache, Cache.DistributedCache>();

            service.TryAddTransient<IDistributedTransaction, DistributedTransaction>();
        }
    }
}
