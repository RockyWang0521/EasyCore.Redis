using EasyCore.Redis.Distributed;
using Microsoft.AspNetCore.Mvc;
using Web.EasyCore.Cache.Models;
using Web.EasyCore.Cache.Services.Cache;

namespace Web.EasyCore.Cache.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DistributedCacheController : ControllerBase
    {
        private readonly IRedisCache _cache;

        public DistributedCacheController(IRedisCache cache) => _cache = cache;

        #region String

        [HttpGet("string/{key}")]
        public Task<string?> GetString(string key) => _cache.GetAsync(key);

        [HttpPost("string/{key}")]
        public Task SetString(string key, [FromQuery] string value, [FromQuery] int expireSeconds = 60)
            => _cache.SetAsync(key, value, expireSeconds);

        [HttpGet("string/dto/{key}")]
        public Task<TestDto?> GetDto(string key) => _cache.GetTestDtoAsync(key);

        [HttpPost("string/dto/{key}")]
        public Task SetDto(string key, [FromQuery] int expireSeconds = 60)
        {
            var value = new TestDto
            {
                DtoBool = true,
                DtoInt = 100,
                DtoString = "Hello, world!"
            };
            return _cache.SetTestDtoAsync(key, value, expireSeconds);
        }

        #endregion

        #region Hash

        [HttpPost("hash/{key}/{field}")]
        public Task HashSet(string key, string field, [FromQuery] string value)
            => _cache.HashSetAsync(key, field, value);

        [HttpGet("hash/{key}/{field}")]
        public Task<string?> HashGet(string key, string field)
            => _cache.HashGetAsync(key, field);

        [HttpGet("hash/{key}")]
        public Task<IDictionary<string, string>> HashGetAll(string key)
            => _cache.HashGetAllAsync(key);

        #endregion

        #region List

        [HttpPost("list/{key}")]
        public Task<long> ListPush(string key, [FromQuery] string[] values)
            => _cache.ListRightPushAsync(key, values);

        [HttpGet("list/{key}")]
        public Task<string[]> ListRange(string key)
            => _cache.ListRangeAsync(key);

        #endregion

        #region Set

        [HttpPost("set/{key}")]
        public Task<long> SetAdd(string key, [FromQuery] string[] members)
            => _cache.SetAddAsync(key, members);

        [HttpGet("set/{key}")]
        public Task<string[]> SetMembers(string key)
            => _cache.SetMembersAsync(key);

        #endregion

        #region SortedSet

        [HttpPost("zset/{key}")]
        public Task ZSetAdd(string key, [FromQuery] string member, [FromQuery] double score)
            => _cache.SortedSetAddAsync(key, member, score);

        [HttpGet("zset/{key}")]
        public Task<RedisSortedSetEntry[]> ZSetRange(string key)
            => _cache.SortedSetRangeWithScoresAsync(key);

        #endregion
    }
}
