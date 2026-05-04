namespace EasyCore.Redis.Service.Attribute
{
    /// <summary>
    /// Marks a method for result caching via Castle DynamicProxy.
    /// Cache keys include declaring type, method name, and argument values.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class ServerCacheAttribute : System.Attribute
    {
        /// <summary>
        /// Cache TTL in seconds. Default: 300.
        /// </summary>
        public int CacheSeconds { get; set; } = 300;

        /// <summary>
        /// Whether to cache null results. Default: <c>false</c>.
        /// </summary>
        public bool CacheNullValues { get; set; }
    }
}
