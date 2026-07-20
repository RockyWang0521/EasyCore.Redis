using System.Reflection;
using Castle.DynamicProxy;
using EasyCore.Redis.Service.Attribute;
using EasyCore.Redis.Service.Interceptor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace EasyCore.Redis.Service;

/// <summary>
/// Options for service-level (AOP) caching registration.
/// </summary>
public sealed class ServerCacheOptions
{
    /// <summary>
    /// Extra assemblies to scan for types decorated with <see cref="ServerCacheAttribute"/>
    /// (interface / class / method). When empty, candidate loaded assemblies are scanned.
    /// </summary>
    public List<Assembly> Assemblies { get; } = new();
}

/// <summary>
/// DI extension methods for <see cref="ServerCacheAttribute"/> via Castle DynamicProxy.
/// Placement: interface / class / method / MVC Controller / EasyCoreAppService.
/// Stacks with other packages' <see cref="IAsyncInterceptor"/> registrations without hardcoding them.
/// </summary>
public static class ServerCacheExtend
{
    /// <summary>
    /// Registers the cache interceptor and auto-registers instrumented services.
    /// </summary>
    public static IServiceCollection AddEasyCoreRedisService(
        this IServiceCollection services,
        Action<ServerCacheOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new ServerCacheOptions();
        configure?.Invoke(options);

        RegisterCore(services);

        var assemblies = options.Assemblies.Count > 0
            ? options.Assemblies.Distinct()
            : GetAutoScanAssemblies();

        var serviceTypes = assemblies
            .SelectMany(SafeGetTypes)
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .Where(t => !typeof(ControllerBase).IsAssignableFrom(t))
            .Where(ServerCacheAttributeLocator.IsInstrumented)
            .Distinct()
            .ToList();

        foreach (var implementation in serviceTypes)
        {
            var interfaces = FindRegisterableInterfaces(implementation);
            if (interfaces.Count == 0)
            {
                continue;
            }

            services.TryAddTransient(implementation);
            foreach (var interfaceType in interfaces)
            {
                services.TryAddTransient(interfaceType, sp => CreateProxy(sp, interfaceType, implementation));
            }
        }

        return services;
    }

    /// <summary>
    /// Registers a single interface/implementation pair with server-cache proxying.
    /// </summary>
    public static IServiceCollection AddServerCacheProxy<TInterface, TImplementation>(this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        ArgumentNullException.ThrowIfNull(services);

        RegisterCore(services);
        services.TryAddTransient<TImplementation>();
        services.TryAddTransient<TInterface>(sp =>
            (TInterface)CreateProxy(sp, typeof(TInterface), typeof(TImplementation)));
        return services;
    }

    private static void RegisterCore(IServiceCollection services)
    {
        services.TryAddSingleton<ProxyGenerator>();
        services.TryAddSingleton<ServerCacheStandardInterceptor>();
        // Typed implementation so multiple packages can each TryAddEnumerable without colliding.
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IAsyncInterceptor, ServerCacheStandardInterceptor>());
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IConfigureOptions<MvcOptions>, ServerCacheMvcOptionsSetup>());
    }

    private static object CreateProxy(IServiceProvider sp, Type interfaceType, Type implementationType)
    {
        var generator = sp.GetRequiredService<ProxyGenerator>();
        var instance = sp.GetRequiredService(implementationType);
        var interceptors = InterceptorOrdering.Order(sp.GetServices<IAsyncInterceptor>());
        return generator.CreateInterfaceProxyWithTarget(interfaceType, instance, interceptors);
    }

    private static IReadOnlyList<Type> FindRegisterableInterfaces(Type implementation)
    {
        var preferred = implementation.GetInterfaces()
            .FirstOrDefault(i => i.Name == $"I{implementation.Name}" && i.IsAssignableFrom(implementation));

        var all = implementation.GetInterfaces()
            .Where(i => !IsFrameworkInterface(i))
            .Distinct()
            .ToList();

        if (preferred is not null)
        {
            return all.Count > 0 ? all : new[] { preferred };
        }

        return all;
    }

    private static bool IsFrameworkInterface(Type type)
    {
        if (type.IsGenericType && !type.IsGenericTypeDefinition)
        {
            type = type.GetGenericTypeDefinition();
        }

        var ns = type.Namespace ?? string.Empty;
        return ns.StartsWith("System", StringComparison.Ordinal)
               || ns.StartsWith("Microsoft", StringComparison.Ordinal)
               || ns.StartsWith("Castle", StringComparison.Ordinal);
    }

    private static IEnumerable<Assembly> GetAutoScanAssemblies()
    {
        var result = new HashSet<Assembly>();

        void TryAdd(Assembly? assembly)
        {
            if (assembly is null || assembly.IsDynamic)
            {
                return;
            }

            if (IsFrameworkOrInfrastructure(assembly))
            {
                return;
            }

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
        {
            TryAdd(assembly);
        }

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

/// <summary>
/// Orders stacked <see cref="IAsyncInterceptor"/> by a public <c>Order</c> property when present
/// (lower = outer). Packages stay independent — no shared interface required.
/// </summary>
internal static class InterceptorOrdering
{
    public static IAsyncInterceptor[] Order(IEnumerable<IAsyncInterceptor> interceptors)
        => interceptors
            .OrderBy(GetOrder)
            .ThenBy(i => i.GetType().FullName, StringComparer.Ordinal)
            .ToArray();

    private static int GetOrder(IAsyncInterceptor interceptor)
    {
        var prop = interceptor.GetType().GetProperty(
            "Order",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (prop?.PropertyType == typeof(int) && prop.GetValue(interceptor) is int order)
        {
            return order;
        }

        return 0;
    }
}
