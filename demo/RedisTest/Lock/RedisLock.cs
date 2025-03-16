using EasyCore.DistributedCache.Cache;
using EasyCore.DistributedLocking.Lock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisTest.Lock
{
    public class RedisLock : IRedisLock
    {
        private readonly IDistributedLock _lock;

        public RedisLock(IDistributedLock locke) => _lock = locke;

        public async Task<bool> AcquireLock(string key, Guid lockId, int seconds) =>
             await _lock.AcquireLockAsync(key, lockId, 100);

        public async Task<bool> BlockingLock(string key, Guid lockId, int seconds, int blockingSeconds) =>
            await _lock.BlockingLockAsync(key, lockId, seconds, blockingSeconds);

        public async Task<bool> RenewableBlockingLock(string key, Guid lockId, int seconds, int blockingSeconds, int renewalSeconds) =>
            await _lock.BlockingLockAsync(key, lockId, seconds, blockingSeconds, renewalSeconds);

        public async Task<bool> UnLock(string key, Guid lockId) => 
            await _lock.UnLockAsync(key, lockId);
    }
}
