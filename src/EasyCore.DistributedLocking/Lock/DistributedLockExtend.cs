using EasyCore.DistributedLocking.Lock;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EasyCore.DistributedLocking
{
    public static class DistributedLockExtend
    {
        public static void EasyCoreDistributedLock(this IServiceCollection service, Action<DistributedLockOption> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            service.AddOptions();

            service.Configure(action);

            service.TryAddSingleton<IDistributedLock, DistributedLock>();
        }
    }
}
