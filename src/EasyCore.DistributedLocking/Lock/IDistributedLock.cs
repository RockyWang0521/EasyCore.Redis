namespace EasyCore.DistributedLocking.Lock
{
    public interface IDistributedLock
    {
        /// <summary>
        /// 释放锁
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        bool UnLock(string key, Guid lockId);

        /// <summary>
        /// 释放锁
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        Task<bool> UnLockAsync(string key, Guid lockId);

        /// <summary>
        /// 非阻塞锁
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="seconds">加锁时间</param>
        /// <returns></returns>
        bool AcquireLock(string key, Guid lockId, int seconds);

        /// <summary>
        /// 非阻塞锁
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="seconds">加锁时间</param>
        /// <returns></returns>
        Task<bool> AcquireLockAsync(string key, Guid lockId, int seconds);

        /// <summary>
        /// 非阻塞锁
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="seconds">加锁时间</param>
        /// <returns></returns>
        bool AcquireLock(string key, Guid lockId, TimeSpan seconds);

        /// <summary>
        /// 非阻塞锁
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="seconds">加锁时间</param>
        /// <returns></returns>
        Task<bool> AcquireLockAsync(string key, Guid lockId, TimeSpan seconds);

        /// <summary>
        /// 阻塞锁(可续期)
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="expireSeconds">加锁时间</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="postponetime">续期时间</param>
        /// <returns></returns>
        bool BlockingLock(string key, Guid lockId, TimeSpan expireSeconds, TimeSpan timeout, TimeSpan? postponetime = null);

        /// <summary>
        /// 阻塞锁(可续期)
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="expireSeconds">加锁时间</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="postponetime">续期时间</param>
        /// <returns></returns>
        Task<bool> BlockingLockAsync(string key, Guid lockId, TimeSpan expireSeconds, TimeSpan timeout, TimeSpan? postponetime = null);

        /// <summary>
        /// 阻塞锁(可续期)
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="expireSeconds">加锁时间</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="postponetime">续期时间</param>
        /// <returns></returns>
        bool BlockingLock(string key, Guid lockId, int expireSeconds, int timeout, int? postponetime = null);

        /// <summary>
        /// 阻塞锁(可续期)
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="expireSeconds">加锁时间</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="postponetime">续期时间</param>
        /// <returns></returns>
        Task<bool> BlockingLockAsync(string key, Guid lockId, int expireSeconds, int timeout, int? postponetime = null);

        /// <summary>
        /// 红锁释放
        /// </summary>
        /// <param name="key"></param>
        /// <param name="seconds"></param>
        /// <returns></returns>
        bool RedUnLock(string key, Guid lockId);

        /// <summary>
        /// 红锁释放
        /// </summary>
        /// <param name="key"></param>
        /// <param name="seconds"></param>
        /// <returns></returns>
        Task<bool> RedUnLockAsync(string key, Guid lockId);

        /// <summary>
        /// 红锁非阻塞锁
        /// </summary>
        /// <param name="key"></param>
        /// <param name="seconds"></param>
        /// <returns></returns>
        bool RedAcquireLock(string key, Guid lockId, TimeSpan seconds);

        /// <summary>
        /// 红锁非阻塞锁
        /// </summary>
        /// <param name="key"></param>
        /// <param name="seconds"></param>
        /// <returns></returns>
        Task<bool> RedAcquireLockAsync(string key, Guid lockId, TimeSpan seconds);

        /// <summary>
        /// 红锁非阻塞锁
        /// </summary>
        /// <param name="key"></param>
        /// <param name="seconds"></param>
        /// <returns></returns>
        bool RedAcquireLock(string key, Guid lockId, int seconds);

        /// <summary>
        /// 红锁非阻塞锁
        /// </summary>
        /// <param name="key"></param>
        /// <param name="seconds"></param>
        /// <returns></returns>
        Task<bool> RedAcquireLockAsync(string key, Guid lockId, int seconds);
    }
}
