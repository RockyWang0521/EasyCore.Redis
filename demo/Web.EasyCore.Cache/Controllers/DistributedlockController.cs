using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RedisTest.Lock;

namespace Web.EasyCore.Cache.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DistributedlockController : ControllerBase
    {
        private readonly IRedisLock _iRedisLock;

        public DistributedlockController(IRedisLock iRedisLock) => _iRedisLock = iRedisLock;

        [HttpGet("UsingAcquireLockAsync")]
        public async Task UsingAcquireLockAsync()
        {
            await _iRedisLock.UsingAcquireLockAsync("UsingAcquireLockAsync", 100);
        }

        [HttpGet("AcquireLockAsync")]
        public async Task AcquireLockAsync()
        {
            await _iRedisLock.AcquireLockAsync("AcquireLockAsync", 100);
        }

        [HttpGet("UsingBlockingLockAsync")]
        public async Task UsingBlockingLockAsync()
        {
            await _iRedisLock.UsingBlockingLockAsync("UsingBlockingLockAsync", 100, 100);
        }


        [HttpGet("BlockingLockAsync")]
        public async Task BlockingLockAsync()
        {
            await _iRedisLock.BlockingLockAsync("BlockingLockAsync", 100, 100);
        }

        [HttpGet("UsingRenewableBlockingLockAsync")]
        public async Task UsingRenewableBlockingLockAsync()
        {
            await _iRedisLock.UsingRenewableBlockingLockAsync("UsingRenewableBlockingLockAsync", 10, 8, 20);
        }

        [HttpGet("RenewableBlockingLockAsync")]
        public async Task RenewableBlockingLockAsync()
        {
            await _iRedisLock.RenewableBlockingLockAsync("RenewableBlockingLockAsync", 10, 8, 20);
        }

        [HttpGet("UsingBlockingLockTestAsync")]
        public async Task UsingBlockingLockTestAsync()
        {
            await _iRedisLock.UsingBlockingLockTestAsync("UsingBlockingLockAsync", 1000, 1000);
        }
    }
}
