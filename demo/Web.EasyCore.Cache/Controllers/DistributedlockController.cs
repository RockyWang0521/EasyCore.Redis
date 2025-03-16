using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RedisTest.Lock;

namespace Web.EasyCore.Cache.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DistributedlockController : ControllerBase
    {
        private readonly IRedisLock _rlock;

        public DistributedlockController(IRedisLock rlock) => _rlock = rlock;

        [HttpGet("AcquireLock")]
        public async Task<string> GetAcquireLock()
        {
            if (!await _rlock.AcquireLock("AcquireLock", Guid.NewGuid(), 100))
                return await Task.FromResult("未抢到非阻塞锁");

            return await Task.FromResult("抢到非阻塞锁");
        }

        [HttpGet("BlockingLock")]
        public async Task<string> GetBlockingLock()
        {
            if (!await _rlock.BlockingLock("Blocking", Guid.NewGuid(), 10, 100))
                return await Task.FromResult("未抢到阻塞锁");

            return await Task.FromResult("抢到阻塞锁");
        }

        [HttpGet("Renewable")]
        public async Task GetRenewableLock()
        {
            var lockId = Guid.NewGuid();
            if (await _rlock.RenewableBlockingLock("Renewable", lockId, 10, 8, 20))
            {
                //模拟一个非常耗时的操作
                await Task.Delay(15000);

                await _rlock.UnLock("Renewable", lockId);
            }
            await Task.CompletedTask;
        }
    }
}
