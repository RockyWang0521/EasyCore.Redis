using EasyCore.Redis.Distributed;
using EasyCore.Redis.Distributed.Connection;
using EasyCore.Redis.Distributed.Transaction;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EasyCore.Redis.Distributed
{
    /// <summary>
    /// DI extension methods for registering Redis distributed cache and transaction services.
    /// </summary>
    public static class DistributedCacheExtend
    {
        /// <summary>
        /// Registers shared Redis connection, distributed cache, and transaction factory using a configure callback.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="configure">Configures <see cref="DistributedOption"/> (endpoints, credentials, prefix, etc.).</param>
        /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
        public static IServiceCollection EasyCoreRedisDistributed(
            this IServiceCollection services,
            Action<DistributedOption> configure)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configure);

            services.AddOptions();
            services.Configure(configure);
            RegisterCore(services);
            return services;
        }

        /// <summary>
        /// Binds <see cref="DistributedOption"/> from configuration (e.g. an appsettings.json section)
        /// and registers shared Redis connection, distributed cache, and transaction factory.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="configuration">Configuration section containing <see cref="DistributedOption"/> values.</param>
        /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
        public static IServiceCollection EasyCoreRedisDistributed(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            services.AddOptions<DistributedOption>()
                .Bind(configuration)
                .Validate(o => o.EndPoints.Count > 0, "DistributedOption.EndPoints must contain at least one endpoint.")
                .ValidateOnStart();

            RegisterCore(services);
            return services;
        }

        private static void RegisterCore(IServiceCollection services)
        {
            services.TryAddSingleton<IRedisConnection, RedisConnection>();
            services.TryAddSingleton<IDistributedCache, DistributedCache>();
            services.TryAddSingleton<IDistributedTransaction, DistributedTransaction>();
        }
    }
}
