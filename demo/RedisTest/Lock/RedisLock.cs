using EasyCore.DistributedLocking.Lock;
using System.Diagnostics;

namespace RedisTest.Lock
{
    public class RedisLock : IRedisLock
    {
        private readonly IDistributedLock _lock;

        public RedisLock(IDistributedLock locke) => _lock = locke;

        public async Task UsingAcquireLockAsync(string key, int seconds)
        {
            using var context = await _lock.AcquireLockAsync(key, 100);

            if (context.IsAcquired)
            {
                Console.WriteLine($"Lock acquired for {context.Key}---{context.LockId}");
            }
        }

        public void UsingAcquireLock(string key, int seconds)
        {
            using var context = _lock.AcquireLock(key, 100);

            if (context.IsAcquired)
            {
                Console.WriteLine($"Lock acquired for {context.Key}---{context.LockId}");
            }
        }

        public async Task AcquireLockAsync(string key, int seconds)
        {
            var context = await _lock.AcquireLockAsync(key, 100);

            if (context.IsAcquired)
            {
                Console.WriteLine($"Lock acquired for {context.Key}---{context.LockId}");
            }

            await _lock.UnLockAsync(context);
        }

        public void AcquireLock(string key, int seconds)
        {
            var context = _lock.AcquireLock(key, 100);

            if (context.IsAcquired)
            {
                Console.WriteLine($"Lock acquired for {context.Key}---{context.LockId}");
            }

            _lock.UnLock(context);
        }

        public async Task UsingBlockingLockAsync(string key, int seconds, int blockingSeconds)
        {
            using var context = await _lock.BlockingLockAsync(key, seconds, blockingSeconds);

            if (context.IsAcquired)
            {
                Console.WriteLine($"Lock acquired for {context.Key}---{context.LockId}");
            }
        }

        public void UsingBlockingLock(string key, int seconds, int blockingSeconds)
        {
            using var context = _lock.BlockingLock(key, seconds, blockingSeconds);

            if (context.IsAcquired)
            {
                Console.WriteLine($"Lock acquired for {context.Key}---{context.LockId}");
            }
        }

        public async Task BlockingLockAsync(string key, int seconds, int blockingSeconds)
        {
            var context = await _lock.BlockingLockAsync(key, seconds, blockingSeconds);

            if (context.IsAcquired)
            {
                Console.WriteLine($"Lock acquired for {context.Key}---{context.LockId}");
            }

            await _lock.UnLockAsync(context);
        }

        public void BlockingLock(string key, int seconds, int blockingSeconds)
        {
            var context = _lock.BlockingLock(key, seconds, blockingSeconds);

            if (context.IsAcquired)
            {
                Console.WriteLine($"Lock acquired for {context.Key}---{context.LockId}");
            }

            _lock.UnLock(context);
        }

        public async Task UsingRenewableBlockingLockAsync(string key, int seconds, int blockingSeconds, int renewalSeconds)
        {
            using var context = await _lock.BlockingLockAsync(key, seconds, blockingSeconds, renewalSeconds);

            if (context.IsAcquired)
            {
                Console.WriteLine($"Lock acquired for {context.Key}---{context.LockId}");
            }
        }

        public void UsingRenewableBlockingLock(string key, int seconds, int blockingSeconds, int renewalSeconds)
        {
            using var context = _lock.BlockingLock(key, seconds, blockingSeconds, renewalSeconds);

            if (context.IsAcquired)
            {
                Console.WriteLine($"Lock acquired for {context.Key}---{context.LockId}");
            }
        }

        public async Task RenewableBlockingLockAsync(string key, int seconds, int blockingSeconds, int renewalSeconds)
        {
            var context = await _lock.BlockingLockAsync(key, seconds, blockingSeconds, renewalSeconds);

            if (context.IsAcquired)
            {
                Console.WriteLine($"Lock acquired for {context.Key}---{context.LockId}");
            }

            _lock.UnLock(context);
        }

        public void RenewableBlockingLock(string key, int seconds, int blockingSeconds, int renewalSeconds)
        {
            var context = _lock.BlockingLock(key, seconds, blockingSeconds, renewalSeconds);

            if (context.IsAcquired)
            {
                Console.WriteLine($"Lock acquired for {context.Key}---{context.LockId}");
            }

            _lock.UnLock(context);
        }

        public async Task UsingBlockingLockTestAsync(string key, int seconds, int blockingSeconds)
        {
            Console.WriteLine($"******************************** 开始 ********************************");

            Stopwatch watch = new Stopwatch();

            watch.Start();

            using var context = await _lock.BlockingLockAsync(key, seconds, blockingSeconds);

            Console.WriteLine($"Lock for {context.Key}---{context.LockId}--{watch.ElapsedMilliseconds}ms--{context.IsAcquired}");

            if (context.IsAcquired)
            {
                await Task.Delay(1000);

                Console.WriteLine($"Lock acquired for {context.Key}---{context.LockId}--{watch.ElapsedMilliseconds}ms");

                watch.Stop();
            }
        }
    }
}
