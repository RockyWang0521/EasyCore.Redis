using EasyCore.Redis.Distributed;
using Web.EasyCore.Cache.Models;

namespace Web.EasyCore.Cache.Services.Cache
{
    public interface IRedisCache
    {
        Task<string?> GetAsync(string key);

        Task SetAsync(string key, string value, int expirationSeconds);

        Task<TestDto?> GetTestDtoAsync(string key);

        Task SetTestDtoAsync(string key, TestDto value, int expirationSeconds);

        Task HashSetAsync(string key, string field, string value);

        Task<string?> HashGetAsync(string key, string field);

        Task<IDictionary<string, string>> HashGetAllAsync(string key);

        Task<long> ListRightPushAsync(string key, params string[] values);

        Task<string[]> ListRangeAsync(string key);

        Task<long> SetAddAsync(string key, params string[] members);

        Task<string[]> SetMembersAsync(string key);

        Task SortedSetAddAsync(string key, string member, double score);

        Task<RedisSortedSetEntry[]> SortedSetRangeWithScoresAsync(string key);
    }
}
