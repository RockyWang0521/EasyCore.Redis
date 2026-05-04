using EasyCore.Redis;
using EasyCore.Redis.Distributed;
using EasyCore.Redis.Distributed.Connection;
using EasyCore.Redis.Distributed.Transaction;
using EasyCore.Redis.Locking;
using EasyCore.Redis.Service;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EasyCore.Redis.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void EasyCoreRedis_RegistersCoreServices()
    {
        var services = new ServiceCollection();
        services.EasyCoreRedis(o =>
        {
            o.EndPoints = new List<string> { "127.0.0.1:6379" };
            o.DistributedName = "Tests";
        });

        Assert.Contains(services, d => d.ServiceType == typeof(IRedisConnection));
        Assert.Contains(services, d => d.ServiceType == typeof(IDistributedCache));
        Assert.Contains(services, d => d.ServiceType == typeof(IDistributedTransaction));
        Assert.Contains(services, d => d.ServiceType == typeof(IDistributedLock));
    }

    [Fact]
    public void AddServerCacheProxy_RegistersInterface()
    {
        var services = new ServiceCollection();
        services.EasyCoreRedisDistributed(o =>
        {
            o.EndPoints = new List<string> { "127.0.0.1:6379" };
        });
        services.AddServerCacheProxy<ISample, Sample>();

        Assert.Contains(services, d => d.ServiceType == typeof(ISample));
        Assert.Contains(services, d => d.ServiceType == typeof(Sample));
    }

    public interface ISample
    {
        Task<string> Ping(string value);
    }

    public sealed class Sample : ISample
    {
        public Task<string> Ping(string value) => Task.FromResult(value);
    }
}
