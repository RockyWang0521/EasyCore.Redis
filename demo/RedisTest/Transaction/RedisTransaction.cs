using EasyCore.DistributedCache.Cache;
using EasyCore.DistributedCache.Transaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisTest.Transaction
{
    public class RedisTransaction : IRedisTransaction
    {
        private readonly IDistributedTransaction _transaction;

        private readonly IDistributedCache _cache;

        public RedisTransaction(IDistributedTransaction transaction,
            IDistributedCache cache)
        {
            _transaction = transaction;
            _cache = cache;
        }

        public async Task Transaction()
        {
            using (var tran = _transaction.CreateTransaction())
            {
                _cache.Set("key1", "value1");
                _cache.Set("key2", "value2");
                _cache.Set("key3", "value3");
                await tran.CommitAsync();
            }
        }
    }
}
