using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisTest.Lock
{
    public interface IRedisLock
    {
        Task UsingAcquireLockAsync(string key, int seconds);

        void UsingAcquireLock(string key, int seconds);

        Task AcquireLockAsync(string key, int seconds);

        void AcquireLock(string key, int seconds);

        Task UsingBlockingLockAsync(string key, int seconds, int blockingSeconds);

        void UsingBlockingLock(string key, int seconds, int blockingSeconds);

        Task BlockingLockAsync(string key, int seconds, int blockingSeconds);

        void BlockingLock(string key, int seconds, int blockingSeconds);

        Task UsingRenewableBlockingLockAsync(string key, int seconds, int blockingSeconds, int renewalSeconds);

        void UsingRenewableBlockingLock(string key, int seconds, int blockingSeconds, int renewalSeconds);

        Task RenewableBlockingLockAsync(string key, int seconds, int blockingSeconds, int renewalSeconds);

        void RenewableBlockingLock(string key, int seconds, int blockingSeconds, int renewalSeconds);

        Task UsingBlockingLockTestAsync(string key, int seconds, int blockingSeconds);
    }
}
