# 🔴 EasyCore.Redis

> **EasyCore.Redis** 是面向 .NET 8 的生产级 Redis 工具库。基于 [StackExchange.Redis](https://stackexchange.github.io/StackExchange.Redis/)，提供**分布式缓存**（五大数据类型）、**MULTI/EXEC 事务**、**分布式锁**，以及基于 Castle DynamicProxy 的 `**[ServerCache]` 方法结果缓存**。

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12-239120?logo=csharp)
![Redis](https://img.shields.io/badge/Redis-StackExchange-DC382D?logo=redis)
![Cache](https://img.shields.io/badge/Cache-String%20%7C%20Hash%20%7C%20List%20%7C%20Set%20%7C%20ZSet-orange)
![Lock](https://img.shields.io/badge/Lock-SET%20NX%20PX%20%2B%20Lua-blueviolet)
![License](https://img.shields.io/badge/License-MIT-yellow)
![Version](https://img.shields.io/badge/Version-8.3.0-blue)

仓库：[github.com/RockyWang0521/EasyCore.Redis](https://github.com/RockyWang0521/EasyCore.Redis)

---

## 🌍 Language

- **中文（当前文档）**
- English: [README.en.md](https://github.com/RockyWang0521/EasyCore.Redis/blob/master/README.en.md)

---

## 📚 目录

### 🧭 第一部分：总览与架构

- [1. 🎯 项目定位](#1-项目定位)
- [2. 🏗 架构与模块关系](#2-架构与模块关系)
- [3. 📦 NuGet / 项目清单](#3-nuget--项目清单)
- [4. 📊 能力对比](#4-能力对比)

### 🚀 第二部分：快速上手

- [5. ⚙ 环境要求](#5-环境要求)
- [6. 📥 安装](#6-安装)
- [7. ⚡ 三分钟快速开始](#7-三分钟快速开始)
- [8. 🧩 配置项完整说明](#8-配置项完整说明)

### 📘 第三部分：API 详解

- [9. 💾 分布式缓存 `IDistributedCache`](#9-分布式缓存-idistributedcache)
- [10. 🔄 事务 `IDistributedTransaction` / `ICacheTransaction`](#10-事务-idistributedtransaction--icachetransaction)
- [11. 🔐 分布式锁 `IDistributedLock`](#11-分布式锁-idistributedlock)
- [12. ✨ 服务缓存 `[ServerCache]`](#12-服务缓存-servercache)

### 🏭 第四部分：生产与示例

- [13. ✅ 生产要点](#13-生产要点)
- [14. 🧪 Demo 项目](#14-demo-项目)
- [15. ❓ FAQ](#15-faq)
- [16. 📄 License](#16-license)

---

## 1. 🎯 项目定位

EasyCore.Redis 解决「在 ASP.NET Core 中安全、清晰地使用 Redis」的问题：


| 痛点                 | EasyCore.Redis 做法                               |
| ------------------ | ----------------------------------------------- |
| SE.Redis 命令多、上手成本高 | `IDistributedCache` 统一封装五大数据类型                  |
| 多服务争用同一连接          | 共享 `IRedisConnection` / `ConnectionMultiplexer` |
| 键冲突 / 多环境串数据       | `DistributedName` 自动前缀隔离                        |
| 临界区并发难控            | `IDistributedLock`（SET NX PX + Lua 校验解锁）        |
| 方法结果重复计算           | `[ServerCache]` + Castle 代理自动缓存                 |
| 批量写入要原子            | `ICacheTransaction`（MULTI/EXEC）                 |


### 1.1 设计原则


| 原则        | 说明                                             |
| --------- | ---------------------------------------------- |
| **低摩擦接入** | 一个 `AddEasyCoreRedis(...)` 即可注册全部能力               |
| **按需拆分**  | `.Distributed` / `.Locking` / `.Service` 可独立引用 |
| **键空间隔离** | 所有逻辑键自动加 `{DistributedName}:` 前缀               |
| **锁安全**   | 解锁校验 `LockId`；`Dispose` 仅在持有时释放                |
| **显式事务**  | 在事务对象上排队，再 `Commit`；未提交丢弃                      |


### 1.2 解决方案目录

```text
EasyCore.Redis/
├── src/
│   ├── EasyCore.Redis/                 # 元包：一键注册全部能力
│   ├── EasyCore.Redis.Distributed/     # 缓存 + 事务 + 连接
│   ├── EasyCore.Redis.Locking/         # 分布式锁
│   └── EasyCore.Redis.Service/         # [ServerCache] AOP
├── demo/Web.EasyCore.Cache/            # Swagger Demo
├── tests/EasyCore.Cache.Tests/
└── docs/svg/                           # README 架构图（NuGet 用绝对 URL）
```

---

## 2. 🏗 架构与模块关系

### 2.1 组件关系图

![architecture-cn](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Redis/master/docs/svg/architecture-cn.svg)

### 2.2 核心流程

![sequence-cn](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Redis/master/docs/svg/sequence-cn.svg)

### 2.3 API 能力一览

![api-overview-cn](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Redis/master/docs/svg/api-overview-cn.svg)

### 2.4 依赖关系（文字版）

```text
AddEasyCoreRedis() / AddEasyCoreRedis(IConfiguration)
        │
        ├── AddEasyCoreRedisDistributed ──► IRedisConnection
        │                              ├─ IDistributedCache
        │                              └─ IDistributedTransaction → ICacheTransaction
        ├── AddEasyCoreRedisLock ─────────► IDistributedLock  (复用同一连接)
        └── AddEasyCoreRedisService ──────► [ServerCache] Proxy (依赖 IDistributedCache)
```

---

## 3. 📦 NuGet / 项目清单


| 包名                           | 职责                        | 是否必须 |
| ---------------------------- | ------------------------- | ---- |
| `EasyCore.Redis`             | 元包，注册缓存 + 锁 + 服务缓存        | ✅ 推荐 |
| `EasyCore.Redis.Distributed` | 连接、`IDistributedCache`、事务 | 按需   |
| `EasyCore.Redis.Locking`     | `IDistributedLock`        | 按需   |
| `EasyCore.Redis.Service`     | `[ServerCache]` Castle 代理 | 按需   |


> 只装锁时需先有连接：可用 `AddEasyCoreRedisLock(configure)`，或先 `AddEasyCoreRedisDistributed` 再 `AddEasyCoreRedisLock()`。

---

## 4. 📊 能力对比


| 能力                                | Distributed | Locking | Service |
| --------------------------------- | ----------- | ------- | ------- |
| String / Hash / List / Set / ZSet | ✅           | —       | —       |
| 键辅助 Exists / TTL / Remove         | ✅           | —       | —       |
| MULTI/EXEC 事务                     | ✅           | —       | —       |
| SET NX PX 锁 + Lua 解锁              | —           | ✅       | —       |
| 阻塞获取 + 续期 Watchdog                | —           | ✅       | —       |
| 方法结果 AOP 缓存                       | —           | —       | ✅       |


---

## 5. ⚙ 环境要求


| 项     | 要求                                     |
| ----- | -------------------------------------- |
| .NET  | 8.0+                                   |
| 宿主    | ASP.NET Core / 通用主机均可                  |
| Redis | 可达的 Redis 实例（Demo 默认 `localhost:6379`） |
| 客户端   | StackExchange.Redis（由包引入）              |


---

## 6. 📥 安装

```bash
dotnet add package EasyCore.Redis
# 当前版本 8.0.0

# 或按需拆分
dotnet add package EasyCore.Redis.Distributed
dotnet add package EasyCore.Redis.Locking
dotnet add package EasyCore.Redis.Service
```

---

## 7. ⚡ 三分钟快速开始

### 7️⃣.1️⃣ 代码配置注册

```csharp
using EasyCore.Redis;

builder.Services.AddEasyCoreRedis(options =>
{
    options.EndPoints = new List<string> { "127.0.0.1:6379" };
    options.ConnectTimeout = TimeSpan.FromSeconds(5);
    options.SyncTimeout = TimeSpan.FromSeconds(5);
    options.DistributedName = "MyApp";   // 键前缀 → MyApp:user:1
    // options.Password = "***";
    // options.DefaultDatabase = 0;
});
```

### 7️⃣.2️⃣ 从 appsettings.json 注册

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

### 7️⃣.3️⃣ 注入并使用

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
        // 临界区…
    }
}
```

### 7️⃣.4️⃣ 按特性拆分注册

```csharp
builder.Services.AddEasyCoreRedisDistributed(o => { /* EndPoints… */ });
builder.Services.AddEasyCoreRedisLock();   // 复用已注册连接
builder.Services.AddEasyCoreRedisService(); // 自动扫描带 [ServerCache] 的服务
// 或显式：builder.Services.AddServerCacheProxy<IMyService, MyService>();
```

---

## 8. 🧩 配置项完整说明

`DistributedOption`（缓存与锁共用）：


| 属性                   | 类型             | 默认           | 说明                 |
| -------------------- | -------------- | ------------ | ------------------ |
| `EndPoints`          | `List<string>` | 空（必填）        | 如 `127.0.0.1:6379` |
| `User`               | `string?`      | `null`       | Redis 6+ ACL 用户名   |
| `Password`           | `string?`      | `null`       | 密码                 |
| `ConnectTimeout`     | `TimeSpan`     | 5s           | 连接超时               |
| `SyncTimeout`        | `TimeSpan`     | 5s           | 同步操作超时             |
| `AbortOnConnectFail` | `bool`         | `false`      | 初次连接失败是否中止         |
| `DefaultDatabase`    | `int`          | `0`          | 默认 DB              |
| `DistributedName`    | `string`       | `"EasyCore"` | 键前缀命名空间            |


键规则：逻辑键 `user:1` → 实际 Redis 键 `{DistributedName}:user:1`（例如 `MyApp:user:1`）。

---

## 9. 💾 分布式缓存 `IDistributedCache`

命名空间：`EasyCore.Redis.Distributed`

几乎所有 API 均提供 **同步** 与 **异步**（`*Async`）成对方法；异步方法支持 `CancellationToken`。复杂对象通过 **JSON** 序列化存取（`StringGet<T>` / `StringSet<T>` / `HashGet<T>` 等）。

### 9.1 快捷方法（映射到 String）


| 方法                       | Redis | 说明                     |
| ------------------------ | ----- | ---------------------- |
| `Get` / `GetAsync`       | GET   | 取字符串                   |
| `Get<T>` / `GetAsync<T>` | GET   | JSON 反序列化              |
| `Set` / `SetAsync`       | SET   | 写字符串；`seconds=0` 表示不过期 |
| `Set<T>` / `SetAsync<T>` | SET   | JSON 序列化写入             |


### 9.2 键辅助


| 方法                                     | Redis  | 说明                                            |
| -------------------------------------- | ------ | --------------------------------------------- |
| `KeyExists` / `KeyExistsAsync`         | EXISTS | 键是否存在                                         |
| `KeyType` / `KeyTypeAsync`             | TYPE   | 返回 `string`/`hash`/`list`/`set`/`zset`/`none` |
| `KeyTimeToLive` / `KeyTimeToLiveAsync` | TTL    | 秒；`-1` 无过期，`-2` 键不存在                          |
| `Refresh` / `RefreshAsync`             | EXPIRE | 重置绝对过期（`seconds` 必须 > 0）                      |
| `Remove` / `RemoveAsync`               | DEL    | 删除键                                           |


### 9.3 String


| 方法                                   | Redis                | 说明                                 |
| ------------------------------------ | -------------------- | ---------------------------------- |
| `StringGet` / `StringGetAsync`       | GET                  | 取字符串                               |
| `StringGet<T>` / `StringGetAsync<T>` | GET                  | JSON 反序列化                          |
| `StringSet` / `StringSetAsync`       | SET / SET NX         | `whenNotExists=true` 时 NX；返回是否写入成功 |
| `StringSet<T>` / `StringSetAsync<T>` | SET                  | JSON 写入                            |
| `StringGetDeleteAsync`               | GETDEL               | 原子取并删                              |
| `StringIncrement` / `*Async`         | INCRBY / INCRBYFLOAT | 整数或浮点自增                            |
| `StringDecrement` / `*Async`         | DECRBY               | 自减                                 |
| `StringAppend` / `*Async`            | APPEND               | 追加，返回新长度                           |
| `StringGetLength` / `*Async`         | STRLEN               | 长度                                 |
| `StringSetExpiry` / `*Async`         | EXPIRE               | 设置过期                               |


```csharp
await cache.StringSetAsync("user:1", "alice", seconds: 60);
await cache.StringSetAsync("profile:1", new { Name = "alice", Age = 18 }, seconds: 300);
var ok = await cache.StringSetAsync("once", "v", whenNotExists: true); // SET NX
await cache.StringIncrementAsync("counter");
var prev = await cache.StringGetDeleteAsync("once");
```

### 9.4 Hash


| 方法                                   | Redis                  | 说明                        |
| ------------------------------------ | ---------------------- | ------------------------- |
| `HashSet(key, field, value)`         | HSET                   | 单字段；返回是否新建字段              |
| `HashSet<T>(...)`                    | HSET                   | JSON 字段值                  |
| `HashSet(key, IDictionary, seconds)` | HSET multi             | 批量；`seconds>0` 时顺便 EXPIRE |
| `HashGet` / `HashGet<T>`             | HGET                   | 取字段                       |
| `HashGetAll`                         | HGETALL                | 全部字段                      |
| `HashDelete`                         | HDEL                   | 删字段，返回删除数                 |
| `HashExists`                         | HEXISTS                | 字段是否存在                    |
| `HashGetKeys` / `HashGetValues`      | HKEYS / HVALS          | 字段名 / 值                   |
| `HashGetLength`                      | HLEN                   | 字段数                       |
| `HashIncrement`                      | HINCRBY / HINCRBYFLOAT | 字段自增                      |


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


| 方法                                  | Redis         | 说明                     |
| ----------------------------------- | ------------- | ---------------------- |
| `ListLeftPush` / `ListRightPush`    | LPUSH / RPUSH | 头/尾入队（`params`，至少 1 个） |
| `ListLeftPop` / `ListRightPop`      | LPOP / RPOP   | 头/尾出队                  |
| `ListRange`                         | LRANGE        | 区间；默认 `0..-1` 全部       |
| `ListGetLength`                     | LLEN          | 长度                     |
| `ListGetByIndex` / `ListSetByIndex` | LINDEX / LSET | 按下标读写                  |
| `ListTrim`                          | LTRIM         | 裁剪                     |
| `ListRemove`                        | LREM          | `count`：0=全部；正=从头；负=从尾 |


> 异步 `params` 重载中，`CancellationToken` 位于 values 之前：  
> `ListRightPushAsync(key, cancellationToken, "a", "b")`。

```csharp
await cache.ListRightPushAsync("queue", cancellationToken: default, "a", "b", "c");
var items = await cache.ListRangeAsync("queue");
var first = await cache.ListLeftPopAsync("queue");
```

### 9.6 Set


| 方法                                     | Redis           | 说明     |
| -------------------------------------- | --------------- | ------ |
| `SetAdd` / `SetRemove`                 | SADD / SREM     | 增删成员   |
| `SetGetMembers`                        | SMEMBERS        | 全部成员   |
| `SetGetLength`                         | SCARD           | 基数     |
| `SetContains`                          | SISMEMBER       | 是否包含   |
| `SetPop`                               | SPOP            | 弹出随机成员 |
| `SetRandomMember` / `SetRandomMembers` | SRANDMEMBER     | 随机查看   |
| `SetMove`                              | SMOVE           | 在集合间移动 |
| `SetIntersect` / `SetUnion`            | SINTER / SUNION | 交 / 并  |
| `SetDifference`                        | SDIFF           | 差集     |


```csharp
await cache.SetAddAsync("tags", cancellationToken: default, "redis", "cache");
var tags = await cache.SetGetMembersAsync("tags");
var both = await cache.SetIntersectAsync(cancellationToken: default, "tags", "tags:hot");
```

### 9.7 Sorted Set（ZSet）

使用 `RedisSortedSetEntry(Member, Score)` 表示带分成员。


| 方法                                | Redis                 | 说明                      |
| --------------------------------- | --------------------- | ----------------------- |
| `SortedSetAdd`                    | ZADD                  | 单条或批量                   |
| `SortedSetRemove`                 | ZREM                  | 删成员                     |
| `SortedSetGetScore`               | ZSCORE                | 取分                      |
| `SortedSetGetRank`                | ZRANK / ZREVRANK      | `ascending=false` 为倒序排名 |
| `SortedSetRangeByRank`            | ZRANGE / ZREVRANGE    | 按排名区间                   |
| `SortedSetRangeByRankWithScores`  | … WITHSCORES          | 带分                      |
| `SortedSetRangeByScore`           | ZRANGEBYSCORE / ZREV… | 按分区间                    |
| `SortedSetRangeByScoreWithScores` | … WITHSCORES          | 带分                      |
| `SortedSetGetLength`              | ZCARD                 | 成员数                     |
| `SortedSetCountByScore`           | ZCOUNT                | 分区间计数                   |
| `SortedSetIncrementScore`         | ZINCRBY               | 加分                      |
| `SortedSetRemoveRangeByRank`      | ZREMRANGEBYRANK       | 按排名删                    |
| `SortedSetRemoveRangeByScore`     | ZREMRANGEBYSCORE      | 按分删                     |


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

## 10. 🔄 事务 `IDistributedTransaction` / `ICacheTransaction`

命名空间：`EasyCore.Redis.Distributed.Transaction`

在事务对象上**排队**写操作，再 `Commit` / `CommitAsync` 执行 `EXEC`。  
**Dispose 且未 Commit → 丢弃，不会执行。**  
不要在事务未提交时对同一逻辑去调 `IDistributedCache` 指望原子性——请用事务 API。


| API                                           | 说明                     |
| --------------------------------------------- | ---------------------- |
| `IDistributedTransaction.CreateTransaction()` | 创建 `ICacheTransaction` |
| `Set(key, value, seconds=0)`                  | 排队 SET（可带过期）           |
| `Set<T>(key, value, seconds=0)`               | 排队 JSON SET            |
| `Remove(key)`                                 | 排队 DEL                 |
| `Commit` / `CommitAsync`                      | EXEC；成功返回 `true`       |


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

## 11. 🔐 分布式锁 `IDistributedLock`

命名空间：`EasyCore.Redis.Locking`


| 行为   | 实现                                                                           |
| ---- | ---------------------------------------------------------------------------- |
| 加锁   | 原子 `SET key lockId PX expiry NX`                                             |
| 解锁   | Lua：仅当值等于 `LockId` 时 DEL                                                     |
| 阻塞获取 | 指数退避（约 20–200ms），非忙等                                                         |
| 续期   | `BlockingLock*` 可传 `renewalInterval`，后台 Watchdog `PEXPIRE`；解锁/`Dispose` 自动取消 |


### 11.1 `LockContext`


| 属性 / 方法                    | 说明             |
| -------------------------- | -------------- |
| `Key`                      | 逻辑锁键           |
| `LockId`                   | 锁令牌（Guid）      |
| `IsAcquired`               | 是否持有锁          |
| `Dispose` / `DisposeAsync` | 持有时自动 `UnLock` |


### 11.2 API 一览


| 方法                                   | 说明                         |
| ------------------------------------ | -------------------------- |
| `AcquireLock` / `AcquireLockAsync`   | 尝试一次；参数为秒或 `TimeSpan`      |
| `BlockingLock` / `BlockingLockAsync` | 阻塞直到成功或超时；可选续期间隔           |
| `UnLock` / `UnLockAsync`             | 主动释放（通常用 `await using` 即可） |


```csharp
// 非阻塞
await using var handle = await locks.AcquireLockAsync("order:42", expirySeconds: 30);
if (handle.IsAcquired)
{
    // 临界区
}

// 阻塞 + 每 10 秒续期（租约 30 秒）
await using var blocking = await locks.BlockingLockAsync(
    "order:42",
    expirySeconds: 30,
    waitTimeoutSeconds: 5,
    renewalIntervalSeconds: 10);
```

---

## 12. ✨ 服务缓存 `[ServerCache]`

命名空间：`EasyCore.Redis.Service`（特性在 `EasyCore.Redis.Service.Attribute`）

独立 NuGet 包（**不依赖** EasyCore.Invocation）。通过 Castle DynamicProxy 拦截返回 `Task<T>` 的方法做 cache-aside；也可直接挂在 MVC Controller / Action（特性实现 `IFilterFactory`）。

### 12.1 放置位置（与 Invocation 风格一致，但自包含）

| 挂载位置 | 命中范围 | 生效路径 |
|---|---|---|
| 接口类型 `[ServerCache] interface IFoo` | 该接口全部方法 | Castle 接口代理 |
| 接口方法 | 仅该方法 | Castle 接口代理 |
| 实现类 | 该类对外接口方法 | Castle 接口代理 |
| 实现方法 | 仅该方法 | Castle 接口代理 |
| Controller / Action | 该类或该 Action | MVC `IFilterFactory`（无需全局 Filter） |

解析优先级（最具体胜出）：**实现方法 → 接口方法 → 类 → 接口类型**。

与 EasyCore.Polly / EasyCore.Invocation 等其它包组合时：各自用 `TryAddEnumerable` 注册自己的 `IAsyncInterceptor`（**不要**用 `TryAdd<IAsyncInterceptor>`，否则只会留下第一个），代理创建时通过 `GetServices<IAsyncInterceptor>()` 堆叠，**互不引用、互不写死类型名**。默认 `Order`：Invocation `0`（最外）→ Polly `50` → ServerCache `100`（最内）。

### 12.2 特性


| 属性                | 默认      | 说明             |
| ----------------- | ------- | -------------- |
| `CacheSeconds`    | `300`   | TTL（秒）         |
| `CacheNullValues` | `false` | 是否缓存 `null` 结果 |
| `Order`           | `100`   | MVC / 拦截器堆叠顺序（越小越外） |


缓存键格式：`svc:{MethodName}:{sha256}`，哈希输入为 `声明类型全名:方法名:参数JSON`。

### 12.3 注册方式

```csharp
using EasyCore.Redis.Service.Attribute;

// 推荐：特性挂在接口上，实现类保持干净
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

// 方式 A：AddEasyCoreRedis / AddEasyCoreRedisService 自动扫描
builder.Services.AddEasyCoreRedisService();
// 或补充程序集：
builder.Services.AddEasyCoreRedisService(o => o.Assemblies.Add(typeof(Server).Assembly));

// 方式 B：显式代理
builder.Services.AddServerCacheProxy<IServer, Server>();
```

注入 `IServer` 后，调用会经代理。非泛型 `Task`（无结果）不会缓存。Controller 上直接标 `[ServerCache]` 即可，无需额外 Filter 注册。

---

## 13. ✅ 生产要点


| 主题     | 行为 / 建议                                           |
| ------ | ------------------------------------------------- |
| 连接     | 单例共享 `IRedisConnection`，随宿主释放                     |
| 超时     | 使用 `TimeSpan` 配置 `ConnectTimeout` / `SyncTimeout` |
| 取消     | 异步 API 支持 `CancellationToken`                     |
| 锁安全    | 解锁校验所有权；仅 `IsAcquired` 时 Dispose 才解锁              |
| 服务缓存扫描 | 可传 `Assemblies`；默认扫描入口程序集及已加载非框架程序集               |
| 密钥     | 密码放入配置中心 / 密钥库，勿提交仓库                              |
| 前缀     | 多环境使用不同 `DistributedName`，避免键冲突                   |


### 生产清单

- [ ] `EndPoints` / 密码来自安全配置源
- [ ] 为每个应用设置独立 `DistributedName`
- [ ] 长临界区使用 `BlockingLock` + `renewalInterval`
- [ ] `[ServerCache]` 仅用于幂等读；写路径主动失效或短 TTL
- [ ] 事务：只通过 `ICacheTransaction` 排队后 `Commit`
- [ ] 监控连接失败与锁争用日志

---

## 14. 🧪 Demo 项目


| 项目                                                   | 说明                           | 命令                                             |
| ---------------------------------------------------- | ---------------------------- | ---------------------------------------------- |
| `[demo/Web.EasyCore.Cache](demo/Web.EasyCore.Cache)` | Swagger：缓存 / 事务 / 锁 / `[ServerCache]` + **交叉堆叠** | `dotnet run --project demo/Web.EasyCore.Cache` |


默认 Redis：`localhost:6379`（见 `appsettings.json` → `EasyCore:Redis`）。

控制器示例：

- `GET /api/demo` — 放置场景 A–F 总览
- A–F Controllers — 接口类型 / 类 / 方法 / 接口方法 / 多接口 / API
- `ServiceCacheController` — `[ServerCache]` 参数重载（legacy）
- `ComboStackController` — `/api/combo`：三包堆叠联调
- `DistributedCacheController` / Transaction / Lock — 底层 API

---

## 15. ❓ FAQ

**Q: 为什么 Redis 里看不到我写的 `user:1`？**  
A: 实际键是 `{DistributedName}:user:1`。检查配置中的 `DistributedName`。

**Q: 只想用锁，不装缓存包？**  
A: 引用 `EasyCore.Redis.Locking`，调用 `AddEasyCoreRedisLock(options => { … })` 会一并注册连接。

**Q: 事务里调用 `IDistributedCache.SetAsync` 会原子执行吗？**  
A: 不会。请使用 `ICacheTransaction.Set(...).CommitAsync()`。

**Q: `[ServerCache]` 为什么没生效？**  
A: 服务场景须注入**接口**（代理目标）；特性可挂在接口 / 类 / 方法上；方法需返回 `Task<T>`；实现需被 `AddServerCacheProxy` 或自动扫描注册。API 场景可直接挂 Controller / Action。

**Q: 能和 EasyCore.Polly / EasyCore.Invocation 一起用吗？**  
A: 可以。三者是独立 NuGet，互不引用；各包以 `TryAddEnumerable` 注册 `IAsyncInterceptor`，代理侧 `GetServices` 堆叠生效。勿用单槽 `TryAdd<IAsyncInterceptor>`，否则只会生效先注册的那一个。

**Q: `List*Async` / `Set*Async` 的 `params` 参数顺序？**  
A: `CancellationToken` 在 `params` 数组之前，例如 `SetAddAsync(key, ct, "a", "b")`。

**Q: 版本是多少？**  
A: 当前为 **8.0.0**。

---

## 16. 📄 License

MIT — 详见 [LICENSE](LICENSE) 与 NuGet 包声明。

---

## 🤝 贡献

1. Fork 并创建特性分支
2. 在 `tests/EasyCore.Cache.Tests` 补充测试
3. 执行 `dotnet test` 与 `dotnet build EasyCore.Redis.sln`
4. 提交 Pull Request

欢迎 Issue / PR 🚀