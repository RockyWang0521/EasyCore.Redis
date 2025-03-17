using EasyCore.DistributedCache;
using EasyCore.DistributedLocking;
using EasyCore.ServiceCache;
using EasyCore.Cache;
using RedisTest.Cache;
using RedisTest.Lock;
using RedisTest.Transaction;

namespace Web.EasyCore.Cache
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            #region Redis Cache

            // Add EasyCore.Cache
            builder.Services.EasyCoreDistributedCache(options =>
            {
                options.EndPoints = new List<string> { "192.168.157.142:6379" };
                options.ConnectTimeout = 100;
                options.SyncTimeout = 100;
                options.DistributedName = "Web.EasyCore.Cache";
            });

            // Dependency Injection
            builder.Services.AddSingleton<IRedisCache, RedisCache>();

            builder.Services.AddTransient<IRedisTransaction, RedisTransaction>();

            #endregion

            #region Redis Lock

            // Add EasyCore.Lock
            builder.Services.EasyCoreDistributedLock(options =>
            {
                options.EndPoints = new List<string> { "192.168.157.142:6379" };
                options.ConnectTimeout = 100;
                options.SyncTimeout = 100;
                options.DistributedName = "Web.EasyCore.Cache";
            });

            // Dependency Injection
            builder.Services.AddSingleton<IRedisLock, RedisLock>();

            #endregion

            #region ServerCache

            // Add EasyCore.ServerCache
            builder.Services.EasyCoreServerCache();

            #endregion

            #region EasyCoreCache

            builder.Services.AddTransient<IRedisTransaction, RedisTransaction>();

            builder.Services.AddSingleton<IRedisCache, RedisCache>();

            builder.Services.AddSingleton<IRedisLock, RedisLock>();

            builder.Services.EasyCoreCache(options =>
            {
                options.EndPoints = new List<string> { "192.168.157.142:6379" };
                options.ConnectTimeout = 100;
                options.SyncTimeout = 100;
                options.DistributedName = "Web.EasyCore.Cache";
            });

            #endregion

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
}
