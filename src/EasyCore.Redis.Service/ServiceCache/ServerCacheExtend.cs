using Castle.DynamicProxy;
using EasyCore.Redis.Service.Attribute;
using EasyCore.Redis.Service.Interceptor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace EasyCore.Redis.Service
{
    /// <summary>
    /// Options for service-level (AOP) caching registration.
    /// </summary>
    public sealed class ServerCacheOptions
    {
        /// <summary>
        /// Extra assemblies to scan for types that have methods marked with <see cref="ServerCacheAttribute"/>.
        /// When empty, the entry assembly and other non-framework loaded assemblies are scanned automatically.
        /// </summary>
        public List<Assembly> Assemblies { get; } = new();
    }

    /// <summary>
    /// DI extension methods for registering service-level (AOP) Redis caching via Castle DynamicProxy.
    /// Types with <see cref="ServerCacheAttribute"/> are discovered and registered automatically.
    /// </summary>
    public static class ServerCacheExtend
    {
        /// <summary>
        /// Registers the cache interceptor and auto-registers services whose methods are marked with <see cref="ServerCacheAttribute"/>.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="configure">Optional extra assemblies to include in the scan.</param>
        /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
        public static IServiceCollection EasyCoreRedisService(
            this IServiceCollection services,
            Action<ServerCacheOptions>? configure = null)
        {
            ArgumentNullException.ThrowIfNull(services);

            var options = new ServerCacheOptions();
            configure?.Invoke(options);

            services.TryAddSingleton<ProxyGenerator>();
            services.TryAddSingleton<IAsyncInterceptor, ServerCacheStandardInterceptor>();

            var assemblies = options.Assemblies.Count > 0
                ? options.Assemblies.Distinct()
                : GetAutoScanAssemblies();

            var serviceTypes = assemblies
                .SelectMany(SafeGetTypes)
                .Where(t => t is { IsClass: true, IsAbstract: false })
                .Where(HasServerCacheMethod)
                .Distinct()
                .ToList();

            foreach (var implementation in serviceTypes)
            {
                var interfaceType = implementation.GetInterfaces()
                    .FirstOrDefault(i => i.Name == $"I{implementation.Name}" && i.IsAssignableFrom(implementation));

                if (interfaceType is null)
                    continue;

                services.TryAddTransient(implementation);
                services.TryAddTransient(interfaceType, sp =>
                {
                    var generator = sp.GetRequiredService<ProxyGenerator>();
                    var instance = sp.GetRequiredService(implementation);
                    var interceptors = sp.GetServices<IAsyncInterceptor>().ToArray();
                    return generator.CreateInterfaceProxyWithTarget(interfaceType, instance, interceptors);
                });
            }

            return services;
        }

        /// <summary>
        /// Registers a single interface/implementation pair with server-cache proxying.
        /// </summary>
        /// <typeparam name="TInterface">Service interface type.</typeparam>
        /// <typeparam name="TImplementation">Concrete implementation type.</typeparam>
        /// <param name="services">Service collection.</param>
        /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
        public static IServiceCollection AddServerCacheProxy<TInterface, TImplementation>(this IServiceCollection services)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            ArgumentNullException.ThrowIfNull(services);

            services.TryAddSingleton<ProxyGenerator>();
            services.TryAddSingleton<IAsyncInterceptor, ServerCacheStandardInterceptor>();
            services.TryAddTransient<TImplementation>();
            services.TryAddTransient<TInterface>(sp =>
            {
                var generator = sp.GetRequiredService<ProxyGenerator>();
                var instance = sp.GetRequiredService<TImplementation>();
                var interceptors = sp.GetServices<IAsyncInterceptor>().ToArray();
                return generator.CreateInterfaceProxyWithTarget<TInterface>(instance, interceptors);
            });

            return services;
        }

        /// <summary>
        /// Resolves assemblies to scan: entry assembly (+ its references) and other non-framework loaded assemblies.
        /// </summary>
        private static IEnumerable<Assembly> GetAutoScanAssemblies()
        {
            var result = new HashSet<Assembly>();

            void TryAdd(Assembly? assembly)
            {
                if (assembly is null || assembly.IsDynamic)
                    return;
                if (IsFrameworkOrInfrastructure(assembly))
                    return;
                result.Add(assembly);
            }

            var entry = Assembly.GetEntryAssembly();
            TryAdd(entry);

            if (entry is not null)
            {
                foreach (var reference in entry.GetReferencedAssemblies())
                {
                    try
                    {
                        TryAdd(Assembly.Load(reference));
                    }
                    catch (Exception)
                    {
                        // Ignore unloadable references.
                    }
                }
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                TryAdd(assembly);

            return result;
        }

        private static bool IsFrameworkOrInfrastructure(Assembly assembly)
        {
            var name = assembly.GetName().Name ?? string.Empty;
            return name.StartsWith("System", StringComparison.OrdinalIgnoreCase)
                   || name.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase)
                   || name.StartsWith("netstandard", StringComparison.OrdinalIgnoreCase)
                   || name.StartsWith("Castle.", StringComparison.OrdinalIgnoreCase)
                   || name.StartsWith("Newtonsoft.", StringComparison.OrdinalIgnoreCase)
                   || name.StartsWith("StackExchange.", StringComparison.OrdinalIgnoreCase)
                   || name.StartsWith("Swashbuckle.", StringComparison.OrdinalIgnoreCase)
                   || name.StartsWith("EasyCore.Redis", StringComparison.OrdinalIgnoreCase);
        }

        private static bool HasServerCacheMethod(Type type)
            => type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Any(m => m.IsDefined(typeof(ServerCacheAttribute), inherit: true));

        private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t is not null)!;
            }
        }
    }
}
