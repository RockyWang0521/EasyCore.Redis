namespace EasyCore.DistributedLocking.Lock
{
    public class LockContext : IAsyncDisposable, IDisposable
    {
        internal RedisLockBase? _base;

#pragma warning disable CS8618
        /// <summary>
        /// The key for the lock.
        /// </summary>
        public string Key { get; set; }

#pragma warning restore CS8618 

        /// <summary>
        /// The LockId for the lock.
        /// </summary>
        public Guid LockId { get; set; }

        /// <summary>
        /// The lock was acquired.
        /// </summary>
        public bool IsAcquired { get; set; } = false;

        /// <summary>
        /// DisposeAsync
        /// </summary>
        /// <returns></returns>
        public async ValueTask DisposeAsync()
        {
            if (_base is not null)
                await _base.UnLockAsync(this);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            if (_base is not null)
                _base.UnLock(this);
        }
    }
}
