using EasyCore.Redis.Distributed;
using Web.EasyCore.Cache.Models;

namespace Web.EasyCore.Cache.Services.Cache
{
    public class RedisCache : IRedisCache
    {
        private readonly IDistributedCache _cache;

        public RedisCache(IDistributedCache cache) => _cache = cache;

        public Task<string?> GetAsync(string key) => _cache.StringGetAsync(key);

        public Task<TestDto?> GetTestDtoAsync(string key) => _cache.StringGetAsync<TestDto>(key);

        public Task SetAsync(string key, string value, int expirationSeconds)
            => _cache.StringSetAsync(key, value, expirationSeconds);

        public Task SetTestDtoAsync(string key, TestDto value, int expirationSeconds)
            => _cache.StringSetAsync(key, value, expirationSeconds);

        public Task HashSetAsync(string key, string field, string value)
            => _cache.HashSetAsync(key, field, value);

        public Task<string?> HashGetAsync(string key, string field)
            => _cache.HashGetAsync(key, field);

        public Task<IDictionary<string, string>> HashGetAllAsync(string key)
            => _cache.HashGetAllAsync(key);

        public Task<long> ListRightPushAsync(string key, params string[] values)
            => _cache.ListRightPushAsync(key, cancellationToken: default, values);

        public Task<string[]> ListRangeAsync(string key)
            => _cache.ListRangeAsync(key);

        public Task<long> SetAddAsync(string key, params string[] members)
            => _cache.SetAddAsync(key, cancellationToken: default, members);

        public Task<string[]> SetMembersAsync(string key)
            => _cache.SetGetMembersAsync(key);

        public Task SortedSetAddAsync(string key, string member, double score)
            => _cache.SortedSetAddAsync(key, member, score);

        public Task<RedisSortedSetEntry[]> SortedSetRangeWithScoresAsync(string key)
            => _cache.SortedSetRangeByRankWithScoresAsync(key);
    }
}
