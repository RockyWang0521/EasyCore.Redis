using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.FileIO;
using StackExchange.Redis;
using System.Diagnostics;

namespace EasyCore.DistributedLocking.Lock
{
    public class DistributedLock : IDistributedLock
    {
        private readonly DistributedOption _option;
        private readonly ILogger<DistributedLock> _logger;
        private ConnectionMultiplexer? _redis;
        private ConfigurationOptions _configurationOptions;
        private IDatabase? _database;
        private IList<IDatabase>? _databases = new List<IDatabase>();
        private int _quorum;

        const string unscript = @"local getLock = redis.call('GET', KEYS[1])
                                                  if getLock == ARGV[1] then
                                                     redis.call('DEL', KEYS[1])
                                                     return 1
                                                  end
                                                  return 0";
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

        /// <summary>
        /// DistributedLock
        /// </summary>
        /// <param name="redisServiceOption"></param>
        public DistributedLock(IOptions<DistributedOption> redisServiceOption, ILogger<DistributedLock> logger)
        {
            _logger = logger;

            _option = redisServiceOption.Value;

            if (_configurationOptions == null) _configurationOptions = _option.ToConfigurationOptions();

            if (_option.EndPoints.Count == 0)
                throw new InvalidOperationException("No Redis endpoints configured.");

            _redis = ConnectionMultiplexer.Connect(_configurationOptions);

            _database = _redis.GetDatabase(_option.DefaultDatabase!);

            if (_option.EndPoints.Count > 1)
            {
                foreach (var point in _redis.GetEndPoints())
                    _databases.Add(ConnectionMultiplexer.Connect(point.ToString()!).GetDatabase(_option.DefaultDatabase!));
            }
            _quorum = (_databases.Count / 2) + 1;
        }

        private string GetLockName(string key)
            => $"{_option.DistributedName}:{key}";


        #region 分布式锁

        /// <summary>
        /// 释放锁
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        public bool UnLock(string key, Guid lockId)
        {
            var lockname = GetLockName(key);
            return UnLock(lockname, lockId, _database!);
        }

        /// <summary>
        /// 释放锁
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        public async Task<bool> UnLockAsync(string key, Guid lockId)
        {
            var lockname = GetLockName(key);
            return await UnLockAsync(lockname, lockId, _database!);
        }

        /// <summary>
        /// 释放锁
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="database"></param>
        /// <returns></returns>
        public bool UnLock(string key, Guid lockId, IDatabase database)
        {
            RedisKey[] scriptkey = { key };
            RedisValue[] scriptvalues = { lockId.ToString() };
            return database.ScriptEvaluate(unscript, scriptkey, scriptvalues).ToString() == "1";
        }

        /// <summary>
        /// 释放锁
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="database"></param>
        /// <returns></returns>
        public async Task<bool> UnLockAsync(string key, Guid lockId, IDatabase database)
        {
            RedisKey[] scriptkey = { key };
            RedisValue[] scriptvalues = { lockId.ToString() };
            return (await database.ScriptEvaluateAsync(unscript, scriptkey, scriptvalues)).ToString() == "1";
        }

        #region 非阻塞锁

        /// <summary>
        /// 非阻塞锁
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="seconds">加锁时间</param>
        /// <returns></returns
        public bool AcquireLock(string key, Guid lockId, int seconds)
        {
            var lockname = GetLockName(key);
            return AcquireLock(lockname, lockId, seconds, _database!);
        }

        /// <summary>
        /// 非阻塞锁
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="seconds">加锁时间</param>
        /// <returns></returns
        public bool AcquireLock(string key, Guid lockId, TimeSpan seconds)
        {
            int sds = seconds.Seconds;
            var lockname = GetLockName(key);
            return AcquireLock(lockname, lockId, sds, _database!);
        }

        /// <summary>
        /// 非阻塞锁
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="seconds">加锁时间</param>
        /// <returns></returns
        public async Task<bool> AcquireLockAsync(string key, Guid lockId, int seconds)
        {
            var lockname = GetLockName(key);
            return await AcquireLockAsync(lockname, lockId, seconds, _database!);
        }

        /// <summary>
        /// 非阻塞锁
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="seconds">加锁时间</param>
        /// <returns></returns
        public async Task<bool> AcquireLockAsync(string key, Guid lockId, TimeSpan seconds)
        {
            int sds = seconds.Seconds;
            var lockname = GetLockName(key);
            return await AcquireLockAsync(lockname, lockId, sds, _database!);
        }

        /// <summary>
        /// 非阻塞锁
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="seconds">加锁时间</param>
        /// <returns></returns
        public bool AcquireLock(string key, Guid lockId, int seconds, IDatabase database)
        {
            RedisKey[] scriptkey = { key };
            RedisValue[] scriptvalues = { lockId.ToString(), seconds * 1000 };
            return database.ScriptEvaluate(lockscript, scriptkey, scriptvalues).ToString() == "1";
        }

        /// <summary>
        /// 非阻塞锁
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="seconds">加锁时间</param>
        /// <returns></returns
        public async Task<bool> AcquireLockAsync(string key, Guid lockId, int seconds, IDatabase database)
        {
            RedisKey[] scriptkey = { key };
            RedisValue[] scriptvalues = { lockId.ToString(), seconds * 1000 };
            return (await database.ScriptEvaluateAsync(lockscript, scriptkey, scriptvalues)).ToString() == "1";
        }

        #endregion

        #region 阻塞锁

        /// <summary>
        /// 阻塞锁(可续期)
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="expireSeconds">加锁时间</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="postponetime">续期时间</param>
        /// <returns></returns>
        public bool BlockingLock(string key, Guid lockId, TimeSpan expireSeconds, TimeSpan timeout, TimeSpan? postponetime = null)
        {
            int expire = (int)expireSeconds.TotalSeconds;
            int tOut = (int)timeout.TotalSeconds;
            int? pTime = postponetime.HasValue ? (int?)postponetime.Value.Seconds : null;
            var lockname = GetLockName(key);
            return BlockingLock(lockname, lockId, expire, tOut, _database!, pTime);
        }

        /// <summary>
        /// 阻塞锁(可续期)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expireSeconds"></param>
        /// <param name="timeout"></param>
        /// <param name="postponetime"></param>
        /// <returns></returns>
        public bool BlockingLock(string key, Guid lockId, int expireSeconds, int timeout, int? postponetime = null)
        {
            var lockname = GetLockName(key);
            return BlockingLock(lockname, lockId, expireSeconds, timeout, _database!, postponetime);
        }

        /// <summary>
        /// 阻塞锁(可续期)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expireSeconds"></param>
        /// <param name="timeout"></param>
        /// <param name="postponetime"></param>
        /// <returns></returns>
        public async Task<bool> BlockingLockAsync(string key, Guid lockId, TimeSpan expireSeconds, TimeSpan timeout, TimeSpan? postponetime = null)
        {
            int expire = (int)expireSeconds.TotalSeconds;
            int tOut = (int)timeout.TotalSeconds;
            int? pTime = postponetime.HasValue ? (int?)postponetime.Value.Seconds : null;
            var lockname = GetLockName(key);
            return await BlockingLockAsync(lockname, lockId, expire, tOut, _database!, pTime);
        }

        /// <summary>
        /// 阻塞锁(可续期)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expireSeconds"></param>
        /// <param name="timeout"></param>
        /// <param name="postponetime"></param>
        /// <returns></returns>
        public async Task<bool> BlockingLockAsync(string key, Guid lockId, int expireSeconds, int timeout, int? postponetime = null)
        {
            var lockname = GetLockName(key);
            return await BlockingLockAsync(lockname, lockId, expireSeconds, timeout, _database!, postponetime);
        }

        /// <summary>
        /// 阻塞锁(可续期)
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="expireSeconds">加锁时间</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="postponetime">续期时间</param>
        /// <returns></returns>
        public async Task<bool> BlockingLockAsync(string key, Guid lockId, int expireSeconds, int timeout, IDatabase database, int? postponetime = null)
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
                    return true;
                }
            }

            await Task.CompletedTask;

            _logger.LogWarning($"DistributedLock : {key} The blocking lock timeout");

            stopwatch.Stop();

            return false;
        }

        /// <summary>
        /// 阻塞锁(可续期)
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="expireSeconds">加锁时间</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="postponetime">续期时间</param>
        /// <returns></returns>
        public bool BlockingLock(string key, Guid lockId, int expireSeconds, int timeout, IDatabase database, int? postponetime = null)
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

                    return true;
                }
            }

            _logger.LogWarning($"DistributedLock : {key} The blocking lock timeout");

            stopwatch.Stop();

            return false;
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


        #endregion

        #region RedLock

        /// <summary>
        /// 红锁释放
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool RedUnLock(string key, Guid lockId)
        {
            var lockname = GetLockName(key);
            foreach (var database in _databases!)
                UnLock(lockname, lockId, database);
            return true;
        }

        /// <summary>
        /// 红锁释放
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<bool> RedUnLockAsync(string key, Guid lockId)
        {
            var lockname = GetLockName(key);
            foreach (var database in _databases!)
                await UnLockAsync(lockname, lockId, database);
            return true;
        }

        /// <summary>
        /// 红锁
        /// </summary>
        /// <param name="key"></param>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public bool RedAcquireLock(string key, Guid lockId, int seconds)
        {
            var lockname = GetLockName(key);
            int quorum = 0;
            foreach (var database in _databases!)
                if (AcquireLock(lockname, lockId, seconds, database)) quorum += 1;
            return quorum >= _quorum;
        }

        /// <summary>
        /// 红锁
        /// </summary>
        /// <param name="key"></param>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public async Task<bool> RedAcquireLockAsync(string key, Guid lockId, int seconds)
        {
            var lockname = GetLockName(key);
            int quorum = 0;
            foreach (var database in _databases!)
                if (await AcquireLockAsync(lockname, lockId, seconds, database)) quorum += 1;
            return quorum >= _quorum;
        }

        /// <summary>
        /// 红锁
        /// </summary>
        /// <param name="key"></param>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public bool RedAcquireLock(string key, Guid lockId, TimeSpan seconds)
        {
            var lockname = GetLockName(key);
            int quorum = 0;
            int sds = seconds.Seconds;
            foreach (var database in _databases!)
                if (AcquireLock(lockname, lockId, sds, database)) quorum += 1;
            return quorum >= _quorum;
        }


        /// <summary>
        /// 红锁
        /// </summary>
        /// <param name="key"></param>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public async Task<bool> RedAcquireLockAsync(string key, Guid lockId, TimeSpan seconds)
        {
            var lockname = GetLockName(key);
            int quorum = 0;
            int sds = seconds.Seconds;
            foreach (var database in _databases!)
                if (await AcquireLockAsync(lockname, lockId, sds, database)) quorum += 1;
            return quorum >= _quorum;
        }

        #endregion

        #endregion
    }
}
