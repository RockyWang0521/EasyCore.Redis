using EasyCore.DistributedCache.Cache;
using RedisTest.Pojo;

namespace RedisTest.Cache
{
    public class RedisCache : IRedisCache
    {
        private readonly IDistributedCache _cache;

        public RedisCache(IDistributedCache cache) => _cache = cache;

        public async Task<string?> GetAsync(string key) => await _cache.GetAsync(key);

        public async Task<TestDto?> GetTestDtoAsync(string key) => await _cache.GetAsync<TestDto>(key);

        public async Task SetAsync(string key, string value, int expirationSeconds) => await _cache.SetAsync(key, value, expirationSeconds);

        public async Task SetTestDtoAsync(string key, TestDto value, int expirationSeconds) => await _cache.SetAsync<TestDto>(key, value, expirationSeconds);
    }
}
