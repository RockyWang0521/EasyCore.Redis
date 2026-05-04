# EasyCore.Redis

Production-ready Redis toolkit for .NET 8: **distributed cache**, **distributed lock**, and **service-level (AOP) result caching**.

> Future Elasticsearch packages will use `EasyCore.Elasticsearch.*`, so Redis and ES stay clearly separated.

| Package | Role |
|---------|------|
| `EasyCore.Redis` | Meta package ? registers all features |
| `EasyCore.Redis.Distributed` | Redis KV cache + MULTI/EXEC transactions |
| `EasyCore.Redis.Locking` | Redis distributed lock (SET NX PX + Lua unlock) |
| `EasyCore.Redis.Service` | `[ServerCache]` method result caching via Castle |

**Version:** 9.0.0 (breaking changes from 8.x ? see below)

---

## Quick start

```csharp
// From code
builder.Services.EasyCoreRedis(options =>
{
    options.EndPoints = new List<string> { "127.0.0.1:6379" };
    options.ConnectTimeout = TimeSpan.FromSeconds(5);
    options.SyncTimeout = TimeSpan.FromSeconds(5);
    options.DistributedName = "MyApp";
},
serverCache => serverCache.Assemblies.Add(typeof(MyService).Assembly));

// Or from appsettings.json
builder.Services.EasyCoreRedis(
    builder.Configuration.GetSection("EasyCore:Redis"),
    serverCache => serverCache.Assemblies.Add(typeof(MyService).Assembly));
```

```json
{
  "EasyCore": {
    "Cache": {
      "EndPoints": [ "127.0.0.1:6379" ],
      "ConnectTimeout": "00:00:05",
      "SyncTimeout": "00:00:05",
      "DistributedName": "MyApp"
    }
  }
}
```

Or register features separately:

```csharp
builder.Services.EasyCoreRedisDistributed(o => { /* ... */ });
builder.Services.EasyCoreRedisLock();          // reuses shared connection
builder.Services.EasyCoreRedisService(o => o.Assemblies.Add(...));
// or: builder.Services.AddServerCacheProxy<IMyService, MyService>();
```

---

## 1. Distributed cache

Access five Redis data types via `IDistributedCache`:

```csharp
IDistributedCache cache;

// String
await cache.String.SetAsync("user:1", "alice", seconds: 60);
var name = await cache.String.GetAsync("user:1");
await cache.String.IncrementAsync("counter");

// Hash
await cache.Hash.SetAsync("user:1:profile", "age", "18");
await cache.Hash.SetAsync("user:1:profile", new Dictionary<string, string>
{
    ["name"] = "alice",
    ["city"] = "SH"
}, seconds: 300);
var profile = await cache.Hash.GetAllAsync("user:1:profile");

// List
await cache.List.RightPushAsync("queue", cancellationToken: default, "a", "b", "c");
var items = await cache.List.RangeAsync("queue"); // 0..-1

// Set
await cache.Set.AddAsync("tags", cancellationToken: default, "redis", "cache");
var tags = await cache.Set.GetMembersAsync("tags");

// Sorted Set (ZSet)
await cache.SortedSet.AddAsync("rank", "alice", 100);
var top = await cache.SortedSet.RangeByRankWithScoresAsync("rank", 0, 9, ascending: false);
```

Shortcut APIs `cache.GetAsync` / `cache.SetAsync` still map to **String** for compatibility.

- Keys are prefixed with `DistributedName` (e.g. `MyApp:user:1`).
- Also: `KeyExists` / `KeyType` / `KeyTimeToLive` / `Refresh` / `Remove`.

### Transactions (MULTI/EXEC)

Queue writes on the transaction object, then commit. Disposing **without** commit does **not** execute.

```csharp
public class RedisTransaction(IDistributedTransaction transactions)
{
    public async Task RunAsync()
    {
        await using var tran = transactions.CreateTransaction();
        tran.Set("key1", "value1", seconds: 60)
            .Set("key2", "value2", seconds: 60)
            .Remove("key3");
        await tran.CommitAsync();
    }
}
```

---

## 2. Distributed lock

- Acquire: atomic `SET key lockId PX expiry NX`
- Release: Lua script deletes only if `lockId` matches
- Blocking acquire: exponential backoff (20??00ms), no busy-spin
- Optional renewal watchdog; cancelled automatically on unlock/`Dispose`

```csharp
// Non-blocking
await using var handle = await locks.AcquireLockAsync("order:42", expirySeconds: 30);
if (handle.IsAcquired)
{
    // critical section
}

// Blocking + renewal every 10s while holding a 30s lease
await using var blocking = await locks.BlockingLockAsync(
    "order:42",
    expirySeconds: 30,
    waitTimeoutSeconds: 5,
    renewalIntervalSeconds: 10);
```

---

## 3. Service cache (`[ServerCache]`)

Cache keys include **declaring type + method name + argument values** (SHA-256 hashed).

```csharp
public interface IServer
{
    Task<string> GetUser(string userId);
}

public class Server : IServer
{
    [ServerCache(CacheSeconds = 100)]
    public Task<string> GetUser(string userId)
        => Task.FromResult($"user-{userId}");
}

// Registration
builder.Services.AddServerCacheProxy<IServer, Server>();
// Inject IServer ??calls are proxied and cached.
```

---

## Production notes

| Topic | Behavior |
|-------|----------|
| Connection | Single shared `IRedisConnection` / `ConnectionMultiplexer`, disposed with the host |
| Timeouts | `ConnectTimeout` / `SyncTimeout` are `TimeSpan` (not raw ?seconds ? 1000??fields) |
| Cancellation | Cache/lock async APIs honor `CancellationToken` where applicable |
| Lock safety | Unlock is ownership-checked; dispose unlocks only if acquired |
| ServerCache scan | Opt-in assemblies only (no scanning of every DLL in the folder) |

---

## Breaking changes (8.x ??9.0)

1. Unified `DistributedOption` (`TimeSpan` timeouts, properties instead of fields).
2. Shared Redis connection; cache no longer uses Hash (`HSET`) for simple KV.
3. Transactions: use `tran.Set(...)` then `CommitAsync()` ??do not call `IDistributedCache` inside the transaction.
4. `IDistributedTransaction.CreateTransaction()` returns `ICacheTransaction`.
5. `EasyCoreRedisService` no longer auto-scans all DLLs; pass assemblies or use `AddServerCacheProxy`.
6. Removed incorrect ?Bloom filter??documentation for `KeyExists`.

---

## Demo

See `demo/Web.EasyCore.Cache` (Swagger).

Default Redis endpoint in the demo: `localhost:6379`.

---

## License

MIT OR Apache-2.0
