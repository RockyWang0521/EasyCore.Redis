using EasyCore.Redis.Service.Attribute;

namespace Web.EasyCore.Cache.Services.Server
{
    public class Server : IServer
    {
        [ServerCache]
        public Task<string> ServerCache()
            => Task.FromResult("这是ServerCache，没有参数");

        [ServerCache]
        public Task<string> ServerCache(string intput)
            => Task.FromResult("这是ServerCache，有一个string类型的intput参数");

        [ServerCache]
        public Task<string> ServerCache(int intput)
            => Task.FromResult("这是ServerCache，有一个int类型的intput参数");

        [ServerCache]
        public Task<string> ServerCache(string intput1, string intput2)
            => Task.FromResult("这是ServerCache，有一个string类型的intput1参数和一个string类型的intput2参数");

        [ServerCache]
        public Task<string> ServerCache(string intput1, int intput2)
            => Task.FromResult("这是ServerCache，有一个string类型的intput1参数和一个int类型的intput2参数");
    }
}
