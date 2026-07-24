using EasyCore.Redis.Service.Attribute;

namespace Web.EasyCore.Cache.Services.Server
{
    /// <summary>
    /// Class-level [ServerCache] weaves all methods; one method overrides TTL.
    /// </summary>
    [ServerCache(CacheSeconds = 120)]
    public class Server : IServer
    {
        public Task<string> ServerCache()
            => Task.FromResult("这是ServerCache，没有参数");

        [ServerCache(CacheSeconds = 60)]
        public Task<string> ServerCache(string intput)
            => Task.FromResult("这是ServerCache，有一个string类型的intput参数");

        public Task<string> ServerCache(int intput)
            => Task.FromResult("这是ServerCache，有一个int类型的intput参数");

        public Task<string> ServerCache(string intput1, string intput2)
            => Task.FromResult("这是ServerCache，有一个string类型的intput1参数和一个string类型的intput2参数");

        public Task<string> ServerCache(string intput1, int intput2)
            => Task.FromResult("这是ServerCache，有一个string类型的intput1参数和一个int类型的intput2参数");
    }
}
