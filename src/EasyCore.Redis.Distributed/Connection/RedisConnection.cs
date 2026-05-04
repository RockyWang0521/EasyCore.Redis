using EasyCore.Redis.Distributed;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace EasyCore.Redis.Distributed.Connection
{
    /// <summary>
    /// Singleton Redis connection wrapper. Disposes the multiplexer on host shutdown.
    /// </summary>
    public sealed class RedisConnection : IRedisConnection
    {
        private readonly DistributedOption _option;
        private readonly ConnectionMultiplexer _multiplexer;
        private int _disposed;

        /// <summary>
        /// Connects to Redis using the configured <see cref="DistributedOption"/>.
        /// </summary>
        /// <param name="options">Options monitor providing <see cref="DistributedOption"/>.</param>
        public RedisConnection(IOptions<DistributedOption> options)
        {
            ArgumentNullException.ThrowIfNull(options);
            _option = options.Value ?? throw new ArgumentNullException(nameof(options));
            _multiplexer = ConnectionMultiplexer.Connect(_option.ToConfigurationOptions());
        }

        /// <inheritdoc />
        public IConnectionMultiplexer Multiplexer
        {
            get
            {
                ThrowIfDisposed();
                return _multiplexer;
            }
        }

        /// <inheritdoc />
        public IDatabase GetDatabase(int? database = null)
        {
            ThrowIfDisposed();
            return _multiplexer.GetDatabase(database ?? _option.DefaultDatabase);
        }

        /// <inheritdoc />
        public string GetPrefixedKey(string key) => _option.GetPrefixedKey(key);

        /// <summary>
        /// Disposes the underlying connection multiplexer.
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            _multiplexer.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Asynchronously disposes the underlying connection multiplexer.
        /// </summary>
        /// <returns>A completed value task when disposal finishes.</returns>
        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            await _multiplexer.DisposeAsync().ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }

        private void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(_disposed != 0, this);
        }
    }
}
