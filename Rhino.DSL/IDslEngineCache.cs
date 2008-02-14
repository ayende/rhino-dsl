namespace Rhino.DSL
{
    using System;

    /// <summary>
    /// Cahce interface for the DslEngine
    /// </summary>
    public interface IDslEngineCache
    {
        /// <summary>
        /// Try to get a cached type for this URL.
        /// </summary>
        /// <param name="url">The url to use as a key for the cache</param>
        /// <returns>The compiled DSL or null if not found</returns>
        Type Get(string url);

        /// <summary>
        /// Put the type in the cache, with the url as the key
        /// </summary>
        void Set(string url, Type type);

        /// <summary>
        /// Removes the url for the from cache.
        /// </summary>
        /// <param name="url">The URL.</param>
        void Remove(string url);
    }
}