using EasyCore.Redis.Service.Attribute;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace EasyCore.Redis.Service;

/// <summary>
/// Options for service-level (AOP) caching registration.
/// </summary>
public sealed class ServerCacheOptions
{
    /// <summary>
    /// Extra assemblies to scan for types decorated with <see cref="ServerCacheAttribute"/>
    /// for DI registration and Castle proxy wrapping. When empty, candidate loaded assemblies are scanned.
    /// </summary>
    public List<Assembly> Assemblies { get; } = new();
}

/// <summary>
/// DI extension methods for <see cref="ServerCacheAttribute"/> via Castle DynamicProxy.
/// </summary>
public static class ServerCacheExtend
{
    /// <summary>
    /// Registers MVC convention, discovers instrumented service types, and applies Castle proxies.
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
            services.TryAddTransient(implementation);
            foreach (var interfaceType in interfaces)
            {
                services.TryAddTransient(interfaceType, implementation);
            }
        }

        ServerCacheCastleProxyApplier.Apply(services);
        return services;
    }

    /// <summary>
    /// Registers a single interface/implementation pair and applies Castle proxies.
    /// </summary>
    public static IServiceCollection AddServerCacheProxy<TInterface, TImplementation>(this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        ArgumentNullException.ThrowIfNull(services);

        RegisterCore(services);
        services.TryAddTransient<TImplementation>();
        services.TryAddTransient<TInterface, TImplementation>();
        ServerCacheCastleProxyApplier.Apply(services);
        return services;
    }

    private static void RegisterCore(IServiceCollection services)
    {
        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IConfigureOptions<MvcOptions>, ServerCacheMvcOptionsSetup>());
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
        if (ns.StartsWith("System", StringComparison.Ordinal)
            || ns.StartsWith("Microsoft", StringComparison.Ordinal)
            || ns.StartsWith("Castle", StringComparison.Ordinal))
        {
            return true;
        }

        // Quartz / Hangfire job marker interfaces.
        // EasyCore.Quartz / EasyCore.Hangfire JobWrapper<T> resolve the concrete job type T from DI,
        // so these interfaces must not become the preferred Castle proxy service registration.
        if (IsJobStyleInterfaceName(type.Name))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns <c>true</c> when <paramref name="implementation"/> is a Quartz/Hangfire-style job
    /// that consumers resolve as the concrete type (e.g. <c>JobWrapper&lt;T&gt;(T inner)</c>).
    /// </summary>
    internal static bool IsJobStyleImplementation(Type implementation)
    {
        ArgumentNullException.ThrowIfNull(implementation);
        return implementation.GetInterfaces().Any(i => IsJobStyleInterfaceName(i.Name));
    }

    /// <summary>
    /// Job interface type names recognized without taking a package reference on Quartz/Hangfire.
    /// </summary>
    private static bool IsJobStyleInterfaceName(string name)
        => name is "IJob" or "IEasyCoreJob" or "IEasyCoreHangfireJob";

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
