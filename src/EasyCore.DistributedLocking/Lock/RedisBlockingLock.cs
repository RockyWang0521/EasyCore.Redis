using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Diagnostics;

namespace EasyCore.DistributedLocking.Lock
{
    internal class RedisBlockingLock : RedisLockBase
    {
        private ILogger _logger;

        const string lockscript = @"local isNX = redis.call('SETNX', KEYS[1], ARGV[1])
                                                     if isNX == 1 then
                                                        redis.call('PEXPIRE', KEYS[1], ARGV[2])
                                                        return 1
                                                    end
                                                    return 0";

        const string scriptpostpone = @"local currentLockId = redis.call('GET', KEYS[1]) 
                                                           if currentLockId and currentLockId == ARGV[1] then 
                                                               redis.call('PEXPIRE', KEYS[1], ARGV[2]) 
                                                           return 1  
                                                           end
                                                           return 0 ";

        public RedisBlockingLock(IDatabase database, LockContext lockContext, DistributedOption option, ILogger logger) : base(database, lockContext, option)
        {
            _logger = logger;
        }

        public LockContext BlockingLock(TimeSpan expireSeconds, TimeSpan timeout, TimeSpan? postponetime = null)
        {
            int expire = (int)expireSeconds.TotalSeconds;

            int tOut = (int)timeout.TotalSeconds;

            int? pTime = postponetime.HasValue ? (int?)postponetime.Value.Seconds : null;

            var lockname = GetLockName(_lockContext.Key);

            return BlockingLock(lockname, _lockContext.LockId, expire, tOut, _database!, pTime);
        }

        public LockContext BlockingLock(int expireSeconds, int timeout, int? postponetime = null)
        {
            var lockname = GetLockName(_lockContext.Key);

            return BlockingLock(lockname, _lockContext.LockId, expireSeconds, timeout, _database!, postponetime);
        }

        public async Task<LockContext> BlockingLockAsync(TimeSpan expireSeconds, TimeSpan timeout, TimeSpan? postponetime = null)
        {
            int expire = (int)expireSeconds.TotalSeconds;

            int tOut = (int)timeout.TotalSeconds;

            int? pTime = postponetime.HasValue ? (int?)postponetime.Value.Seconds : null;

            var lockname = GetLockName(_lockContext.Key);

            return await BlockingLockAsync(lockname, _lockContext.LockId, expire, tOut, _database!, pTime);
        }

        public async Task<LockContext> BlockingLockAsync(int expireSeconds, int timeout, int? postponetime = null)
        {
            var lockname = GetLockName(_lockContext.Key);

            return await BlockingLockAsync(lockname, _lockContext.LockId, expireSeconds, timeout, _database!, postponetime);
        }

        public async Task<LockContext> BlockingLockAsync(string key, Guid lockId, int expireSeconds, int timeout, IDatabase database, int? postponetime = null)
        {
            RedisKey[] scriptkey = { key };

            RedisValue[] scriptvalues = { lockId.ToString(), expireSeconds * 1000 };

            var stopwatch = Stopwatch.StartNew();

            while (stopwatch.Elapsed.TotalSeconds < timeout)
            {
                if (database.ScriptEvaluate(lockscript, scriptkey, scriptvalues).ToString() == "1")
                {
                    stopwatch.Stop();
                    if (postponetime != null && postponetime > 0)
                    {
#pragma warning disable CS4014 
                        Task.Run(() => PostponeAction(key, lockId, expireSeconds, (int)postponetime, database));
#pragma warning restore CS4014 
                    }

                    _lockContext.IsAcquired = true;

                    _lockContext._base = base.GetRedisLockBase();

                    return _lockContext;
                }
            }

            await Task.CompletedTask;

            _logger.LogWarning($"DistributedLock : {key} The blocking lock timeout");

            stopwatch.Stop();

            _lockContext._base = base.GetRedisLockBase();

            return _lockContext;
        }

        public LockContext BlockingLock(string key, Guid lockId, int expireSeconds, int timeout, IDatabase database, int? postponetime = null)
        {
            RedisKey[] scriptkey = { key };

            RedisValue[] scriptvalues = { lockId.ToString(), expireSeconds * 1000 };

            var stopwatch = Stopwatch.StartNew();

            while (stopwatch.Elapsed.TotalSeconds < timeout)
            {
                if (database.ScriptEvaluate(lockscript, scriptkey, scriptvalues).ToString() == "1")
                {
                    stopwatch.Stop();

                    if (postponetime != null && postponetime > 0)
                    {
                        Task.Run(() => PostponeAction(key, lockId, expireSeconds, (int)postponetime, database));
                    }

                    _lockContext.IsAcquired = true;

                    _lockContext._base = base.GetRedisLockBase();

                    return _lockContext;
                }
            }

            _logger.LogWarning($"DistributedLock : {key} The blocking lock timeout");

            stopwatch.Stop();

            _lockContext._base = base.GetRedisLockBase();

            return _lockContext;
        }

        async Task PostponeAction(string key, Guid lockId, int expireSeconds, int postponetime, IDatabase database)
        {
            var stopwatchpostpone = Stopwatch.StartNew();

            while (stopwatchpostpone.Elapsed.TotalSeconds <= expireSeconds)
            {
                if (stopwatchpostpone.Elapsed.TotalSeconds > expireSeconds * 0.66)
                {
                    RedisKey[] scriptkey = { key };
                    RedisValue[] scriptvalues = { lockId.ToString(), postponetime * 1000 };

                    var result = await database.ScriptEvaluateAsync(scriptpostpone, scriptkey, scriptvalues);

                    if (result.ToString() == "1")
                        _logger.LogInformation($"DistributedLock : {key} The blocking lock renewal successful");
                    else
                        _logger.LogWarning($"DistributedLock : {key} The blocking lock renewal failed");

                    return;
                }
                await Task.Delay(50);
            }
        }
    }
}
