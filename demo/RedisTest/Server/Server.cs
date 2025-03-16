using EasyCore.ServiceCache.ServiceCache.CacheAttribute;

namespace RedisTest.ServerCache
{
    public class Server : IServer
    {
        [ServerCache]
        public async Task<string> ServerCache()
        {
            return await Task.FromResult("这是ServerCache，没有参数");
        }

        [ServerCache]
        public async Task<string> ServerCache(string intput)
        {
            return await Task.FromResult("这是ServerCache，有一个string类型的intput参数");
        }

        [ServerCache]
        public async Task<string> ServerCache(int intput)
        {
            return await Task.FromResult("这是ServerCache，有一个int类型的intput参数");
        }

        [ServerCache]
        public async Task<string> ServerCache(string intput1, string intput2)
        {
            return await Task.FromResult("这是ServerCache，有一个string类型的intput1参数和一个string类型的intput2参数");
        }

        [ServerCache]
        public async Task<string> ServerCache(string intput1, int intput2)
        {
            return await Task.FromResult("这是ServerCache，有一个string类型的intput1参数和一个int类型的intput2参数");
        }
    }
}
