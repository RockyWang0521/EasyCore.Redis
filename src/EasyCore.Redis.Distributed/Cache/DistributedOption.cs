using StackExchange.Redis;

namespace EasyCore.Redis.Distributed
{
    /// <summary>
    /// Redis connection and key-prefix options shared by cache and distributed lock.
    /// </summary>
    public sealed class DistributedOption
    {
        /// <summary>
        /// Redis endpoints, e.g. <c>127.0.0.1:6379</c>. At least one is required.
        /// </summary>
        public List<string> EndPoints { get; set; } = new();

        /// <summary>
        /// Redis ACL username (Redis 6+). Optional.
        /// </summary>
        public string? User { get; set; }

        /// <summary>
        /// Redis password. Optional.
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Connection timeout. Default: 5 seconds.
        /// </summary>
        public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Synchronous operation timeout. Default: 5 seconds.
        /// </summary>
        public TimeSpan SyncTimeout { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Whether to abort when the initial connection fails.
        /// </summary>
        public bool AbortOnConnectFail { get; set; }

        /// <summary>
        /// Default Redis database index.
        /// </summary>
        public int DefaultDatabase { get; set; }

        /// <summary>
        /// Key prefix for all cache/lock keys (namespace isolation). Default: <c>EasyCore</c>.
        /// </summary>
        public string DistributedName { get; set; } = "EasyCore";

        /// <summary>
        /// Converts these options to a StackExchange.Redis <see cref="ConfigurationOptions"/> instance.
        /// </summary>
        /// <returns>Configured connection options.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no valid endpoints are configured.</exception>
        public ConfigurationOptions ToConfigurationOptions()
        {
            if (EndPoints.Count == 0)
                throw new InvalidOperationException("No Redis endpoints configured. Set DistributedOption.EndPoints.");

            var configOptions = new ConfigurationOptions
            {
                User = User,
                Password = Password,
                ConnectTimeout = (int)Math.Max(1, ConnectTimeout.TotalMilliseconds),
                SyncTimeout = (int)Math.Max(1, SyncTimeout.TotalMilliseconds),
                AbortOnConnectFail = AbortOnConnectFail,
                DefaultDatabase = DefaultDatabase
            };

            foreach (var endpoint in EndPoints)
            {
                if (!string.IsNullOrWhiteSpace(endpoint))
                    configOptions.EndPoints.Add(endpoint.Trim());
            }

            if (configOptions.EndPoints.Count == 0)
                throw new InvalidOperationException("No valid Redis endpoints configured.");

            return configOptions;
        }

        /// <summary>
        /// Builds a prefixed Redis key using <see cref="DistributedName"/>.
        /// </summary>
        /// <param name="key">Logical key (must not be null or whitespace).</param>
        /// <returns>Prefixed key in the form <c>{DistributedName}:{key}</c>, or <paramref name="key"/> when the prefix is empty.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is null or whitespace.</exception>
        public string GetPrefixedKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key must not be null or whitespace.", nameof(key));

            return string.IsNullOrWhiteSpace(DistributedName)
                ? key
                : $"{DistributedName}:{key}";
        }
    }
}
