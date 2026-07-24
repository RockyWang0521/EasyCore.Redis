using EasyCore.Redis.Service.Attribute;
using Microsoft.AspNetCore.Mvc;
using Web.EasyCore.Cache.Attributes;
using Web.EasyCore.Cache.Services.Server;

namespace Web.EasyCore.Cache.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceCacheController : ControllerBase
    {
        private readonly IServer _server;
        private static int _actionHits;

        public ServiceCacheController(IServer server) => _server = server;

        [HttpGet("ServerCacheNoParameter")]
        public Task<string> ServerCacheNoParameter()
            => _server.ServerCache();

        [HttpGet("ServerCacheOneParameter/int")]
        public Task<string> ServerCacheOneParameterInt()
            => _server.ServerCache(1);

        [HttpGet("ServerCacheOneParameter/string")]
        public Task<string> ServerCacheOneParameterString()
            => _server.ServerCache("string");

        [HttpGet("ServerCacheTwoParameter/string/string")]
        public Task<string> ServerCacheTwoParameterStringString()
            => _server.ServerCache("string", "string");

        [HttpGet("ServerCacheTwoParameter/string/int")]
        public Task<string> ServerCacheTwoParameterStringInt()
            => _server.ServerCache("string", 1);

        /// <summary>
        /// Action 直接挂 [ServerCache]（IFilterFactory），不经过服务接口。
        /// 连调两次：第 2 次不应再打 body 日志（缓存命中）。
        /// </summary>
        [HttpGet("ActionCache")]
        [ServerCache(CacheSeconds = 60)]
        public Task<string> ActionCache()
        {
            var n = Interlocked.Increment(ref _actionHits);
            Console.WriteLine($"  [ServiceCacheController.ActionCache] body #{n}");
            return Task.FromResult($"action-cache hit#{n} at {DateTime.UtcNow:O}");
        }
    }
}
