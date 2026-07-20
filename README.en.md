# 🔴 EasyCore.Redis

> **EasyCore.Redis** is a production-oriented Redis toolkit for .NET 8. Built on [StackExchange.Redis](https://stackexchange.github.io/StackExchange.Redis/), it provides **distributed cache** (five data types), **MULTI/EXEC transactions**, **distributed locks**, and Castle DynamicProxy **`[ServerCache]` method-result caching**.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12-239120?logo=csharp)
![Redis](https://img.shields.io/badge/Redis-StackExchange-DC382D?logo=redis)
![Cache](https://img.shields.io/badge/Cache-String%20%7C%20Hash%20%7C%20List%20%7C%20Set%20%7C%20ZSet-orange)
![Lock](https://img.shields.io/badge/Lock-SET%20NX%20PX%20%2B%20Lua-blueviolet)
![License](https://img.shields.io/badge/License-MIT-yellow)
![Version](https://img.shields.io/badge/Version-8.3.0-blue)

Repository: [github.com/RockyWang0521/EasyCore.Redis](https://github.com/RockyWang0521/EasyCore.Redis)

---

## 🌍 Language

- Chinese: [README.md](https://github.com/RockyWang0521/EasyCore.Redis/blob/master/README.md)
- **English (this document)**

---

## 📚 Table of Contents

### 🧭 Part I — Overview & Architecture
- [1. 🎯 Positioning](#1-positioning)
- [2. 🏗 Architecture](#2-architecture)
- [3. 📦 NuGet Packages](#3-nuget-packages)
- [4. 📊 Capability Matrix](#4-capability-matrix)

### 🚀 Part II — Getting Started
- [5. ⚙ Requirements](#5-requirements)
- [6. 📥 Installation](#6-installation)
- [7. ⚡ Quick Start (3 minutes)](#7-quick-start-3-minutes)
- [8. 🧩 Options Reference](#8-options-reference)

### 📘 Part III — API Reference
- [9. 💾 Distributed Cache `IDistributedCache`](#9-distributed-cache-idistributedcache)
- [10. 🔄 Transactions `IDistributedTransaction` / `ICacheTransaction`](#10-transactions-idistributedtransaction--icachetransaction)
- [11. 🔐 Distributed Lock `IDistributedLock`](#11-distributed-lock-idistributedlock)
- [12. ✨ Service Cache `[ServerCache]`](#12-service-cache-servercache)

### 🏭 Part IV — Production & Demo
- [13. ✅ Production Notes](#13-production-notes)
- [14. 🧪 Demo Project](#14-demo-project)
- [15. ❓ FAQ](#15-faq)
- [16. 📄 License](#16-license)

---

## 1. 🎯 Positioning

EasyCore.Redis makes Redis safe and approachable in ASP.NET Core:

| Pain point | EasyCore.Redis approach |
|---|---|
| Raw SE.Redis is verbose | Unified `IDistributedCache` for five data types |
| Multiple services fighting for connections | Shared `IRedisConnection` / `ConnectionMultiplexer` |
| Key collisions across apps/envs | Automatic `{DistributedName}:` prefix |
| Hard concurrent critical sections | `IDistributedLock` (SET NX PX + Lua unlock) |
| Repeated method computation | `[ServerCache]` + Castle proxy |
| Need atomic multi-write | `ICacheTransaction` (MULTI/EXEC) |

### 1.1 Design Principles

| Principle | Meaning |
|---|---|
| **Low friction** | One `AddEasyCoreRedis(...)` registers everything |
| **Composable** | `.Distributed` / `.Locking` / `.Service` can be referenced alone |
| **Key isolation** | Every logical key is prefixed with `{DistributedName}:` |
| **Safe locks** | Unlock checks `LockId`; `Dispose` unlocks only when acquired |
| **Explicit transactions** | Queue on the transaction object, then `Commit`; dispose discards |

### 1.2 Repository Layout

```text
EasyCore.Redis/
├── src/
│   ├── EasyCore.Redis/                 # Meta package: register all features
│   ├── EasyCore.Redis.Distributed/     # Cache + transactions + connection
│   ├── EasyCore.Redis.Locking/         # Distributed lock
│   └── EasyCore.Redis.Service/         # [ServerCache] AOP
├── demo/Web.EasyCore.Cache/            # Swagger demo
├── tests/EasyCore.Cache.Tests/
└── docs/svg/                           # README diagrams (absolute URLs for NuGet)
```

---

## 2. 🏗 Architecture

### 2.1 Component Diagram

![architecture-en](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Redis/master/docs/svg/architecture-en.svg)

### 2.2 Core Flows

![sequence-en](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Redis/master/docs/svg/sequence-en.svg)

### 2.3 API Overview

![api-overview-en](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Redis/master/docs/svg/api-overview-en.svg)

### 2.4 Dependency Graph

```text
AddEasyCoreRedis() / AddEasyCoreRedis(IConfiguration)
        │
        ├── AddEasyCoreRedisDistributed ──► IRedisConnection
        │                              ├─ IDistributedCache
        │                              └─ IDistributedTransaction → ICacheTransaction
        ├── AddEasyCoreRedisLock ─────────► IDistributedLock  (reuses same connection)
        └── AddEasyCoreRedisService ──────► [ServerCache] Proxy (needs IDistributedCache)
```

---

## 3. 📦 NuGet Packages

| Package | Role | Required |
|---|---|---|
| `EasyCore.Redis` | Meta package: cache + lock + service cache | ✅ Recommended |
| `EasyCore.Redis.Distributed` | Connection, `IDistributedCache`, transactions | Optional |
| `EasyCore.Redis.Locking` | `IDistributedLock` | Optional |
| `EasyCore.Redis.Service` | `[ServerCache]` Castle proxy | Optional |

> Lock-only setups still need a connection: use `AddEasyCoreRedisLock(configure)`, or register `AddEasyCoreRedisDistributed` then `AddEasyCoreRedisLock()`.

---

## 4. 📊 Capability Matrix

| Capability | Distributed | Locking | Service |
|---|---|---|---|
| String / Hash / List / Set / ZSet | ✅ | — | — |
| Key helpers Exists / TTL / Remove | ✅ | — | — |
| MULTI/EXEC transactions | ✅ | — | — |
| SET NX PX lock + Lua unlock | — | ✅ | — |
| Blocking acquire + renewal watchdog | — | ✅ | — |
| Method-result AOP cache | — | — | ✅ |

---

## 5. ⚙ Requirements

| Item | Requirement |
|---|---|
| .NET | 8.0+ |
| Host | ASP.NET Core or generic host |
| Redis | Reachable Redis instance (demo default `localhost:6379`) |
| Client | StackExchange.Redis (brought by packages) |

---

## 6. 📥 Installation

```bash
dotnet add package EasyCore.Redis
# Current version: 8.0.0

# Or pick features separately
dotnet add package EasyCore.Redis.Distributed
dotnet add package EasyCore.Redis.Locking
dotnet add package EasyCore.Redis.Service
```

---

## 7. ⚡ Quick Start (3 minutes)

### 7️⃣.1️⃣ Register from code

```csharp
using EasyCore.Redis;

builder.Services.AddEasyCoreRedis(options =>
{
    options.EndPoints = new List<string> { "127.0.0.1:6379" };
    options.ConnectTimeout = TimeSpan.FromSeconds(5);
    options.SyncTimeout = TimeSpan.FromSeconds(5);
    options.DistributedName = "MyApp";   // keys become MyApp:user:1
    // options.Password = "***";
    // options.DefaultDatabase = 0;
});
```

### 7️⃣.2️⃣ Register from appsettings.json

```csharp
builder.Services.AddEasyCoreRedis(
    builder.Configuration.GetSection("EasyCore:Redis"));
```

```json
{
  "EasyCore": {
    "Redis": {
      "EndPoints": [ "127.0.0.1:6379" ],
      "ConnectTimeout": "00:00:05",
      "SyncTimeout": "00:00:05",
      "DefaultDatabase": 0,
      "AbortOnConnectFail": false,
      "DistributedName": "MyApp"
    }
  }
}
```

### 7️⃣.3️⃣ Inject and use

```csharp
public class UserService(IDistributedCache cache, IDistributedLock locks)
{
    public async Task<string?> GetNameAsync(string userId)
    {
        await cache.StringSetAsync($"user:{userId}", "alice", seconds: 60);
        return await cache.StringGetAsync($"user:{userId}");
    }

    public async Task ProcessOrderAsync(string orderId)
    {
        await using var handle = await locks.AcquireLockAsync($"order:{orderId}", expirySeconds: 30);
        if (!handle.IsAcquired) return;
        // critical section…
    }
}
```

### 7️⃣.4️⃣ Register features separately

```csharp
builder.Services.AddEasyCoreRedisDistributed(o => { /* EndPoints… */ });
builder.Services.AddEasyCoreRedisLock();    // reuses shared connection
builder.Services.AddEasyCoreRedisService(); // auto-scan [ServerCache] services
// or: builder.Services.AddServerCacheProxy<IMyService, MyService>();
```

---

## 8. 🧩 Options Reference

`DistributedOption` (shared by cache and lock):

| Property | Type | Default | Description |
|---|---|---|---|
| `EndPoints` | `List<string>` | empty (required) | e.g. `127.0.0.1:6379` |
| `User` | `string?` | `null` | Redis 6+ ACL username |
| `Password` | `string?` | `null` | Password |
| `ConnectTimeout` | `TimeSpan` | 5s | Connect timeout |
| `SyncTimeout` | `TimeSpan` | 5s | Sync operation timeout |
| `AbortOnConnectFail` | `bool` | `false` | Abort on initial connect failure |
| `DefaultDatabase` | `int` | `0` | Default DB index |
| `DistributedName` | `string` | `"EasyCore"` | Key prefix namespace |

Key rule: logical `user:1` → Redis key `{DistributedName}:user:1` (e.g. `MyApp:user:1`).

---

## 9. 💾 Distributed Cache `IDistributedCache`

Namespace: `EasyCore.Redis.Distributed`

Almost every API has **sync** and **async** (`*Async`) pairs; async methods accept `CancellationToken`. Complex values use **JSON** (`StringGet<T>` / `StringSet<T>` / `HashGet<T>`, etc.).

### 9.1 Shortcuts (map to String)

| Method | Redis | Description |
|---|---|---|
| `Get` / `GetAsync` | GET | Get string |
| `Get<T>` / `GetAsync<T>` | GET | Deserialize JSON |
| `Set` / `SetAsync` | SET | Set string; `seconds=0` means no expiry |
| `Set<T>` / `SetAsync<T>` | SET | Serialize JSON |

### 9.2 Key helpers

| Method | Redis | Description |
|---|---|---|
| `KeyExists` / `KeyExistsAsync` | EXISTS | Whether the key exists |
| `KeyType` / `KeyTypeAsync` | TYPE | `string`/`hash`/`list`/`set`/`zset`/`none` |
| `KeyTimeToLive` / `KeyTimeToLiveAsync` | TTL | Seconds; `-1` no expiry, `-2` missing |
| `Refresh` / `RefreshAsync` | EXPIRE | Reset absolute expiry (`seconds` must be > 0) |
| `Remove` / `RemoveAsync` | DEL | Delete key |

### 9.3 String

| Method | Redis | Description |
|---|---|---|
| `StringGet` / `StringGetAsync` | GET | Get string |
| `StringGet<T>` / `StringGetAsync<T>` | GET | Deserialize JSON |
| `StringSet` / `StringSetAsync` | SET / SET NX | `whenNotExists=true` ⇒ NX; returns whether set |
| `StringSet<T>` / `StringSetAsync<T>` | SET | JSON write |
| `StringGetDeleteAsync` | GETDEL | Atomic get-and-delete |
| `StringIncrement` / `*Async` | INCRBY / INCRBYFLOAT | Integer or float increment |
| `StringDecrement` / `*Async` | DECRBY | Decrement |
| `StringAppend` / `*Async` | APPEND | Append; returns new length |
| `StringGetLength` / `*Async` | STRLEN | Length |
| `StringSetExpiry` / `*Async` | EXPIRE | Set expiry |

```csharp
await cache.StringSetAsync("user:1", "alice", seconds: 60);
await cache.StringSetAsync("profile:1", new { Name = "alice", Age = 18 }, seconds: 300);
var ok = await cache.StringSetAsync("once", "v", whenNotExists: true); // SET NX
await cache.StringIncrementAsync("counter");
var prev = await cache.StringGetDeleteAsync("once");
```

### 9.4 Hash

| Method | Redis | Description |
|---|---|---|
| `HashSet(key, field, value)` | HSET | Single field; returns whether field was created |
| `HashSet<T>(...)` | HSET | JSON field value |
| `HashSet(key, IDictionary, seconds)` | HSET multi | Batch; `seconds>0` also EXPIRE |
| `HashGet` / `HashGet<T>` | HGET | Get field |
| `HashGetAll` | HGETALL | All fields |
| `HashDelete` | HDEL | Delete fields; returns count removed |
| `HashExists` | HEXISTS | Field exists? |
| `HashGetKeys` / `HashGetValues` | HKEYS / HVALS | Field names / values |
| `HashGetLength` | HLEN | Field count |
| `HashIncrement` | HINCRBY / HINCRBYFLOAT | Increment field |

```csharp
await cache.HashSetAsync("user:1:profile", "age", "18");
await cache.HashSetAsync("user:1:profile", new Dictionary<string, string>
{
    ["name"] = "alice",
    ["city"] = "SH"
}, seconds: 300);
var all = await cache.HashGetAllAsync("user:1:profile");
await cache.HashIncrementAsync("user:1:profile", "score", 10);
```

### 9.5 List

| Method | Redis | Description |
|---|---|---|
| `ListLeftPush` / `ListRightPush` | LPUSH / RPUSH | Push head/tail (`params`, ≥1 value) |
| `ListLeftPop` / `ListRightPop` | LPOP / RPOP | Pop head/tail |
| `ListRange` | LRANGE | Range; default `0..-1` = all |
| `ListGetLength` | LLEN | Length |
| `ListGetByIndex` / `ListSetByIndex` | LINDEX / LSET | Read/write by index |
| `ListTrim` | LTRIM | Trim |
| `ListRemove` | LREM | `count`: 0=all; positive=from head; negative=from tail |

> For async `params` overloads, `CancellationToken` comes **before** values:  
> `ListRightPushAsync(key, cancellationToken, "a", "b")`.

```csharp
await cache.ListRightPushAsync("queue", cancellationToken: default, "a", "b", "c");
var items = await cache.ListRangeAsync("queue");
var first = await cache.ListLeftPopAsync("queue");
```

### 9.6 Set

| Method | Redis | Description |
|---|---|---|
| `SetAdd` / `SetRemove` | SADD / SREM | Add / remove members |
| `SetGetMembers` | SMEMBERS | All members |
| `SetGetLength` | SCARD | Cardinality |
| `SetContains` | SISMEMBER | Membership test |
| `SetPop` | SPOP | Pop random member |
| `SetRandomMember` / `SetRandomMembers` | SRANDMEMBER | Random peek |
| `SetMove` | SMOVE | Move between sets |
| `SetIntersect` / `SetUnion` | SINTER / SUNION | Intersection / union |
| `SetDifference` | SDIFF | Difference |

```csharp
await cache.SetAddAsync("tags", cancellationToken: default, "redis", "cache");
var tags = await cache.SetGetMembersAsync("tags");
var both = await cache.SetIntersectAsync(cancellationToken: default, "tags", "tags:hot");
```

### 9.7 Sorted Set (ZSet)

Use `RedisSortedSetEntry(Member, Score)` for scored members.

| Method | Redis | Description |
|---|---|---|
| `SortedSetAdd` | ZADD | Single or batch |
| `SortedSetRemove` | ZREM | Remove members |
| `SortedSetGetScore` | ZSCORE | Get score |
| `SortedSetGetRank` | ZRANK / ZREVRANK | `ascending=false` ⇒ reverse rank |
| `SortedSetRangeByRank` | ZRANGE / ZREVRANGE | By rank range |
| `SortedSetRangeByRankWithScores` | … WITHSCORES | With scores |
| `SortedSetRangeByScore` | ZRANGEBYSCORE / ZREV… | By score range |
| `SortedSetRangeByScoreWithScores` | … WITHSCORES | With scores |
| `SortedSetGetLength` | ZCARD | Count |
| `SortedSetCountByScore` | ZCOUNT | Count in score range |
| `SortedSetIncrementScore` | ZINCRBY | Increment score |
| `SortedSetRemoveRangeByRank` | ZREMRANGEBYRANK | Remove by rank |
| `SortedSetRemoveRangeByScore` | ZREMRANGEBYSCORE | Remove by score |

```csharp
await cache.SortedSetAddAsync("rank", "alice", 100);
await cache.SortedSetAddAsync("rank", new[]
{
    new RedisSortedSetEntry("bob", 90),
    new RedisSortedSetEntry("carol", 95)
});
var top = await cache.SortedSetRangeByRankWithScoresAsync("rank", 0, 9, ascending: false);
```

---

## 10. 🔄 Transactions `IDistributedTransaction` / `ICacheTransaction`

Namespace: `EasyCore.Redis.Distributed.Transaction`

**Queue** writes on the transaction object, then `Commit` / `CommitAsync` to `EXEC`.  
**Dispose without Commit → discard (nothing executes).**  
Do not call `IDistributedCache` inside an open transaction expecting atomicity — use the transaction API.

| API | Description |
|---|---|
| `IDistributedTransaction.CreateTransaction()` | Creates `ICacheTransaction` |
| `Set(key, value, seconds=0)` | Queue SET (optional expiry) |
| `Set<T>(key, value, seconds=0)` | Queue JSON SET |
| `Remove(key)` | Queue DEL |
| `Commit` / `CommitAsync` | EXEC; returns `true` on success |

```csharp
public class RedisTransaction(IDistributedTransaction transactions)
{
    public async Task RunAsync()
    {
        await using var tran = transactions.CreateTransaction();
        tran.Set("key1", "value1", seconds: 60)
            .Set("key2", new { Ok = true }, seconds: 60)
            .Remove("key3");
        await tran.CommitAsync();
    }
}
```

---

## 11. 🔐 Distributed Lock `IDistributedLock`

Namespace: `EasyCore.Redis.Locking`

| Behavior | Implementation |
|---|---|
| Acquire | Atomic `SET key lockId PX expiry NX` |
| Release | Lua: DEL only if value equals `LockId` |
| Blocking acquire | Exponential backoff (~20–200ms), no busy-spin |
| Renewal | `BlockingLock*` accepts `renewalInterval`; watchdog `PEXPIRE`; cancelled on unlock/`Dispose` |

### 11.1 `LockContext`

| Member | Description |
|---|---|
| `Key` | Logical lock key |
| `LockId` | Lock token (Guid) |
| `IsAcquired` | Whether this handle owns the lock |
| `Dispose` / `DisposeAsync` | Auto `UnLock` when acquired |

### 11.2 API

| Method | Description |
|---|---|
| `AcquireLock` / `AcquireLockAsync` | Try once; seconds or `TimeSpan` |
| `BlockingLock` / `BlockingLockAsync` | Block until success or timeout; optional renewal |
| `UnLock` / `UnLockAsync` | Explicit release (usually prefer `await using`) |

```csharp
// Non-blocking
await using var handle = await locks.AcquireLockAsync("order:42", expirySeconds: 30);
if (handle.IsAcquired)
{
    // critical section
}

// Blocking + renew every 10s while holding a 30s lease
await using var blocking = await locks.BlockingLockAsync(
    "order:42",
    expirySeconds: 30,
    waitTimeoutSeconds: 5,
    renewalIntervalSeconds: 10);
```

---

## 12. ✨ Service Cache `[ServerCache]`

Namespace: `EasyCore.Redis.Service` (attribute in `EasyCore.Redis.Service.Attribute`)

Standalone NuGet (**does not depend** on EasyCore.Invocation). Castle DynamicProxy cache-aside for `Task<T>` methods; also works on MVC Controller / Action via `IFilterFactory`.

### 12.1 Placement

| Placement | Hits | Path |
|---|---|---|
| Interface type | All methods on that interface | Castle |
| Interface method | That method only | Castle |
| Implementation class | Interface methods of the class | Castle |
| Implementation method | That method only | Castle |
| Controller / Action | That type or action | MVC `IFilterFactory` |

Resolution (most specific wins): **impl method → interface method → class → interface type**.

Compose with EasyCore.Polly / EasyCore.Invocation without package references: each package registers its `IAsyncInterceptor` via `TryAddEnumerable` (**not** a single-slot `TryAdd<IAsyncInterceptor>`, or only the first wins). Proxies stack them with `GetServices<IAsyncInterceptor>()`. Default `Order`: Invocation `0` (outer) → Polly `50` → ServerCache `100` (inner).

### 12.2 Attribute

| Property | Default | Description |
|---|---|---|
| `CacheSeconds` | `300` | TTL in seconds |
| `CacheNullValues` | `false` | Whether to cache `null` results |
| `Order` | `100` | MVC / interceptor stack order (lower = outer) |

Cache key format: `svc:{MethodName}:{sha256}`, hashed from `declaringType:methodName:argsJson`.

### 12.3 Registration

```csharp
using EasyCore.Redis.Service.Attribute;

[ServerCache(CacheSeconds = 120)]
public interface IServer
{
    Task<string> GetUser(string userId);

    [ServerCache(CacheSeconds = 60)]
    Task<string> GetHot(string id);
}

public class Server : IServer
{
    public Task<string> GetUser(string userId)
        => Task.FromResult($"user-{userId}");

    public Task<string> GetHot(string id)
        => Task.FromResult($"hot-{id}");
}

builder.Services.AddEasyCoreRedisService();
// or: builder.Services.AddServerCacheProxy<IServer, Server>();
```

Inject `IServer` so calls go through the proxy. Non-generic `Task` is never cached. Put `[ServerCache]` on Controllers/Actions directly — no global filter registration required.

---

## 13. ✅ Production Notes

| Topic | Behavior / advice |
|---|---|
| Connection | Singleton shared `IRedisConnection`, disposed with the host |
| Timeouts | Configure `ConnectTimeout` / `SyncTimeout` as `TimeSpan` |
| Cancellation | Async APIs honor `CancellationToken` |
| Lock safety | Unlock is ownership-checked; dispose unlocks only if acquired |
| ServerCache scan | Pass `Assemblies`, or rely on entry + loaded non-framework assemblies |
| Secrets | Keep passwords in a secret store |
| Prefix | Use distinct `DistributedName` per environment/app |

### Production checklist

- [ ] Load `EndPoints` / password from a secure configuration source
- [ ] Give each app its own `DistributedName`
- [ ] Use `BlockingLock` + `renewalInterval` for long critical sections
- [ ] Apply `[ServerCache]` only to idempotent reads; invalidate or use short TTL on writes
- [ ] Transactions: queue via `ICacheTransaction`, then `Commit`
- [ ] Monitor connect failures and lock contention

---

## 14. 🧪 Demo Project

| Project | Description | Command |
|---|---|---|
| [`demo/Web.EasyCore.Cache`](demo/Web.EasyCore.Cache) | Swagger: cache / tx / lock / `[ServerCache]` + **cross-stack** | `dotnet run --project demo/Web.EasyCore.Cache` |

Default Redis: `localhost:6379` (`appsettings.json` → `EasyCore:Redis`).

Controllers:

- `GET /api/demo` — placement scenarios A–F
- A–F Controllers — interface / class / method / interface-method / multi-interface / API
- `ServiceCacheController` — `[ServerCache]` overloads (legacy)
- `ComboStackController` — `/api/combo` cross-stack
- Distributed / Transaction / Lock — low-level APIs

---

## 15. ❓ FAQ

**Q: Why don’t I see `user:1` in Redis?**  
A: The real key is `{DistributedName}:user:1`. Check `DistributedName`.

**Q: Can I use only the lock package?**  
A: Yes. Reference `EasyCore.Redis.Locking` and call `AddEasyCoreRedisLock(options => { … })` to register the connection as well.

**Q: Does calling `IDistributedCache.SetAsync` inside a transaction make it atomic?**  
A: No. Use `ICacheTransaction.Set(...).CommitAsync()`.

**Q: Why isn’t `[ServerCache]` working?**  
A: For services, inject the **interface**; place the attribute on interface / class / method; return `Task<T>`; register via `AddServerCacheProxy` or auto-scan. For APIs, place it on Controller / Action.

**Q: Can I use it with EasyCore.Polly / EasyCore.Invocation?**  
A: Yes. They are independent NuGet packages. Each registers `IAsyncInterceptor` via `TryAddEnumerable`; proxies stack with `GetServices`. Do not use single-slot `TryAdd<IAsyncInterceptor>` or only the first package wins.

**Q: Parameter order for `List*Async` / `Set*Async` with `params`?**  
A: `CancellationToken` comes before the `params` array, e.g. `SetAddAsync(key, ct, "a", "b")`.

**Q: What version is this?**  
A: Current release is **8.0.0**.

---

## 16. 📄 License

MIT — see [LICENSE](LICENSE) and the NuGet package declaration.

---

## 🤝 Contributing

1. Fork and create a feature branch  
2. Add tests under `tests/EasyCore.Cache.Tests`  
3. Run `dotnet test` and `dotnet build EasyCore.Redis.sln`  
4. Open a pull request  

Issues and PRs are welcome 🚀
