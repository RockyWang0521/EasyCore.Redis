using Castle.DynamicProxy;
using EasyCore.ServiceCache.ServiceCache.CacheAttribute;
using EasyCore.ServiceCache.ServiceCache.StandardInterceptor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace EasyCore.ServiceCache
{
    public static class ServerCacheExtend
    {
        public static void EasyCoreServerCache(this IServiceCollection service)
        {
            service.AddScoped<IAsyncInterceptor, ServerCacheStandardInterceptor>();

            service.TryAddSingleton<ProxyGenerator>();

            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;

            string[] dllFiles = Directory.GetFiles(rootDirectory, "*.dll", SearchOption.TopDirectoryOnly).Where(path =>
            {
                string fileName = Path.GetFileName(path);
                return !(fileName.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase) || fileName.StartsWith("System.", StringComparison.OrdinalIgnoreCase));
            }).ToArray();

            List<Type> types = new List<Type>();

            foreach (var dll in dllFiles)
            {
                Assembly assembly = Assembly.LoadFrom(dll);

                foreach (var type in assembly.GetTypes())
                {
                    var methodsWithPollyConfigAttribute = type.GetMethods().Where(m => m.GetCustomAttributes(typeof(ServerCacheAttribute), false).Any()).ToList();

                    if (methodsWithPollyConfigAttribute.Count > 0) types.Add(type);
                }
            }

            if (types.Count <= 0) return;

            foreach (var dll in dllFiles)
            {
                Assembly assembly = Assembly.LoadFrom(dll);

                foreach (var type in types)
                {
                    service.AddTransient(type);

                    var interfaceType = assembly.GetTypes().FirstOrDefault(t => t.IsInterface && t.Name == $"I{type.Name}");

                    if (interfaceType is null) continue;

                    if (interfaceType != null)
                    {
                        service.AddTransient(interfaceType, serviceProvider =>
                        {
                            var generator = serviceProvider.GetRequiredService<ProxyGenerator>();

                            var instance = serviceProvider.GetRequiredService(type);

                            var interceptors = serviceProvider.GetServices<IAsyncInterceptor>().ToArray();

                            return generator.CreateInterfaceProxyWithTarget(interfaceType, instance, interceptors);
                        });
                    }
                }
            }
        }
    }
}
