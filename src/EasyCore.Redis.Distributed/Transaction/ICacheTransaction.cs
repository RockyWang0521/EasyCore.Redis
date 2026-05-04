namespace EasyCore.Redis.Distributed.Transaction
{
    /// <summary>
    /// A Redis MULTI/EXEC transaction for cache write operations.
    /// Queue commands via fluent methods, then call <see cref="Commit"/> or <see cref="CommitAsync"/>.
    /// Disposing without commit discards the transaction (does not execute).
    /// </summary>
    public interface ICacheTransaction : IAsyncDisposable, IDisposable
    {
        /// <summary>
        /// Queues a string set (SET). Pass <paramref name="seconds"/> &gt; 0 to set expiry.
        /// </summary>
        /// <param name="key">Logical cache key.</param>
        /// <param name="value">String value.</param>
        /// <param name="seconds">TTL in seconds; 0 means no expiry.</param>
        /// <returns>This transaction for fluent chaining.</returns>
        ICacheTransaction Set(string key, string value, int seconds = 0);

        /// <summary>
        /// Queues a JSON-serialized set (SET).
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="key">Logical cache key.</param>
        /// <param name="value">Value to serialize and store.</param>
        /// <param name="seconds">TTL in seconds; 0 means no expiry.</param>
        /// <returns>This transaction for fluent chaining.</returns>
        ICacheTransaction Set<T>(string key, T value, int seconds = 0);

        /// <summary>
        /// Queues a key delete (DEL).
        /// </summary>
        /// <param name="key">Logical cache key.</param>
        /// <returns>This transaction for fluent chaining.</returns>
        ICacheTransaction Remove(string key);

        /// <summary>
        /// Executes the queued commands (EXEC).
        /// </summary>
        /// <returns><c>true</c> if the transaction executed successfully.</returns>
        bool Commit();

        /// <summary>
        /// Executes the queued commands (EXEC).
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns><c>true</c> if the transaction executed successfully.</returns>
        Task<bool> CommitAsync(CancellationToken cancellationToken = default);
    }
}
