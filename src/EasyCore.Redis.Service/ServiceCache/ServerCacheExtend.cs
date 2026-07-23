using EasyCore.Ambient;
using EasyCore.Redis.Service.Attribute;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
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
    /// for plain DI registration (no proxies). When empty, candidate loaded assemblies are scanned.
    /// </summary>
    public List<Assembly> Assemblies { get; } = new();
}

/// <summary>
/// DI extension methods for <see cref="ServerCacheAttribute"/> via AspectInjector weave.
/// </summary>
public static class ServerCacheExtend
{
    /// <summary>
    /// Registers ambient DI + MVC convention and optionally registers instrumented service types.
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

        return services;
    }

    /// <summary>
    /// Registers a single interface/implementation pair (weaving applies on the implementation).
    /// </summary>
    public static IServiceCollection AddServerCacheProxy<TInterface, TImplementation>(this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        ArgumentNullException.ThrowIfNull(services);

        RegisterCore(services);
        services.TryAddTransient<TImplementation>();
        services.TryAddTransient<TInterface, TImplementation>();
        return services;
    }

    private static void RegisterCore(IServiceCollection services)
    {
        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IHostedService, EasyCoreAmbientHostedService>());
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IStartupFilter, EasyCoreAmbientStartupFilter>());
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

internal sealed class EasyCoreAmbientHostedService : IHostedService
{
    private readonly IServiceProvider _root;

    public EasyCoreAmbientHostedService(IServiceProvider root) => _root = root;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        EasyCoreSharedAmbient.SetRoot(_root);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

internal sealed class EasyCoreAmbientStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            app.Use(async (context, nextMiddleware) =>
            {
                EasyCoreSharedAmbient.SetCurrent(context.RequestServices);
                try
                {
                    await nextMiddleware().ConfigureAwait(false);
                }
                finally
                {
                    EasyCoreSharedAmbient.ClearCurrent();
                }
            });
            next(app);
        };
    }
}
