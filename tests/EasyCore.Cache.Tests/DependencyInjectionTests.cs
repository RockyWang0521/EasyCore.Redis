using Castle.DynamicProxy;
using EasyCore.Redis;
using EasyCore.Redis.Distributed;
using EasyCore.Redis.Distributed.Connection;
using EasyCore.Redis.Distributed.Transaction;
using EasyCore.Redis.Locking;
using EasyCore.Redis.Service;
using EasyCore.Redis.Service.Attribute;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EasyCore.Redis.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void EasyCoreRedis_RegistersCoreServices()
    {
        var services = new ServiceCollection();
        services.AddEasyCoreRedis(o =>
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
        services.AddEasyCoreRedisDistributed(o =>
        {
            o.EndPoints = new List<string> { "127.0.0.1:6379" };
        });
        services.AddServerCacheProxy<ISample, Sample>();

        Assert.Contains(services, d => d.ServiceType == typeof(ISample));
        Assert.Contains(services, d => d.ServiceType == typeof(Sample));
    }

    [Fact]
    public void IsJobStyleImplementation_Recognizes_Hangfire_And_Quartz_Markers()
    {
        Assert.True(ServerCacheExtend.IsJobStyleImplementation(typeof(SampleHangfireCacheJob)));
        Assert.False(ServerCacheExtend.IsJobStyleImplementation(typeof(Sample)));
    }

    /// <summary>
    /// Simulates JobWrapper&lt;T&gt; resolving concrete T while IEasyCoreHangfireJob is also registered.
    /// </summary>
    [Fact]
    public void Concrete_hangfire_job_style_type_is_proxied_when_resolved_as_T()
    {
        var services = new ServiceCollection();
        services.AddTransient<SampleHangfireCacheJob>();
        services.AddTransient<IEasyCoreHangfireJob>(sp => sp.GetRequiredService<SampleHangfireCacheJob>());
        services.AddEasyCoreRedisService(o => o.Assemblies.Add(typeof(SampleHangfireCacheJob).Assembly));

        using var provider = services.BuildServiceProvider();
        var job = provider.GetRequiredService<SampleHangfireCacheJob>();
        Assert.True(ProxyUtil.IsProxy(job));
    }

    public interface ISample
    {
        Task<string> Ping(string value);
    }

    public sealed class Sample : ISample
    {
        public Task<string> Ping(string value) => Task.FromResult(value);
    }

    // Name matters: ServerCacheExtend treats IEasyCoreHangfireJob as a framework job interface.
    public interface IEasyCoreHangfireJob
    {
        Task ExecuteAsync(CancellationToken cancellationToken = default);
    }

    [ServerCache]
    public class SampleHangfireCacheJob : IEasyCoreHangfireJob
    {
        public virtual Task ExecuteAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
