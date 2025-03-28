namespace EasyCore.ServiceCache.ServiceCache.CacheAttribute
{
    /// <summary>
    /// 服务缓存特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class ServerCacheAttribute : Attribute
    {
        /// <summary>
        /// 缓存时间 (单位秒)，默认300秒
        /// </summary>
        public int CacheSeconds { get; set; } = 300;
    }
}
