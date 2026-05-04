namespace EasyCore.Redis.Distributed.Transaction
{
    /// <summary>
    /// Factory for Redis MULTI/EXEC cache transactions.
    /// </summary>
    public interface IDistributedTransaction
    {
        /// <summary>
        /// Creates a new transaction scope. Queue operations on the returned scope, then commit.
        /// Disposing without commit discards the transaction (does not execute).
        /// </summary>
        /// <returns>A new <see cref="ICacheTransaction"/> scope.</returns>
        ICacheTransaction CreateTransaction();
    }
}
