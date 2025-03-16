using EasyCore.DistributedCache.Cache;
using StackExchange.Redis;

namespace EasyCore.DistributedCache.Transaction
{
    public class DistributedTransaction : IDistributedTransaction
    {
        private readonly ITransaction _transaction;

        private readonly IDistributedCache _cache;

        public DistributedTransaction(IDistributedCache cache)
        {
            _cache = cache;
            _transaction = _cache.GetTransaction();
        }

        public bool Commit()
            => _transaction.Execute();

        public Task<bool> CommitAsync()
            => _transaction.ExecuteAsync();

        public DistributedTransaction CreateTransaction() => this;

        public void Dispose()
            => _transaction.Execute();
    }
}
