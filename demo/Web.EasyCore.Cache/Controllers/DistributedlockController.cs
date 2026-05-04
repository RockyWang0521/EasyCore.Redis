using Microsoft.AspNetCore.Mvc;
using Web.EasyCore.Cache.Services.Lock;

namespace Web.EasyCore.Cache.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DistributedlockController : ControllerBase
    {
        private readonly IRedisLock _iRedisLock;

        public DistributedlockController(IRedisLock iRedisLock) => _iRedisLock = iRedisLock;

        [HttpGet("UsingAcquireLockAsync")]
        public Task UsingAcquireLockAsync()
            => _iRedisLock.UsingAcquireLockAsync("UsingAcquireLockAsync", 100);

        [HttpGet("AcquireLockAsync")]
        public Task AcquireLockAsync()
            => _iRedisLock.AcquireLockAsync("AcquireLockAsync", 100);

        [HttpGet("UsingBlockingLockAsync")]
        public Task UsingBlockingLockAsync()
            => _iRedisLock.UsingBlockingLockAsync("UsingBlockingLockAsync", 100, 100);

        [HttpGet("BlockingLockAsync")]
        public Task BlockingLockAsync()
            => _iRedisLock.BlockingLockAsync("BlockingLockAsync", 100, 100);

        [HttpGet("UsingRenewableBlockingLockAsync")]
        public Task UsingRenewableBlockingLockAsync()
            => _iRedisLock.UsingRenewableBlockingLockAsync("UsingRenewableBlockingLockAsync", 10, 8, 20);

        [HttpGet("RenewableBlockingLockAsync")]
        public Task RenewableBlockingLockAsync()
            => _iRedisLock.RenewableBlockingLockAsync("RenewableBlockingLockAsync", 10, 8, 20);

        [HttpGet("UsingBlockingLockTestAsync")]
        public Task UsingBlockingLockTestAsync()
            => _iRedisLock.UsingBlockingLockTestAsync("UsingBlockingLockAsync", 1000, 1000);
    }
}
