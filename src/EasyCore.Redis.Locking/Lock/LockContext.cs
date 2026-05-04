namespace EasyCore.Redis.Locking
{
    /// <summary>
    /// Represents an acquired (or failed) lock handle. Dispose to release when acquired.
    /// </summary>
    public sealed class LockContext : IAsyncDisposable, IDisposable
    {
        private DistributedLock? _owner;
        private int _disposed;

        /// <summary>
        /// Logical lock key (prefix is applied by the lock service).
        /// </summary>
        public string Key { get; init; } = string.Empty;

        /// <summary>
        /// Unique token written as the Redis lock value (used for safe unlock/renew).
        /// </summary>
        public Guid LockId { get; init; }

        /// <summary>
        /// Whether this handle currently owns the lock.
        /// </summary>
        public bool IsAcquired { get; internal set; }

        /// <summary>
        /// Attaches the owning lock service so dispose can unlock.
        /// </summary>
        /// <param name="owner">Lock service instance.</param>
        internal void Attach(DistributedLock owner) => _owner = owner;

        /// <summary>
        /// Releases the lock if acquired (calls <see cref="IDistributedLock.UnLock"/>).
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            if (IsAcquired && _owner is not null)
                _owner.UnLock(this);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Asynchronously releases the lock if acquired (calls <see cref="IDistributedLock.UnLockAsync"/>).
        /// </summary>
        /// <returns>A value task that completes when unlock finishes.</returns>
        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            if (IsAcquired && _owner is not null)
                await _owner.UnLockAsync(this).ConfigureAwait(false);

            GC.SuppressFinalize(this);
        }
    }
}
