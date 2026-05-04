using EasyCore.Redis.Distributed.Transaction;

namespace Web.EasyCore.Cache.Services.Transaction
{
    public class RedisTransaction : IRedisTransaction
    {
        private readonly IDistributedTransaction _transaction;

        public RedisTransaction(IDistributedTransaction transaction)
            => _transaction = transaction;

        public async Task Transaction()
        {
            await using var tran = _transaction.CreateTransaction();
            tran.Set("key1", "value1", seconds: 60)
                .Set("key2", "value2", seconds: 60)
                .Set("key3", "value3", seconds: 60);
            await tran.CommitAsync();
        }
    }
}
