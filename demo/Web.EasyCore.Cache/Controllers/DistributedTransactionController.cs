using Microsoft.AspNetCore.Mvc;
using RedisTest.Cache;
using RedisTest.Transaction;

namespace Web.EasyCore.Cache.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DistributedTransactionController : Controller
    {
        private readonly IRedisTransaction _transaction;

        public DistributedTransactionController(IRedisTransaction transaction) => _transaction = transaction;

        [HttpPost("Transaction")]
        public async Task Transaction() => await _transaction.Transaction();
    }
}
