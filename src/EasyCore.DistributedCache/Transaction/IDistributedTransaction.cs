using StackExchange.Redis;

namespace EasyCore.DistributedCache.Transaction
{
    public interface IDistributedTransaction : IDisposable
    {
        /// <summary>
        /// 创建事务
        /// </summary>
        /// <returns></returns>
        DistributedTransaction CreateTransaction();

        /// <summary>
        /// 提交事务
        /// </summary>
        /// <returns></returns>
        Task<bool> CommitAsync();

        /// <summary>
        /// 提交事务
        /// </summary>
        /// <returns></returns>
        bool Commit();
    }
}
