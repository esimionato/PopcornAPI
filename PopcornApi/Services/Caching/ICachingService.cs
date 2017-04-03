using System;

namespace PopcornApi.Services.Caching
{
    /// <summary>
    /// Caching service
    /// </summary>
    public interface ICachingService
    {
        /// <summary>
        /// Cache
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <param name="expiry">Expiry</param>
        void SetCache(string key, string value, TimeSpan? expiry = null);

        /// <summary>
        /// Cache
        /// </summary>
        /// <param name="key">Key</param>
        string GetCache(string key);
    }
}
