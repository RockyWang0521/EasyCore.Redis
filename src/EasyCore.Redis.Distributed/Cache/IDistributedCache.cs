namespace EasyCore.Redis.Distributed
{
    /// <summary>
    /// Redis distributed data access for String, Hash, List, Set, and SortedSet,
    /// plus key helpers and String shortcuts.
    /// </summary>
    public interface IDistributedCache
    {
        #region String shortcuts

        /// <summary>
        /// Gets a string value by key. Shortcut for <see cref="StringGetAsync"/>.
        /// </summary>
        /// <param name="key">Logical cache key (prefix is applied automatically).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The stored string, or <c>null</c> if missing.</returns>
        Task<string?> GetAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets and deserializes a JSON value by key. Shortcut for <see cref="StringGetAsync{T}"/>.
        /// </summary>
        /// <typeparam name="T">Target type.</typeparam>
        /// <param name="key">Logical cache key.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Deserialized value, or default if missing.</returns>
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a string value by key (sync). Shortcut for <see cref="StringGet"/>.
        /// </summary>
        /// <param name="key">Logical cache key.</param>
        /// <returns>The stored string, or <c>null</c> if missing.</returns>
        string? Get(string key);

        /// <summary>
        /// Gets and deserializes a JSON value by key (sync). Shortcut for <see cref="StringGet{T}"/>.
        /// </summary>
        /// <typeparam name="T">Target type.</typeparam>
        /// <param name="key">Logical cache key.</param>
        /// <returns>Deserialized value, or default if missing.</returns>
        T? Get<T>(string key);

        /// <summary>
        /// Sets a string value. Shortcut for <see cref="StringSetAsync(string, string, int, bool, CancellationToken)"/>.
        /// </summary>
        /// <param name="key">Logical cache key.</param>
        /// <param name="value">String value.</param>
        /// <param name="seconds">TTL in seconds; 0 means no expiry.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task SetAsync(string key, string value, int seconds = 0, CancellationToken cancellationToken = default);

        /// <summary>
        /// Serializes and sets a value as JSON. Shortcut for <see cref="StringSetAsync{T}"/>.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="key">Logical cache key.</param>
        /// <param name="value">Value to serialize.</param>
        /// <param name="seconds">TTL in seconds; 0 means no expiry.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task SetAsync<T>(string key, T value, int seconds = 0, CancellationToken cancellationToken = default);

        #endregion

        #region Key helpers

        /// <summary>
        /// Resets the absolute expiry of an existing key.
        /// </summary>
        /// <param name="key">Logical cache key.</param>
        /// <param name="seconds">New TTL in seconds (must be greater than zero).</param>
        void Refresh(string key, int seconds);

        /// <summary>
        /// Resets the absolute expiry of an existing key.
        /// </summary>
        /// <param name="key">Logical cache key.</param>
        /// <param name="seconds">New TTL in seconds (must be greater than zero).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task RefreshAsync(string key, int seconds, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes the key.
        /// </summary>
        /// <param name="key">Logical cache key.</param>
        void Remove(string key);

        /// <summary>
        /// Deletes the key.
        /// </summary>
        /// <param name="key">Logical cache key.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns whether the key exists.
        /// </summary>
        /// <param name="key">Logical cache key.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task<bool> KeyExistsAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns whether the key exists.
        /// </summary>
        /// <param name="key">Logical cache key.</param>
        bool KeyExists(string key);

        /// <summary>
        /// Returns the Redis type name of the key (string/hash/list/set/zset/none).
        /// </summary>
        /// <param name="key">Logical cache key.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task<string> KeyTypeAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the Redis type name of the key (string/hash/list/set/zset/none).
        /// </summary>
        /// <param name="key">Logical cache key.</param>
        string KeyType(string key);

        /// <summary>
        /// Gets TTL in seconds. Returns -1 if no expiry, -2 if the key does not exist.
        /// </summary>
        /// <param name="key">Logical cache key.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task<long> KeyTimeToLiveAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets TTL in seconds. Returns -1 if no expiry, -2 if the key does not exist.
        /// </summary>
        /// <param name="key">Logical cache key.</param>
        long KeyTimeToLive(string key);

        #endregion

        #region String

        /// <summary>
        /// Gets the string value of <paramref name="key"/> (GET).
        /// </summary>
        /// <param name="key">Logical key.</param>
        /// <returns>Stored string, or <c>null</c> if missing.</returns>
        string? StringGet(string key);

        /// <summary>
        /// Gets the string value of <paramref name="key"/> (GET).
        /// </summary>
        /// <param name="key">Logical key.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Stored string, or <c>null</c> if missing.</returns>
        Task<string?> StringGetAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets and deserializes a JSON value stored at <paramref name="key"/>.
        /// </summary>
        /// <typeparam name="T">Target type.</typeparam>
        /// <param name="key">Logical key.</param>
        /// <returns>Deserialized value, or default if missing.</returns>
        T? StringGet<T>(string key);

        /// <summary>
        /// Gets and deserializes a JSON value stored at <paramref name="key"/>.
        /// </summary>
        /// <typeparam name="T">Target type.</typeparam>
        /// <param name="key">Logical key.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Deserialized value, or default if missing.</returns>
        Task<T?> StringGetAsync<T>(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets a string value (SET). Optionally only when the key does not exist (SET NX).
        /// </summary>
        /// <param name="key">Logical key.</param>
        /// <param name="value">String value.</param>
        /// <param name="seconds">TTL in seconds; 0 means no expiry.</param>
        /// <param name="whenNotExists">When true, uses NX semantics.</param>
        /// <returns><c>true</c> if the value was set.</returns>
        bool StringSet(string key, string value, int seconds = 0, bool whenNotExists = false);

        /// <summary>
        /// Sets a string value (SET). Optionally only when the key does not exist (SET NX).
        /// </summary>
        /// <param name="key">Logical key.</param>
        /// <param name="value">String value.</param>
        /// <param name="seconds">TTL in seconds; 0 means no expiry.</param>
        /// <param name="whenNotExists">When true, uses NX semantics.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns><c>true</c> if the value was set.</returns>
        Task<bool> StringSetAsync(string key, string value, int seconds = 0, bool whenNotExists = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Serializes <paramref name="value"/> as JSON and sets it.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="key">Logical key.</param>
        /// <param name="value">Value to store.</param>
        /// <param name="seconds">TTL in seconds; 0 means no expiry.</param>
        /// <param name="whenNotExists">When true, uses NX semantics.</param>
        /// <returns><c>true</c> if the value was set.</returns>
        bool StringSet<T>(string key, T value, int seconds = 0, bool whenNotExists = false);

        /// <summary>
        /// Serializes <paramref name="value"/> as JSON and sets it.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="key">Logical key.</param>
        /// <param name="value">Value to store.</param>
        /// <param name="seconds">TTL in seconds; 0 means no expiry.</param>
        /// <param name="whenNotExists">When true, uses NX semantics.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns><c>true</c> if the value was set.</returns>
        Task<bool> StringSetAsync<T>(string key, T value, int seconds = 0, bool whenNotExists = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Atomically gets and deletes the key (GETDEL).
        /// </summary>
        /// <param name="key">Logical key.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Previous value, or <c>null</c> if missing.</returns>
        Task<string?> StringGetDeleteAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Increments an integer string by <paramref name="value"/> (INCRBY).
        /// </summary>
        /// <param name="key">Logical key.</param>
        /// <param name="value">Increment amount (default 1).</param>
        /// <returns>Value after increment.</returns>
        long StringIncrement(string key, long value = 1);

        /// <summary>
        /// Increments an integer string by <paramref name="value"/> (INCRBY).
        /// </summary>
        /// <param name="key">Logical key.</param>
        /// <param name="value">Increment amount (default 1).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Value after increment.</returns>
        Task<long> StringIncrementAsync(string key, long value = 1, CancellationToken cancellationToken = default);

        /// <summary>
        /// Increments a floating-point string by <paramref name="value"/> (INCRBYFLOAT).
        /// </summary>
        /// <param name="key">Logical key.</param>
        /// <param name="value">Increment amount.</param>
        /// <returns>Value after increment.</returns>
        double StringIncrement(string key, double value);

        /// <summary>
        /// Increments a floating-point string by <paramref name="value"/> (INCRBYFLOAT).
        /// </summary>
        /// <param name="key">Logical key.</param>
        /// <param name="value">Increment amount.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Value after increment.</returns>
        Task<double> StringIncrementAsync(string key, double value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Decrements an integer string by <paramref name="value"/> (DECRBY).
        /// </summary>
        /// <param name="key">Logical key.</param>
        /// <param name="value">Decrement amount (default 1).</param>
        /// <returns>Value after decrement.</returns>
        long StringDecrement(string key, long value = 1);

        /// <summary>
        /// Decrements an integer string by <paramref name="value"/> (DECRBY).
        /// </summary>
        /// <param name="key">Logical key.</param>
        /// <param name="value">Decrement amount (default 1).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Value after decrement.</returns>
        Task<long> StringDecrementAsync(string key, long value = 1, CancellationToken cancellationToken = default);

        /// <summary>
        /// Appends <paramref name="value"/> to the string (APPEND).
        /// </summary>
        /// <param name="key">Logical key.</param>
        /// <param name="value">Text to append.</param>
        /// <returns>Length of the string after append.</returns>
        long StringAppend(string key, string value);

        /// <summary>
        /// Appends <paramref name="value"/> to the string (APPEND).
        /// </summary>
        /// <param name="key">Logical key.</param>
        /// <param name="value">Text to append.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Length of the string after append.</returns>
        Task<long> StringAppendAsync(string key, string value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the string length (STRLEN).
        /// </summary>
        /// <param name="key">Logical key.</param>
        long StringGetLength(string key);

        /// <summary>
        /// Returns the string length (STRLEN).
        /// </summary>
        /// <param name="key">Logical key.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task<long> StringGetLengthAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets an absolute expiry on the key (EXPIRE).
        /// </summary>
        /// <param name="key">Logical key.</param>
        /// <param name="seconds">TTL in seconds (must be greater than zero).</param>
        /// <returns><c>true</c> if expiry was set.</returns>
        bool StringSetExpiry(string key, int seconds);

        /// <summary>
        /// Sets an absolute expiry on the key (EXPIRE).
        /// </summary>
        /// <param name="key">Logical key.</param>
        /// <param name="seconds">TTL in seconds (must be greater than zero).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns><c>true</c> if expiry was set.</returns>
        Task<bool> StringSetExpiryAsync(string key, int seconds, CancellationToken cancellationToken = default);

        #endregion

        #region Hash

        /// <summary>
        /// Sets a hash field to a string value (HSET).
        /// </summary>
        /// <param name="key">Logical key of the hash.</param>
        /// <param name="field">Hash field name.</param>
        /// <param name="value">Field value.</param>
        /// <returns><c>true</c> if the field was newly created; <c>false</c> if it was updated.</returns>
        bool HashSet(string key, string field, string value);

        /// <summary>
        /// Sets a hash field to a string value (HSET).
        /// </summary>
        /// <param name="key">Logical key of the hash.</param>
        /// <param name="field">Hash field name.</param>
        /// <param name="value">Field value.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns><c>true</c> if the field was newly created; <c>false</c> if it was updated.</returns>
        Task<bool> HashSetAsync(string key, string field, string value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Serializes <paramref name="value"/> as JSON and sets the hash field (HSET).
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="key">Logical key of the hash.</param>
        /// <param name="field">Hash field name.</param>
        /// <param name="value">Value to serialize and store.</param>
        /// <returns><c>true</c> if the field was newly created; <c>false</c> if it was updated.</returns>
        bool HashSet<T>(string key, string field, T value);

        /// <summary>
        /// Serializes <paramref name="value"/> as JSON and sets the hash field (HSET).
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="key">Logical key of the hash.</param>
        /// <param name="field">Hash field name.</param>
        /// <param name="value">Value to serialize and store.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns><c>true</c> if the field was newly created; <c>false</c> if it was updated.</returns>
        Task<bool> HashSetAsync<T>(string key, string field, T value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets multiple hash fields (HMSET / HSET multi). When <paramref name="seconds"/> is greater than zero, also sets key expiry.
        /// </summary>
        /// <param name="key">Logical key of the hash.</param>
        /// <param name="fields">Field/value pairs to set.</param>
        /// <param name="seconds">TTL in seconds; 0 means no expiry change.</param>
        void HashSet(string key, IDictionary<string, string> fields, int seconds = 0);

        /// <summary>
        /// Sets multiple hash fields (HMSET / HSET multi). When <paramref name="seconds"/> is greater than zero, also sets key expiry.
        /// </summary>
        /// <param name="key">Logical key of the hash.</param>
        /// <param name="fields">Field/value pairs to set.</param>
        /// <param name="seconds">TTL in seconds; 0 means no expiry change.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task HashSetAsync(string key, IDictionary<string, string> fields, int seconds = 0, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the string value of a hash field (HGET).
        /// </summary>
        /// <param name="key">Logical key of the hash.</param>
        /// <param name="field">Hash field name.</param>
        /// <returns>Field value, or <c>null</c> if missing.</returns>
        string? HashGet(string key, string field);

        /// <summary>
        /// Gets the string value of a hash field (HGET).
        /// </summary>
        /// <param name="key">Logical key of the hash.</param>
        /// <param name="field">Hash field name.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Field value, or <c>null</c> if missing.</returns>
        Task<string?> HashGetAsync(string key, string field, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets and deserializes a JSON value from a hash field (HGET).
        /// </summary>
        /// <typeparam name="T">Target type.</typeparam>
        /// <param name="key">Logical key of the hash.</param>
        /// <param name="field">Hash field name.</param>
        /// <returns>Deserialized value, or default if missing.</returns>
        T? HashGet<T>(string key, string field);

        /// <summary>
        /// Gets and deserializes a JSON value from a hash field (HGET).
        /// </summary>
        /// <typeparam name="T">Target type.</typeparam>
        /// <param name="key">Logical key of the hash.</param>
        /// <param name="field">Hash field name.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Deserialized value, or default if missing.</returns>
        Task<T?> HashGetAsync<T>(string key, string field, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all fields and values of the hash (HGETALL).
        /// </summary>
        /// <param name="key">Logical key of the hash.</param>
        /// <returns>Dictionary of field/value pairs (empty when the key does not exist).</returns>
        IDictionary<string, string> HashGetAll(string key);

        /// <summary>
        /// Gets all fields and values of the hash (HGETALL).
        /// </summary>
        /// <param name="key">Logical key of the hash.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Dictionary of field/value pairs (empty when the key does not exist).</returns>
        Task<IDictionary<string, string>> HashGetAllAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes one or more hash fields (HDEL).
        /// </summary>
        /// <param name="key">Logical key of the hash.</param>
        /// <param name="fields">Field names to delete.</param>
        /// <returns>Number of fields that were removed.</returns>
        long HashDelete(string key, params string[] fields);

        /// <summary>
        /// Deletes one or more hash fields (HDEL).
        /// </summary>
        /// <param name="key">Logical key of the hash.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="fields">Field names to delete.</param>
        /// <returns>Number of fields that were removed.</returns>
        Task<long> HashDeleteAsync(string key, CancellationToken cancellationToken = default, params string[] fields);

        /// <summary>
        /// Returns whether a hash field exists (HEXISTS).
        /// </summary>
        /// <param name="key">Logical key of the hash.</param>
        /// <param name="field">Hash field name.</param>
        /// <returns><c>true</c> if the field exists.</returns>
        bool HashExists(string key, string field);

        /// <summary>
        /// Returns whether a hash field exists (HEXISTS).
        /// </summary>
        /// <param name="key">Logical key of the hash.</param>
        /// <param name="field">Hash field name.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns><c>true</c> if the field exists.</returns>
        Task<bool> HashExistsAsync(string key, string field, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all field names in the hash (HKEYS).
        /// </summary>
        /// <param name="key">Logical key of the hash.</param>
        /// <returns>Array of field names.</returns>
        string[] HashGetKeys(string key);

        /// <summary>
        /// Gets all field names in the hash (HKEYS).
        /// </summary>
        /// <param name="key">Logical key of the hash.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Array of field names.</returns>
        Task<string[]> HashGetKeysAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all values in the hash (HVALS).
        /// </summary>
        /// <param name="key">Logical key of the hash.</param>
        /// <returns>Array of field values.</returns>
        string[] HashGetValues(string key);

        /// <summary>
        /// Gets all values in the hash (HVALS).
        /// </summary>
        /// <param name="key">Logical key of the hash.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Array of field values.</returns>
        Task<string[]> HashGetValuesAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the number of fields in the hash (HLEN).
        /// </summary>
        /// <param name="key">Logical key of the hash.</param>
        /// <returns>Field count, or 0 if the key does not exist.</returns>
        long HashGetLength(string key);

        /// <summary>
        /// Gets the number of fields in the hash (HLEN).
        /// </summary>
        /// <param name="key">Logical key of the hash.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Field count, or 0 if the key does not exist.</returns>
        Task<long> HashGetLengthAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Increments the integer value of a hash field by <paramref name="value"/> (HINCRBY).
        /// </summary>
        /// <param name="key">Logical key of the hash.</param>
        /// <param name="field">Hash field name.</param>
        /// <param name="value">Amount to add (default 1).</param>
        /// <returns>Value after increment.</returns>
        long HashIncrement(string key, string field, long value = 1);

        /// <summary>
        /// Increments the integer value of a hash field by <paramref name="value"/> (HINCRBY).
        /// </summary>
        /// <param name="key">Logical key of the hash.</param>
        /// <param name="field">Hash field name.</param>
        /// <param name="value">Amount to add (default 1).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Value after increment.</returns>
        Task<long> HashIncrementAsync(string key, string field, long value = 1, CancellationToken cancellationToken = default);

        /// <summary>
        /// Increments the floating-point value of a hash field by <paramref name="value"/> (HINCRBYFLOAT).
        /// </summary>
        /// <param name="key">Logical key of the hash.</param>
        /// <param name="field">Hash field name.</param>
        /// <param name="value">Amount to add.</param>
        /// <returns>Value after increment.</returns>
        double HashIncrement(string key, string field, double value);

        /// <summary>
        /// Increments the floating-point value of a hash field by <paramref name="value"/> (HINCRBYFLOAT).
        /// </summary>
        /// <param name="key">Logical key of the hash.</param>
        /// <param name="field">Hash field name.</param>
        /// <param name="value">Amount to add.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Value after increment.</returns>
        Task<double> HashIncrementAsync(string key, string field, double value, CancellationToken cancellationToken = default);

        #endregion

        #region List

        /// <summary>
        /// Inserts one or more values at the head of the list (LPUSH).
        /// </summary>
        /// <param name="key">Logical key of the list.</param>
        /// <param name="values">Values to push (at least one required).</param>
        /// <returns>List length after the push.</returns>
        long ListLeftPush(string key, params string[] values);

        /// <summary>
        /// Inserts one or more values at the head of the list (LPUSH).
        /// </summary>
        /// <param name="key">Logical key of the list.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="values">Values to push (at least one required).</param>
        /// <returns>List length after the push.</returns>
        Task<long> ListLeftPushAsync(string key, CancellationToken cancellationToken = default, params string[] values);

        /// <summary>
        /// Inserts one or more values at the tail of the list (RPUSH).
        /// </summary>
        /// <param name="key">Logical key of the list.</param>
        /// <param name="values">Values to push (at least one required).</param>
        /// <returns>List length after the push.</returns>
        long ListRightPush(string key, params string[] values);

        /// <summary>
        /// Inserts one or more values at the tail of the list (RPUSH).
        /// </summary>
        /// <param name="key">Logical key of the list.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="values">Values to push (at least one required).</param>
        /// <returns>List length after the push.</returns>
        Task<long> ListRightPushAsync(string key, CancellationToken cancellationToken = default, params string[] values);

        /// <summary>
        /// Removes and returns the first element of the list (LPOP).
        /// </summary>
        /// <param name="key">Logical key of the list.</param>
        /// <returns>Popped value, or <c>null</c> if the list is empty or missing.</returns>
        string? ListLeftPop(string key);

        /// <summary>
        /// Removes and returns the first element of the list (LPOP).
        /// </summary>
        /// <param name="key">Logical key of the list.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Popped value, or <c>null</c> if the list is empty or missing.</returns>
        Task<string?> ListLeftPopAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes and returns the last element of the list (RPOP).
        /// </summary>
        /// <param name="key">Logical key of the list.</param>
        /// <returns>Popped value, or <c>null</c> if the list is empty or missing.</returns>
        string? ListRightPop(string key);

        /// <summary>
        /// Removes and returns the last element of the list (RPOP).
        /// </summary>
        /// <param name="key">Logical key of the list.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Popped value, or <c>null</c> if the list is empty or missing.</returns>
        Task<string?> ListRightPopAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a range of elements (LRANGE). Indexes are inclusive. Use start=0, stop=-1 for all.
        /// </summary>
        /// <param name="key">Logical key of the list.</param>
        /// <param name="start">Start index (0-based; negative indexes count from the end).</param>
        /// <param name="stop">Stop index (inclusive).</param>
        /// <returns>Elements in the requested range.</returns>
        string[] ListRange(string key, long start = 0, long stop = -1);

        /// <summary>
        /// Returns a range of elements (LRANGE). Indexes are inclusive. Use start=0, stop=-1 for all.
        /// </summary>
        /// <param name="key">Logical key of the list.</param>
        /// <param name="start">Start index (0-based; negative indexes count from the end).</param>
        /// <param name="stop">Stop index (inclusive).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Elements in the requested range.</returns>
        Task<string[]> ListRangeAsync(string key, long start = 0, long stop = -1, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the length of the list (LLEN).
        /// </summary>
        /// <param name="key">Logical key of the list.</param>
        /// <returns>Element count, or 0 if the key does not exist.</returns>
        long ListGetLength(string key);

        /// <summary>
        /// Gets the length of the list (LLEN).
        /// </summary>
        /// <param name="key">Logical key of the list.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Element count, or 0 if the key does not exist.</returns>
        Task<long> ListGetLengthAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the element at the given index (LINDEX).
        /// </summary>
        /// <param name="key">Logical key of the list.</param>
        /// <param name="index">Zero-based index (negative indexes count from the end).</param>
        /// <returns>Element value, or <c>null</c> if out of range.</returns>
        string? ListGetByIndex(string key, long index);

        /// <summary>
        /// Gets the element at the given index (LINDEX).
        /// </summary>
        /// <param name="key">Logical key of the list.</param>
        /// <param name="index">Zero-based index (negative indexes count from the end).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Element value, or <c>null</c> if out of range.</returns>
        Task<string?> ListGetByIndexAsync(string key, long index, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the element at the given index (LSET).
        /// </summary>
        /// <param name="key">Logical key of the list.</param>
        /// <param name="index">Zero-based index.</param>
        /// <param name="value">New value.</param>
        void ListSetByIndex(string key, long index, string value);

        /// <summary>
        /// Sets the element at the given index (LSET).
        /// </summary>
        /// <param name="key">Logical key of the list.</param>
        /// <param name="index">Zero-based index.</param>
        /// <param name="value">New value.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task ListSetByIndexAsync(string key, long index, string value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Trims the list to the specified range (LTRIM). Indexes are inclusive.
        /// </summary>
        /// <param name="key">Logical key of the list.</param>
        /// <param name="start">Start index to keep.</param>
        /// <param name="stop">Stop index to keep (inclusive).</param>
        void ListTrim(string key, long start, long stop);

        /// <summary>
        /// Trims the list to the specified range (LTRIM). Indexes are inclusive.
        /// </summary>
        /// <param name="key">Logical key of the list.</param>
        /// <param name="start">Start index to keep.</param>
        /// <param name="stop">Stop index to keep (inclusive).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task ListTrimAsync(string key, long start, long stop, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes elements equal to <paramref name="value"/> (LREM).
        /// </summary>
        /// <param name="key">Logical key of the list.</param>
        /// <param name="value">Value to remove.</param>
        /// <param name="count">Removal count semantics: 0 removes all; positive from head; negative from tail.</param>
        /// <returns>Number of removed elements.</returns>
        long ListRemove(string key, string value, long count = 0);

        /// <summary>
        /// Removes elements equal to <paramref name="value"/> (LREM).
        /// </summary>
        /// <param name="key">Logical key of the list.</param>
        /// <param name="value">Value to remove.</param>
        /// <param name="count">Removal count semantics: 0 removes all; positive from head; negative from tail.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Number of removed elements.</returns>
        Task<long> ListRemoveAsync(string key, string value, long count = 0, CancellationToken cancellationToken = default);

        #endregion

        #region Set

        /// <summary>
        /// Adds one or more members to the set (SADD).
        /// </summary>
        /// <param name="key">Logical key of the set.</param>
        /// <param name="members">Members to add (at least one required).</param>
        /// <returns>Number of members that were newly added.</returns>
        long SetAdd(string key, params string[] members);

        /// <summary>
        /// Adds one or more members to the set (SADD).
        /// </summary>
        /// <param name="key">Logical key of the set.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="members">Members to add (at least one required).</param>
        /// <returns>Number of members that were newly added.</returns>
        Task<long> SetAddAsync(string key, CancellationToken cancellationToken = default, params string[] members);

        /// <summary>
        /// Removes one or more members from the set (SREM).
        /// </summary>
        /// <param name="key">Logical key of the set.</param>
        /// <param name="members">Members to remove (at least one required).</param>
        /// <returns>Number of members that were removed.</returns>
        long SetRemove(string key, params string[] members);

        /// <summary>
        /// Removes one or more members from the set (SREM).
        /// </summary>
        /// <param name="key">Logical key of the set.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="members">Members to remove (at least one required).</param>
        /// <returns>Number of members that were removed.</returns>
        Task<long> SetRemoveAsync(string key, CancellationToken cancellationToken = default, params string[] members);

        /// <summary>
        /// Returns all members of the set (SMEMBERS).
        /// </summary>
        /// <param name="key">Logical key of the set.</param>
        /// <returns>All set members.</returns>
        string[] SetGetMembers(string key);

        /// <summary>
        /// Returns all members of the set (SMEMBERS).
        /// </summary>
        /// <param name="key">Logical key of the set.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>All set members.</returns>
        Task<string[]> SetGetMembersAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the cardinality of the set (SCARD).
        /// </summary>
        /// <param name="key">Logical key of the set.</param>
        /// <returns>Member count, or 0 if the key does not exist.</returns>
        long SetGetLength(string key);

        /// <summary>
        /// Gets the cardinality of the set (SCARD).
        /// </summary>
        /// <param name="key">Logical key of the set.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Member count, or 0 if the key does not exist.</returns>
        Task<long> SetGetLengthAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns whether <paramref name="member"/> is in the set (SISMEMBER).
        /// </summary>
        /// <param name="key">Logical key of the set.</param>
        /// <param name="member">Member to test.</param>
        /// <returns><c>true</c> if the member exists.</returns>
        bool SetContains(string key, string member);

        /// <summary>
        /// Returns whether <paramref name="member"/> is in the set (SISMEMBER).
        /// </summary>
        /// <param name="key">Logical key of the set.</param>
        /// <param name="member">Member to test.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns><c>true</c> if the member exists.</returns>
        Task<bool> SetContainsAsync(string key, string member, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes and returns a random member (SPOP).
        /// </summary>
        /// <param name="key">Logical key of the set.</param>
        /// <returns>Removed member, or <c>null</c> if the set is empty.</returns>
        string? SetPop(string key);

        /// <summary>
        /// Removes and returns a random member (SPOP).
        /// </summary>
        /// <param name="key">Logical key of the set.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Removed member, or <c>null</c> if the set is empty.</returns>
        Task<string?> SetPopAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a random member without removing it (SRANDMEMBER).
        /// </summary>
        /// <param name="key">Logical key of the set.</param>
        /// <returns>Random member, or <c>null</c> if the set is empty.</returns>
        string? SetRandomMember(string key);

        /// <summary>
        /// Returns a random member without removing it (SRANDMEMBER).
        /// </summary>
        /// <param name="key">Logical key of the set.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Random member, or <c>null</c> if the set is empty.</returns>
        Task<string?> SetRandomMemberAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns up to <paramref name="count"/> random members without removing them (SRANDMEMBER count).
        /// </summary>
        /// <param name="key">Logical key of the set.</param>
        /// <param name="count">Number of members to return.</param>
        /// <returns>Random members.</returns>
        string[] SetRandomMembers(string key, long count);

        /// <summary>
        /// Returns up to <paramref name="count"/> random members without removing them (SRANDMEMBER count).
        /// </summary>
        /// <param name="key">Logical key of the set.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="count">Number of members to return.</param>
        /// <returns>Random members.</returns>
        Task<string[]> SetRandomMembersAsync(string key, long count, CancellationToken cancellationToken = default);

        /// <summary>
        /// Moves a member from one set to another (SMOVE).
        /// </summary>
        /// <param name="sourceKey">Source set key.</param>
        /// <param name="destinationKey">Destination set key.</param>
        /// <param name="member">Member to move.</param>
        /// <returns><c>true</c> if the member was moved.</returns>
        bool SetMove(string sourceKey, string destinationKey, string member);

        /// <summary>
        /// Moves a member from one set to another (SMOVE).
        /// </summary>
        /// <param name="sourceKey">Source set key.</param>
        /// <param name="destinationKey">Destination set key.</param>
        /// <param name="member">Member to move.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns><c>true</c> if the member was moved.</returns>
        Task<bool> SetMoveAsync(string sourceKey, string destinationKey, string member, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the intersection of the given sets (SINTER).
        /// </summary>
        /// <param name="keys">Set keys (at least one required).</param>
        /// <returns>Members present in all sets.</returns>
        string[] SetIntersect(params string[] keys);

        /// <summary>
        /// Returns the intersection of the given sets (SINTER).
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="keys">Set keys (at least one required).</param>
        /// <returns>Members present in all sets.</returns>
        Task<string[]> SetIntersectAsync(CancellationToken cancellationToken = default, params string[] keys);

        /// <summary>
        /// Returns the union of the given sets (SUNION).
        /// </summary>
        /// <param name="keys">Set keys (at least one required).</param>
        /// <returns>Members present in any of the sets.</returns>
        string[] SetUnion(params string[] keys);

        /// <summary>
        /// Returns the union of the given sets (SUNION).
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="keys">Set keys (at least one required).</param>
        /// <returns>Members present in any of the sets.</returns>
        Task<string[]> SetUnionAsync(CancellationToken cancellationToken = default, params string[] keys);

        /// <summary>
        /// Returns members of <paramref name="key"/> that are not in any of <paramref name="otherKeys"/> (SDIFF).
        /// </summary>
        /// <param name="key">Primary set key.</param>
        /// <param name="otherKeys">Sets to subtract.</param>
        /// <returns>Difference members.</returns>
        string[] SetDifference(string key, params string[] otherKeys);

        /// <summary>
        /// Returns members of <paramref name="key"/> that are not in any of <paramref name="otherKeys"/> (SDIFF).
        /// </summary>
        /// <param name="key">Primary set key.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="otherKeys">Sets to subtract.</param>
        /// <returns>Difference members.</returns>
        Task<string[]> SetDifferenceAsync(string key, CancellationToken cancellationToken = default, params string[] otherKeys);

        #endregion

        #region SortedSet

        /// <summary>
        /// Adds a member with the given score (ZADD). Updates the score if the member already exists.
        /// </summary>
        /// <param name="key">Logical key of the sorted set.</param>
        /// <param name="member">Member value.</param>
        /// <param name="score">Score for ordering.</param>
        /// <returns><c>true</c> if the member was newly added; <c>false</c> if the score was updated.</returns>
        bool SortedSetAdd(string key, string member, double score);

        /// <summary>
        /// Adds a member with the given score (ZADD). Updates the score if the member already exists.
        /// </summary>
        /// <param name="key">Logical key of the sorted set.</param>
        /// <param name="member">Member value.</param>
        /// <param name="score">Score for ordering.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns><c>true</c> if the member was newly added; <c>false</c> if the score was updated.</returns>
        Task<bool> SortedSetAddAsync(string key, string member, double score, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds multiple members with scores (ZADD).
        /// </summary>
        /// <param name="key">Logical key of the sorted set.</param>
        /// <param name="entries">Members and scores to add.</param>
        /// <returns>Number of members newly added (not counting score updates).</returns>
        long SortedSetAdd(string key, IEnumerable<RedisSortedSetEntry> entries);

        /// <summary>
        /// Adds multiple members with scores (ZADD).
        /// </summary>
        /// <param name="key">Logical key of the sorted set.</param>
        /// <param name="entries">Members and scores to add.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Number of members newly added (not counting score updates).</returns>
        Task<long> SortedSetAddAsync(string key, IEnumerable<RedisSortedSetEntry> entries, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a member (ZREM).
        /// </summary>
        /// <param name="key">Logical key of the sorted set.</param>
        /// <param name="member">Member to remove.</param>
        /// <returns><c>true</c> if the member was removed.</returns>
        bool SortedSetRemove(string key, string member);

        /// <summary>
        /// Removes a member (ZREM).
        /// </summary>
        /// <param name="key">Logical key of the sorted set.</param>
        /// <param name="member">Member to remove.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns><c>true</c> if the member was removed.</returns>
        Task<bool> SortedSetRemoveAsync(string key, string member, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes one or more members (ZREM).
        /// </summary>
        /// <param name="key">Logical key of the sorted set.</param>
        /// <param name="members">Members to remove.</param>
        /// <returns>Number of members removed.</returns>
        long SortedSetRemove(string key, params string[] members);

        /// <summary>
        /// Removes one or more members (ZREM).
        /// </summary>
        /// <param name="key">Logical key of the sorted set.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="members">Members to remove.</param>
        /// <returns>Number of members removed.</returns>
        Task<long> SortedSetRemoveAsync(string key, CancellationToken cancellationToken = default, params string[] members);

        /// <summary>
        /// Gets the score of a member (ZSCORE).
        /// </summary>
        /// <param name="key">Logical key of the sorted set.</param>
        /// <param name="member">Member whose score to read.</param>
        /// <returns>Score, or <c>null</c> if the member does not exist.</returns>
        double? SortedSetGetScore(string key, string member);

        /// <summary>
        /// Gets the score of a member (ZSCORE).
        /// </summary>
        /// <param name="key">Logical key of the sorted set.</param>
        /// <param name="member">Member whose score to read.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Score, or <c>null</c> if the member does not exist.</returns>
        Task<double?> SortedSetGetScoreAsync(string key, string member, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the rank (0-based index) of a member (ZRANK / ZREVRANK).
        /// </summary>
        /// <param name="key">Logical key of the sorted set.</param>
        /// <param name="member">Member whose rank to read.</param>
        /// <param name="ascending">When <c>true</c>, uses ZRANK; when <c>false</c>, uses ZREVRANK.</param>
        /// <returns>Rank, or <c>null</c> if the member does not exist.</returns>
        long? SortedSetGetRank(string key, string member, bool ascending = true);

        /// <summary>
        /// Gets the rank (0-based index) of a member (ZRANK / ZREVRANK).
        /// </summary>
        /// <param name="key">Logical key of the sorted set.</param>
        /// <param name="member">Member whose rank to read.</param>
        /// <param name="ascending">When <c>true</c>, uses ZRANK; when <c>false</c>, uses ZREVRANK.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Rank, or <c>null</c> if the member does not exist.</returns>
        Task<long?> SortedSetGetRankAsync(string key, string member, bool ascending = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns members by rank range (ZRANGE / ZREVRANGE). Indexes are inclusive.
        /// </summary>
        /// <param name="key">Logical key of the sorted set.</param>
        /// <param name="start">Start rank (0-based; use -1 for the last element).</param>
        /// <param name="stop">Stop rank (inclusive).</param>
        /// <param name="ascending">When <c>true</c>, ascending by score; otherwise descending.</param>
        /// <returns>Members in the requested rank range.</returns>
        string[] SortedSetRangeByRank(string key, long start = 0, long stop = -1, bool ascending = true);

        /// <summary>
        /// Returns members by rank range (ZRANGE / ZREVRANGE). Indexes are inclusive.
        /// </summary>
        /// <param name="key">Logical key of the sorted set.</param>
        /// <param name="start">Start rank (0-based; use -1 for the last element).</param>
        /// <param name="stop">Stop rank (inclusive).</param>
        /// <param name="ascending">When <c>true</c>, ascending by score; otherwise descending.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Members in the requested rank range.</returns>
        Task<string[]> SortedSetRangeByRankAsync(
            string key,
            long start = 0,
            long stop = -1,
            bool ascending = true,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns members with scores by rank range (ZRANGE / ZREVRANGE WITHSCORES).
        /// </summary>
        /// <param name="key">Logical key of the sorted set.</param>
        /// <param name="start">Start rank (0-based).</param>
        /// <param name="stop">Stop rank (inclusive).</param>
        /// <param name="ascending">When <c>true</c>, ascending by score; otherwise descending.</param>
        /// <returns>Members and scores in the requested rank range.</returns>
        RedisSortedSetEntry[] SortedSetRangeByRankWithScores(string key, long start = 0, long stop = -1, bool ascending = true);

        /// <summary>
        /// Returns members with scores by rank range (ZRANGE / ZREVRANGE WITHSCORES).
        /// </summary>
        /// <param name="key">Logical key of the sorted set.</param>
        /// <param name="start">Start rank (0-based).</param>
        /// <param name="stop">Stop rank (inclusive).</param>
        /// <param name="ascending">When <c>true</c>, ascending by score; otherwise descending.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Members and scores in the requested rank range.</returns>
        Task<RedisSortedSetEntry[]> SortedSetRangeByRankWithScoresAsync(
            string key,
            long start = 0,
            long stop = -1,
            bool ascending = true,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns members with scores between <paramref name="min"/> and <paramref name="max"/> (ZRANGEBYSCORE / ZREVRANGEBYSCORE).
        /// </summary>
        /// <param name="key">Logical key of the sorted set.</param>
        /// <param name="min">Minimum score (inclusive).</param>
        /// <param name="max">Maximum score (inclusive).</param>
        /// <param name="ascending">When <c>true</c>, ascending by score; otherwise descending.</param>
        /// <returns>Members in the score range.</returns>
        string[] SortedSetRangeByScore(string key, double min = double.NegativeInfinity, double max = double.PositiveInfinity, bool ascending = true);

        /// <summary>
        /// Returns members with scores between <paramref name="min"/> and <paramref name="max"/> (ZRANGEBYSCORE / ZREVRANGEBYSCORE).
        /// </summary>
        /// <param name="key">Logical key of the sorted set.</param>
        /// <param name="min">Minimum score (inclusive).</param>
        /// <param name="max">Maximum score (inclusive).</param>
        /// <param name="ascending">When <c>true</c>, ascending by score; otherwise descending.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Members in the score range.</returns>
        Task<string[]> SortedSetRangeByScoreAsync(
            string key,
            double min = double.NegativeInfinity,
            double max = double.PositiveInfinity,
            bool ascending = true,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns members with scores between <paramref name="min"/> and <paramref name="max"/> (ZRANGEBYSCORE / ZREVRANGEBYSCORE WITHSCORES).
        /// </summary>
        /// <param name="key">Logical key of the sorted set.</param>
        /// <param name="min">Minimum score (inclusive).</param>
        /// <param name="max">Maximum score (inclusive).</param>
        /// <param name="ascending">When <c>true</c>, ascending by score; otherwise descending.</param>
        /// <returns>Members and scores in the score range.</returns>
        RedisSortedSetEntry[] SortedSetRangeByScoreWithScores(
            string key,
            double min = double.NegativeInfinity,
            double max = double.PositiveInfinity,
            bool ascending = true);

        /// <summary>
        /// Returns members with scores between <paramref name="min"/> and <paramref name="max"/> (ZRANGEBYSCORE / ZREVRANGEBYSCORE WITHSCORES).
        /// </summary>
        /// <param name="key">Logical key of the sorted set.</param>
        /// <param name="min">Minimum score (inclusive).</param>
        /// <param name="max">Maximum score (inclusive).</param>
        /// <param name="ascending">When <c>true</c>, ascending by score; otherwise descending.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Members and scores in the score range.</returns>
        Task<RedisSortedSetEntry[]> SortedSetRangeByScoreWithScoresAsync(
            string key,
            double min = double.NegativeInfinity,
            double max = double.PositiveInfinity,
            bool ascending = true,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the number of members in the sorted set (ZCARD).
        /// </summary>
        /// <param name="key">Logical key of the sorted set.</param>
        /// <returns>Member count, or 0 if the key does not exist.</returns>
        long SortedSetGetLength(string key);

        /// <summary>
        /// Gets the number of members in the sorted set (ZCARD).
        /// </summary>
        /// <param name="key">Logical key of the sorted set.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Member count, or 0 if the key does not exist.</returns>
        Task<long> SortedSetGetLengthAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Counts members with scores between <paramref name="min"/> and <paramref name="max"/> (ZCOUNT).
        /// </summary>
        /// <param name="key">Logical key of the sorted set.</param>
        /// <param name="min">Minimum score (inclusive).</param>
        /// <param name="max">Maximum score (inclusive).</param>
        /// <returns>Number of members in the score range.</returns>
        long SortedSetCountByScore(string key, double min, double max);

        /// <summary>
        /// Counts members with scores between <paramref name="min"/> and <paramref name="max"/> (ZCOUNT).
        /// </summary>
        /// <param name="key">Logical key of the sorted set.</param>
        /// <param name="min">Minimum score (inclusive).</param>
        /// <param name="max">Maximum score (inclusive).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Number of members in the score range.</returns>
        Task<long> SortedSetCountByScoreAsync(string key, double min, double max, CancellationToken cancellationToken = default);

        /// <summary>
        /// Increments the score of a member by <paramref name="value"/> (ZINCRBY).
        /// </summary>
        /// <param name="key">Logical key of the sorted set.</param>
        /// <param name="member">Member whose score to increment.</param>
        /// <param name="value">Amount to add to the score.</param>
        /// <returns>New score after increment.</returns>
        double SortedSetIncrementScore(string key, string member, double value);

        /// <summary>
        /// Increments the score of a member by <paramref name="value"/> (ZINCRBY).
        /// </summary>
        /// <param name="key">Logical key of the sorted set.</param>
        /// <param name="member">Member whose score to increment.</param>
        /// <param name="value">Amount to add to the score.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>New score after increment.</returns>
        Task<double> SortedSetIncrementScoreAsync(string key, string member, double value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes members by rank range (ZREMRANGEBYRANK). Indexes are inclusive.
        /// </summary>
        /// <param name="key">Logical key of the sorted set.</param>
        /// <param name="start">Start rank.</param>
        /// <param name="stop">Stop rank (inclusive).</param>
        /// <returns>Number of members removed.</returns>
        long SortedSetRemoveRangeByRank(string key, long start, long stop);

        /// <summary>
        /// Removes members by rank range (ZREMRANGEBYRANK). Indexes are inclusive.
        /// </summary>
        /// <param name="key">Logical key of the sorted set.</param>
        /// <param name="start">Start rank.</param>
        /// <param name="stop">Stop rank (inclusive).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Number of members removed.</returns>
        Task<long> SortedSetRemoveRangeByRankAsync(string key, long start, long stop, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes members by score range (ZREMRANGEBYSCORE).
        /// </summary>
        /// <param name="key">Logical key of the sorted set.</param>
        /// <param name="min">Minimum score (inclusive).</param>
        /// <param name="max">Maximum score (inclusive).</param>
        /// <returns>Number of members removed.</returns>
        long SortedSetRemoveRangeByScore(string key, double min, double max);

        /// <summary>
        /// Removes members by score range (ZREMRANGEBYSCORE).
        /// </summary>
        /// <param name="key">Logical key of the sorted set.</param>
        /// <param name="min">Minimum score (inclusive).</param>
        /// <param name="max">Maximum score (inclusive).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Number of members removed.</returns>
        Task<long> SortedSetRemoveRangeByScoreAsync(string key, double min, double max, CancellationToken cancellationToken = default);

        #endregion
    }
}
