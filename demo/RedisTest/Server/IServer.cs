using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisTest.ServerCache
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
