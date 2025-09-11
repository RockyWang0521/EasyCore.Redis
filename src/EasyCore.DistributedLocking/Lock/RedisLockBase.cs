using StackExchange.Redis;

namespace EasyCore.DistributedLocking.Lock
{
    internal class RedisLockBase : IAsyncDisposable, IDisposable
    {
        protected readonly IDatabase _database;
        protected readonly LockContext _lockContext;
        private DistributedOption _option;

        const string unscript = @"local getLock = redis.call('GET', KEYS[1])
                                                  if getLock == ARGV[1] then
                                                     redis.call('DEL', KEYS[1])
                                                     return 1
                                                  end
                                                  return 0";

        public string GetLockName(string key) => $"{_option.DistributedName}:{key}";

        internal RedisLockBase(IDatabase database, LockContext lockContext, DistributedOption option)
        {
            _database = database;
            _lockContext = lockContext;
            _option = option;
        }

        protected RedisLockBase GetRedisLockBase() => this;

        public async Task<bool> UnLockAsync(LockContext lockContext)
        {
            var lockName = GetLockName(lockContext.Key);

            RedisKey[] scriptkey = { lockName };

            RedisValue[] scriptvalues = { lockContext.LockId.ToString() };

            return (await _database.ScriptEvaluateAsync(unscript, scriptkey, scriptvalues)).ToString() == "1";
        }

        public bool UnLock(LockContext lockContext)
        {
            var lockName = GetLockName(lockContext.Key);

            RedisKey[] scriptkey = { lockName };

            RedisValue[] scriptvalues = { lockContext.LockId.ToString() };

            return _database.ScriptEvaluate(unscript, scriptkey, scriptvalues).ToString() == "1";
        }

        public async ValueTask DisposeAsync()
        {
            if (_lockContext != null) await UnLockAsync(_lockContext);
        }

        public void Dispose()
        {
            if (_lockContext != null) UnLock(_lockContext);
        }
    }
}
