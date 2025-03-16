using StackExchange.Redis;

namespace EasyCore.DistributedCache.Cache
{
    public interface IDistributedCache
    {
        /// <summary>
        /// 根据给定的键获取值
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        Task<string?> GetAsync(string key, CancellationToken token = default(CancellationToken));

        /// <summary>
        /// 根据给定的键获取值
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        Task<T?> GetAsync<T>(string key, CancellationToken token = default(CancellationToken));

        /// <summary>
        /// 根据给定的键获取值
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        string? Get(string key);

        /// <summary>
        /// 根据给定的键获取值
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        T? Get<T>(string key);

        /// <summary>
        /// 根据给定的键设置值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="seconds">过期时间</param>
        void Set(string key, string value, int seconds = 0);

        /// <summary>
        /// 根据给定的键设置值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="seconds">过期时间</param>
        Task SetAsync(string key, string value, int seconds = 0, CancellationToken token = default(CancellationToken));

        /// <summary>
        /// 根据给定的键设置值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="seconds">过期时间</param>
        void Set<T>(string key, T value, int seconds = 0);

        /// <summary>
        /// 根据给定的键设置值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="seconds">过期时间</param>
        Task SetAsync<T>(string key, T value, int seconds = 0, CancellationToken token = default(CancellationToken));

        /// <summary>
        /// 根据键刷新缓存中的值，重置其滑动过期时间。
        /// </summary>
        /// <param name="key">键</param>
        void Refresh(string key, int seconds = 0);

        /// <summary>
        /// 根据键刷新缓存中的值，重置其滑动过期时间。
        /// </summary>
        /// <param name="key">键</param>
        Task RefreshAsync(string key, int seconds = 0, CancellationToken token = default(CancellationToken));

        /// <summary>
        /// 根据给定的键移除值
        /// </summary>
        /// <param name="key">键</param>
        void Remove(string key);

        /// <summary>
        /// 根据给定的键移除值
        /// </summary>
        /// <param name="key">键</param>
        Task RemoveAsync(string key, CancellationToken token = default(CancellationToken));

        /// <summary>
        /// 布隆过滤器
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<bool> KeyExistsAsync(string key, CancellationToken token = default(CancellationToken));

        /// <summary>
        /// 布隆过滤器
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="token"></param>
        /// <returns></returns>
        bool KeyExists(string key, CancellationToken token = default(CancellationToken));

        /// <summary>
        /// 获取事务
        /// </summary>
        /// <returns></returns>
        ITransaction GetTransaction();
    }
}
