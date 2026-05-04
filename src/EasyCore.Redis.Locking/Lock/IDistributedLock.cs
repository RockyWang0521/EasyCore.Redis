namespace EasyCore.Redis.Locking
{
    /// <summary>
    /// Redis distributed lock (SET NX PX acquire, Lua compare-and-delete unlock).
    /// </summary>
    public interface IDistributedLock
    {
        /// <summary>
        /// Releases a previously acquired lock (Lua GET+DEL compare-and-delete).
        /// </summary>
        /// <param name="lockContext">Lock handle returned by an acquire method.</param>
        /// <returns><c>true</c> if this process still owned the lock and it was deleted.</returns>
        bool UnLock(LockContext lockContext);

        /// <summary>
        /// Releases a previously acquired lock (Lua GET+DEL compare-and-delete).
        /// </summary>
        /// <param name="lockContext">Lock handle returned by an acquire method.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns><c>true</c> if this process still owned the lock and it was deleted.</returns>
        Task<bool> UnLockAsync(LockContext lockContext, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to acquire a lock once (SET NX PX) with a TTL in seconds.
        /// </summary>
        /// <param name="key">Logical lock key.</param>
        /// <param name="expirySeconds">Lock TTL in seconds (must be greater than zero).</param>
        /// <returns>Lock context; check <see cref="LockContext.IsAcquired"/> for success.</returns>
        LockContext AcquireLock(string key, int expirySeconds);

        /// <summary>
        /// Attempts to acquire a lock once (SET NX PX).
        /// </summary>
        /// <param name="key">Logical lock key.</param>
        /// <param name="expiry">Lock TTL (must be greater than zero).</param>
        /// <returns>Lock context; check <see cref="LockContext.IsAcquired"/> for success.</returns>
        LockContext AcquireLock(string key, TimeSpan expiry);

        /// <summary>
        /// Attempts to acquire a lock once (SET NX PX) with a TTL in seconds.
        /// </summary>
        /// <param name="key">Logical lock key.</param>
        /// <param name="expirySeconds">Lock TTL in seconds (must be greater than zero).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Lock context; check <see cref="LockContext.IsAcquired"/> for success.</returns>
        Task<LockContext> AcquireLockAsync(string key, int expirySeconds, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to acquire a lock once (SET NX PX).
        /// </summary>
        /// <param name="key">Logical lock key.</param>
        /// <param name="expiry">Lock TTL (must be greater than zero).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Lock context; check <see cref="LockContext.IsAcquired"/> for success.</returns>
        Task<LockContext> AcquireLockAsync(string key, TimeSpan expiry, CancellationToken cancellationToken = default);

        /// <summary>
        /// Blocks until the lock is acquired or <paramref name="waitTimeout"/> elapses.
        /// When <paramref name="renewalInterval"/> is set, a background watchdog renews the lock (PEXPIRE).
        /// </summary>
        /// <param name="key">Logical lock key.</param>
        /// <param name="expiry">Lock TTL after acquire.</param>
        /// <param name="waitTimeout">Maximum time to wait for acquisition.</param>
        /// <param name="renewalInterval">Optional renewal interval; when set, starts a renewal watchdog.</param>
        /// <returns>Lock context; check <see cref="LockContext.IsAcquired"/> for success.</returns>
        LockContext BlockingLock(string key, TimeSpan expiry, TimeSpan waitTimeout, TimeSpan? renewalInterval = null);

        /// <summary>
        /// Blocks until the lock is acquired or the wait timeout elapses (integer seconds overload).
        /// When <paramref name="renewalIntervalSeconds"/> is set, a background watchdog renews the lock.
        /// </summary>
        /// <param name="key">Logical lock key.</param>
        /// <param name="expirySeconds">Lock TTL in seconds.</param>
        /// <param name="waitTimeoutSeconds">Maximum wait time in seconds.</param>
        /// <param name="renewalIntervalSeconds">Optional renewal interval in seconds.</param>
        /// <returns>Lock context; check <see cref="LockContext.IsAcquired"/> for success.</returns>
        LockContext BlockingLock(string key, int expirySeconds, int waitTimeoutSeconds, int? renewalIntervalSeconds = null);

        /// <summary>
        /// Blocks until the lock is acquired or <paramref name="waitTimeout"/> elapses.
        /// When <paramref name="renewalInterval"/> is set, a background watchdog renews the lock (PEXPIRE).
        /// </summary>
        /// <param name="key">Logical lock key.</param>
        /// <param name="expiry">Lock TTL after acquire.</param>
        /// <param name="waitTimeout">Maximum time to wait for acquisition.</param>
        /// <param name="renewalInterval">Optional renewal interval; when set, starts a renewal watchdog.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Lock context; check <see cref="LockContext.IsAcquired"/> for success.</returns>
        Task<LockContext> BlockingLockAsync(
            string key,
            TimeSpan expiry,
            TimeSpan waitTimeout,
            TimeSpan? renewalInterval = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Blocks until the lock is acquired or the wait timeout elapses (integer seconds overload).
        /// When <paramref name="renewalIntervalSeconds"/> is set, a background watchdog renews the lock.
        /// </summary>
        /// <param name="key">Logical lock key.</param>
        /// <param name="expirySeconds">Lock TTL in seconds.</param>
        /// <param name="waitTimeoutSeconds">Maximum wait time in seconds.</param>
        /// <param name="renewalIntervalSeconds">Optional renewal interval in seconds.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Lock context; check <see cref="LockContext.IsAcquired"/> for success.</returns>
        Task<LockContext> BlockingLockAsync(
            string key,
            int expirySeconds,
            int waitTimeoutSeconds,
            int? renewalIntervalSeconds = null,
            CancellationToken cancellationToken = default);
    }
}
