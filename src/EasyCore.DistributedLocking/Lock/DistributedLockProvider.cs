using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace EasyCore.DistributedLocking.Lock
{
    internal class DistributedLockProvider
    {
        private readonly DistributedOption _option;
        private readonly ILogger _logger;
        private readonly IDatabase _database;
        private RedisLockBase? _lockBase;
        private RedisBlockingLock? _blockingLock;
        private RedisNotBlockingLock? _notBlockingLock;

        public DistributedLockProvider(IDatabase database, ILogger logger, DistributedOption option)
        {
            _logger = logger;
            _database = database;
            _option = option;
        }

        public bool UnLock(LockContext lockContext)
        {
            _lockBase = new RedisLockBase(_database!, lockContext, _option);

            return _lockBase.UnLock(lockContext);
        }

        public async Task<bool> UnLockAsync(LockContext lockContext)
        {
            _lockBase = new RedisLockBase(_database!, lockContext, _option);

            return await _lockBase.UnLockAsync(lockContext);
        }

        public LockContext AcquireLock(string key, int seconds)
        {
            var lockContext = new LockContext()
            {
                Key = key,

                LockId = Guid.NewGuid(),
            };

            _notBlockingLock = new RedisNotBlockingLock(_database!, lockContext, _option, _logger);

            return _notBlockingLock.AcquireLock(seconds);
        }

        public LockContext AcquireLock(string key, TimeSpan seconds)
        {
            var lockContext = new LockContext()
            {
                Key = key,

                LockId = Guid.NewGuid(),
            };

            _notBlockingLock = new RedisNotBlockingLock(_database!, lockContext, _option, _logger);

            return _notBlockingLock.AcquireLock(seconds);
        }

        public async Task<LockContext> AcquireLockAsync(string key, int seconds)
        {
            var lockContext = new LockContext()
            {
                Key = key,

                LockId = Guid.NewGuid(),
            };

            _notBlockingLock = new RedisNotBlockingLock(_database!, lockContext, _option, _logger);

            return await _notBlockingLock.AcquireLockAsync(seconds);
        }

        public async Task<LockContext> AcquireLockAsync(string key, TimeSpan seconds)
        {
            var lockContext = new LockContext()
            {
                Key = key,

                LockId = Guid.NewGuid(),
            };

            _notBlockingLock = new RedisNotBlockingLock(_database!, lockContext, _option, _logger);

            return await _notBlockingLock.AcquireLockAsync(seconds);
        }

        public LockContext BlockingLock(string key, TimeSpan expireSeconds, TimeSpan timeout, TimeSpan? postponetime = null)
        {
            int expire = (int)expireSeconds.TotalSeconds;

            int tOut = (int)timeout.TotalSeconds;

            int? pTime = postponetime.HasValue ? (int?)postponetime.Value.Seconds : null;

            var lockContext = new LockContext()
            {
                Key = key,

                LockId = Guid.NewGuid(),
            };

            _blockingLock = new RedisBlockingLock(_database!, lockContext, _option, _logger);

            return _blockingLock.BlockingLock(expire, tOut, pTime);
        }

        public LockContext BlockingLock(string key, int expireSeconds, int timeout, int? postponetime = null)
        {
            var lockContext = new LockContext()
            {
                Key = key,

                LockId = Guid.NewGuid(),
            };

            _blockingLock = new RedisBlockingLock(_database!, lockContext, _option, _logger);

            return _blockingLock.BlockingLock(expireSeconds, timeout, postponetime);
        }

        public async Task<LockContext> BlockingLockAsync(string key, TimeSpan expireSeconds, TimeSpan timeout, TimeSpan? postponetime = null)
        {
            int expire = (int)expireSeconds.TotalSeconds;

            int tOut = (int)timeout.TotalSeconds;

            int? pTime = postponetime.HasValue ? (int?)postponetime.Value.Seconds : null;

            var lockContext = new LockContext()
            {
                Key = key,

                LockId = Guid.NewGuid(),
            };

            _blockingLock = new RedisBlockingLock(_database!, lockContext, _option, _logger);

            return await _blockingLock.BlockingLockAsync(expire, tOut, pTime);
        }

        public async Task<LockContext> BlockingLockAsync(string key, int expireSeconds, int timeout, int? postponetime = null)
        {
            var lockContext = new LockContext()
            {
                Key = key,

                LockId = Guid.NewGuid(),
            };

            _blockingLock = new RedisBlockingLock(_database!, lockContext, _option, _logger);

            return await _blockingLock.BlockingLockAsync(expireSeconds, timeout, postponetime);
        }
    }
}
