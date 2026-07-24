using EasyCore.Redis.Service.Attribute;

namespace Web.EasyCore.Cache.Services.Server
{
    /// <summary>
    /// Demo contract — put <see cref="ServerCacheAttribute"/> on the implementation for weave.
    /// </summary>
    public interface IServer
    {
        Task<string> ServerCache();

        Task<string> ServerCache(string intput);

        Task<string> ServerCache(int intput);

        Task<string> ServerCache(string intput1, string intput2);

        Task<string> ServerCache(string intput1, int intput2);
    }
}
