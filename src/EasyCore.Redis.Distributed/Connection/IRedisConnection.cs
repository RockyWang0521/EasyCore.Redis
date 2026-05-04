using StackExchange.Redis;

namespace EasyCore.Redis.Distributed.Connection
{
    /// <summary>
    /// Shared Redis connection (singleton). Owns the multiplexer lifecycle.
    /// </summary>
    public interface IRedisConnection : IAsyncDisposable, IDisposable
    {
        /// <summary>
        /// Underlying StackExchange.Redis connection multiplexer.
        /// </summary>
        IConnectionMultiplexer Multiplexer { get; }

        /// <summary>
        /// Gets a database using the configured default, or an explicit override.
        /// </summary>
        /// <param name="database">Optional database index; when <c>null</c>, uses the configured default database.</param>
        /// <returns>Redis database handle.</returns>
        IDatabase GetDatabase(int? database = null);

        /// <summary>
        /// Applies the configured key prefix.
        /// </summary>
        /// <param name="key">Logical key.</param>
        /// <returns>Prefixed Redis key.</returns>
        string GetPrefixedKey(string key);
    }
}
