using EasyCore.Redis.Distributed.Connection;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace EasyCore.Redis.Distributed
{
    /// <summary>
    /// Redis distributed cache exposing String / Hash / List / Set / SortedSet APIs,
    /// plus key helpers and String shortcuts.
    /// </summary>
    public sealed class DistributedCache : IDistributedCache
    {
        private static readonly JsonSerializerSettings JsonSettings = new()
        {
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        private readonly IRedisConnection _connection;
        private readonly IDatabase _database;

        /// <summary>
        /// Creates a cache facade over the shared Redis connection.
        /// </summary>
        /// <param name="connection">Shared Redis connection (typically a singleton).</param>
        public DistributedCache(IRedisConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _database = _connection.GetDatabase();
        }

        #region String shortcuts

        /// <inheritdoc />
        public Task<string?> GetAsync(string key, CancellationToken cancellationToken = default)
            => StringGetAsync(key, cancellationToken);

        /// <inheritdoc />
        public string? Get(string key)
            => StringGet(key);

        /// <inheritdoc />
        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
            => StringGetAsync<T>(key, cancellationToken);

        /// <inheritdoc />
        public T? Get<T>(string key)
            => StringGet<T>(key);

        /// <inheritdoc />
        public Task SetAsync(string key, string value, int seconds = 0, CancellationToken cancellationToken = default)
            => StringSetAsync(key, value, seconds, cancellationToken: cancellationToken);

        /// <inheritdoc />
        public Task SetAsync<T>(string key, T value, int seconds = 0, CancellationToken cancellationToken = default)
            => StringSetAsync(key, value, seconds, cancellationToken: cancellationToken);

        #endregion

        #region Key helpers

        /// <inheritdoc />
        public void Refresh(string key, int seconds)
        {
            if (seconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(seconds), "Expiry seconds must be greater than zero.");

            var redisKey = Key(key);
            if (_database.KeyExists(redisKey))
                _database.KeyExpire(redisKey, TimeSpan.FromSeconds(seconds));
        }

        /// <inheritdoc />
        public async Task RefreshAsync(string key, int seconds, CancellationToken cancellationToken = default)
        {
            if (seconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(seconds), "Expiry seconds must be greater than zero.");

            cancellationToken.ThrowIfCancellationRequested();
            var redisKey = Key(key);
            if (await _database.KeyExistsAsync(redisKey).ConfigureAwait(false))
                await _database.KeyExpireAsync(redisKey, TimeSpan.FromSeconds(seconds)).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public void Remove(string key)
            => _database.KeyDelete(Key(key));

        /// <inheritdoc />
        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _database.KeyDeleteAsync(Key(key)).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public bool KeyExists(string key)
            => _database.KeyExists(Key(key));

        /// <inheritdoc />
        public async Task<bool> KeyExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _database.KeyExistsAsync(Key(key)).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public string KeyType(string key)
            => ToTypeName(_database.KeyType(Key(key)));

        /// <inheritdoc />
        public async Task<string> KeyTypeAsync(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ToTypeName(await _database.KeyTypeAsync(Key(key)).ConfigureAwait(false));
        }

        /// <inheritdoc />
        public long KeyTimeToLive(string key)
        {
            var ttl = _database.KeyTimeToLive(Key(key));
            if (ttl is null)
                return KeyExists(key) ? -1 : -2;
            return (long)ttl.Value.TotalSeconds;
        }

        /// <inheritdoc />
        public async Task<long> KeyTimeToLiveAsync(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var redisKey = Key(key);
            var ttl = await _database.KeyTimeToLiveAsync(redisKey).ConfigureAwait(false);
            if (ttl is null)
                return await _database.KeyExistsAsync(redisKey).ConfigureAwait(false) ? -1 : -2;
            return (long)ttl.Value.TotalSeconds;
        }

        #endregion

        #region String

        /// <inheritdoc />
        public string? StringGet(string key)
            => ToNullableString(_database.StringGet(Key(key)));

        /// <inheritdoc />
        public async Task<string?> StringGetAsync(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ToNullableString(await _database.StringGetAsync(Key(key)).ConfigureAwait(false));
        }

        /// <inheritdoc />
        public T? StringGet<T>(string key)
            => Deserialize<T>(_database.StringGet(Key(key)));

        /// <inheritdoc />
        public async Task<T?> StringGetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Deserialize<T>(await _database.StringGetAsync(Key(key)).ConfigureAwait(false));
        }

        /// <inheritdoc />
        public bool StringSet(string key, string value, int seconds = 0, bool whenNotExists = false)
        {
            ArgumentNullException.ThrowIfNull(value);
            return _database.StringSet(Key(key), value, ToExpiry(seconds), whenNotExists ? When.NotExists : When.Always);
        }

        /// <inheritdoc />
        public async Task<bool> StringSetAsync(
            string key,
            string value,
            int seconds = 0,
            bool whenNotExists = false,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(value);
            cancellationToken.ThrowIfCancellationRequested();
            return await _database.StringSetAsync(Key(key), value, ToExpiry(seconds), whenNotExists ? When.NotExists : When.Always)
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public bool StringSet<T>(string key, T value, int seconds = 0, bool whenNotExists = false)
        {
            ArgumentNullException.ThrowIfNull(value);
            return StringSet(key, Serialize(value), seconds, whenNotExists);
        }

        /// <inheritdoc />
        public Task<bool> StringSetAsync<T>(
            string key,
            T value,
            int seconds = 0,
            bool whenNotExists = false,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(value);
            return StringSetAsync(key, Serialize(value), seconds, whenNotExists, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<string?> StringGetDeleteAsync(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ToNullableString(await _database.StringGetDeleteAsync(Key(key)).ConfigureAwait(false));
        }

        /// <inheritdoc />
        public long StringIncrement(string key, long value = 1)
            => _database.StringIncrement(Key(key), value);

        /// <inheritdoc />
        public async Task<long> StringIncrementAsync(string key, long value = 1, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _database.StringIncrementAsync(Key(key), value).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public double StringIncrement(string key, double value)
            => _database.StringIncrement(Key(key), value);

        /// <inheritdoc />
        public async Task<double> StringIncrementAsync(string key, double value, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _database.StringIncrementAsync(Key(key), value).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public long StringDecrement(string key, long value = 1)
            => _database.StringDecrement(Key(key), value);

        /// <inheritdoc />
        public async Task<long> StringDecrementAsync(string key, long value = 1, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _database.StringDecrementAsync(Key(key), value).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public long StringAppend(string key, string value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return _database.StringAppend(Key(key), value);
        }

        /// <inheritdoc />
        public async Task<long> StringAppendAsync(string key, string value, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(value);
            cancellationToken.ThrowIfCancellationRequested();
            return await _database.StringAppendAsync(Key(key), value).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public long StringGetLength(string key)
            => _database.StringLength(Key(key));

        /// <inheritdoc />
        public async Task<long> StringGetLengthAsync(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _database.StringLengthAsync(Key(key)).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public bool StringSetExpiry(string key, int seconds)
        {
            if (seconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(seconds));
            return _database.KeyExpire(Key(key), TimeSpan.FromSeconds(seconds));
        }

        /// <inheritdoc />
        public async Task<bool> StringSetExpiryAsync(string key, int seconds, CancellationToken cancellationToken = default)
        {
            if (seconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(seconds));
            cancellationToken.ThrowIfCancellationRequested();
            return await _database.KeyExpireAsync(Key(key), TimeSpan.FromSeconds(seconds)).ConfigureAwait(false);
        }

        #endregion

        #region Hash

        /// <inheritdoc />
        public bool HashSet(string key, string field, string value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return _database.HashSet(Key(key), field, value);
        }

        /// <inheritdoc />
        public async Task<bool> HashSetAsync(string key, string field, string value, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(value);
            cancellationToken.ThrowIfCancellationRequested();
            return await _database.HashSetAsync(Key(key), field, value).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public bool HashSet<T>(string key, string field, T value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return HashSet(key, field, Serialize(value));
        }

        /// <inheritdoc />
        public Task<bool> HashSetAsync<T>(string key, string field, T value, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(value);
            return HashSetAsync(key, field, Serialize(value), cancellationToken);
        }

        /// <inheritdoc />
        public void HashSet(string key, IDictionary<string, string> fields, int seconds = 0)
        {
            ArgumentNullException.ThrowIfNull(fields);
            if (fields.Count == 0)
                return;

            var entries = fields.Select(kv => new HashEntry(kv.Key, kv.Value)).ToArray();
            var redisKey = Key(key);
            _database.HashSet(redisKey, entries);
            if (seconds > 0)
                _database.KeyExpire(redisKey, TimeSpan.FromSeconds(seconds));
        }

        /// <inheritdoc />
        public async Task HashSetAsync(
            string key,
            IDictionary<string, string> fields,
            int seconds = 0,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(fields);
            cancellationToken.ThrowIfCancellationRequested();
            if (fields.Count == 0)
                return;

            var entries = fields.Select(kv => new HashEntry(kv.Key, kv.Value)).ToArray();
            var redisKey = Key(key);
            await _database.HashSetAsync(redisKey, entries).ConfigureAwait(false);
            if (seconds > 0)
                await _database.KeyExpireAsync(redisKey, TimeSpan.FromSeconds(seconds)).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public string? HashGet(string key, string field)
            => ToNullableString(_database.HashGet(Key(key), field));

        /// <inheritdoc />
        public async Task<string?> HashGetAsync(string key, string field, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ToNullableString(await _database.HashGetAsync(Key(key), field).ConfigureAwait(false));
        }

        /// <inheritdoc />
        public T? HashGet<T>(string key, string field)
            => Deserialize<T>(_database.HashGet(Key(key), field));

        /// <inheritdoc />
        public async Task<T?> HashGetAsync<T>(string key, string field, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Deserialize<T>(await _database.HashGetAsync(Key(key), field).ConfigureAwait(false));
        }

        /// <inheritdoc />
        public IDictionary<string, string> HashGetAll(string key)
        {
            var entries = _database.HashGetAll(Key(key));
            return entries.ToDictionary(e => e.Name.ToString(), e => e.Value.ToString());
        }

        /// <inheritdoc />
        public async Task<IDictionary<string, string>> HashGetAllAsync(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var entries = await _database.HashGetAllAsync(Key(key)).ConfigureAwait(false);
            return entries.ToDictionary(e => e.Name.ToString(), e => e.Value.ToString());
        }

        /// <inheritdoc />
        public long HashDelete(string key, params string[] fields)
        {
            if (fields is null || fields.Length == 0)
                return 0;
            return _database.HashDelete(Key(key), fields.Select(f => (RedisValue)f).ToArray());
        }

        /// <inheritdoc />
        public async Task<long> HashDeleteAsync(string key, CancellationToken cancellationToken = default, params string[] fields)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (fields is null || fields.Length == 0)
                return 0;
            return await _database.HashDeleteAsync(Key(key), fields.Select(f => (RedisValue)f).ToArray()).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public bool HashExists(string key, string field)
            => _database.HashExists(Key(key), field);

        /// <inheritdoc />
        public async Task<bool> HashExistsAsync(string key, string field, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _database.HashExistsAsync(Key(key), field).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public string[] HashGetKeys(string key)
            => _database.HashKeys(Key(key)).Select(v => v.ToString()).ToArray();

        /// <inheritdoc />
        public async Task<string[]> HashGetKeysAsync(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var keys = await _database.HashKeysAsync(Key(key)).ConfigureAwait(false);
            return keys.Select(v => v.ToString()).ToArray();
        }

        /// <inheritdoc />
        public string[] HashGetValues(string key)
            => _database.HashValues(Key(key)).Select(v => v.ToString()).ToArray();

        /// <inheritdoc />
        public async Task<string[]> HashGetValuesAsync(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var values = await _database.HashValuesAsync(Key(key)).ConfigureAwait(false);
            return values.Select(v => v.ToString()).ToArray();
        }

        /// <inheritdoc />
        public long HashGetLength(string key)
            => _database.HashLength(Key(key));

        /// <inheritdoc />
        public async Task<long> HashGetLengthAsync(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _database.HashLengthAsync(Key(key)).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public long HashIncrement(string key, string field, long value = 1)
            => _database.HashIncrement(Key(key), field, value);

        /// <inheritdoc />
        public async Task<long> HashIncrementAsync(string key, string field, long value = 1, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _database.HashIncrementAsync(Key(key), field, value).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public double HashIncrement(string key, string field, double value)
            => _database.HashIncrement(Key(key), field, value);

        /// <inheritdoc />
        public async Task<double> HashIncrementAsync(string key, string field, double value, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _database.HashIncrementAsync(Key(key), field, value).ConfigureAwait(false);
        }

        #endregion

        #region List

        /// <inheritdoc />
        public long ListLeftPush(string key, params string[] values)
        {
            EnsureValues(values);
            return _database.ListLeftPush(Key(key), ToRedisValues(values));
        }

        /// <inheritdoc />
        public async Task<long> ListLeftPushAsync(string key, CancellationToken cancellationToken = default, params string[] values)
        {
            EnsureValues(values);
            cancellationToken.ThrowIfCancellationRequested();
            return await _database.ListLeftPushAsync(Key(key), ToRedisValues(values)).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public long ListRightPush(string key, params string[] values)
        {
            EnsureValues(values);
            return _database.ListRightPush(Key(key), ToRedisValues(values));
        }

        /// <inheritdoc />
        public async Task<long> ListRightPushAsync(string key, CancellationToken cancellationToken = default, params string[] values)
        {
            EnsureValues(values);
            cancellationToken.ThrowIfCancellationRequested();
            return await _database.ListRightPushAsync(Key(key), ToRedisValues(values)).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public string? ListLeftPop(string key)
            => ToNullableString(_database.ListLeftPop(Key(key)));

        /// <inheritdoc />
        public async Task<string?> ListLeftPopAsync(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ToNullableString(await _database.ListLeftPopAsync(Key(key)).ConfigureAwait(false));
        }

        /// <inheritdoc />
        public string? ListRightPop(string key)
            => ToNullableString(_database.ListRightPop(Key(key)));

        /// <inheritdoc />
        public async Task<string?> ListRightPopAsync(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ToNullableString(await _database.ListRightPopAsync(Key(key)).ConfigureAwait(false));
        }

        /// <inheritdoc />
        public string[] ListRange(string key, long start = 0, long stop = -1)
            => _database.ListRange(Key(key), start, stop).Select(v => v.ToString()).ToArray();

        /// <inheritdoc />
        public async Task<string[]> ListRangeAsync(string key, long start = 0, long stop = -1, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var values = await _database.ListRangeAsync(Key(key), start, stop).ConfigureAwait(false);
            return values.Select(v => v.ToString()).ToArray();
        }

        /// <inheritdoc />
        public long ListGetLength(string key)
            => _database.ListLength(Key(key));

        /// <inheritdoc />
        public async Task<long> ListGetLengthAsync(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _database.ListLengthAsync(Key(key)).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public string? ListGetByIndex(string key, long index)
            => ToNullableString(_database.ListGetByIndex(Key(key), index));

        /// <inheritdoc />
        public async Task<string?> ListGetByIndexAsync(string key, long index, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ToNullableString(await _database.ListGetByIndexAsync(Key(key), index).ConfigureAwait(false));
        }

        /// <inheritdoc />
        public void ListSetByIndex(string key, long index, string value)
        {
            ArgumentNullException.ThrowIfNull(value);
            _database.ListSetByIndex(Key(key), index, value);
        }

        /// <inheritdoc />
        public async Task ListSetByIndexAsync(string key, long index, string value, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(value);
            cancellationToken.ThrowIfCancellationRequested();
            await _database.ListSetByIndexAsync(Key(key), index, value).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public void ListTrim(string key, long start, long stop)
            => _database.ListTrim(Key(key), start, stop);

        /// <inheritdoc />
        public async Task ListTrimAsync(string key, long start, long stop, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _database.ListTrimAsync(Key(key), start, stop).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public long ListRemove(string key, string value, long count = 0)
        {
            ArgumentNullException.ThrowIfNull(value);
            return _database.ListRemove(Key(key), value, count);
        }

        /// <inheritdoc />
        public async Task<long> ListRemoveAsync(string key, string value, long count = 0, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(value);
            cancellationToken.ThrowIfCancellationRequested();
            return await _database.ListRemoveAsync(Key(key), value, count).ConfigureAwait(false);
        }

        #endregion

        #region Set

        /// <inheritdoc />
        public long SetAdd(string key, params string[] members)
        {
            EnsureMembers(members);
            return _database.SetAdd(Key(key), ToRedisValues(members));
        }

        /// <inheritdoc />
        public async Task<long> SetAddAsync(string key, CancellationToken cancellationToken = default, params string[] members)
        {
            EnsureMembers(members);
            cancellationToken.ThrowIfCancellationRequested();
            return await _database.SetAddAsync(Key(key), ToRedisValues(members)).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public long SetRemove(string key, params string[] members)
        {
            EnsureMembers(members);
            return _database.SetRemove(Key(key), ToRedisValues(members));
        }

        /// <inheritdoc />
        public async Task<long> SetRemoveAsync(string key, CancellationToken cancellationToken = default, params string[] members)
        {
            EnsureMembers(members);
            cancellationToken.ThrowIfCancellationRequested();
            return await _database.SetRemoveAsync(Key(key), ToRedisValues(members)).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public string[] SetGetMembers(string key)
            => _database.SetMembers(Key(key)).Select(v => v.ToString()).ToArray();

        /// <inheritdoc />
        public async Task<string[]> SetGetMembersAsync(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var members = await _database.SetMembersAsync(Key(key)).ConfigureAwait(false);
            return members.Select(v => v.ToString()).ToArray();
        }

        /// <inheritdoc />
        public long SetGetLength(string key)
            => _database.SetLength(Key(key));

        /// <inheritdoc />
        public async Task<long> SetGetLengthAsync(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _database.SetLengthAsync(Key(key)).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public bool SetContains(string key, string member)
            => _database.SetContains(Key(key), member);

        /// <inheritdoc />
        public async Task<bool> SetContainsAsync(string key, string member, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _database.SetContainsAsync(Key(key), member).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public string? SetPop(string key)
            => ToNullableString(_database.SetPop(Key(key)));

        /// <inheritdoc />
        public async Task<string?> SetPopAsync(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ToNullableString(await _database.SetPopAsync(Key(key)).ConfigureAwait(false));
        }

        /// <inheritdoc />
        public string? SetRandomMember(string key)
            => ToNullableString(_database.SetRandomMember(Key(key)));

        /// <inheritdoc />
        public async Task<string?> SetRandomMemberAsync(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ToNullableString(await _database.SetRandomMemberAsync(Key(key)).ConfigureAwait(false));
        }

        /// <inheritdoc />
        public string[] SetRandomMembers(string key, long count)
            => _database.SetRandomMembers(Key(key), count).Select(v => v.ToString()).ToArray();

        /// <inheritdoc />
        public async Task<string[]> SetRandomMembersAsync(string key, long count, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var members = await _database.SetRandomMembersAsync(Key(key), count).ConfigureAwait(false);
            return members.Select(v => v.ToString()).ToArray();
        }

        /// <inheritdoc />
        public bool SetMove(string sourceKey, string destinationKey, string member)
            => _database.SetMove(Key(sourceKey), Key(destinationKey), member);

        /// <inheritdoc />
        public async Task<bool> SetMoveAsync(
            string sourceKey,
            string destinationKey,
            string member,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _database.SetMoveAsync(Key(sourceKey), Key(destinationKey), member).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public string[] SetIntersect(params string[] keys)
        {
            EnsureKeys(keys);
            return _database.SetCombine(SetOperation.Intersect, ToRedisKeys(keys)).Select(v => v.ToString()).ToArray();
        }

        /// <inheritdoc />
        public async Task<string[]> SetIntersectAsync(CancellationToken cancellationToken = default, params string[] keys)
        {
            EnsureKeys(keys);
            cancellationToken.ThrowIfCancellationRequested();
            var values = await _database.SetCombineAsync(SetOperation.Intersect, ToRedisKeys(keys)).ConfigureAwait(false);
            return values.Select(v => v.ToString()).ToArray();
        }

        /// <inheritdoc />
        public string[] SetUnion(params string[] keys)
        {
            EnsureKeys(keys);
            return _database.SetCombine(SetOperation.Union, ToRedisKeys(keys)).Select(v => v.ToString()).ToArray();
        }

        /// <inheritdoc />
        public async Task<string[]> SetUnionAsync(CancellationToken cancellationToken = default, params string[] keys)
        {
            EnsureKeys(keys);
            cancellationToken.ThrowIfCancellationRequested();
            var values = await _database.SetCombineAsync(SetOperation.Union, ToRedisKeys(keys)).ConfigureAwait(false);
            return values.Select(v => v.ToString()).ToArray();
        }

        /// <inheritdoc />
        public string[] SetDifference(string key, params string[] otherKeys)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            var keys = new[] { key }.Concat(otherKeys ?? Array.Empty<string>()).ToArray();
            EnsureKeys(keys);
            return _database.SetCombine(SetOperation.Difference, ToRedisKeys(keys)).Select(v => v.ToString()).ToArray();
        }

        /// <inheritdoc />
        public async Task<string[]> SetDifferenceAsync(
            string key,
            CancellationToken cancellationToken = default,
            params string[] otherKeys)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            cancellationToken.ThrowIfCancellationRequested();
            var keys = new[] { key }.Concat(otherKeys ?? Array.Empty<string>()).ToArray();
            EnsureKeys(keys);
            var values = await _database.SetCombineAsync(SetOperation.Difference, ToRedisKeys(keys)).ConfigureAwait(false);
            return values.Select(v => v.ToString()).ToArray();
        }

        #endregion

        #region SortedSet

        /// <inheritdoc />
        public bool SortedSetAdd(string key, string member, double score)
            => _database.SortedSetAdd(Key(key), member, score);

        /// <inheritdoc />
        public async Task<bool> SortedSetAddAsync(string key, string member, double score, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _database.SortedSetAddAsync(Key(key), member, score).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public long SortedSetAdd(string key, IEnumerable<RedisSortedSetEntry> entries)
        {
            ArgumentNullException.ThrowIfNull(entries);
            var arr = entries.Select(e => new SortedSetEntry(e.Member, e.Score)).ToArray();
            return arr.Length == 0 ? 0 : _database.SortedSetAdd(Key(key), arr);
        }

        /// <inheritdoc />
        public async Task<long> SortedSetAddAsync(
            string key,
            IEnumerable<RedisSortedSetEntry> entries,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entries);
            cancellationToken.ThrowIfCancellationRequested();
            var arr = entries.Select(e => new SortedSetEntry(e.Member, e.Score)).ToArray();
            return arr.Length == 0 ? 0 : await _database.SortedSetAddAsync(Key(key), arr).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public bool SortedSetRemove(string key, string member)
            => _database.SortedSetRemove(Key(key), member);

        /// <inheritdoc />
        public async Task<bool> SortedSetRemoveAsync(string key, string member, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _database.SortedSetRemoveAsync(Key(key), member).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public long SortedSetRemove(string key, params string[] members)
        {
            if (members is null || members.Length == 0)
                return 0;
            return _database.SortedSetRemove(Key(key), members.Select(m => (RedisValue)m).ToArray());
        }

        /// <inheritdoc />
        public async Task<long> SortedSetRemoveAsync(string key, CancellationToken cancellationToken = default, params string[] members)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (members is null || members.Length == 0)
                return 0;
            return await _database.SortedSetRemoveAsync(Key(key), members.Select(m => (RedisValue)m).ToArray()).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public double? SortedSetGetScore(string key, string member)
            => _database.SortedSetScore(Key(key), member);

        /// <inheritdoc />
        public async Task<double?> SortedSetGetScoreAsync(string key, string member, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _database.SortedSetScoreAsync(Key(key), member).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public long? SortedSetGetRank(string key, string member, bool ascending = true)
            => ascending
                ? _database.SortedSetRank(Key(key), member)
                : _database.SortedSetRank(Key(key), member, Order.Descending);

        /// <inheritdoc />
        public async Task<long?> SortedSetGetRankAsync(
            string key,
            string member,
            bool ascending = true,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ascending
                ? await _database.SortedSetRankAsync(Key(key), member).ConfigureAwait(false)
                : await _database.SortedSetRankAsync(Key(key), member, Order.Descending).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public string[] SortedSetRangeByRank(string key, long start = 0, long stop = -1, bool ascending = true)
        {
            var order = ascending ? Order.Ascending : Order.Descending;
            return _database.SortedSetRangeByRank(Key(key), start, stop, order).Select(v => v.ToString()).ToArray();
        }

        /// <inheritdoc />
        public async Task<string[]> SortedSetRangeByRankAsync(
            string key,
            long start = 0,
            long stop = -1,
            bool ascending = true,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var order = ascending ? Order.Ascending : Order.Descending;
            var values = await _database.SortedSetRangeByRankAsync(Key(key), start, stop, order).ConfigureAwait(false);
            return values.Select(v => v.ToString()).ToArray();
        }

        /// <inheritdoc />
        public RedisSortedSetEntry[] SortedSetRangeByRankWithScores(string key, long start = 0, long stop = -1, bool ascending = true)
        {
            var order = ascending ? Order.Ascending : Order.Descending;
            return _database.SortedSetRangeByRankWithScores(Key(key), start, stop, order)
                .Select(e => new RedisSortedSetEntry(e.Element.ToString(), e.Score))
                .ToArray();
        }

        /// <inheritdoc />
        public async Task<RedisSortedSetEntry[]> SortedSetRangeByRankWithScoresAsync(
            string key,
            long start = 0,
            long stop = -1,
            bool ascending = true,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var order = ascending ? Order.Ascending : Order.Descending;
            var values = await _database.SortedSetRangeByRankWithScoresAsync(Key(key), start, stop, order).ConfigureAwait(false);
            return values.Select(e => new RedisSortedSetEntry(e.Element.ToString(), e.Score)).ToArray();
        }

        /// <inheritdoc />
        public string[] SortedSetRangeByScore(
            string key,
            double min = double.NegativeInfinity,
            double max = double.PositiveInfinity,
            bool ascending = true)
        {
            var order = ascending ? Order.Ascending : Order.Descending;
            return _database.SortedSetRangeByScore(Key(key), min, max, Exclude.None, order)
                .Select(v => v.ToString())
                .ToArray();
        }

        /// <inheritdoc />
        public async Task<string[]> SortedSetRangeByScoreAsync(
            string key,
            double min = double.NegativeInfinity,
            double max = double.PositiveInfinity,
            bool ascending = true,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var order = ascending ? Order.Ascending : Order.Descending;
            var values = await _database.SortedSetRangeByScoreAsync(Key(key), min, max, Exclude.None, order).ConfigureAwait(false);
            return values.Select(v => v.ToString()).ToArray();
        }

        /// <inheritdoc />
        public RedisSortedSetEntry[] SortedSetRangeByScoreWithScores(
            string key,
            double min = double.NegativeInfinity,
            double max = double.PositiveInfinity,
            bool ascending = true)
        {
            var order = ascending ? Order.Ascending : Order.Descending;
            return _database.SortedSetRangeByScoreWithScores(Key(key), min, max, Exclude.None, order)
                .Select(e => new RedisSortedSetEntry(e.Element.ToString(), e.Score))
                .ToArray();
        }

        /// <inheritdoc />
        public async Task<RedisSortedSetEntry[]> SortedSetRangeByScoreWithScoresAsync(
            string key,
            double min = double.NegativeInfinity,
            double max = double.PositiveInfinity,
            bool ascending = true,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var order = ascending ? Order.Ascending : Order.Descending;
            var values = await _database.SortedSetRangeByScoreWithScoresAsync(Key(key), min, max, Exclude.None, order)
                .ConfigureAwait(false);
            return values.Select(e => new RedisSortedSetEntry(e.Element.ToString(), e.Score)).ToArray();
        }

        /// <inheritdoc />
        public long SortedSetGetLength(string key)
            => _database.SortedSetLength(Key(key));

        /// <inheritdoc />
        public async Task<long> SortedSetGetLengthAsync(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _database.SortedSetLengthAsync(Key(key)).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public long SortedSetCountByScore(string key, double min, double max)
            => _database.SortedSetLength(Key(key), min, max);

        /// <inheritdoc />
        public async Task<long> SortedSetCountByScoreAsync(string key, double min, double max, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _database.SortedSetLengthAsync(Key(key), min, max).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public double SortedSetIncrementScore(string key, string member, double value)
            => _database.SortedSetIncrement(Key(key), member, value);

        /// <inheritdoc />
        public async Task<double> SortedSetIncrementScoreAsync(
            string key,
            string member,
            double value,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _database.SortedSetIncrementAsync(Key(key), member, value).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public long SortedSetRemoveRangeByRank(string key, long start, long stop)
            => _database.SortedSetRemoveRangeByRank(Key(key), start, stop);

        /// <inheritdoc />
        public async Task<long> SortedSetRemoveRangeByRankAsync(
            string key,
            long start,
            long stop,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _database.SortedSetRemoveRangeByRankAsync(Key(key), start, stop).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public long SortedSetRemoveRangeByScore(string key, double min, double max)
            => _database.SortedSetRemoveRangeByScore(Key(key), min, max);

        /// <inheritdoc />
        public async Task<long> SortedSetRemoveRangeByScoreAsync(
            string key,
            double min,
            double max,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _database.SortedSetRemoveRangeByScoreAsync(Key(key), min, max).ConfigureAwait(false);
        }

        #endregion

        #region Helpers

        private RedisKey Key(string key) => _connection.GetPrefixedKey(key);

        private static TimeSpan? ToExpiry(int seconds)
            => seconds > 0 ? TimeSpan.FromSeconds(seconds) : null;

        private static string Serialize<T>(T value)
            => JsonConvert.SerializeObject(value, JsonSettings);

        private static T? Deserialize<T>(RedisValue value)
            => value.IsNullOrEmpty ? default : JsonConvert.DeserializeObject<T>(value!, JsonSettings);

        private static string? ToNullableString(RedisValue value)
            => value.IsNullOrEmpty ? null : value.ToString();

        private static string ToTypeName(RedisType type) => type switch
        {
            RedisType.String => "string",
            RedisType.Hash => "hash",
            RedisType.List => "list",
            RedisType.Set => "set",
            RedisType.SortedSet => "zset",
            RedisType.Stream => "stream",
            RedisType.None => "none",
            _ => type.ToString().ToLowerInvariant()
        };

        private static void EnsureValues(string[] values)
        {
            if (values is null || values.Length == 0)
                throw new ArgumentException("At least one value is required.", nameof(values));
        }

        private static void EnsureMembers(string[] members)
        {
            if (members is null || members.Length == 0)
                throw new ArgumentException("At least one member is required.", nameof(members));
        }

        private static void EnsureKeys(string[] keys)
        {
            if (keys is null || keys.Length == 0)
                throw new ArgumentException("At least one key is required.", nameof(keys));
        }

        private static RedisValue[] ToRedisValues(string[] values)
            => values.Select(v => (RedisValue)v).ToArray();

        private RedisKey[] ToRedisKeys(string[] keys)
            => keys.Select(Key).ToArray();

        #endregion
    }
}
