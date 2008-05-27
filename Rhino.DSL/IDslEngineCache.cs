namespace Rhino.DSL
{
    using System;

	/// <summary>
	/// An action delegate to be executed under a cache lock
	/// </summary>
	public delegate void CacheAction();

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

		/// <summary>
		/// Execute the action under a write lock
		/// </summary>
		/// <param name="cacheAction">The cache action.</param>
    	void WriteLock(CacheAction cacheAction);

		/// <summary>
		/// Execute the action under a read lock
		/// </summary>
		/// <param name="cacheAction">The cache action.</param>
    	void ReadLock(CacheAction cacheAction);
    }
}