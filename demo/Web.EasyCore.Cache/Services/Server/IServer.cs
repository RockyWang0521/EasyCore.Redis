using EasyCore.Redis.Service.Attribute;

namespace Web.EasyCore.Cache.Services.Server
{
    /// <summary>
    /// Demo: attribute on interface type applies to all members.
    /// </summary>
    [ServerCache(CacheSeconds = 120)]
    public interface IServer
    {
        Task<string> ServerCache();

        /// <summary>Method-level attribute can override / refine interface type placement.</summary>
        [ServerCache(CacheSeconds = 60)]
        Task<string> ServerCache(string intput);

        Task<string> ServerCache(int intput);

        Task<string> ServerCache(string intput1, string intput2);

        Task<string> ServerCache(string intput1, int intput2);
    }
}
