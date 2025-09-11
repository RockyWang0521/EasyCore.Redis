namespace EasyCore.DistributedLocking.Lock
{
    public interface IDistributedLock
    {
        /// <summary>
        /// 释放锁
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        bool UnLock(LockContext lockContext);

        /// <summary>
        /// 释放锁
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        Task<bool> UnLockAsync(LockContext lockContext);

        /// <summary>
        /// 非阻塞锁
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="seconds">加锁时间</param>
        /// <returns></returns
        LockContext AcquireLock(string key, int seconds);

        /// <summary>
        /// 非阻塞锁
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="seconds">加锁时间</param>
        /// <returns></returns
        LockContext AcquireLock(string key, TimeSpan seconds);

        /// <summary>
        /// 非阻塞锁
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="seconds">加锁时间</param>
        /// <returns></returns
        Task<LockContext> AcquireLockAsync(string key, int seconds);

        /// <summary>
        /// 非阻塞锁
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="seconds">加锁时间</param>
        /// <returns></returns
        Task<LockContext> AcquireLockAsync(string key, TimeSpan seconds);

        /// <summary>
        /// 阻塞锁(可续期)
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="expireSeconds">加锁时间</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="postponetime">续期时间</param>
        /// <returns></returns>
        LockContext BlockingLock(string key, TimeSpan expireSeconds, TimeSpan timeout, TimeSpan? postponetime = null);

        /// <summary>
        /// 阻塞锁(可续期)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expireSeconds"></param>
        /// <param name="timeout"></param>
        /// <param name="postponetime"></param>
        /// <returns></returns>
        LockContext BlockingLock(string key, int expireSeconds, int timeout, int? postponetime = null);

        /// <summary>
        /// 阻塞锁(可续期)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expireSeconds"></param>
        /// <param name="timeout"></param>
        /// <param name="postponetime"></param>
        /// <returns></returns>
        Task<LockContext> BlockingLockAsync(string key, TimeSpan expireSeconds, TimeSpan timeout, TimeSpan? postponetime = null);

        /// <summary>
        /// 阻塞锁(可续期)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expireSeconds"></param>
        /// <param name="timeout"></param>
        /// <param name="postponetime"></param>
        /// <returns></returns>
        Task<LockContext> BlockingLockAsync(string key, int expireSeconds, int timeout, int? postponetime = null);
    }
}
