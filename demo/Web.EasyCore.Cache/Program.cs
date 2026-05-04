using EasyCore.Redis;
using Web.EasyCore.Cache.Services.Cache;
using Web.EasyCore.Cache.Services.Lock;
using Web.EasyCore.Cache.Services.Transaction;

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

            builder.Services.AddSingleton<IRedisCache, RedisCache>();
            builder.Services.AddTransient<IRedisTransaction, RedisTransaction>();
            builder.Services.AddSingleton<IRedisLock, RedisLock>();

            builder.Services.EasyCoreRedis(builder.Configuration.GetSection("EasyCore:Redis"));

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
