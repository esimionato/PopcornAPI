using StackExchange.Redis;
using System;
using System.Threading.Tasks;

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
        public async Task SetCache(string key, string value, TimeSpan? expiry = null)
        {
            try
            {
                await _redisDatabase.StringSetAsync(key, value, expiry);
            }
            catch (Exception)
            {
                
            }
        }

        /// <summary>
        /// Cache
        /// </summary>
        /// <param name="key">Key</param>
        public async Task<string> GetCache(string key)
        {
            return await _redisDatabase.StringGetAsync(key);
        }
    }
}
