using Microsoft.AspNetCore.Mvc;
using RedisTest.ServerCache;

namespace Web.EasyCore.Cache.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceCacheController : ControllerBase
    {
        private readonly IServer _server;

        public ServiceCacheController(IServer server) => _server = server;

        [HttpGet("ServerCacheNoParameter")]
        public async Task<string> ServerCacheNoParameter() =>
            await _server.ServerCache();

        [HttpGet("ServerCacheOneParameter/int")]
        public async Task<string> ServerCacheOneParameterInt() =>
            await _server.ServerCache(1);

        [HttpGet("ServerCacheOneParameter/string")]
        public async Task<string> ServerCacheOneParameterString() =>
            await _server.ServerCache("string");

        [HttpGet("ServerCacheTwoParameter/string/string")]
        public async Task<string> ServerCacheTwoParameterStringString() =>
            await _server.ServerCache("string", "string");

        [HttpGet("ServerCacheTwoParameter/string/int")]
        public async Task<string> ServerCacheTwoParameterStringInt() =>
            await _server.ServerCache("string", 1);

    }
}
