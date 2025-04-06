using EasyCore.DistributedCache;
using EasyCore.DistributedLocking;
using EasyCore.ServiceCache;
using Microsoft.Extensions.DependencyInjection;

namespace EasyCore.Cache
{
    public static class EasyCoreCacheExtend
    {
        public static void EasyCoreCache(this IServiceCollection service, Action<DistributedOption> action)
        {
            var cacheOptions = new DistributedOption();

            action(cacheOptions);

            service.EasyCoreDistributedCache(options =>
            {
                options.AbortOnConnectFail   = cacheOptions.AbortOnConnectFail;
                options.ConnectTimeout      = cacheOptions.ConnectTimeout;
                options.DefaultDatabase     = cacheOptions.DefaultDatabase;
                options.DistributedName = cacheOptions.DistributedName;
                options.EndPoints           = cacheOptions.EndPoints;
                options.Password = cacheOptions.Password;
                options.SyncTimeout         = cacheOptions.SyncTimeout;
                options.User            = cacheOptions.User;
            });

            service.EasyCoreDistributedLock(options =>
            {
                options.AbortOnConnectFail = cacheOptions.AbortOnConnectFail;
                options.ConnectTimeout = cacheOptions.ConnectTimeout;
                options.DefaultDatabase = cacheOptions.DefaultDatabase;
                options.DistributedName = cacheOptions.DistributedName;
                options.EndPoints = cacheOptions.EndPoints;
                options.Password = cacheOptions.Password;
                options.SyncTimeout = cacheOptions.SyncTimeout;
                options.User = cacheOptions.User;
            });

            service.EasyCoreServerCache();
        }
    }
}
