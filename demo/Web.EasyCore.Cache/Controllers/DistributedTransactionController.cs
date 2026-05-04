using Microsoft.AspNetCore.Mvc;
using Web.EasyCore.Cache.Services.Transaction;

namespace Web.EasyCore.Cache.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DistributedTransactionController : ControllerBase
    {
        private readonly IRedisTransaction _transaction;

        public DistributedTransactionController(IRedisTransaction transaction) => _transaction = transaction;

        [HttpPost("Transaction")]
        public Task Transaction() => _transaction.Transaction();
    }
}
