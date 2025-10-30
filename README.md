# EasyCore.Cache

Redis（Remote Dictionary Server）是一个开源的，基于内存存储的键值型NoSQL数据库，其具备丰富的数据结构、原子操作、持久化机制和主从复制等功能。

Redis的特点：

高性能：Redis是完全在内存中运行的，数据读写速度非常快，每秒可以执行数十万次读写操作，是传统关系型数据库的数倍。

数据持久化：虽然Redis主要驻留在内存中，但它也提供了一些策略来将数据持久化到磁盘上，以防服务器突然断电导致数据丢失。主要有两种方式，一种是RDB(快照)，另一种是AOF(日志)。

支持主从复制：Redis支持多个副本，可以进行数据备份和负载均衡，提高系统的可用性。

多种数据类型：Redis支持丰富的数据类型，包括字符串、哈希、列表、集合、有序集合等，这使得它可以用于各种场景。

事务处理：Redis支持简单的事务操作，可以在一次命令执行中完成多个操作，保证了操作的原子性。

发布/订阅：Redis内置了发布/订阅功能，可以作为消息队列使用，实现进程间通信。

Redis的使用场景：

缓存：由于Redis的操作速度快，常被用作Web应用的缓存系统，减轻数据库的压力。

计数器：例如网站的访问量统计，用户的点赞、收藏等操作计数。

排行榜：利用有序集合的特性，可以方便地实现动态更新的排行榜。

消息队列：通过Redis的发布/订阅功能，可以构建消息队列系统，实现异步处理任务。

分布式锁：在分布式系统中，Redis可以用来实现分布式锁。

随着云计算和大数据的发展，Redis也在不断进化。例如，Redis Cluster提供了一种无中心节点的分布式解决方案，可以自动处理数据分区和故障恢复。另外，Redis 6引入了多线程模型，进一步提高了性能。同时，社区也在开发更多的插件和扩展，如RediSearch（全文搜索）和RedisGears（脚本处理）等，使得Redis能更好地服务于各种复杂的业务需求。

EasyCore.Cache提供对Redis的支持：

1. 分布式缓存(DistributedCache)

1.1 注册EasyCoreDistributedCache

```
   builder.Services.EasyCoreDistributedCache(options =>
   {
       options.EndPoints = new List<string> { "192.168.157.142:6379" };
       options.ConnectTimeout = 100;
       options.SyncTimeout = 100;
       options.DistributedName = "Web.EasyCore.Cache";
   });
```

1.2 使用分布式缓存

```
   public class RedisCache : IRedisCache
   {
       private readonly IDistributedCache _cache;

       public RedisCache(IDistributedCache cache) => _cache = cache;

       public async Task<string?> GetAsync(string key) => await _cache.GetAsync(key);   
    }
```

1.3 使用分布式事务

```
    public class RedisTransaction : IRedisTransaction
    {
        private readonly IDistributedTransaction _transaction;

        private readonly IDistributedCache _cache;

        public RedisTransaction(IDistributedTransaction transaction,
            IDistributedCache cache)
        {
            _transaction = transaction;
            _cache = cache;
        }

        public async Task Transaction()
        {
            using (var tran = _transaction.CreateTransaction())
            {
                _cache.Set("key1", "value1");
                _cache.Set("key2", "value2");
                _cache.Set("key3", "value3");
                await tran.CommitAsync();
            }
        }
    }
```

EasyCoreDistributedCache提供了大量的api，如写入值、读取值、布隆过滤器等。

2. 分布式锁(DistributedLock)

2.1 注册EasyCoreDistributedLock

```
   builder.Services.EasyCoreDistributedLock(options =>
   {
       options.EndPoints = new List<string> { "192.168.157.142:6379" };
       options.ConnectTimeout = 100;
       options.SyncTimeout = 100;
       options.DistributedName = "Web.EasyCore.Cache";
   });
```

2.2 使用分布式锁

```
    private readonly IDistributedLock _lock;

    public RedisLock(IDistributedLock locke) => _lock = locke;

    public async Task UsingAcquireLockAsync(string key, int seconds)
    {
        using var context = await _lock.AcquireLockAsync(key, 100);

        if (context.IsAcquired)
        {
            Console.WriteLine($"Lock acquired for {context.Key}---{context.LockId}");
        }
    }

    public void UsingAcquireLock(string key, int seconds)
    {
        using var context = _lock.AcquireLock(key, 100);

        if (context.IsAcquired)
        {
            Console.WriteLine($"Lock acquired for {context.Key}---{context.LockId}");
        }
    }

    public async Task AcquireLockAsync(string key, int seconds)
    {
        var context = await _lock.AcquireLockAsync(key, 100);

        if (context.IsAcquired)
        {
            Console.WriteLine($"Lock acquired for {context.Key}---{context.LockId}");
        }

        await _lock.UnLockAsync(context);
    }

    public void AcquireLock(string key, int seconds)
    {
        var context = _lock.AcquireLock(key, 100);

        if (context.IsAcquired)
        {
            Console.WriteLine($"Lock acquired for {context.Key}---{context.LockId}");
        }

        _lock.UnLock(context);
    }

    public async Task UsingBlockingLockAsync(string key, int seconds, int blockingSeconds)
    {
        using var context = await _lock.BlockingLockAsync(key, seconds, blockingSeconds);

        if (context.IsAcquired)
        {
            Console.WriteLine($"Lock acquired for {context.Key}---{context.LockId}");
        }
    }

    public void UsingBlockingLock(string key, int seconds, int blockingSeconds)
    {
        using var context = _lock.BlockingLock(key, seconds, blockingSeconds);

        if (context.IsAcquired)
        {
            Console.WriteLine($"Lock acquired for {context.Key}---{context.LockId}");
        }
    }

    public async Task BlockingLockAsync(string key, int seconds, int blockingSeconds)
    {
        var context = await _lock.BlockingLockAsync(key, seconds, blockingSeconds);

        if (context.IsAcquired)
        {
            Console.WriteLine($"Lock acquired for {context.Key}---{context.LockId}");
        }

        await _lock.UnLockAsync(context);
    }

    public void BlockingLock(string key, int seconds, int blockingSeconds)
    {
        var context = _lock.BlockingLock(key, seconds, blockingSeconds);

        if (context.IsAcquired)
        {
            Console.WriteLine($"Lock acquired for {context.Key}---{context.LockId}");
        }

        _lock.UnLock(context);
    }

    public async Task UsingRenewableBlockingLockAsync(string key, int seconds, int blockingSeconds, int renewalSeconds)
    {
        using var context = await _lock.BlockingLockAsync(key, seconds, blockingSeconds, renewalSeconds);

        if (context.IsAcquired)
        {
            Console.WriteLine($"Lock acquired for {context.Key}---{context.LockId}");
        }
    }

    public void UsingRenewableBlockingLock(string key, int seconds, int blockingSeconds, int renewalSeconds)
    {
        using var context = _lock.BlockingLock(key, seconds, blockingSeconds, renewalSeconds);

        if (context.IsAcquired)
        {
            Console.WriteLine($"Lock acquired for {context.Key}---{context.LockId}");
        }
    }

    public async Task RenewableBlockingLockAsync(string key, int seconds, int blockingSeconds, int renewalSeconds)
    {
        var context = await _lock.BlockingLockAsync(key, seconds, blockingSeconds, renewalSeconds);

        if (context.IsAcquired)
        {
            Console.WriteLine($"Lock acquired for {context.Key}---{context.LockId}");
        }

        _lock.UnLock(context);
    }

    public void RenewableBlockingLock(string key, int seconds, int blockingSeconds, int renewalSeconds)
    {
        var context = _lock.BlockingLock(key, seconds, blockingSeconds, renewalSeconds);

        if (context.IsAcquired)
        {
            Console.WriteLine($"Lock acquired for {context.Key}---{context.LockId}");
        }

        _lock.UnLock(context);
    }
}
```

EasyCoreDistributedLock提供了非阻塞锁、阻塞锁的api。支持using使用或直接调用获取锁，直接调用获取锁时最后一定要释放锁。

3. 服务缓存(ServerCache)

3.1 注册服务缓存

```
   builder.Services.EasyCoreServerCache();
```

3.2 使用服务缓存

```
 public class Server : IServer
 {
     [ServerCache]
     public async Task<string> ServerCache()
     {
         return await Task.FromResult("这是ServerCache，没有参数");
     }

     [ServerCache]
     public async Task<string> ServerCache(string intput)
     {
         return await Task.FromResult("这是ServerCache，有一个string类型的intput参数");
     }

     [ServerCache]
     public async Task<string> ServerCache(int intput)
     {
         return await Task.FromResult("这是ServerCache，有一个int类型的intput参数");
     }

     [ServerCache]
     public async Task<string> ServerCache(string intput1, string intput2)
     {
         return await Task.FromResult("这是ServerCache，有一个string类型的intput1参数和一个string类型的intput2参数");
     }

     [ServerCache]
     public async Task<string> ServerCache(string intput1, int intput2)
     {
         return await Task.FromResult("这是ServerCache，有一个string类型的intput1参数和一个int类型的intput2参数");
     }
 }
```
[ServerCache] 特性默认对服务数据进行300秒的缓存。可在特性内修改缓存时间，如[ServerCache(CacheSeconds =100)]，即可设置缓存100秒。









