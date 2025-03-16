using RedisTest.Pojo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisTest.Cache
{
    public interface IRedisCache
    {
        Task<string?> GetAsync(string key);

        Task SetAsync(string key, string value, int expirationSeconds);

        Task<TestDto?> GetTestDtoAsync(string key);

        Task SetTestDtoAsync(string key, TestDto value, int expirationSeconds);
    }
}
