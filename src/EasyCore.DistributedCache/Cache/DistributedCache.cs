using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace EasyCore.DistributedCache.Cache
{
    public class DistributedCache : IDistributedCache
    {
        private readonly DistributedOption _option;
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _database;

        public DistributedCache(IOptions<DistributedOption> options)
        {
            _option = options.Value ?? throw new ArgumentNullException(nameof(options));

            var configurationOptions = _option.ToConfigurationOptions();

            if (_option.EndPoints.Count == 0)
                throw new InvalidOperationException("No Redis endpoints configured.");

            _redis = ConnectionMultiplexer.Connect(configurationOptions);

            _database = _redis.GetDatabase(_option.DefaultDatabase);
        }

        private string GetCacheKey(string key)
            => $"{_option.DistributedName}:{key}";

        public async Task<string?> GetAsync(string key, CancellationToken token = default)
            => await _database.HashGetAsync(GetCacheKey(key), key);

        public string? Get(string key)
            => _database.HashGet(GetCacheKey(key), key);

        public async Task<T?> GetAsync<T>(string key, CancellationToken token = default)
        {
            var value = await _database.HashGetAsync(GetCacheKey(key), key);
            return value.IsNullOrEmpty ? default : JsonConvert.DeserializeObject<T>(value!);
        }

        public T? Get<T>(string key)
        {
            var value = _database.StringGet(GetCacheKey(key));
            return value.IsNullOrEmpty ? default : JsonConvert.DeserializeObject<T>(value!);
        }

        public async Task RefreshAsync(string key, int seconds, CancellationToken token = default)
        {
            var cacheKey = GetCacheKey(key);
            if (await _database.KeyExistsAsync(cacheKey))
                await _database.KeyExpireAsync(cacheKey, TimeSpan.FromSeconds(seconds));
        }

        public void Refresh(string key, int seconds)
        {
            var cacheKey = GetCacheKey(key);
            if (_database.KeyExists(cacheKey))
                _database.KeyExpire(cacheKey, TimeSpan.FromSeconds(seconds));
        }

        public async Task RemoveAsync(string key, CancellationToken token = default)
            => await _database.KeyDeleteAsync(GetCacheKey(key));

        public void Remove(string key)
            => _database.KeyDelete(GetCacheKey(key));

        public async Task SetAsync(string key, string value, int seconds, CancellationToken token = default)
        {
            var cacheKey = GetCacheKey(key);
            await _database!.HashSetAsync(cacheKey, key, value);
            if (seconds > 0)
                await _database!.KeyExpireAsync(cacheKey, TimeSpan.FromSeconds(seconds));
        }

        public async Task SetAsync<T>(string key, T value, int seconds = 0, CancellationToken token = default)
        {
            var cacheKey = GetCacheKey(key);
            var json = JsonConvert.SerializeObject(value);
            await _database!.HashSetAsync(cacheKey, key, json);
            if (seconds > 0)
                await _database!.KeyExpireAsync(cacheKey, TimeSpan.FromSeconds(seconds));
        }

        public void Set(string key, string value, int seconds)
        {
            var cacheKey = GetCacheKey(key);
            _database!.HashSet(cacheKey, key, value);
            if (seconds > 0)
                _database!.KeyExpire(cacheKey, TimeSpan.FromSeconds(seconds));
        }

        public void Set<T>(string key, T value, int seconds = 0)
        {
            var cacheKey = GetCacheKey(key);
            var json = JsonConvert.SerializeObject(value);
            _database!.HashSet(cacheKey, key, json);
            if (seconds > 0)
                _database!.KeyExpire(cacheKey, TimeSpan.FromSeconds(seconds));
        }

        public async Task<bool> KeyExistsAsync(string key, CancellationToken token = default)
            => await _database.KeyExistsAsync(GetCacheKey(key));

        public bool KeyExists(string key, CancellationToken token = default)
            => _database.KeyExists(GetCacheKey(key));

        public ITransaction GetTransaction()
            => _database.CreateTransaction();
    }
}
