using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisTest.Lock
{
    public interface IRedisLock
    {
        Task<bool> AcquireLock(string key, Guid lockId, int seconds);

        Task<bool> BlockingLock(string key, Guid lockId, int seconds, int blockingSeconds);

        Task<bool> RenewableBlockingLock(string key, Guid lockId, int seconds, int blockingSeconds, int renewalSeconds);

        Task<bool> UnLock (string key, Guid lockId);
    }
}
