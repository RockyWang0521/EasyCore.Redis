using EasyCore.Redis.Distributed;
using EasyCore.Redis.Distributed.Connection;
using EasyCore.Redis.Locking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EasyCore.Redis.Locking
{
    /// <summary>
    /// DI extension methods for registering Redis distributed lock services.
    /// </summary>
    public static class DistributedLockExtend
    {
        /// <summary>
        /// Registers distributed lock. Requires <see cref="IRedisConnection"/>
        /// (register via EasyCoreRedisDistributed or EasyCoreRedis).
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
        public static IServiceCollection AddEasyCoreRedisLock(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.TryAddSingleton<IDistributedLock>(sp =>
            {
                var connection = sp.GetRequiredService<IRedisConnection>();
                var logger = sp.GetService<ILogger<DistributedLock>>() ?? NullLogger<DistributedLock>.Instance;
                return new DistributedLock(connection, logger);
            });

            return services;
        }

        /// <summary>
        /// Registers shared Redis connection (if missing) and distributed lock.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="configure">Configures <see cref="DistributedOption"/>.</param>
        /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
        public static IServiceCollection AddEasyCoreRedisLock(
            this IServiceCollection services,
            Action<DistributedOption> configure)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configure);

            services.AddOptions();
            services.Configure(configure);
            services.TryAddSingleton<IRedisConnection, RedisConnection>();

            return services.AddEasyCoreRedisLock();
        }
    }
}
