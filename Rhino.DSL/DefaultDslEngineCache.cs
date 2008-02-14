namespace Rhino.DSL
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provide cache support for a DSL
    /// </summary>
    public class DefaultDslEngineCache : IDslEngineCache
    {
        private readonly IDictionary<string, Type> urlToTypeCache = new Dictionary<string, Type>();

        /// <summary>
        /// Try to get a cached type for this URL.
        /// </summary>
        /// <param name="url">The url to use as a key for the cache</param>
        /// <returns>The compiled DSL or null if not found</returns>
        public virtual Type Get(string url)
        {
            Type result;
            urlToTypeCache.TryGetValue(url, out result);
            return result;
        }

        /// <summary>
        /// Put the type in the cache, with the url as the key
        /// </summary>
        public virtual void Set(string url, Type type)
        {
            urlToTypeCache[url] = type;
        }

        /// <summary>
        /// Removes the url for the from cache.
        /// </summary>
        /// <param name="url">The URL.</param>
        public virtual void Remove(string url)
        {
            urlToTypeCache.Remove(url);
        }
    }
}