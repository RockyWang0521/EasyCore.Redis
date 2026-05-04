namespace Web.EasyCore.Cache.Services.Server
{
    public interface IServer
    {
        Task<string> ServerCache();

        Task<string> ServerCache(string intput);

        Task<string> ServerCache(int intput);

        Task<string> ServerCache(string intput1, string intput2);

        Task<string> ServerCache(string intput1, int intput2);
    }
}
