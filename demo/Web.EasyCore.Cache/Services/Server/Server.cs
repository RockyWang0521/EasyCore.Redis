namespace Web.EasyCore.Cache.Services.Server
{
    /// <summary>
    /// Implementation has no attributes — caching comes from <see cref="IServer"/> placement.
    /// </summary>
    public class Server : IServer
    {
        public Task<string> ServerCache()
            => Task.FromResult("这是ServerCache，没有参数");

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
