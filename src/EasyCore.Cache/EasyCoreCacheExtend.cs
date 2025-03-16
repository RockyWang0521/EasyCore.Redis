using EasyCore.DistributedCache;
using EasyCore.DistributedLocking;
using EasyCore.CacheOptions;
using EasyCore.ServiceCache;
using Microsoft.Extensions.DependencyInjection;

namespace EasyCore.Cache
{
    public static class EasyCoreCacheExtend
    {
        public static void EasyCoreCache(this IServiceCollection service, Action<DistributedOption> action)
        {
            service.EasyCoreDistributedCache(action);

            service.EasyCoreDistributedLock(action);

            service.EasyCoreServerCache();
        }
    }
}
