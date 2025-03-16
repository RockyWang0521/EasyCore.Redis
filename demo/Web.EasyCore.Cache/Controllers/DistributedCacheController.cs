using EasyCore.DistributedCache.Cache;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RedisTest.Cache;
using RedisTest.Pojo;

namespace Web.EasyCore.Cache.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DistributedCacheController : ControllerBase
    {
        private readonly IRedisCache _cache;

        public DistributedCacheController(IRedisCache cache) => _cache = cache;

        [HttpGet("GetString:{key}")]
        public async Task<string?> GetString(string key) => await _cache.GetAsync(key);

        [HttpPost("SetString:{key}/{value}/{expireSeconds}")]
        public async Task SetString(string key, string value, int expireSeconds) => await _cache.SetAsync(key, value, expireSeconds);

        [HttpGet("GetDto:{key}")]
        public async Task<TestDto?> GetDto(string key) => await _cache.GetTestDtoAsync(key);

        [HttpPost("SetDto:{key}/{expireSeconds}")]
        public async Task SetDto(string key, int expireSeconds)
        {
            var value = new TestDto()
            {
                DtoBool = true,
                DtoInt = 100,
                DtoString = "Hello, world!"
            };
            await _cache.SetTestDtoAsync(key, value, expireSeconds);
        }
    }
}
