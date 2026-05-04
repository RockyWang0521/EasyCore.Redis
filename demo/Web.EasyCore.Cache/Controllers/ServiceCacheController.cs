using Microsoft.AspNetCore.Mvc;
using Web.EasyCore.Cache.Services.Server;

namespace Web.EasyCore.Cache.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceCacheController : ControllerBase
    {
        private readonly IServer _server;

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
    }
}
