using StackExchange.Redis;
using System;

namespace PopcornApi.Services.Caching
{
    /// <summary>
    /// The caching service
    /// </summary>
    public class CachingService : ICachingService
    {
        /// <summary>
        /// The Redis connection multiplexer
        /// </summary>
        private readonly IConnectionMultiplexer _connection;

        /// <summary>
        /// Redis database
        /// </summary>
        private readonly IDatabase _redisDatabase;

        /// <summary>
        /// Create an instance of <see cref="CachingService"/>
        /// </summary>
        public CachingService(string redisConnectionString)
        {
            var redisConfig = ConfigurationOptions.Parse(redisConnectionString);
            redisConfig.SyncTimeout = 5000;
             _connection = ConnectionMultiplexer.Connect(redisConfig);
            _connection.IncludeDetailInExceptions = true;
            _redisDatabase = _connection.GetDatabase();
        }

        /// <summary>
        /// Cache
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <param name="expiry">Expiry</param>
        public void SetCache(string key, string value, TimeSpan? expiry = null)
        {
            _redisDatabase.StringSet(key, value, expiry);
        }

        /// <summary>
        /// Cache
        /// </summary>
        /// <param name="key">Key</param>
        public string GetCache(string key)
        {
            return _redisDatabase.StringGet(key);
        }
    }
}
