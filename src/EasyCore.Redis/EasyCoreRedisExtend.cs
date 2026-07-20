using EasyCore.Redis.Distributed;
using EasyCore.Redis.Locking;
using EasyCore.Redis.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EasyCore.Redis
{
    /// <summary>
    /// Top-level DI extension methods that register Redis cache, lock, and service-cache features together.
    /// </summary>
    public static class EasyCoreRedisExtend
    {
        /// <summary>
        /// Registers Redis connection, distributed cache, distributed lock, and server-cache
        /// (auto-discovers types with <c>[ServerCache]</c>).
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="configure">Configures Redis connection options.</param>
        /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
        public static IServiceCollection AddEasyCoreRedis(
            this IServiceCollection services,
            Action<DistributedOption> configure)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configure);

            services.AddEasyCoreRedisDistributed(configure);
            services.AddEasyCoreRedisLock();
            services.AddEasyCoreRedisService();
            return services;
        }

        /// <summary>
        /// Registers all EasyCore.Redis features using a configuration section.
        /// Types marked with <c>[ServerCache]</c> are registered automatically.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="configuration">Configuration section containing Redis options.</param>
        /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
        public static IServiceCollection AddEasyCoreRedis(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            services.AddEasyCoreRedisDistributed(configuration);
            services.AddEasyCoreRedisLock();
            services.AddEasyCoreRedisService();
            return services;
        }
    }
}
