using EasyCore.Redis.Distributed.Connection;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace EasyCore.Redis.Distributed.Transaction
{
    /// <summary>
    /// Internal MULTI/EXEC transaction scope implementing <see cref="ICacheTransaction"/>.
    /// </summary>
    internal sealed class CacheTransaction : ICacheTransaction
    {
        private static readonly JsonSerializerSettings JsonSettings = new()
        {
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        private readonly IRedisConnection _connection;
        private readonly ITransaction _transaction;
        private int _state; // 0 = open, 1 = committed, 2 = disposed

        /// <summary>
        /// Creates a new Redis transaction on the shared connection's default database.
        /// </summary>
        /// <param name="connection">Shared Redis connection.</param>
        public CacheTransaction(IRedisConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _transaction = _connection.GetDatabase().CreateTransaction();
        }

        /// <inheritdoc />
        public ICacheTransaction Set(string key, string value, int seconds = 0)
        {
            EnsureOpen();
            ArgumentNullException.ThrowIfNull(value);

            var redisKey = _connection.GetPrefixedKey(key);
            var expiry = seconds > 0 ? TimeSpan.FromSeconds(seconds) : (TimeSpan?)null;
            _ = _transaction.StringSetAsync(redisKey, value, expiry);
            return this;
        }

        /// <inheritdoc />
        public ICacheTransaction Set<T>(string key, T value, int seconds = 0)
        {
            ArgumentNullException.ThrowIfNull(value);
            var json = JsonConvert.SerializeObject(value, JsonSettings);
            return Set(key, json, seconds);
        }

        /// <inheritdoc />
        public ICacheTransaction Remove(string key)
        {
            EnsureOpen();
            _ = _transaction.KeyDeleteAsync(_connection.GetPrefixedKey(key));
            return this;
        }

        /// <inheritdoc />
        public bool Commit()
        {
            EnsureOpen();
            var ok = _transaction.Execute();
            Interlocked.Exchange(ref _state, 1);
            return ok;
        }

        /// <inheritdoc />
        public async Task<bool> CommitAsync(CancellationToken cancellationToken = default)
        {
            EnsureOpen();
            cancellationToken.ThrowIfCancellationRequested();
            var ok = await _transaction.ExecuteAsync().ConfigureAwait(false);
            Interlocked.Exchange(ref _state, 1);
            return ok;
        }

        /// <summary>
        /// Marks the scope disposed. Does not auto-EXEC; uncommitted work is discarded.
        /// </summary>
        public void Dispose()
        {
            // Discard if not committed — do not auto-EXEC.
            Interlocked.Exchange(ref _state, 2);
        }

        /// <summary>
        /// Asynchronously disposes the scope (same semantics as <see cref="Dispose"/>).
        /// </summary>
        /// <returns>A completed value task.</returns>
        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }

        private void EnsureOpen()
        {
            var state = Volatile.Read(ref _state);
            if (state == 1)
                throw new InvalidOperationException("Transaction has already been committed.");
            if (state == 2)
                throw new ObjectDisposedException(nameof(CacheTransaction));
        }
    }
}
