using EasyCore.Redis.Distributed.Connection;

namespace EasyCore.Redis.Distributed.Transaction
{
    /// <summary>
    /// Creates Redis MULTI/EXEC transaction scopes.
    /// </summary>
    public sealed class DistributedTransaction : IDistributedTransaction
    {
        private readonly IRedisConnection _connection;

        /// <summary>
        /// Initializes a new transaction factory over the shared Redis connection.
        /// </summary>
        /// <param name="connection">Shared Redis connection.</param>
        public DistributedTransaction(IRedisConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        /// <inheritdoc />
        public ICacheTransaction CreateTransaction()
            => new CacheTransaction(_connection);
    }
}
