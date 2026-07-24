using EasyCore.EventBus;
using EasyCore.Invocation;
using EasyCore.Polly;
using EasyCore.Redis;
using Web.EasyCore.Cache.Invocations;
using Web.EasyCore.Cache.Services.Cache;
using Web.EasyCore.Cache.Services.Combo;
using Web.EasyCore.Cache.Services.Lock;
using Web.EasyCore.Cache.Services.Transaction;

namespace Web.EasyCore.Cache;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new()
            {
                Title = "EasyCore.Redis Demo",
                Version = "v1",
                Description =
                    "Placement A–F ([ServerCache] on interface / class / method / API) + Redis APIs + cross-stack + EventBus+Polly.\n" +
                    "Start at GET /api/demo (need Redis localhost:6379)"
            });
        });

        builder.Services.AddSingleton<IRedisCache, RedisCache>();
        builder.Services.AddTransient<IRedisTransaction, RedisTransaction>();
        builder.Services.AddSingleton<IRedisLock, RedisLock>();

        // Register IInvocation implementations before Apply so they resolve inside the pipeline.
        builder.Services.Invocation<TraceInvocation>(ServiceLifetime.Singleton);

        // EventBus first so handlers are registered before Castle Apply.
        builder.Services.AddEasyCoreEventBus();

        // Castle DynamicProxy: each AddEasyCore* applies its own interceptor (nested stack).
        // Order: Invocation → Polly → Redis.
        builder.Services.AddEasyCoreInvocation();
        builder.Services.AddEasyCorePolly();
        builder.Services.AddEasyCoreRedis(builder.Configuration.GetSection("EasyCore:Redis"));

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
