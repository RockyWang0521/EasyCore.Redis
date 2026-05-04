using EasyCore.Redis.Distributed.Connection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace EasyCore.Redis.Locking
{
    /// <summary>
    /// Production Redis distributed lock using SET NX PX + Lua compare-and-delete unlock,
    /// exponential backoff for blocking acquire, and optional renewal watchdog.
    /// </summary>
    public sealed class DistributedLock : IDistributedLock
    {
        private const string UnlockScript = @"
local current = redis.call('GET', KEYS[1])
if current == ARGV[1] then
  return redis.call('DEL', KEYS[1])
end
return 0";

        private const string RenewScript = @"
local current = redis.call('GET', KEYS[1])
if current == ARGV[1] then
  return redis.call('PEXPIRE', KEYS[1], ARGV[2])
end
return 0";

        private static readonly TimeSpan MinBackoff = TimeSpan.FromMilliseconds(20);
        private static readonly TimeSpan MaxBackoff = TimeSpan.FromMilliseconds(200);

        private readonly IRedisConnection _connection;
        private readonly IDatabase _database;
        private readonly ILogger<DistributedLock> _logger;
        private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _renewals = new();

        /// <summary>
        /// Creates a distributed lock service over the shared Redis connection.
        /// </summary>
        /// <param name="connection">Shared Redis connection.</param>
        /// <param name="logger">Logger for acquire timeouts and renewal events.</param>
        public DistributedLock(IRedisConnection connection, ILogger<DistributedLock> logger)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _database = _connection.GetDatabase();
        }

        /// <inheritdoc />
        public LockContext AcquireLock(string key, int expirySeconds)
            => AcquireLock(key, TimeSpan.FromSeconds(expirySeconds));

        /// <inheritdoc />
        public LockContext AcquireLock(string key, TimeSpan expiry)
        {
            ValidateExpiry(expiry);
            var context = CreateContext(key);
            context.IsAcquired = TryAcquire(context, expiry);
            context.Attach(this);
            return context;
        }

        /// <inheritdoc />
        public Task<LockContext> AcquireLockAsync(string key, int expirySeconds, CancellationToken cancellationToken = default)
            => AcquireLockAsync(key, TimeSpan.FromSeconds(expirySeconds), cancellationToken);

        /// <inheritdoc />
        public async Task<LockContext> AcquireLockAsync(string key, TimeSpan expiry, CancellationToken cancellationToken = default)
        {
            ValidateExpiry(expiry);
            cancellationToken.ThrowIfCancellationRequested();

            var context = CreateContext(key);
            context.IsAcquired = await TryAcquireAsync(context, expiry, cancellationToken).ConfigureAwait(false);
            context.Attach(this);
            return context;
        }

        /// <inheritdoc />
        public LockContext BlockingLock(string key, int expirySeconds, int waitTimeoutSeconds, int? renewalIntervalSeconds = null)
            => BlockingLock(
                key,
                TimeSpan.FromSeconds(expirySeconds),
                TimeSpan.FromSeconds(waitTimeoutSeconds),
                renewalIntervalSeconds.HasValue ? TimeSpan.FromSeconds(renewalIntervalSeconds.Value) : null);

        /// <inheritdoc />
        public LockContext BlockingLock(string key, TimeSpan expiry, TimeSpan waitTimeout, TimeSpan? renewalInterval = null)
            => BlockingLockAsync(key, expiry, waitTimeout, renewalInterval).GetAwaiter().GetResult();

        /// <inheritdoc />
        public Task<LockContext> BlockingLockAsync(
            string key,
            int expirySeconds,
            int waitTimeoutSeconds,
            int? renewalIntervalSeconds = null,
            CancellationToken cancellationToken = default)
            => BlockingLockAsync(
                key,
                TimeSpan.FromSeconds(expirySeconds),
                TimeSpan.FromSeconds(waitTimeoutSeconds),
                renewalIntervalSeconds.HasValue ? TimeSpan.FromSeconds(renewalIntervalSeconds.Value) : null,
                cancellationToken);

        /// <inheritdoc />
        public async Task<LockContext> BlockingLockAsync(
            string key,
            TimeSpan expiry,
            TimeSpan waitTimeout,
            TimeSpan? renewalInterval = null,
            CancellationToken cancellationToken = default)
        {
            ValidateExpiry(expiry);
            if (waitTimeout <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(waitTimeout), "Wait timeout must be greater than zero.");

            var context = CreateContext(key);
            context.Attach(this);

            var stopwatch = Stopwatch.StartNew();
            var backoff = MinBackoff;

            while (stopwatch.Elapsed < waitTimeout)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (await TryAcquireAsync(context, expiry, cancellationToken).ConfigureAwait(false))
                {
                    context.IsAcquired = true;
                    if (renewalInterval is { } interval && interval > TimeSpan.Zero)
                        StartRenewalWatchdog(context, expiry, interval);

                    return context;
                }

                var remaining = waitTimeout - stopwatch.Elapsed;
                if (remaining <= TimeSpan.Zero)
                    break;

                var delay = backoff < remaining ? backoff : remaining;
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                backoff = TimeSpan.FromMilliseconds(Math.Min(backoff.TotalMilliseconds * 2, MaxBackoff.TotalMilliseconds));
            }

            _logger.LogWarning("Distributed lock acquire timed out. Key={Key}, WaitTimeout={WaitTimeout}", context.Key, waitTimeout);
            return context;
        }

        /// <inheritdoc />
        public bool UnLock(LockContext lockContext)
        {
            ArgumentNullException.ThrowIfNull(lockContext);
            StopRenewal(lockContext);

            if (!lockContext.IsAcquired)
                return false;

            var redisKey = _connection.GetPrefixedKey(lockContext.Key);
            var result = (int)_database.ScriptEvaluate(
                UnlockScript,
                new RedisKey[] { redisKey },
                new RedisValue[] { lockContext.LockId.ToString() });

            lockContext.IsAcquired = false;
            return result == 1;
        }

        /// <inheritdoc />
        public async Task<bool> UnLockAsync(LockContext lockContext, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(lockContext);
            cancellationToken.ThrowIfCancellationRequested();
            StopRenewal(lockContext);

            if (!lockContext.IsAcquired)
                return false;

            var redisKey = _connection.GetPrefixedKey(lockContext.Key);
            var result = (int)await _database.ScriptEvaluateAsync(
                UnlockScript,
                new RedisKey[] { redisKey },
                new RedisValue[] { lockContext.LockId.ToString() }).ConfigureAwait(false);

            lockContext.IsAcquired = false;
            return result == 1;
        }

        private bool TryAcquire(LockContext context, TimeSpan expiry)
        {
            var redisKey = _connection.GetPrefixedKey(context.Key);
            return _database.StringSet(redisKey, context.LockId.ToString(), expiry, When.NotExists);
        }

        private Task<bool> TryAcquireAsync(LockContext context, TimeSpan expiry, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var redisKey = _connection.GetPrefixedKey(context.Key);
            return _database.StringSetAsync(redisKey, context.LockId.ToString(), expiry, When.NotExists);
        }

        private void StartRenewalWatchdog(LockContext context, TimeSpan expiry, TimeSpan renewalInterval)
        {
            var cts = new CancellationTokenSource();
            if (!_renewals.TryAdd(context.LockId, cts))
            {
                cts.Dispose();
                return;
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    while (!cts.IsCancellationRequested)
                    {
                        await Task.Delay(renewalInterval, cts.Token).ConfigureAwait(false);
                        var redisKey = _connection.GetPrefixedKey(context.Key);
                        var ttlMs = (long)expiry.TotalMilliseconds;
                        var result = (int)await _database.ScriptEvaluateAsync(
                            RenewScript,
                            new RedisKey[] { redisKey },
                            new RedisValue[] { context.LockId.ToString(), ttlMs }).ConfigureAwait(false);

                        if (result == 1)
                            _logger.LogDebug("Distributed lock renewed. Key={Key}", context.Key);
                        else
                        {
                            _logger.LogWarning("Distributed lock renewal failed (lost ownership). Key={Key}", context.Key);
                            break;
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // expected on unlock
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Distributed lock renewal crashed. Key={Key}", context.Key);
                }
            }, cts.Token);
        }

        private void StopRenewal(LockContext lockContext)
        {
            if (_renewals.TryRemove(lockContext.LockId, out var cts))
            {
                try
                {
                    cts.Cancel();
                }
                catch (ObjectDisposedException)
                {
                }
                finally
                {
                    cts.Dispose();
                }
            }
        }

        private static LockContext CreateContext(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Lock key must not be null or whitespace.", nameof(key));

            return new LockContext
            {
                Key = key,
                LockId = Guid.NewGuid()
            };
        }

        private static void ValidateExpiry(TimeSpan expiry)
        {
            if (expiry <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(expiry), "Lock expiry must be greater than zero.");
        }
    }
}
