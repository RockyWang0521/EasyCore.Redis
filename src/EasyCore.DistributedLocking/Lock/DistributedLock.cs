using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace EasyCore.DistributedLocking.Lock
{
    public class DistributedLock : IDistributedLock
    {
        private readonly DistributedOption _option;
        private readonly ILogger<DistributedLock> _logger;
        private ConnectionMultiplexer? _redis;
        private ConfigurationOptions _configurationOptions;
        private IDatabase? _database;

        public DistributedLock(IOptions<DistributedOption> redisServiceOption, ILogger<DistributedLock> logger)
        {
            _logger = logger;

            _option = redisServiceOption.Value;

            if (_configurationOptions == null) _configurationOptions = _option.ToConfigurationOptions();

            if (_option.EndPoints.Count == 0)
                throw new InvalidOperationException("No Redis endpoints configured.");

            _redis = ConnectionMultiplexer.Connect(_configurationOptions);

            _database = _redis.GetDatabase(_option.DefaultDatabase!);
        }

        public bool UnLock(LockContext lockContext)
        {
            var provider = new DistributedLockProvider(_database!, _logger, _option);

            return provider.UnLock(lockContext);
        }

        public async Task<bool> UnLockAsync(LockContext lockContext)
        {
            var provider = new DistributedLockProvider(_database!, _logger, _option);

            return await provider.UnLockAsync(lockContext);
        }

        public LockContext AcquireLock(string key, int seconds)
        {
            var provider = new DistributedLockProvider(_database!, _logger, _option);

            return provider.AcquireLock(key, seconds);
        }

        public LockContext AcquireLock(string key, TimeSpan seconds)
        {
            var provider = new DistributedLockProvider(_database!, _logger, _option);

            return provider.AcquireLock(key, seconds);
        }

        public async Task<LockContext> AcquireLockAsync(string key, int seconds)
        {
            var provider = new DistributedLockProvider(_database!, _logger, _option);

            return await provider.AcquireLockAsync(key, seconds);
        }

        public async Task<LockContext> AcquireLockAsync(string key, TimeSpan seconds)
        {
            var provider = new DistributedLockProvider(_database!, _logger, _option);

            return await provider.AcquireLockAsync(key, seconds);
        }

        public LockContext BlockingLock(string key, TimeSpan expireSeconds, TimeSpan timeout, TimeSpan? postponetime = null)
        {
            var provider = new DistributedLockProvider(_database!, _logger, _option);

            return provider.BlockingLock(key, expireSeconds, timeout, postponetime);
        }

        public LockContext BlockingLock(string key, int expireSeconds, int timeout, int? postponetime = null)
        {
            var provider = new DistributedLockProvider(_database!, _logger, _option);

            return provider.BlockingLock(key, expireSeconds, timeout, postponetime);
        }

        public async Task<LockContext> BlockingLockAsync(string key, TimeSpan expireSeconds, TimeSpan timeout, TimeSpan? postponetime = null)
        {
            var provider = new DistributedLockProvider(_database!, _logger, _option);

            return await provider.BlockingLockAsync(key, expireSeconds, timeout, postponetime);
        }

        public async Task<LockContext> BlockingLockAsync(string key, int expireSeconds, int timeout, int? postponetime = null)
        {
            var provider = new DistributedLockProvider(_database!, _logger, _option);

            return await provider.BlockingLockAsync(key, expireSeconds, timeout, postponetime);
        }
    }
}
