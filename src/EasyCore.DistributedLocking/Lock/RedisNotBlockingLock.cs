using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace EasyCore.DistributedLocking.Lock
{
    internal class RedisNotBlockingLock : RedisLockBase
    {
        private ILogger _logger;

        const string lockscript = @"local isNX = redis.call('SETNX', KEYS[1], ARGV[1])
                                                     if isNX == 1 then
                                                        redis.call('PEXPIRE', KEYS[1], ARGV[2])
                                                        return 1
                                                    end
                                                    return 0";

        public RedisNotBlockingLock(IDatabase database, LockContext lockContext, DistributedOption option, ILogger logger) : base(database, lockContext, option)
        {
            _logger = logger;
        }

        public LockContext AcquireLock(int seconds)
        {
            var lockname = GetLockName(_lockContext.Key);

            return AcquireLock(lockname, _lockContext.LockId, seconds, _database!);
        }

        public LockContext AcquireLock(TimeSpan seconds)
        {
            int sds = seconds.Seconds;

            var lockname = GetLockName(_lockContext.Key);

            return AcquireLock(lockname, _lockContext.LockId, sds, _database!);
        }

        public async Task<LockContext> AcquireLockAsync(int seconds)
        {
            var lockname = GetLockName(_lockContext.Key);

            return await AcquireLockAsync(lockname, _lockContext.LockId, seconds, _database!);
        }

        public async Task<LockContext> AcquireLockAsync(TimeSpan seconds)
        {
            int sds = seconds.Seconds;

            var lockname = GetLockName(_lockContext.Key);

            return await AcquireLockAsync(lockname, _lockContext.LockId, sds, _database!);
        }

        public LockContext AcquireLock(string key, Guid lockId, int seconds, IDatabase database)
        {
            RedisKey[] scriptkey = { key };

            RedisValue[] scriptvalues = { lockId.ToString(), seconds * 1000 };

            _lockContext.IsAcquired = database.ScriptEvaluate(lockscript, scriptkey, scriptvalues).ToString() == "1";

            _lockContext._base = base.GetRedisLockBase();

            return _lockContext;
        }

        public async Task<LockContext> AcquireLockAsync(string key, Guid lockId, int seconds, IDatabase database)
        {
            RedisKey[] scriptkey = { key };

            RedisValue[] scriptvalues = { lockId.ToString(), seconds * 1000 };

            _lockContext.IsAcquired = (await database.ScriptEvaluateAsync(lockscript, scriptkey, scriptvalues)).ToString() == "1";

            _lockContext._base = base.GetRedisLockBase();

            return _lockContext;
        }
    }
}
